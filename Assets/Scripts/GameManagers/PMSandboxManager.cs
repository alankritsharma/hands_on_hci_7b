using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PMSandboxManager : GameManager
{
    [SerializeField] private TMP_Dropdown pmDropdown;
    [SerializeField] private Button randomPMButton;
    [SerializeField] private TMP_Text gainIndexValueText;
    [SerializeField] private TMP_Text gainListText;
    [SerializeField] private int emptyPMIndex;
    [SerializeField] private bool fineTuneOnReset;

    private PMManager pmManager;
    private NoiseManager noiseManager;
    private int iteration;
    private int gainIndex;
    private float transitionDuration = 0;

    private new void Start()
    {
        base.Start();

        // get references
        this.pmManager = this.GetComponent<PMManager>();
        this.noiseManager = this.GetComponent<NoiseManager>();

        // choose the first PM to automatically start it
        this.ChoosePMConfig(0);

        // intercept PM manager UI after a delay to make sure it is executed after the PMManager's Start method
        Invoke("InterceptPMManagerUI", this.initDelay);
    }

    private new void Update()
    {
        this.HandleInput();
    }

    // intercept the UI events for the PM manager to get more control/data over the current state
    private void InterceptPMManagerUI()
    {
        // dropdown
        this.pmDropdown.onValueChanged.RemoveAllListeners();
        this.pmDropdown.onValueChanged.AddListener(this.ChoosePMConfig);

        // button
        this.randomPMButton.onClick.RemoveAllListeners();
        this.randomPMButton.onClick.AddListener(this.ChooseRandomPMConfig);
    }

    // pass on the intercepted dropdown input to the PM manager
    private void ChoosePMConfig(int index)
    {
        this.pmManager.ChoosePMConfig(index);
        this.ResetPMCounters();

        this.pmManager.UpdatePM(this.iteration, this.gainIndex);
    }

    // pass on the intercepted button input to the PM manager
    private void ChooseRandomPMConfig()
    {
        this.pmManager.ChooseRandomPMConfig();
        this.ResetPMCounters();
    }

    // reset the counter variables to start with a new PM
    private void ResetPMCounters()
    {
        this.iteration = 0;
        this.gainIndex = 0;
        this.UpdateGainIndexDisplay();
    }
    
    private void UpdateDurationUI()
    {
        this.gainIndexValueText.text = this.modelManager.GetModelFadeDuration().ToString("#.##s");
    }

    // update the text showing the current gain index value
    private void UpdateGainIndexDisplay()
    {
        // show the current gain index
        this.gainIndexValueText.text = this.gainIndex.ToString();

        // show the list of gains for the current PM
        float[] gains = this.pmManager.GetGains(this.iteration);
        string gainsString = "";
        for (int i = 0; i < gains.Length; i++)
        {
            if (i == this.gainIndex) gainsString += "<color=green>";
            gainsString += i.ToString() + ": " + gains[i].ToString("#.###");
            if (i == this.gainIndex) gainsString += "</color>";
            if (i + 1 < gains.Length) gainsString += ", ";
        }
        this.gainListText.text = gainsString;
    }

    // trigger the according actions, whenever a button is pressed
    private void HandleInput()
    {
        // handle the alignment in the base GameManager
        if (!this.alignment.GetAlignmentDone() || !this.alignment.GetFineTuningDone())
        {
            this.HandleAlignmentInput();
            this.UpdateSpatialAnchorLoading();
        }
        else
        {
            // start/option button to toggle the model
            if (OVRInput.GetUp(ButtonMapping.Option, OVRInput.Controller.Touch))
            {
                this.modelManager.ToggleModelVisibility(true);
            }

            // press right thumbstick to toggle the noise
            if (OVRInput.GetUp(ButtonMapping.StickR, OVRInput.Controller.Touch))
            {
                this.noiseManager.ToggleNoiseVolume();
            }

            // button A to update the PM, going to the next gain in the iteration
            if (OVRInput.GetUp(ButtonMapping.A, OVRInput.Controller.Touch))
            {
                if (this.gainIndex + 1 < this.pmManager.GetGainCount(this.iteration)) this.gainIndex++;
                else this.gainIndex = 0;

                this.UpdateGainIndexDisplay();
                this.pmManager.UpdatePM(this.iteration, this.gainIndex);
            }
            // button B to update the PM, going to the previous gain in the iteration
            else if (OVRInput.GetUp(ButtonMapping.B, OVRInput.Controller.Touch))
            {
                if (this.gainIndex > 0) this.gainIndex--;
                else this.gainIndex = this.pmManager.GetGainCount(this.iteration) - 1;

                this.UpdateGainIndexDisplay();
                this.pmManager.UpdatePM(this.iteration, this.gainIndex);
            }
            // button X to restore the model alignment and allow fine tuning again
            else if (OVRInput.GetUp(ButtonMapping.X, OVRInput.Controller.Touch))
            {
                // choose empty PM to stop any ongoing manipulation
                this.ChoosePMConfig(this.emptyPMIndex);
                this.pmDropdown.SetValueWithoutNotify(this.emptyPMIndex);

                // hide the menu
                this.pmManager.HideUI();

                // realign and start fine tuning (if configured to do so)
                this.alignment.Realign(this.fineTuneOnReset);
            }
        }
    }

    public void changeTransitionDuration(float delta)
    {
        this.transitionDuration += delta;
        if (this.transitionDuration < 0) this.transitionDuration = 0;
        this.modelManager.SetModelFadeDuration(this.transitionDuration);
        this.UpdateDurationUI();
    }
}
