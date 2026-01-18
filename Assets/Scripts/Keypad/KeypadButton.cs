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
        keypad = GetComponentInParent<Keypad>(true);
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (gameObject.name.Length == 1)
        {
            nameToButtonText();
            GetComponentInChildren<ButtonVR>(true).onRelease.AddListener(delegate {
                if (keypad != null)
                {
                    keypad.AddDigit(gameObject.name);
                }
                else
                {
                    Debug.Log("NULL KEYPAD");
                }
            });
        }
        else if (gameObject.name == "clear") {
            GetComponentInChildren<ButtonVR>(true).onRelease.AddListener(delegate
            {
                if (keypad != null)
                {
                    keypad.ClearCode();
                }
                else
                {
                    Debug.Log("NULL KEYPAD");
                }
            });
        }
        else if (gameObject.name == "enter")  {
            GetComponentInChildren<ButtonVR>(true).onRelease.AddListener(delegate
            {
                if (keypad != null)
                {
                    keypad.CheckCode();
                }
                else
                {
                    Debug.Log("NULL KEYPAD");
                }
            });
        }
    }
}
