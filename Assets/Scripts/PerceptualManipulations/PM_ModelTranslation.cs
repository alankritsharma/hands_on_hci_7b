using UnityEngine;

public class PM_ModelTranslation : PerceptualManipulation
{
    [Header("References")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private Transform axisReference;

    [Header("Settings")]
    [SerializeField] private bool translateAlongX;
    [SerializeField] private bool translateAlongY;
    [SerializeField] private bool translateAlongZ;

    private bool currentlyTranslating;
    private float gain;

    private void Update()
    {
        if (this.currentlyTranslating)
        {
            if (this.translateAlongX) this.modelParent.Translate(this.axisReference.right * this.gain * Time.deltaTime, Space.World);
            if (this.translateAlongY) this.modelParent.Translate(this.axisReference.up * this.gain * Time.deltaTime, Space.World);
            if (this.translateAlongZ) this.modelParent.Translate(this.axisReference.forward * this.gain * Time.deltaTime, Space.World);
        }
    }

    public override void StartPM() { }

    public override void StopPM(bool resolve)
    {
        this.currentlyTranslating = false;

        // TODO: remove resolve param or implement resolve here
    }

    public override void UpdatePM(float gain)
    {
        this.gain = gain;
        this.currentlyTranslating = true;
    }
}
