using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionProtection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StudyManager taskManager; // TODO: change broke other scenes, which are not using the StudyManager yet, e.g. ColorTuning and Demo 0
    [SerializeField] private AbstractModelManager modelManager;
    [SerializeField] private AlignmentManager alignment;
    [SerializeField] private Transform[] obstacles;
    [SerializeField] private Transform obstacleParent;

    [Header("Settings")]
    [SerializeField] private bool active;
    [SerializeField] private string obstacleTag;
    [Range(0, 1)][SerializeField] private float vibrationIntensity;

    private bool parentChanged;
    private bool collisionDetected;

    private void Update()
    {
        if (!this.parentChanged && this.alignment.GetAlignmentDone() && this.alignment.GetFineTuningDone())
        {
            // change parent to prevent obstacles from being manipulated by the RDW
            foreach (Transform obstacle in this.obstacles) obstacle.parent = this.obstacleParent;
            this.parentChanged = true;
        }

        if (this.collisionDetected) OVRInput.SetControllerVibration(1, this.vibrationIntensity, OVRInput.Controller.All);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.active && other.tag == this.obstacleTag) this.OnImpendingCollision();
    }

    // if it is currently active, deactivate the model so the user can see the impending collision
    private void OnImpendingCollision()
    {
        // don't check for collisions during alignment and fine tuning
        if (!this.taskManager.GetCollisionDetectionWanted()) return;

        // no need to do something if there is no model being displayed anyway
        if (!this.modelManager.IsModelVisible()) return;

        // deactivate virtual content, so the user can see the real world again
        this.modelManager.ToggleModelVisibility();
        this.modelManager.SetPaintingsActive(false);

        // give haptic feedback in the Update method
        this.collisionDetected = true;

        // notify the task manager
        this.taskManager.EnterCollisionDeadlockState();
    }

    // restore the state from before the collision
    public void Recover()
    {
        // reset the flag (disables haptic feedback)
        this.collisionDetected = false;

        // restore painting and model visibility
        this.modelManager.SetPaintingsActive(true);
        if (!this.modelManager.IsModelVisible()) this.modelManager.ToggleModelVisibility();
    }

    // stop the haptic feedback
    public void StopHapticFeedback()
    {
        this.collisionDetected = false;
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.All);
    }
}
