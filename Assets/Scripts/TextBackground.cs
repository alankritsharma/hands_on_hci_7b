using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBackground : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform backgroundImage;
    [SerializeField] private TMP_Text text;

    [Header("Settings")]
    [SerializeField] private Vector2 padding;
    [SerializeField] private Vector2 minSize;

    private bool textChanged = false;

    private void Start()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(this.UpdateBackgroundSize);
    }

    private void Update()
    {
        if (this.textChanged)
        {
            this.textChanged = false;
            Vector2 textSize = this.text.GetRenderedValues();
            Vector2 bgSize = new Vector2(textSize.x + this.padding.x * 2, textSize.y + this.padding.y * 2);
            this.backgroundImage.sizeDelta = new Vector2(Mathf.Max(bgSize.x, this.minSize.x), Mathf.Max(bgSize.y, this.minSize.y));
        }
    }

    private void UpdateBackgroundSize(Object obj)
    {
        if (obj != this.text) return;
        this.textChanged = true;
    }
}
