using TMPro;
using UnityEngine;

// script implementing tooltips for the controller-buttons
public class Tooltip : MonoBehaviour
{
    [SerializeField] private Transform lineTarget;
    [SerializeField] private Vector3 lineSourceOffset;
    [SerializeField] private Vector3 lineTargetOffset;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private TMP_Text textField;

    private RectTransform canvasRect;

    void Start()
    {
        this.canvasRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        this.AlignLineRenderer();
    }

    // update line renderer positions to make sure the tooltip stays attached to its button (lineTarget)
    private void AlignLineRenderer()
    {
        Vector3 lineSource = this.canvasRect.position + this.lineSourceOffset.x * this.canvasRect.right + this.lineSourceOffset.y * this.canvasRect.up + this.lineSourceOffset.z * this.canvasRect.forward;
        Vector3 lineTargetPos = this.lineTarget.position + this.lineTargetOffset.x * this.lineTarget.right + this.lineTargetOffset.y * this.lineTarget.up + this.lineTargetOffset.z * this.lineTarget.forward;
        this.lineRenderer.SetPositions(new Vector3[] { lineSource, lineTargetPos });
    }

    // set content of the tooltip's text field
    public void SetTextContent(string content)
    {
        this.textField.text = content;
    }

    // update tooltip visualization in the editor
    void OnDrawGizmosSelected()
    {

#if UNITY_EDITOR
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetComponent<RectTransform>().position + this.lineSourceOffset, 0.001f);
        Gizmos.color = Color.white;

        if (this.lineTarget && this.lineRenderer)
        {
            RectTransform canvasRect = this.GetComponent<RectTransform>();
            Vector3 lineSource = canvasRect.position + this.lineSourceOffset.x * canvasRect.right + this.lineSourceOffset.y * canvasRect.up + lineSourceOffset.z * canvasRect.forward;
            Vector3 lineTargetPos = this.lineTarget.position + this.lineTargetOffset.x * this.lineTarget.right + this.lineTargetOffset.y * this.lineTarget.up + this.lineTargetOffset.z * this.lineTarget.forward;
            this.lineRenderer.SetPositions(new Vector3[] { lineSource, lineTargetPos });
        }
#endif
    }
}