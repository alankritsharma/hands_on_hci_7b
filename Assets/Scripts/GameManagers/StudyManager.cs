using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// manages the study experiment
public class StudyManager : GameManager
{
    [SerializeField] private Transform centerEyeAnchor;
    [SerializeField] private ChangingPainting[] paintings;
    [SerializeField] private CollisionProtection collisionProtection;
    [SerializeField] private Trail trail;
    [SerializeField] private GrabbableCube[] grabbableCubes;
    
    [Header("Task Settings")]
    [SerializeField] private float paintingTimer = 20;
    [SerializeField] private bool adjustPaintingsHeight;
    [SerializeField] private int activateModelAtPaintingIndex;
    [SerializeField] private bool showPaintingCounterInsteadOfTimer;

    private UIAnimator uiAnimator;
    private UIManagerPaintingsTask uiManagerPaintingsTask;
    private DataLogging dataLogging;
    private PMManager pmManager;
    private NoiseManager noiseManager;
    private int state = 0;
    private int iteration = 0;
    private int paintingIndex = 0;
    private int paintingsLookedAt = 0;
    private float currentTimer = 0;
    private float currentPaintingTimer;
    private bool paintingTimerRunning;
    private int preCollisionState;
    private bool allowAButtonIllusionReveal;

    /*
        states:
            - 0: alignment
            - 1: waiting for button input to start the timer and continue
            - 2: walking towards a painting
            - 3: looking at a painting (not the final one)
            - 4: walking towards the final painting
            - 5: looking at the final painting
            - 6: after the final painting was hidden
            - -1: deadlock state, entered in case of an impending collision
    */
    private static int STATE_ALIGNMENT = 0;
    private static int STATE_WAITING_FOR_START = 1;
    private static int STATE_WALKING = 2;
    private static int STATE_LOOKING = 3;
    private static int STATE_WALKING_FINAL = 4;
    private static int STATE_LOOKING_FINAL = 5;
    private static int STATE_DONE = 6;
    private static int STATE_COLLISION_DEADLOCK = -1;

    private new void Start()
    {
        base.Start();

        // get references
        this.uiAnimator = this.GetComponent<UIAnimator>();
        this.uiManagerPaintingsTask = this.GetComponent<UIManagerPaintingsTask>();
        this.dataLogging = this.GetComponent<DataLogging>();
        this.pmManager = this.GetComponent<PMManager>();
        this.noiseManager = this.GetComponent<NoiseManager>();

        Invoke("Init", this.initDelay);
    }

    private new void Update()
    {
        // update timers
        if (this.state > STATE_WAITING_FOR_START) this.UpdateTimer();
        if (this.paintingTimerRunning) this.UpdatePaintingTimer();

        // handle state and input
        this.UpdateState();
        this.HandleInput();
    }

    // initialize the task
    private void Init()
    {
        // clear the painting timer
        this.ClearPaintingTimer();

        // show the first alignment instruction
        this.uiManagerAlignment.ShowAlignmentInstruction(0);
    }

    // trigger the according actions, whenever a button is pressed
    private void HandleInput()
    {
        //Debug.LogWarning($"HandleInput with state {this.state}");
        if (this.state == STATE_ALIGNMENT) // alignment
        {
            this.HandleAlignmentInput();
        }
        else if (this.state == STATE_WAITING_FOR_START) // waiting for button input to start the timer and continue 
        {
            // button Y to continue the paintings task (start the timer)
            if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Y, OVRInput.Controller.Touch))
            {
                this.StartCurrentIteration();
            }

