using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class KeypadButton : MonoBehaviour
{
    Keypad keypad;
    TextMeshProUGUI buttonText;

    public void nameToButtonText()
    {
        buttonText.text = gameObject.name;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        keypad = GetComponentInParent<Keypad>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText.text.Length == 1)
        {
            nameToButtonText();
            GetComponentInChildren<ButtonVR>().onRelease.AddListener(delegate { keypad.AddDigit(buttonText.text); });
        }
        else if (buttonText.text == "Clear") {
        
        }
        else if (buttonText.text == "Enter")  {
        
        }
    }
}
