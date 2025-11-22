using System.Collections.Generic;
using DefaultNamespace;
using Managers;
using TMPro;
using UnityEngine;

namespace GameManagers
{
    public class SwitchSandboxManager : GameManager
    {
        
        [SerializeField] private TMP_Dropdown modelDropdown;
        [SerializeField] private TMP_Text transitionTimeValueText;
        [SerializeField] private bool fineTuneOnReset;
        [SerializeField] private GameObject obstacleCube;
        [SerializeField] private NatNetBridgeReceiver natNetBridgeReceiver;

        private NoiseManager noiseManager;
        private MultiModelManager MultiModelManager => modelManager as MultiModelManager;

        private new void Start()
        {
            base.Start();
            
            // get reference
            this.noiseManager = this.GetComponent<NoiseManager>();
            // var streamingCLient = this.GetComponent<OptitrackStreamingClient>();
            // streamingCLient.enabled = true;
            this.InitUI();
        }

        private void UpdateDurationUI()
        {
            this.transitionTimeValueText.text = this.modelManager.GetModelFadeDuration().ToString("0.#") + "s";
        }
        
        private new void Update()
        {
            this.HandleInput();
        }
        
        // intercept the UI events for the model manager to get more control/data over the current state
        private void InterceptModelManagerUI()
        {
            this.modelDropdown.onValueChanged.RemoveAllListeners();
            this.modelDropdown.onValueChanged.AddListener(this.MultiModelManager.ChooseCurrentModel);
        }

        private void InitUI()
        {
            // fill the dropdown
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var model in this.MultiModelManager.Models) options.Add(new TMP_Dropdown.OptionData(model.name));
            this.modelDropdown.AddOptions(options);
            
            // set listeners
            this.modelDropdown.onValueChanged.AddListener(this.MultiModelManager.ChooseCurrentModel);
        }

        private void HandleInput()
        {
            // handle alignment in base GameManager
            if (!this.alignment.GetAlignmentDone() || !this.alignment.GetFineTuningDone())
            {
                this.HandleAlignmentInput();
                this.UpdateSpatialAnchorLoading();
            }
            else
            {
                // button A to toggle the model
                if (OVRInput.GetUp(ButtonMapping.A, OVRInput.Controller.Touch))
                {
                    this.obstacleCube.SetActive(!this.modelManager.IsModelVisible());
                    this.modelManager.ToggleModelVisibility(true);
                }

                // press left thumbstick to toggle obstacle cube
                if (OVRInput.GetUp(ButtonMapping.StickL, OVRInput.Controller.Touch))
                {
                    Utils.ToggleGameObject(this.obstacleCube);
                }

                // press right thumbstick to toggle the noise
                if (OVRInput.GetUp(ButtonMapping.StickR, OVRInput.Controller.Touch))
                {
                    this.noiseManager.ToggleNoiseVolume();
                }
                
                // button X to restore the model alignment and allow fine-tuning after
                else if (OVRInput.GetUp(ButtonMapping.X, OVRInput.Controller.Touch))
                {
                    //realign and start fine-tuning (if configured to do so)
                    this.alignment.Realign(this.fineTuneOnReset);
                }
                
                // button Y to toggle optitrack tracking
                else if (OVRInput.GetUp(ButtonMapping.Y, OVRInput.Controller.Touch))
                {
                    this.natNetBridgeReceiver.ToggleRelayMovement();
                }
            }
        }

        public void ChangeTransitionDuration(float delta)
        {
            Debug.LogError(delta);
            var currentFadeDuration = this.modelManager.GetModelFadeDuration(); 
            if (currentFadeDuration + delta < 0) delta = 0;
            this.modelManager.SetModelFadeDuration(currentFadeDuration + delta);
            UpdateDurationUI();
        }
        
    }
}