using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;

public class UIManagerPaintingsTask : UIManagerAlignment
{
    [Header("Painting - References")]
    [SerializeField] private Transform paintingTimerParent;

    [Header("Painting - UI Elements")]
    [SerializeField] private TMP_Text paintingInstructionText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text paintingTimerText;

    [Header("Painting - UI Texts")]
    [SerializeField] private string[] paintingsInstructions;
    [SerializeField] private string takeOffInstruction;
    [SerializeField] private string[] endOfTaskInstructions;

    // show paintings instruction with the given index to the user
    public void ShowPaintingsInstruction(int index)
    {
        this.paintingInstructionText.text = this.paintingsInstructions[index];
    }

    // update timer text to show the given text
    public void UpdateTimerText(string text)
    {
        this.timerText.text = text;
    }

    // place the painting timer canvas at the position of the given transform (also applies the transform's rotation)
    public void PositionPaintingTimer(Transform transform)
    {
        Transform timerCanvas = this.paintingTimerText.transform.parent;

        // make sure the canvas is a child of the object being manipulated by the RDW
        if (timerCanvas.parent != this.paintingTimerParent) timerCanvas.parent = this.paintingTimerParent;

        // make sure the canvas is active
        timerCanvas.gameObject.SetActive(true);

        // update position and rotation of the canvas
        timerCanvas.position = transform.position;
        timerCanvas.rotation = transform.rotation;
    }

    // update painting timer text to show the given timer value
    public void UpdatePaintingTimerText(float timer)
    {
        this.paintingTimerText.text = $"{Mathf.Max(0, timer):#0.0}";
    }

    // clear the painting timer text
    public void ClearPaintingTimerText()
    {
        this.paintingTimerText.text = "";
    }

    // instruct the user to take off the headset
    public void ShowTakeOffInstruction()
    {
        this.paintingInstructionText.text = this.takeOffInstruction;
    }

    // instruct the user what to do at the end of the task
    public void ShowEndOfTaskInstruction(int index)
    {
        this.paintingInstructionText.text = this.endOfTaskInstructions[index];
    }
}
