using UnityEngine;

public class PM_ModelRotation : PerceptualManipulation
{
    [Header("References")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private Transform axisReference;

    [Header("Settings")]
    [SerializeField] private bool rotateAroundX;
    [SerializeField] private bool rotateAroundY;
    [SerializeField] private bool rotateAroundZ;

    private bool currentlyRotating;
    private float gain;

    private void Update()
    {
        if (this.currentlyRotating)
        {
            if (this.rotateAroundX) this.modelParent.Rotate(this.axisReference.right, this.gain * Time.deltaTime, Space.World);
            if (this.rotateAroundY) this.modelParent.Rotate(this.axisReference.up, this.gain * Time.deltaTime, Space.World);
            if (this.rotateAroundZ) this.modelParent.Rotate(this.axisReference.forward, this.gain * Time.deltaTime, Space.World);
        }
    }

    public override void StartPM() {}

    public override void StopPM(bool resolve)
    {
        this.currentlyRotating = false;

        // TODO: remove resolve param or implement resolve here
    }

    public override void UpdatePM(float gain)
    {
        this.gain = gain;
        this.currentlyRotating = true;
    }
}
