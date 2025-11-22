using UnityEngine;

public class PM_ModelScaling : PerceptualManipulation
{
    [Header("References")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private Transform centerEyeAnchor;

    [Header("Settings")]
    [SerializeField] private bool scaleByTimeInsteadOfDistance;
    [SerializeField] private bool scaleAlongX;
    [SerializeField] private bool scaleAlongY;
    [SerializeField] private bool scaleAlongZ;

    private bool currentlyScaling;
    private float gain;
    private Vector3 lastPos;

    private void Update()
    {
        if (this.currentlyScaling)
        {
            if (this.scaleByTimeInsteadOfDistance) this.ScaleByTime(this.scaleAlongX ? this.gain : 0, this.scaleAlongY ? this.gain : 0, this.scaleAlongZ ? this.gain : 0);
            else this.ScaleByDistanceWalked(this.scaleAlongX ? this.gain : 0, this.scaleAlongY ? this.gain : 0, this.scaleAlongZ ? this.gain : 0);
        }
    }

    private void ScaleByDistanceWalked(float x, float y, float z)
    {
        // get distance walked since the last frame (ignoring y-axis)
        float distance = Utils.WithoutY(this.centerEyeAnchor.position - this.lastPos).magnitude;

        // scale the model
        this.modelParent.localScale += new Vector3(x, y, z) * distance;

        this.UpdateLastPosVariable();
    }

    private void ScaleByTime(float x, float y, float z)
    {
        // scale the model
        this.modelParent.localScale += new Vector3(x , y, z) * Time.deltaTime;
    }

    // update the lastPos variable to the current player position
    private void UpdateLastPosVariable()
    {
        this.lastPos = this.centerEyeAnchor.position;
    }

    public override void StartPM() { }

    public override void StopPM(bool resolve)
    {
        this.currentlyScaling = false;

        // TODO: remove resolve param or implement resolve here
    }

    public override void UpdatePM(float gain)
    {
        this.gain = gain;

        if (!this.currentlyScaling)
        {
            this.currentlyScaling = true;
            this.UpdateLastPosVariable();
        }
    }
}
