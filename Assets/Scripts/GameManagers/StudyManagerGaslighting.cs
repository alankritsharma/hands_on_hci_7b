using System.Collections.Generic;
using DefaultNamespace;
using Managers;
using UnityEngine;

namespace GameManagers
{
    public class StudyManagerGaslighting : GameManager
    {
        [Header("References")]
        [SerializeField] private Transform centerEyeAnchor;
        [SerializeField] private UIManagerGaslightTask uiManagerGaslightTask;
        [SerializeField] private GameObject obstacle;
        [SerializeField] private Transform collisionTransform;
        [SerializeField] private List<GameObject> parkourElements;
        
        // private bool adjustInstructionHeight;
        
        private DataLoggingGaslight dataLogging;
        private NoiseManager noiseManager;

        private int participantId;
        
        private GaslightState state = GaslightState.Alignment;
        private BlockType blockType;
        private int iteration;
        private float currentTimer;
        private float currentRunStart;
        private bool isLookingAtStartingDot;
        private Transform originalTransform;

        public enum BlockType
        {
            AR,         // a block in AR
            VR,         // a block in VR
            Gaslight    // a block with manipulations
        }
        
        private enum GaslightState
        {
            Alignment,          // alignment
            SelectTrialType,    // select with manipulations or just AR
            WaitingForStart,    // waiting for the start of the run
            Walking,            // walking the parkour
            Done                // after the final run of a block
        }

        private new void Start()
        {
            base.Start();

            iteration = 0;
            
            // get references 
            dataLogging = GetComponent<DataLoggingGaslight>();
            noiseManager = GetComponent<NoiseManager>();
            
            // setup event for cube interaction
            CubeInteractionManager.CubeInteracted += CubeInteracted;
            
            // setup ui event to trigger start
            UIManagerGaslightTask.LookedLongEnough += StartRun;
            
            uiManagerAlignment.ShowAlignmentInstruction(0);
            // safe the obstacle transform to restore it later
            originalTransform = obstacle.transform;
        }

        private new void Update()
        {
            UpdateState();
            HandleInput();
        }

        // trigger the according actions, whenever a button is pressed
        private void HandleInput()
        {
            switch (state)
            {
                case GaslightState.Alignment:
                    HandleAlignmentInput();
                    break;
                case GaslightState.Done:
                {
                    // show instructions to take off glasses
                    // press Options to return to the selection phase
                    if (CanInteract() && ButtonPressed("Option"))
                    {
                        state = GaslightState.SelectTrialType;
                        uiManagerGaslightTask.ToggleTrialSelection();
                        uiManagerGaslightTask.ResetInstructionFieldToDefault();
                        iteration = 0;
                        noiseManager.ToggleNoiseVolume();
                    }

                    break;
                }
                case GaslightState.SelectTrialType:
                case GaslightState.WaitingForStart:
                case GaslightState.Walking:
                default: break;
            }
        }

        private void UpdateState()
        {
            switch (state)
            {
                // while in alignment state
                case GaslightState.Alignment:
                {
                    UpdateSpatialAnchorLoading();

                    if (alignment.GetAlignmentDone() && alignment.GetFineTuningDone())
					{
						state = GaslightState.SelectTrialType;
						uiManagerGaslightTask.ToggleTrialSelection();
					}
                    break;
                }
                // when selecting the trial order
                case GaslightState.SelectTrialType:
                {
                    // switch to the next state when the UI for it is pressed
                    // enable the noise volume
                    break;
                }
                case GaslightState.WaitingForStart:
                {
                    // have the participant looking at the dot and press Y to start
                    // if it's a switch round, switch here, and if it's a gaslight round, gaslight
                    break;
                }
                case GaslightState.Walking:
                {
                    // when the participant sits back down and presses the button, switch to next state
                    // if it's a gaslight round, switch back on collision
                    // if it's not the last round and there was no collision, try again in the next round
                    break;
                }
                case GaslightState.Done:
                {
                    // instruct the participant to take off the headset
                    break;
                }
            }
        }

        public void StartBlock()
        {
            participantId = uiManagerGaslightTask.selectedParticipantId;
            blockType = uiManagerGaslightTask.selectedBlockType;
            uiManagerGaslightTask.ChangeInstructionVisibility(true, uiManagerGaslightTask.allowHeightAdjustment);
            uiManagerGaslightTask.ToggleTrialSelection();
            state = GaslightState.WaitingForStart;
            uiManagerGaslightTask.ToggleInteractionCube();
            noiseManager.ToggleNoiseVolume();
        }

        private void StartRun()
        {
            // only allow when in the right state
            if (state != GaslightState.WaitingForStart) return;
            state = GaslightState.Walking;
            currentRunStart = Time.time;
            
            // change instruction field to GO!
            uiManagerGaslightTask.ChangeToGo();
            
            dataLogging.AddTimestamp();
            
            // switch model if VR or GR run
            if (blockType is BlockType.VR or BlockType.Gaslight)
            {
                if (!modelManager.IsModelVisible())
                {
                    modelManager.ToggleModelVisibility();
                    obstacle.SetActive(true);
                    foreach (var item in parkourElements) item.SetActive(true);
                } 
            }
            // enable listener on GR run for collision and move obstacle
            if (iteration is 1 or 4 && blockType is BlockType.Gaslight)
            {
                NatNetBridgeReceiver.PositionChanged += OnObstacleCollision;
                obstacle.transform.position = collisionTransform.position;
                obstacle.transform.rotation = collisionTransform.rotation;
            }
        }
        
        private void OnObstacleCollision()
        {
            modelManager.ToggleModelVisibility();
            foreach (var item in parkourElements) item.SetActive(false);
            DisableGaslitPosition();
            obstacle.SetActive(false);
        }

        private void DisableGaslitPosition()
        {
            NatNetBridgeReceiver.PositionChanged -= OnObstacleCollision;
            // move the obstacle back
            obstacle.transform.position = originalTransform.position;
            obstacle.transform.rotation = originalTransform.rotation;
        }

        private void EndRun()
        {
            dataLogging.AddTime(Time.time - currentRunStart);
            
            // if it's the last run of a block
            if (iteration == 4)
            {
                dataLogging.OnBlockFinished(blockType.ToString(), participantId);
                state = GaslightState.Done;
                uiManagerGaslightTask.ChangeToEnd();
                if (modelManager.IsModelVisible()) modelManager.ToggleModelVisibility();
                DisableGaslitPosition();
                obstacle.SetActive(false);
                foreach (var item in parkourElements) item.SetActive(false);
            }
            // otherwise return to starting setup
            else
            {
                if (iteration is 1 && blockType is BlockType.Gaslight) DisableGaslitPosition();
                state = GaslightState.WaitingForStart;
                uiManagerGaslightTask.ResetInstructionFieldToDefault();
                uiManagerGaslightTask.ChangeInstructionVisibility(true, false);
                NextIteration();
            }
        }

        private void AdjustInstructionHeight()
        {
            var instructionCenterHeight = centerEyeAnchor.position.y;
            uiManagerGaslightTask.ChangeInstructionCenterHeight(instructionCenterHeight);
        }

        private void NextIteration()
        {
            iteration++;
            
        }

        private void CubeInteracted()
        {
            switch (state)
            {
                case GaslightState.WaitingForStart:
                    if (iteration == 0)
                    {
                        AdjustInstructionHeight();
                    }
                    break;
                case GaslightState.Walking:
                    EndRun();
                    break;
                case GaslightState.Alignment:
                case GaslightState.SelectTrialType:
                case GaslightState.Done:
                default: break;
            }
        }
    }
}