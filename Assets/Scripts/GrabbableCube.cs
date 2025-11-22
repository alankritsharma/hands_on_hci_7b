using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrabbableCube : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer cubeMesh;
    [SerializeField] private Collider respawnIntersectionCheckCollider;

    [Header("Settings")]
    [SerializeField] private float respawnDeltaY;
    [SerializeField] private float respawnDelay;
    [SerializeField] private float respawnIntersectionCheckStep;
    
    /// <summary>
    /// If the transform has an associated rigidbody, make it kinematic during this
    /// number of frames after a respawn, in order to avoid ghost collisions.
    /// </summary>
    [SerializeField]
    [Tooltip("If the transform has an associated rigidbody, make it kinematic during this number of frames after a respawn, in order to avoid ghost collisions.")]
    private int _sleepFrames = 0;

    /// <summary>
    /// UnityEvent triggered when a respawn occurs.
    /// </summary>
    [SerializeField]
    [Tooltip("UnityEvent triggered when a respawn occurs.")]
    private UnityEvent _whenRespawned = new UnityEvent();

    public UnityEvent WhenRespawned => _whenRespawned;

    // cached starting transform
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private TwoGrabFreeTransformer[] freeTransformers;
    private Rigidbody rigidBody;
    private int _sleepCountDown;
    private bool initialized;
    private bool locked;
   
    public void Init()
    {
        this.initialPosition = this.transform.position;
        this.initialRotation = this.transform.rotation;
        this.initialScale = this.transform.localScale;
        this.freeTransformers = this.GetComponents<TwoGrabFreeTransformer>();
        this.rigidBody = this.GetComponent<Rigidbody>();
        this.initialized = true;
        this.locked = false;
    }

    private void Update()
    {
        if (this.locked) return;
        if (this.initialized && this.initialPosition.y - this.transform.position.y > this.respawnDeltaY) Invoke("Respawn", this.respawnDelay);
    }

    protected virtual void FixedUpdate()
    {
        if (this.locked) return;

        if (_sleepCountDown > 0)
        {
            if (--_sleepCountDown == 0)
            {
                rigidBody.isKinematic = false;
            }
        }
    }

    public void Respawn()
    {
        this.transform.position = initialPosition;
        this.transform.rotation = initialRotation;
        this.transform.localScale = initialScale;

        while (this.respawnIntersectionCheckCollider.bounds.Contains(this.transform.position - Vector3.up * this.cubeMesh.bounds.size.y)) this.transform.position += Vector3.up * this.respawnIntersectionCheckStep;

        if (rigidBody)
        {
            rigidBody.linearVelocity = Vector3.zero; // was automatically updated by Unity, originally it was rigidBody.velocity
            rigidBody.angularVelocity = Vector3.zero;

            if (!rigidBody.isKinematic && _sleepFrames > 0)
            {
                _sleepCountDown = _sleepFrames;
                rigidBody.isKinematic = true;
            }
        }

        foreach (var freeTransformer in freeTransformers)
        {
            freeTransformer.MarkAsBaseScale();
        }

        _whenRespawned.Invoke();
    }

    // prevent the cube from being moved, also disables the respawning
    public void LockInPlace()
    {
        this.locked = true;
        this.rigidBody.isKinematic = true;
        this.GetComponent<TouchHandGrabInteractable>().enabled = false;
        this.GetComponent<Grabbable>().enabled = false;
    }

    // allow interaction with the cube again
    public void Unlock()
    {
        this.locked = false;
        this.rigidBody.isKinematic = false;
        this.GetComponent<TouchHandGrabInteractable>().enabled = true;
        this.GetComponent<Grabbable>().enabled = true;
    }
}