            // press right thumbstick to toggle the noise
            if (OVRInput.GetUp(ButtonMapping.StickR, OVRInput.Controller.Touch))
            {
                this.noiseManager.ToggleNoiseVolume();
            }
        }
        else if (this.state == STATE_WALKING || this.state == STATE_WALKING_FINAL) // walking towards a painting
        {
            // button Y to continue the paintings task (looking at the painting)
            if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Y, OVRInput.Controller.Touch))
            {
                this.UpdateLastInteractionTime();
                this.ShowNextPainting();
            }
        }
        else if (this.state == STATE_DONE) // after the final painting was hidden
        {
            // button Y to continue the paintings task (deactivate the room model to resolve the RDW, i.e. resolve the illusion)
            if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Y, OVRInput.Controller.Touch))
            {
                this.modelManager.ToggleModelVisibility();
                this.modelManager.TogglePaintings();
                this.trail.Toggle();
            }
            // click left joystick to go to the next iteration, if it wasn't the last one already
            if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.StickL, OVRInput.Controller.Touch))
            {
                if (this.iteration + 1 < this.pmManager.GetIterationCount()) this.NextIteration();
                else this.ResetIteration();
            }
            // if enabled, button X or left hand pinch to toggle the model
            if (this.allowAButtonIllusionReveal && Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.X, OVRInput.Controller.Touch))
            {
                this.modelManager.ToggleModelVisibility();
            }
        }
        else if (this.state == STATE_COLLISION_DEADLOCK) // deadlock state, entered in case of an impending collision
        {
            // button Y to toggle room model to show user how the RDW almost caused a collision
            if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Y, OVRInput.Controller.Touch))
            {
                this.modelManager.ToggleModelVisibility();
                this.modelManager.TogglePaintings();

                // stop the controller vibration, as we know it was noticed already (a button on the controller was pressed)
                this.collisionProtection.StopHapticFeedback();
            }
            // start button to recover from the collision and allow the study to continue
            if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Option, OVRInput.Controller.Touch))
            {
                // restore the original state
                this.state = this.preCollisionState;

                // stop the controller vibration and restore painting and model visibility
                this.collisionProtection.Recover();
            }
        }
    }

    // update the current state of the task
    private void UpdateState()
    {
        if (this.state == STATE_ALIGNMENT) // alignment
        {
            this.UpdateSpatialAnchorLoading();

            if (this.alignment.GetAlignmentDone() && this.alignment.GetFineTuningDone())
            {
                // initialize the grabbable cubes
                foreach (GrabbableCube gc in this.grabbableCubes) gc.Init();

                // hint the user to go to the first painting
                this.DirectUserToPainting(paintings[0]);

                state++;
            }
        }
        else if (this.state == STATE_LOOKING) // user looking at a painting (not the final one)
        {
            if (this.currentPaintingTimer <= 0)
            {
                this.ClearPaintingTimer();

                // hide the painting again
                this.paintings[this.paintingIndex].HidePainting();

                // start the perceptual manipulation
                this.paintingsLookedAt++;
                if (this.paintingsLookedAt > this.activateModelAtPaintingIndex) this.pmManager.UpdatePM(this.iteration, this.paintingsLookedAt - 1);

                // hint the user to go to the next painting
                this.paintingIndex = (this.paintingIndex + 1) % this.paintings.Length;
                this.DirectUserToPainting(this.paintings[this.paintingIndex]);

                // go to next state if the next painting is the last one
                if (this.paintingsLookedAt == this.pmManager.GetGainCount(this.iteration)) state++;
                // go back to the previous state, as long as there are more paintings to look at
                else state--;
            }
        }
        else if (this.state == STATE_LOOKING_FINAL) // user looking at the final painting
        {
            if (this.currentPaintingTimer <= 0)
            {
                this.ClearPaintingTimer();

                // hide the painting again
                this.paintings[this.paintingIndex].HidePainting();

                // hide the directions UI
                this.uiAnimator.SetTarget(null);

                // show instruction to interact with the cubes again
                this.uiManagerPaintingsTask.ShowEndOfTaskInstruction(0);

                // log the data from the finished iteration
                this.dataLogging.OnIterationFinished(this.iteration);

                // unlock the grabbable cubes and use their respawn event to unlock the pinch to reveal the illusion
                foreach (GrabbableCube cube in this.grabbableCubes)
                {
                    cube.Unlock();
                    cube.WhenRespawned.AddListener(this.UnlockPinchToToggleModel);
                }

                state++;
            }
        }
    }

    // unlock the pinch input for revealing the illusion
    public void UnlockPinchToToggleModel()
    {
        // remove the event listeners
        foreach (GrabbableCube cube in this.grabbableCubes) cube.WhenRespawned.RemoveListener(this.UnlockPinchToToggleModel);

        // allow the pinch input
        this.allowAButtonIllusionReveal = true;

        // show instruction to pinch to toggle the model
        this.uiManagerPaintingsTask.ShowEndOfTaskInstruction(1);
    }

    // start the current iteration
    public void StartCurrentIteration()
    {
        // only allowed, if we are currently waiting for the iteration to start
        if (this.state != STATE_WAITING_FOR_START) return;

        this.state++;
        this.trail.AddPosition();

        // lock the grabbable cubes 
        foreach (GrabbableCube gc in this.grabbableCubes) gc.LockInPlace();

        // adjust the height of the paintings
        if (this.adjustPaintingsHeight) this.AdjustPaintingsHeight();

        // TODO: test this more
        OVRPlugin.ResetBodyTrackingCalibration();
    }

    // check, if we are currently waiting for the current iteration to start
    public bool GetCurrentlyWaitingToStart()
    {
        return this.state == STATE_WAITING_FOR_START;
    }

    // hint user to go to the given painting
    private void DirectUserToPainting(ChangingPainting painting)
    {
        this.uiManagerPaintingsTask.ShowPaintingsInstruction(0);
        this.uiManagerPaintingsTask.PositionPaintingTimer(painting.GetTimerTransform());
        painting.StartMaterialInterpolation();
        this.uiAnimator.SetTarget(painting.transform);
        this.uiAnimator.Show();
    }


    // update the timer
    private void UpdateTimer(bool reset=false)
    {
        if (reset) this.currentTimer = 0;
        else this.currentTimer += Time.deltaTime;

        if (this.showPaintingCounterInsteadOfTimer) this.uiManagerPaintingsTask.UpdateTimerText($"{this.paintingsLookedAt + 1}/{this.pmManager.GetGainCount(this.iteration) + 1}");
        else this.uiManagerPaintingsTask.UpdateTimerText($"{Mathf.Max(0, this.currentTimer):#0.0}");
    }

    // update the painting timer
    private void UpdatePaintingTimer()
    {
        this.currentPaintingTimer -= Time.deltaTime;
        this.uiManagerPaintingsTask.UpdatePaintingTimerText(this.currentPaintingTimer);
    }

    // clear the painting timer text
    private void ClearPaintingTimer()
    {
        this.paintingTimerRunning = false;
        this.uiManagerPaintingsTask.ClearPaintingTimerText();
    }

    // adjust the height of the paintings to the user's height
    private void AdjustPaintingsHeight()
    {
        float paintingsHeight = this.centerEyeAnchor.position.y;
        foreach (ChangingPainting painting in this.paintings) painting.transform.position = Utils.VectorOverride(painting.transform.position, null, paintingsHeight, null);

        // position the paintings timer again, so it matches the new height of the painting
        this.uiManagerPaintingsTask.PositionPaintingTimer(this.paintings[0].GetTimerTransform());
    }

    // called when the user has been looking at the painting for long enough
    public void OnPaintingLookedAt()
    {
        if (this.state == STATE_WALKING || this.state == STATE_WALKING_FINAL) this.ShowNextPainting();
    }

    // show the next painting and start its timer
    private void ShowNextPainting()
    {
        // make the painting visible to the user
        this.paintings[this.paintingIndex].ShowNextPainting();
        this.uiManagerPaintingsTask.ClearInstructionsText();
        this.uiAnimator.SetTarget(null);

        // store the painting's position for data logging
        this.dataLogging.AddPosition(this.paintings[this.paintingIndex].transform.position);

        // activate the room model at the configured painting
        if (this.paintingsLookedAt == this.activateModelAtPaintingIndex && !this.modelManager.IsModelVisible())
        {
            this.modelManager.ToggleModelVisibility(true);
            StartCoroutine(this.pmManager.StartPM(this.modelManager.GetModelFadeDuration()));
        }

        // hide the room model at the final painting
        if (this.pmManager.GetHideModelAtFinalPainting(this.iteration) && this.state == STATE_WALKING_FINAL)
        {
            this.modelManager.ToggleModelVisibility(true);
            StartCoroutine(this.pmManager.StopPM(this.modelManager.GetModelFadeDuration(), this.iteration));
        }

        // start the timer
        this.currentPaintingTimer = this.paintingTimer;
        this.paintingTimerRunning = true;

        // add the current user position to the trail
        this.trail.AddPosition();

        // go to the next state
        this.state++;
    }

    // get the transform of the current painting
    public Transform GetCurrentPainting()
    {
        return this.paintings[this.paintingIndex].transform;
    }

    // go to the deadlock state when an impending collision was detected
    public void EnterCollisionDeadlockState()
    {
        this.preCollisionState = this.state;
        this.state = STATE_COLLISION_DEADLOCK;
    }

    // go to the next iteration
    private void NextIteration()
    {
        // increase the iteration index
        this.iteration++;

        this.ResetIteration();
    }

    // restart the current iteration
    private void ResetIteration()
    {
        // stop the PM
        StartCoroutine(this.pmManager.StopPM(0, this.iteration));

        // reset and hide the trail
        this.trail.ClearAndHide();

        // reset the paintings
        this.paintingIndex = 0;
        this.paintingsLookedAt = 0;
        foreach (ChangingPainting cp in this.paintings) cp.ResetIndex();
        this.modelManager.SetPaintingsActive(true);

        // reset the timers
        this.ClearPaintingTimer();
        this.UpdateTimer(true);

        // reset the model to its true position and reenable fine tuning
        this.alignment.Realign(true);

        // block the pinch input again
        this.allowAButtonIllusionReveal = false;

        // clear the instructions text
        this.uiManagerPaintingsTask.ClearInstructionsText();

        this.state = STATE_ALIGNMENT;
    }

    // check whether the collision detection should currently be active
    public bool GetCollisionDetectionWanted()
    {
        return this.state == STATE_LOOKING || this.state == STATE_LOOKING_FINAL || this.state == STATE_WALKING || this.state == STATE_WALKING_FINAL || this.state == STATE_DONE;
    }
}
