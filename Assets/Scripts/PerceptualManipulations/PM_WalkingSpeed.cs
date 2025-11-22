using UnityEngine;

public class PM_WalkingSpeed : PerceptualManipulation
{
    [Header("References")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private Transform centerEyeAnchor;

    private bool currentlyTranslating;
    private float gain;
    private Vector3 lastPos;

    private void Update()
    {
        if (this.currentlyTranslating)
        {
            // get walking direction since the last frame (ignoring y-axis)
            Vector3 direction = Utils.WithoutY(this.centerEyeAnchor.position - this.lastPos);

            // manipulate walking speed by moving the model along the movement axis
            this.modelParent.position += direction * this.gain;

            this.UpdateLastPosVariable();
        }
    }

    // update the lastPos variable to the current player position
    private void UpdateLastPosVariable()
    {
        this.lastPos = this.centerEyeAnchor.position;
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

        if (!this.currentlyTranslating)
        {
            this.currentlyTranslating = true;
            this.UpdateLastPosVariable();
        }
    }
}
