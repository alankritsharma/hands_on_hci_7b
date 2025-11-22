using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_RDW : PerceptualManipulation
{
    [Header("References")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private Transform centerEyeAnchor;

    [Header("Settings")]
    [SerializeField] private float curvatureGain;
    [SerializeField] private float rotationGain;
    [SerializeField] private float rotationStopOffset;
    [SerializeField] private bool debugOutput;

    private Vector3 rdwOrigin;
    private Vector3 initialModelPosition;
    private Quaternion initialModelRotation;

    // curvature-based RDW
    private bool rdwCurvatureActive;
    private bool redirectLeft;
    private float currentAngle;

    // rotation-based RDW
    private bool rdwRotationActive;
    private bool rotateLeft;
    private Vector3 lastForward;

    private void FixedUpdate()
    {
        if (this.rdwCurvatureActive) this.UpdateCurvatureRDW();
        else if (this.rdwRotationActive) this.UpdateRotationRDW();        
    }

    // update for the curvature-based RDW
    private void UpdateCurvatureRDW()
    {
        // apply rotation based on the distance from the origin
        float newAngle = (this.centerEyeAnchor.position - this.rdwOrigin).magnitude * this.curvatureGain;
        if (this.redirectLeft) newAngle *= -1;
        this.RotateModel(newAngle - this.currentAngle);
        this.currentAngle = newAngle;

        if (this.debugOutput)
        {
            Debug.Log($"Distance: {(this.centerEyeAnchor.position - this.rdwOrigin).magnitude}");
            Debug.Log($"New angle: {newAngle}");
        }
    }

    // update for the rotation-based RDW
    private void UpdateRotationRDW()
    {
        // amplify the user's rotation
        Vector3 currentForward = Utils.WithoutY(this.centerEyeAnchor.forward);
        float angle = Vector3.Angle(currentForward, this.lastForward);
        if (this.rotateLeft) angle *= -1;
        this.RotateModel(angle * this.rotationGain);

        // update the stored forward axis
        this.lastForward = currentForward;

        // check, whether the rotation should be stopped (based on the distance the user moved)
        float distance = Utils.WithoutY(this.centerEyeAnchor.position - this.rdwOrigin).magnitude;
        if (this.rotationStopOffset > 0 && distance >= this.rotationStopOffset) this.StopRotationRDW();
    }

    // start curvature-based RDW with the player's current position as the origin
    public void StartCurvatureRDW()
    {
        this.rdwOrigin = this.centerEyeAnchor.position;
        this.currentAngle = 0;
        this.rdwCurvatureActive = true;
    }

    // set which direction the next curvature RDW should use
    public void SetCurvatureDirection(bool dir)
    {
        this.redirectLeft = dir;
    }

    // set which direction the next rotation RDW should use
    public void SetRotationDirection(bool dir)
    {
        this.rotateLeft = dir;
    }

    // flip the direction for the next rotation RDW
    public void FlipRotationDirection()
    {
        this.rotateLeft = !this.rotateLeft;
    }

    // stop the curvature-based RDW
    public void StopCurvatureRDW()
    {
        this.rdwCurvatureActive = false;
    }

    // start rotation-based RDW
    public void StartRotationRDW()
    {
        this.rdwRotationActive = true;

        // initialize with the current forward axis and position
        this.lastForward = Utils.WithoutY(this.centerEyeAnchor.forward);
        this.rdwOrigin = this.centerEyeAnchor.position;
    }

    // start rotation-based RDW with the given rotation gain
    public void StartRotationRDW(float gain)
    {
        this.rdwRotationActive = true;

        // set the given gain, rotateLeft should be false so the sign isn't flipped
        this.rotationGain = gain;
        this.rotateLeft = false;

        // initialize with the current forward axis and position
        this.lastForward = Utils.WithoutY(this.centerEyeAnchor.forward);
        this.rdwOrigin = this.centerEyeAnchor.position;
    }

    // stop the rotation-based RDW
    public void StopRotationRDW()
    {
        this.rdwRotationActive = false;
    }

    // rotate the model around the up-axis
    private void RotateModel(float angle)
    {
        this.modelParent.RotateAround(this.centerEyeAnchor.position, Vector3.up, angle);
    }

    // activate the PM
    public override void StartPM() 
    {
        // store the model alignment from before the PM started
        this.initialModelPosition = this.modelParent.position;
        this.initialModelRotation = this.modelParent.rotation;
    }

    // start the RDW with the given gain
    public override void UpdatePM(float gain)
    {
        this.StartRotationRDW(gain);
    }

    // stop the RDW
    public override void StopPM(bool resolve) 
    {
        this.StopRotationRDW();

        // restore the initial model alignment to resolve the manipulations
        if (resolve)
        {
            this.modelParent.position = this.initialModelPosition;
            this.modelParent.rotation = this.initialModelRotation;
        }
    }
}
