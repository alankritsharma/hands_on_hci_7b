using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float power;
    [SerializeField] private bool allowReleasingToHolster;
    [SerializeField] private float maxVibrationIntensityDrawing;
    [SerializeField] private float vibrationIntensityShooting;
    [SerializeField] private float vibrationDurationShooting;
    [SerializeField] private float vibrationDelayShooting;

    [Header("References")]
    [SerializeField] private ArrowPool arrowPool;
    [SerializeField] private Transform arrowParent;

    [Header("Child Transforms")]
    [SerializeField] private Transform handle;
    [SerializeField] private Transform notch;
    [SerializeField] private Transform stringMin;
    [SerializeField] private Transform stringMax;

    [Header("Bowstring")]
    [SerializeField] private LineRenderer stringRenderer;
    
    [Header("Audio")]
    [SerializeField] private AudioClip releaseAudio;

    private AudioSource audioSource;
    private float maxDrawMagnitude;
    private GameObject currentArrow;

    private void Start()
    {
        this.maxDrawMagnitude = (this.stringMax.position - this.stringMin.position).magnitude;
        this.audioSource = this.GetComponent<AudioSource>();
    }

    public Transform GetHandle()
    {
        return this.handle;
    }

    public void DrawString(Vector3 controllerPos)
    {
        if (!this.gameObject.activeInHierarchy)
        {
            this.StopVibrationFeedback();
            return;
        }

        // project the controller's position on the forward axis of the bowstring
        Vector3 projectedPos = Vector3.Project(controllerPos - this.stringRenderer.transform.position, this.stringRenderer.transform.forward);
        // only allow drawing the string to positions between the min- and max-positions
        if (this.DrawPosAllowed(projectedPos)) this.stringRenderer.SetPosition(1, this.stringRenderer.transform.InverseTransformPoint(this.stringRenderer.transform.position + projectedPos));

        // move arrow along with the string
        if (this.currentArrow != null)
        {
            this.currentArrow.transform.localPosition = this.stringRenderer.GetPosition(1);
        }

        this.GiveVibrationFeedbackDrawing();
    }

    private bool DrawPosAllowed(Vector3 pos)
    {
        Vector3 globalPos = this.stringRenderer.transform.position + pos;
        float distanceFromMin = (globalPos - this.stringMin.position).magnitude;
        float distanceToMax = (this.stringMax.position - globalPos).magnitude;

        return distanceFromMin <= this.maxDrawMagnitude && distanceToMax <= this.maxDrawMagnitude;
    }

    public void HandleStringGrabbed()
    {
        this.SpawnArrow();
    }

    public void HandleStringReleased()
    {
        this.stringRenderer.SetPosition(1, Vector3.zero);
        this.ShootArrow();
    }

    public void CancelDraw()
    {
        this.stringRenderer.SetPosition(1, Vector3.zero);

        if (this.currentArrow != null)
        {
            this.currentArrow.transform.parent = this.arrowParent;
            this.StopVibrationFeedback();
            this.currentArrow.GetComponent<Arrow>().ReturnArrow();
            this.currentArrow = null;
        }
    }

    private void SpawnArrow()
    {
        if (this.currentArrow != null) return;

        this.currentArrow = this.arrowPool.GetArrow();
        this.currentArrow.transform.parent = this.stringRenderer.transform;
        this.currentArrow.transform.localPosition = Vector3.zero;
        this.currentArrow.transform.localRotation = Quaternion.identity;
    }

    private void ShootArrow()
    {
        if (this.currentArrow == null) return;

        this.currentArrow.transform.parent = this.arrowParent;
        this.currentArrow.GetComponent<Rigidbody>().isKinematic = false;
        this.currentArrow.GetComponent<Arrow>().HandleArrowShot();

        this.currentArrow.GetComponent<Rigidbody>().AddForce(this.notch.forward * this.power * this.GetCurrentMaxPowerFraction());
        Invoke("GiveVibrationFeedbackShooting", this.vibrationDelayShooting);

        this.PlayAudio(this.releaseAudio);
    }

    private void PlayAudio(AudioClip clip)
    {
        this.audioSource.PlayOneShot(clip);
    }

    public void SwitchArrowPool(ArrowPool pool, Transform parent)
    {
        this.arrowPool = pool;
        this.arrowParent = parent;
    }

    public bool GetAllowReleasingToHolster()
    {
        return this.allowReleasingToHolster;
    }

    private void GiveVibrationFeedbackDrawing()
    {
        OVRInput.SetControllerVibration(1, this.GetCurrentMaxPowerFraction() * this.maxVibrationIntensityDrawing, OVRInput.Controller.All);
    }

    private void GiveVibrationFeedbackShooting()
    {
        OVRInput.SetControllerVibration(1, this.GetCurrentMaxPowerFraction() * this.vibrationIntensityShooting, OVRInput.Controller.All);
        Invoke("StopVibrationFeedback", this.vibrationDurationShooting);

        this.currentArrow = null;
    }

    private void StopVibrationFeedback()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.All);
    }

    private float GetCurrentMaxPowerFraction()
    {
        return (this.currentArrow.transform.position - this.stringMin.position).magnitude / this.maxDrawMagnitude;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(this.notch.position, 0.01f);
        Gizmos.DrawSphere(this.stringMin.position, 0.01f);
        Gizmos.DrawSphere(this.stringMax.position, 0.01f);
    }
}
