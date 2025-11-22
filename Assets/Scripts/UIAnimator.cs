using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    private enum ScalingAnimationState { none, shrinking, growing}
    private enum TranslatingAnimationState { none, negative, positive}

    [Header("References")]
    [SerializeField] private Camera cam;

    [Header("UI Elements")]
    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private GameObject lookTargetCanvas;
    [SerializeField] private Transform lookTargetRotationParent;
    [SerializeField] private Slider progressSlider;

    [Header("Settings")]
    [SerializeField] private float cursorScaleDelta;
    [SerializeField] private float cursorAnimationDuration;
    [Space(10)]
    [SerializeField] private float arrowTranslationDistance;
    [SerializeField] private float arrowAnimationDuration;
    [Space(10)]
    [SerializeField] private float lookDistanceThresholdWide;
    [SerializeField] private float lookDistanceThresholdNarrow;
    [SerializeField] private float lookRaycastLength;
    [Space(10)]
    [SerializeField] private float lookTargetRotationSpeed;
    [SerializeField] private float lookTargetOffset;
    [Space(10)]
    [SerializeField] private float lookAtTimeToReach;
    [SerializeField] private float lookAtMaxDistance;
    [Space(10)]
    [SerializeField] private LayerMask lookTargetLayer;

    private UIManagerPaintingsTask uiManagerPaintingsTask;
    private StudyManager taskManager;

    // cursor animation
    private ScalingAnimationState cursorAnimationState;
    private float cursorScaleDeltaPerSecond;

    // arrow animation
    private TranslatingAnimationState arrowAnimationState;
    private Vector2 initialArrowOffset;
    private Vector2 currentArrowOffset;
    private float arrowTranslationPerSecond;
    private float currentArrowDistance;
    private Transform currentTarget;

    // progress slider animation
    private float lookProgressTimer;

    private void Start()
    {
        // get references
        this.uiManagerPaintingsTask = this.GetComponent<UIManagerPaintingsTask>();
        this.taskManager = this.GetComponent<StudyManager>();

        // the scale delta needs to be achieved in a quarter of the duration (animation consists of growing, shrinking back to normal, shrinking, growing back to normal)
        this.cursorScaleDeltaPerSecond = this.cursorScaleDelta / (this.cursorAnimationDuration / 4);

        // the transform distance needs to be achieved in a quarter of the duration (animation consists of positive, negative back to normal, negative, positive back to normal)
        this.arrowTranslationPerSecond = this.arrowTranslationDistance/ (this.arrowAnimationDuration/ 4);

        // initialize offsets to the positions assigned in the editor
        this.initialArrowOffset = this.arrow.localPosition;
        this.currentArrowOffset = this.initialArrowOffset;

        this.cursorAnimationState = ScalingAnimationState.growing;
        this.arrowAnimationState = TranslatingAnimationState.positive;

    }

    private void Update()
    {
        // hide cursor and arrow when there is no target to direct the user towards
        if (this.currentTarget == null)
        {
            this.Hide();
            return;
        }

        // check if user is looking at the center of the target (two different thresholds)
        float lookCenterDistance = this.GetLookDistanceToCenter();
        bool lookingRoughly = lookCenterDistance >= 0 && lookCenterDistance < this.lookDistanceThresholdWide;
        bool lookingClosely = lookCenterDistance >= 0 && lookCenterDistance < this.lookDistanceThresholdNarrow;


        // update the animations
        this.UpdateArrowOffset();
        this.CursorAnimation(lookingClosely);
        this.ArrowAnimation(lookingRoughly);
        this.LookTargetIndicatorAnimation(lookingRoughly);
        this.ProgressSliderAnimation(lookingClosely);
    }

    // update the cursor animation
    private void CursorAnimation(bool lookingClosely)
    {
        if (this.cursorAnimationState == ScalingAnimationState.none) return;

        if (this.cursorAnimationState == ScalingAnimationState.growing)
        {
            // switch to shrinking
            if (this.cursor.localScale.x >= 1 + this.cursorScaleDelta) this.cursorAnimationState = ScalingAnimationState.shrinking;
            // increase the scale
            else this.cursor.localScale += new Vector3(this.cursorScaleDeltaPerSecond, this.cursorScaleDeltaPerSecond, 0) * Time.deltaTime;
        }

        if (this.cursorAnimationState == ScalingAnimationState.shrinking)
        {
            // switch to growing
            if (this.cursor.localScale.x <= 1 - this.cursorScaleDelta) this.cursorAnimationState = ScalingAnimationState.growing;
            // increase the scale
            else this.cursor.localScale -= new Vector3(this.cursorScaleDeltaPerSecond, this.cursorScaleDeltaPerSecond, 0) * Time.deltaTime;
        }

        // only show cursor, if the user is not looking directly at the painting
        this.cursor.gameObject.SetActive(!lookingClosely);
    }

    // update the direction arrow animation
    private void ArrowAnimation(bool lookingRoughly)
    {
        if (this.arrowAnimationState == TranslatingAnimationState.none) return;

        // hide the arrow, if the player is looking at the picture
        if (lookingRoughly)
        {
            this.arrow.gameObject.SetActive(false);
            return;
        }

        if (this.arrowAnimationState == TranslatingAnimationState.positive)
        {
            // switch to negative
            if (this.currentArrowDistance >= this.arrowTranslationDistance) this.arrowAnimationState = TranslatingAnimationState.negative;
            // increase the current distance
            else this.currentArrowDistance += this.arrowTranslationPerSecond * Time.deltaTime;
        }

        if (this.arrowAnimationState == TranslatingAnimationState.negative)
        {
            // switch to positive
            if (this.currentArrowDistance <= -this.arrowTranslationDistance) this.arrowAnimationState = TranslatingAnimationState.positive;
            // decrease the current distance
            else this.currentArrowDistance -= this.arrowTranslationPerSecond * Time.deltaTime;
        }

        // show arrow and apply translation
        this.arrow.gameObject.SetActive(true);
        this.arrow.localPosition = this.currentArrowOffset + this.currentArrowOffset.normalized * this.currentArrowDistance;

        // rotate arrow to point in the direction of the translation
        float angle = Vector2.Angle(Vector2.right, this.currentArrowOffset.normalized);
        if (this.currentArrowOffset.y < 0) angle = 360 - angle;
        this.arrow.localRotation = Quaternion.Euler(0, 0, angle);
    }

    // update the animation of the look target indicator
    private void LookTargetIndicatorAnimation(bool lookingRoughly)
    {
        // place the indicator in front of the target painting
        this.lookTargetCanvas.transform.position = this.currentTarget.position + this.currentTarget.forward * this.lookTargetOffset;
        this.lookTargetCanvas.transform.forward = this.currentTarget.forward;

        // animate the indicator, if the player's look cursor is close enough
        if (lookingRoughly) this.lookTargetRotationParent.transform.Rotate(0, 0, this.lookTargetRotationSpeed * Time.deltaTime, Space.Self);
    }

    // update the animation of the progress slider
    private void ProgressSliderAnimation(bool lookingClosely)
    {
        // calculate how long the user has been looking at the target
        if (lookingClosely && this.GetTargetDistance() <= this.lookAtMaxDistance) this.lookProgressTimer += Time.deltaTime;
        else this.lookProgressTimer = 0;

        // update slider value
        this.progressSlider.value = this.lookProgressTimer / this.lookAtTimeToReach;

        // start the iteration, once the user is close enough to the painting and looking at it
        if (this.lookProgressTimer > 0 && this.taskManager != null && this.taskManager.GetCurrentlyWaitingToStart()) this.taskManager.StartCurrentIteration(); 

        // only show slider, if the user is looking somewhat close enough at the painting
        this.progressSlider.gameObject.SetActive(lookingClosely);

        // notify task manager if the user has been looking at the target long enough
        if (this.taskManager != null && this.lookProgressTimer >= this.lookAtTimeToReach) this.taskManager.OnPaintingLookedAt();
    }

    // get distance between the center of the target and the point on the canvas the user is looking at
    private float GetLookDistanceToCenter()
    {
        RaycastHit hit;
        if (Physics.Raycast(this.cam.transform.position, this.cam.transform.forward, out hit, this.lookRaycastLength, this.lookTargetLayer))
        {
            return (this.currentTarget.position - hit.point).magnitude;
        }
        return -1;
    }

    // get distance between the user and the target
    private float GetTargetDistance()
    {
        return (Utils.WithoutY(this.currentTarget.position) - Utils.WithoutY(this.cam.transform.position)).magnitude;
    }

    // update the arrow offset based on the direction the user has to look into to get to the painting
    private void UpdateArrowOffset()
    {
        Vector3 relativePos = (this.currentTarget.position - this.cursor.transform.position).normalized;
        float angle = Vector3.SignedAngle(relativePos, this.cam.transform.forward, Vector3.up);
        this.currentArrowOffset = Quaternion.Euler(0, 0, angle) * this.initialArrowOffset;
    }

    // update the target, in whose direction the arrow should be guiding the user
    public void SetTarget(Transform target)
    {
        this.currentTarget = target;
    }

    // show cursor and arrow
    public void Show()
    {
        // hide instructions text
        this.uiManagerPaintingsTask.SetInstructionsVisibility(false);

        // start animations
        this.cursorAnimationState = ScalingAnimationState.growing;
        this.arrowAnimationState = TranslatingAnimationState.positive;

        // update the animations once before showing the images (makes sure the images already fit the new target)
        this.Update();

        // show images
        this.cursor.gameObject.SetActive(true);
        this.arrow.gameObject.SetActive(true);
        this.lookTargetCanvas.SetActive(true);
    }

    // hide cursor and arrow
    public void Hide()
    {
        // show instructions text again
        this.uiManagerPaintingsTask.SetInstructionsVisibility(true);

        // hide images
        this.cursor.gameObject.SetActive(false);
        this.arrow.gameObject.SetActive(false);
        this.lookTargetCanvas.SetActive(false);

        // stop animations
        this.cursorAnimationState = ScalingAnimationState.none;
        this.arrowAnimationState = TranslatingAnimationState.none;
    }
}
