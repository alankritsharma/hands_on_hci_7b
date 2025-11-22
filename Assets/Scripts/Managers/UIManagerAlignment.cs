using TMPro;
using UnityEngine;

namespace Managers
{
    public class UIManagerAlignment: MonoBehaviour
    {
        [Header("Alignment - UI Elements")] 
        [SerializeField] private TMP_Text instructionText;
        
        [Header("Alignment - UI Texts")]
        [SerializeField] private string modelLoadingInstruction;
        [SerializeField] private string[] alignmentInstructions;

        // show loading instructions to the user
        public void ShowLoadingInstruction()
        {
            this.instructionText.text = this.modelLoadingInstruction;
        }

        // show alignment instructions with the given index to the user
        public void ShowAlignmentInstruction(int index)
        {
            this.instructionText.text = this.alignmentInstructions[index];
        }

        // clear the instructions text
        public void ClearInstructionsText()
        {
            this.instructionText.text = "";
        }

        // set visibility of the instruction text
        public void SetInstructionsVisibility(bool visible)
        {
            this.instructionText.gameObject.SetActive(visible);
        }

    }
}