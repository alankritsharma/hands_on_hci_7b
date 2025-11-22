using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform centerEyeAnchor;

    private LineRenderer lineRenderer;
    private Color visibleColor;
    private Color hiddenColor;
    private List<Vector3> positions;

    private void Start()
    {
        // get references
        this.lineRenderer = this.GetComponent<LineRenderer>();
        this.visibleColor = this.lineRenderer.material.color;
        this.hiddenColor = this.lineRenderer.material.color;
        this.hiddenColor.a = 0;
        this.positions = new List<Vector3>();

        this.Hide();
    }

    // hide the trail
    private void Hide()
    {
        this.lineRenderer.material.color = this.hiddenColor;
    }

    // show the trail
    private void Show()
    {
        this.lineRenderer.material.color = this.visibleColor;
    }

    // toggle the trail
    public void Toggle()
    {
        if (this.lineRenderer.material.color.a == 0) this.Show();
        else this.Hide();
    }

    // add the current position to the trail
    public void AddPosition()
    {
        this.positions.Add(Utils.WithoutY(this.centerEyeAnchor.position));
        this.lineRenderer.positionCount = this.positions.Count;
        this.lineRenderer.SetPositions(this.positions.ToArray());
    }

    // clear all positions and hide the trail
    public void ClearAndHide()
    {
        this.positions.Clear();
        this.lineRenderer.positionCount = 0;
        this.lineRenderer.SetPositions(new Vector3[] { });
        this.Hide();
    }
}
