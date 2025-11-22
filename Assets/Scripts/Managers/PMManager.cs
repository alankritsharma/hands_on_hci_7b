using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PMManager : MonoBehaviour
{
    [System.Serializable]
    public struct IterationData
    {
        public float[] pmGains;
        public bool hideModelAtFinalPainting;
        public bool resolveWhenStopping;

        public IterationData(float[] gains, bool hideModel, bool resolveWhenStopping)
        {
            this.pmGains = gains;
            this.hideModelAtFinalPainting = hideModel;
            this.resolveWhenStopping = resolveWhenStopping;
        }
    }

    [System.Serializable]
    public struct PMConfig
    {
        public string name;
        public PerceptualManipulation pm;
        public IterationData[] iterations;

        public PMConfig(string name, PerceptualManipulation pm, IterationData[] iterations)
        {
            this.name = name;
            this.pm = pm;
            this.iterations = iterations;
        }
    }

    [SerializeField] private PMConfig[] pmConfigs;

    [Header("Settings")]
    [SerializeField] private bool revealRandomPM;
    [SerializeField] private bool sandboxMode;

    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Dropdown pmDropdown;
    [SerializeField] private TMP_Text pmDropdownLabel;
    [SerializeField] private TMP_Text gainTuningValueText;

    private int chosenConfigIndex;
    private float gainTuning = 1f;

    private void Start()
    {
        this.InitUI();
    }

    private void Update()
    {
        // press left joystick to toggle the menu visibility
        if (OVRInput.GetUp(ButtonMapping.StickL, OVRInput.Controller.Touch))
        {
            Utils.ToggleGameObject(this.canvas);
        }
    }

    // hide the menu
    public void HideUI()
    {
        this.canvas.SetActive(false);
    }

    // initialize the UI, e.g. filling the dropdown with the available PM configs
    private void InitUI()
    {
        // fill the dropdown
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (PMConfig config in this.pmConfigs) options.Add(new TMP_Dropdown.OptionData(config.name));
        this.pmDropdown.AddOptions(options);

        // set listeners
        this.pmDropdown.onValueChanged.AddListener(this.ChoosePMConfig);

        // init the text field
        this.UpdateGainTuningUI();
    }

    // set the index for the PM config to be used
    public void ChoosePMConfig(int index)
    {
        // automatically stop the old and start the new PM when in sandbox mode
        if (this.sandboxMode)
        {
            this.pmConfigs[this.chosenConfigIndex].pm.StopPM(this.pmConfigs[this.chosenConfigIndex].iterations[0].resolveWhenStopping);
            this.pmConfigs[index].pm.StartPM();
        }

        // store the new active config index
        this.chosenConfigIndex = index;
        Debug.LogWarning("PM chosen: " + index);
        if (BLEReceiver.Instance != null) BLEReceiver.Instance.SendStateUpdate();
    }

    // choose a random PM config
    public void ChooseRandomPMConfig()
    {
        // activate a random PM
        this.ChoosePMConfig(UnityEngine.Random.Range(0, this.pmConfigs.Length));

        // either display the randomly chosen PM's name on the dropdown or keep it secret
        if (this.revealRandomPM) this.pmDropdown.SetValueWithoutNotify(this.chosenConfigIndex);
        else this.pmDropdownLabel.text = "Random";
    }

    // get the currently active PM's index
    public int GetChosenConfigIndex()
    {
        return this.chosenConfigIndex;
    }

    // get the number of PM configs
    public int GetPMCount()
    {
        return this.pmConfigs.Length;
    }

    // get the name of the PM config with the given index
    public string GetPMName(int index)
    {
        if (index >= 0 && index < this.pmConfigs.Length) return this.pmConfigs[index].name;
        return "INVALID PM INDEX!";
    }

    // change the gain tuning value
    public void ChangeGainTuning(float delta)
    {
        this.gainTuning += delta;
        this.UpdateGainTuningUI();
    }

    // update the UI text showing the current gain tuning value
    private void UpdateGainTuningUI()
    {
        this.gainTuningValueText.text = this.gainTuning.ToString("#.##");
    }

    // return the number of iterations from the chosen PM config
    public int GetIterationCount()
    {
        return this.pmConfigs[this.chosenConfigIndex].iterations.Length;
    }

    // get the number of gains in the given iteration from the chosen PM config
    public int GetGainCount(int iteration)
    {
        return this.pmConfigs[this.chosenConfigIndex].iterations[iteration].pmGains.Length;
    }

    // get the list of gains in the given iteration from the chosen PM config
    public float[] GetGains(int iteration)
    {
        return this.pmConfigs[this.chosenConfigIndex].iterations[iteration].pmGains;
    }

    // check if the model should be hidden at the final painting, based on the chosen PM config
    public bool GetHideModelAtFinalPainting(int iteration)
    {
        return this.pmConfigs[this.chosenConfigIndex].iterations[iteration].hideModelAtFinalPainting;
    }

    // store the given iteration index to make it available

    // start the chosen PM
    public IEnumerator StartPM(float delay)
    {
        yield return new WaitForSeconds(delay);

        this.pmConfigs[this.chosenConfigIndex].pm.StartPM();
    }

    // update the chosen PM with the current gain from the given iteration, multiplied with the current gain tuning value
    public void UpdatePM(int iteration, int index)
    {
        float gain = this.pmConfigs[this.chosenConfigIndex].iterations[iteration].pmGains[index];
        this.pmConfigs[this.chosenConfigIndex].pm.UpdatePM(gain * this.gainTuning);
    }

    // stop the currently chosen PM
    public IEnumerator StopPM(float delay, int iteration)
    {
        yield return new WaitForSeconds(delay);

        bool resolve = this.pmConfigs[this.chosenConfigIndex].iterations[iteration].resolveWhenStopping;
        this.pmConfigs[this.chosenConfigIndex].pm.StopPM(resolve);
    }
}
