using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected float interactionCooldown = 1;
    [SerializeField] protected float initDelay = 1;

    [Header("References")]
    [SerializeField] protected AlignmentManager alignment;
    [SerializeField] protected AbstractModelManager modelManager;

    // managers
    protected UIManagerAlignment uiManagerAlignment;

    // fine tuning
    [SerializeField] private float fineTuningMinThreshold = 0.1f;
    [SerializeField] private float fineTuningTranslationSpeed;
    [SerializeField] private float fineTuningRotationSpeed;
    private bool fineTuningRotationMode;

    protected float lastInteraction;
    private bool loadingSpatialAnchors;

    protected void Start()
    {
        // get references
        this.uiManagerAlignment = this.GetComponent<UIManagerAlignment>();
    }

    protected void Update()
    {
        this.HandleInput();
        this.UpdateSpatialAnchorLoading();
    }

    // trigger the according actions, whenever a button is pressed
    private void HandleInput()
    {
        // model alignment
        if (!this.alignment.GetAlignmentDone() || !this.alignment.GetFineTuningDone())
        {
            this.HandleAlignmentInput();
        }
    }

    // handle input during the initial alignment
    protected void HandleAlignmentInput()
    {
        // don't allow further input while the spatial anchors are loading
        if (this.loadingSpatialAnchors) return;

        // button B to continue the alignment progress
        if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.B, OVRInput.Controller.Touch))
        {
            this.alignment.NextStep();
            this.UpdateLastInteractionTime();

            // show the next alignment instruction
            ShowAlignmentInstruction();
        }
        // button Start to load the alignment from spatial anchors, if no anchor was set yet
        if (this.alignment.GetAlignmentPositionsCollected() == 0 && Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Option, OVRInput.Controller.Touch))
        {
            this.alignment.LoadPositionsFromSpatialAnchors();
            this.loadingSpatialAnchors = true;
            this.UpdateLastInteractionTime();

            // instruct player to wait
            this.uiManagerAlignment.ShowLoadingInstruction();
        }
        // input for the fine tuning
        if (this.alignment.GetAlignmentDone()) this.HandleFineTuningInput();
    }

    // update UI when the spatial anchors finished loading
    protected void UpdateSpatialAnchorLoading()
    {
        if (this.loadingSpatialAnchors && this.alignment.GetAlignmentPositionsCollected() == this.alignment.GetAlignmentPositionsToBeCollected())
        {
            // allow button input again, once the spatial anchors have been loaded
            this.loadingSpatialAnchors = false;

            // show the final alignment instruction
            ShowAlignmentInstruction();
        }
    }

    // handle input during the fine tuning
    private void HandleFineTuningInput()
    {
        // start button to switch between translation and rotation fine tuning
        if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Option, OVRInput.Controller.Touch))
        {
            this.fineTuningRotationMode = !this.fineTuningRotationMode;
            if (!this.modelManager.IsModelVisible()) this.modelManager.ToggleModelVisibility();
            this.modelManager.AssignFineTuningMaterial(this.fineTuningRotationMode);
        }

        // button Y to show/hide the model with its actual texture during fine tuning
        if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.Y, OVRInput.Controller.Touch))
        {
            this.modelManager.ApplyOpaqueMaterial();
            this.modelManager.ToggleModelVisibility();
        }

        // button X to reset the model's up-axis
        if (Time.time - this.lastInteraction > this.interactionCooldown && OVRInput.GetUp(ButtonMapping.X, OVRInput.Controller.Touch))
        {
            this.alignment.FineTuneResetUpAxis();
        }

        // fine tuning input with the joysticks
        Vector2 leftJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.Touch);
        Vector2 rightJoystick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch);

        // THIS CHECK HAS TO BE AFTER THE OTHER FINE TUNING BUTTON INPUT, BECAUSE OF THE RETURN
        if (leftJoystick.magnitude < this.fineTuningMinThreshold && rightJoystick.magnitude < this.fineTuningMinThreshold) return;

        // make sure the model is visible with the correct fine tuning material
        if (!this.modelManager.IsModelVisible()) this.modelManager.ToggleModelVisibility();
        this.modelManager.AssignFineTuningMaterial(this.fineTuningRotationMode);

        // apply the input to either the rotation or translation fine tuning
        if (this.fineTuningRotationMode) this.alignment.FineTuneRotation(leftJoystick * this.fineTuningRotationSpeed * Time.deltaTime, rightJoystick * this.fineTuningRotationSpeed * Time.deltaTime);
        else this.alignment.FineTuneTranslation(leftJoystick * this.fineTuningTranslationSpeed * Time.deltaTime, rightJoystick * this.fineTuningTranslationSpeed * Time.deltaTime);
    }

    // update time of last interaction
    public void UpdateLastInteractionTime()
    {
        this.lastInteraction = Time.time;
    }
    
    private void ShowAlignmentInstruction()
    {
        if (this.alignment.GetAlignmentDone() && this.alignment.GetFineTuningDone()) this.uiManagerAlignment.ClearInstructionsText();
        else if (this.alignment.GetAlignmentPositionsCollected() == this.alignment.GetAlignmentPositionsToBeCollected())
        {
            this.uiManagerAlignment.ShowAlignmentInstruction(3);
        }
        else this.uiManagerAlignment.ShowAlignmentInstruction(this.alignment.GetAlignmentPositionsCollected());
    }
    
    protected bool CanInteract()
    {
        return Time.time - lastInteraction > interactionCooldown;
    }

    /// <summary>
    /// Method <c>ButtonPressed</c> checks if specified button is pressed.
    /// <param name="button">possible buttons are <c>A</c>, <c>B</c>, <c>X</c>, <c>Y</c>, <c>StickL</c>, <c>StickR</c>, and <c>Option</c></param>
    /// </summary>
    protected static bool ButtonPressed(string button)
    {
        OVRInput.Button mapping;
        switch (button)
        {
            case "A": mapping = ButtonMapping.A; break;
            case "B": mapping = ButtonMapping.B; break;
            case "X": mapping = ButtonMapping.X; break;
            case "Y": mapping = ButtonMapping.Y; break;
            case "StickL": mapping = ButtonMapping.StickL; break;
            case "StickR": mapping = ButtonMapping.StickR; break;
            case "Option": mapping = ButtonMapping.Option; break;
            default: return false;
        }
        return OVRInput.Get(mapping, OVRInput.Controller.Touch);
    }
}
