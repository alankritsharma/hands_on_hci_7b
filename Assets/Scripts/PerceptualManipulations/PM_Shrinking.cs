using UnityEngine;

public class PM_Shrinking : PerceptualManipulation
{
    [Header("References")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private Transform centerEyeAnchor;
    [SerializeField] private StudyManager studyManager;

    [Header("Settings")]
    [SerializeField] private float maxHeightOffset = -1;
    [SerializeField] private Transform[] transformsToPreserveHeightOf;

    private float gain;
    private float initialModelHeight;
    private float[] initialTransformHeights;
    private float maxModelHeight;
    private bool currentlyShrinking;
    private bool currentlyShowingOriginalHeight;
    private Vector3 lastPos;

    // save the initial model height and transform heights to be able to resolve the PM later on
    public void Init()
    {
        this.initialModelHeight = this.modelParent.position.y;
        this.initialTransformHeights = new float[this.transformsToPreserveHeightOf.Length];
        for (int i = 0; i <  this.transformsToPreserveHeightOf.Length; i++) this.initialTransformHeights[i] = this.transformsToPreserveHeightOf[i].position.y;
    }

    private void Update()
    {
        if (this.currentlyShrinking && (this.maxHeightOffset == -1 || Mathf.Abs(this.modelParent.position.y - this.initialModelHeight) < this.maxHeightOffset))
        {
            // get distance walked since the last frame (ignoring y-axis)
            float distance = Utils.WithoutY(this.centerEyeAnchor.position - this.lastPos).magnitude;

            // "shrink" user by moving the model upwards
            this.modelParent.position += Vector3.up * distance * this.gain;

            this.UpdateLastPosVariable();
        }
    }

    // start shrinking the user
    private void StartShrinking()
    {
        if (this.currentlyShowingOriginalHeight) this.ToggleInitialHeightDisplay();

        this.currentlyShrinking = true;
        this.UpdateLastPosVariable();
    }

    // stop shrinking the user
    private void StopShrinking()
    {
        this.currentlyShrinking = false;
    }

    // toggle whether the user shrinking is active
    public void ToggleShrinking()
    {
        if (this.currentlyShrinking) this.StopShrinking();
        else this.StartShrinking();
    }

    // toggle between showing the initial user height and the height resulting from the preceding shrinking
    public void ToggleInitialHeightDisplay()
    {
        if (this.currentlyShrinking) this.StopShrinking();

        this.currentlyShowingOriginalHeight = !this.currentlyShowingOriginalHeight;

        // reset the model height to the original value
        if (this.currentlyShowingOriginalHeight)
        {
            this.maxModelHeight = this.modelParent.position.y;
            float offset = this.maxModelHeight - this.initialModelHeight;

            // move the model back down
            this.modelParent.position -= Vector3.up * offset;

            // restore the initial heights of e.g. the paintings
            for (int i = 0; i < this.transformsToPreserveHeightOf.Length; i++)
                this.transformsToPreserveHeightOf[i].position = Utils.VectorOverride(this.transformsToPreserveHeightOf[i].position, null, this.initialTransformHeights[i], null);
        }
        // set the model height to the max height caused by preceding shrinking
        else
        {
            this.modelParent.position = Utils.VectorOverride(this.modelParent.position, null, this.maxModelHeight, null);
        }
    }

    // update the lastPos variable to the current player position
    private void UpdateLastPosVariable()
    {
        this.lastPos = this.centerEyeAnchor.position;
    }

    // preserve the height of e.g. the paintings relative to the user
    public void UpdatePositionsOfHeightPreserveTransforms()
    {
        Transform transformToExclude = this.studyManager != null ? this.studyManager.GetCurrentPainting() : null;

        for (int i = 0; i < this.transformsToPreserveHeightOf.Length; i++)
        {
            // skip the current painting so the height change only appears outside of the user's FOV
            if (this.transformsToPreserveHeightOf[i] == transformToExclude) continue;

            // set the transform back to its original height
            this.transformsToPreserveHeightOf[i].position = Utils.VectorOverride(this.transformsToPreserveHeightOf[i].position, null, this.initialTransformHeights[i], null);
        }
    }

    // activate the PM
    public override void StartPM() 
    {
        this.Init();
    }

    // set the given gain and start the shrinking, if it isn't running yet
    public override void UpdatePM(float gain)
    {
        this.gain = gain;
        if (!this.currentlyShrinking) this.StartShrinking();

        this.UpdatePositionsOfHeightPreserveTransforms();
    }

    // stop the shrinking
    public override void StopPM(bool resolve) {
        this.StopShrinking();

        if (resolve && !this.currentlyShowingOriginalHeight) this.ToggleInitialHeightDisplay();
    }
}
