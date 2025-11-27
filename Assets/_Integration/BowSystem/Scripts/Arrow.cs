using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform tip;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip impactAudioSolid;

    [Header("Settings")]
    [SerializeField] private float lifetimeAfterHit = 1.0f;   // Time before returning/destroying arrow
    [SerializeField] private LayerMask hitMask;               // Assign to "Default" or specific layer

    private AudioSource audioSource;
    private Rigidbody rb;
    private ArrowPool arrowPool;
    private float tipDistance;
    private bool hasHit = false;
    private Vector3 hitPoint;
    private bool usePooling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        tipDistance = tip != null
            ? Vector3.Distance(tip.position, transform.position)
            : 0.3f;
    }

    private void FixedUpdate()
    {
        RotateWithTrajectory();
    }

    //----------------------------------------------
    // Called when bow releases the arrow
    //----------------------------------------------
    public void HandleArrowShot()
    {
        hasHit = false;

        if (trailRenderer != null)
            trailRenderer.enabled = true;
    }

    //----------------------------------------------
    // Arrow faces velocity direction
    //----------------------------------------------
    private void RotateWithTrajectory()
    {
        if (rb.isKinematic || rb.linearVelocity == Vector3.zero) return;

        transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
    }

    //----------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        rb.isKinematic = true;

        // Play impact sound
        PlayAudio(impactAudioSolid);

        // 1. Determine hit position precisely
        hitPoint = GetPreciseHitPoint(collision);

        // 2. Notify ScoreManager if this collider is a target
        TargetZone target = collision.collider.GetComponent<TargetZone>();
        if (target != null)
        {
            ScoreManager.Instance.RegisterHit(this, hitPoint);
            target.OnArrowHit(hitPoint);
        }


        // 3. Destroy/Return after delay
        Invoke(nameof(ReturnArrow), lifetimeAfterHit);
    }

    //----------------------------------------------
    private Vector3 GetPreciseHitPoint(Collision collision)
    {
        // Use contact point if available
        if (collision.contactCount > 0)
            return collision.GetContact(0).point;

        // Backup raycast for precision
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, tipDistance * 2f, hitMask))
            return hit.point;

        return tip != null ? tip.position : transform.position;
    }

    //----------------------------------------------
    private void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    //----------------------------------------------
    public void Init(ArrowPool pool = null)
    {
        arrowPool = pool;
        usePooling = (pool != null);

        if (trailRenderer != null)
            trailRenderer.enabled = false;
    }

    //----------------------------------------------
    public void ReturnArrow()
    {
        if (trailRenderer != null)
            trailRenderer.enabled = false;

        hasHit = false;
        rb.isKinematic = false;

        if (usePooling && arrowPool != null)
        {
            arrowPool.ReturnArrow(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //----------------------------------------------
    // Manual cleanup
    public void ResetArrow()
    {
        hasHit = false;
        rb.isKinematic = false;
        CancelInvoke(nameof(ReturnArrow));

        if (trailRenderer != null)
            trailRenderer.enabled = false;
    }
}
