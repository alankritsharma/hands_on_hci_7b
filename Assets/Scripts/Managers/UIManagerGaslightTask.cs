using System;
using System.Collections.Generic;
using System.Linq;
using GameManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class UIManagerGaslightTask : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform centerEyeAnchor;
        
        [Header("References - Trial Selection")]
        [SerializeField] private GameObject trialSelectionCanvas;
        [SerializeField] private TMP_Dropdown participantIdDropdown;
        [SerializeField] private TMP_Text currentTrialType;
        
        [Header("References - Instructions")]
        [SerializeField] private GameObject instructionField;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text heightText;
        [SerializeField] private TMP_Text goText;
        [SerializeField] private TMP_Text endText;
        [SerializeField] private Image dot;
        
        [SerializeField] private GameObject interactionCube;

        [Header("Settings")]
        // the dot product, 1 is perfectly looking at it
        [SerializeField] private float lookThreshold;
        [SerializeField] private float lookRaycastLength;
        [SerializeField] private LayerMask lookTargetLayer;
        [SerializeField] public bool allowHeightAdjustment;
        
        private float elapsedTimeLooking;
        private float startedGoTime;
        private bool fadeGo;
        public int selectedParticipantId;
        public StudyManagerGaslighting.BlockType selectedBlockType = StudyManagerGaslighting.BlockType.AR;

        public static event Action LookedLongEnough; 
        
        private void Start()
        {
            var options = Enumerable.Range(0, 40).Select(n => n.ToString()).ToList();
            participantIdDropdown.ClearOptions();
            participantIdDropdown.AddOptions(options);
            selectedParticipantId = participantIdDropdown.value;
            participantIdDropdown.onValueChanged.AddListener(UpdateCurrentParticipantId);
        }
        
        private void Update()
        {
            // check if the user is looking at the center of the target
            var lookCenterAngle = GetLookAngleToCenter();
            var lookingAtCenter = lookCenterAngle >= 0 && lookCenterAngle > lookThreshold;
            UpdateDotColor(lookingAtCenter);

            if (!fadeGo) return;
            if (!(Time.time - startedGoTime > 7f)) return;
            fadeGo = false;
            dot.color = Color.red;
            interactionCube.SetActive(true);
            ChangeInstructionVisibility(false, false);
        }

        public void SelectTrialType(string type)
        {
            switch (type)
            {
                case "AR":
                    selectedBlockType = StudyManagerGaslighting.BlockType.AR;
                    break;
                case "VR":
                    selectedBlockType = StudyManagerGaslighting.BlockType.VR;
                    break;
                case "GR":
                    selectedBlockType = StudyManagerGaslighting.BlockType.Gaslight;
                    break;
            }
            currentTrialType.text = type;
        }

        private float GetLookAngleToCenter()
        {
            var vectorToImage = dot.transform.position - centerEyeAnchor.position;
            
            if (vectorToImage.magnitude < lookRaycastLength)
            {
                return Vector3.Dot(centerEyeAnchor.forward, vectorToImage.normalized);
            }

            return -1;
        }

        private void UpdateDotColor(bool lookingAtCenter)
        {
            if (lookingAtCenter)
            {
                if (!(elapsedTimeLooking < 3f) || !dot.isActiveAndEnabled) return;
                elapsedTimeLooking += Time.deltaTime;
                var t = elapsedTimeLooking / 3f;
                dot.color = Color.Lerp(Color.red, Color.green, t);
                if (t >= 1) LookedLongEnough?.Invoke();
            }
            else
            {
                elapsedTimeLooking = 0;
            }
        }
        
        public void ChangeInstructionCenterHeight(float height)
        {
            instructionField.transform.position = Utils.VectorOverride(instructionField.transform.position, null, height, null);
        }

        public void ChangeInstructionVisibility(bool visibility, bool showHeightAdjustment)
        {
            instructionField.SetActive(visibility);
            heightText.gameObject.SetActive(showHeightAdjustment);
            
        }

        public void ChangeToGo()
        {
            heightText.gameObject.SetActive(false);
            dot.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            goText.gameObject.SetActive(true);
            endText.gameObject.SetActive(false);
            interactionCube.SetActive(false);
            
            fadeGo = true;
            startedGoTime = Time.time;
        }

        public void ChangeToEnd()
        {
            heightText.gameObject.SetActive(false);
            dot.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            instructionField.SetActive(true);
            goText.gameObject.SetActive(false);
            endText.gameObject.SetActive(true);
        }

        public void ResetInstructionFieldToDefault(bool showHeightAdjustment = false)
        {
            heightText.gameObject.SetActive(showHeightAdjustment);
            dot.gameObject.SetActive(true);
            instructionText.gameObject.SetActive(true);
            goText.gameObject.SetActive(false);
            endText.gameObject.SetActive(false);
            interactionCube.SetActive(false);
        }
        
        private void UpdateCurrentParticipantId(int newId)
        {
            selectedParticipantId = newId;
        }

        public void ToggleTrialSelection()
        {
            trialSelectionCanvas.SetActive(!trialSelectionCanvas.activeSelf);
        }

        public void ToggleInteractionCube()
        {
            interactionCube.SetActive(!interactionCube.activeSelf);
        }
    }
}