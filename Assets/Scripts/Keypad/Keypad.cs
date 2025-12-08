using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum LightState
{
    OFF,
    FAILED,
    PASS
}

public class Keypad : MonoBehaviour
{

    public int rounds = 3;
    private string correctCode;
    private int setLen = 4;
    private string inputCode;

    private GameObject lightBulb;
    private TextMeshProUGUI stickyNoteText;


    public string getInputCode()
    {
        return inputCode;
    }

    public void AddDigit(string digit)
    {
        if (inputCode.Length < setLen)
        {
            inputCode += digit;
            Debug.Log("Current code: " + inputCode);
        }
        else
        {
            Debug.Log("Limit reached! Length: " + inputCode.Length);
        }
    }

    public void ClearCode()
    {
        inputCode = string.Empty;
        Debug.Log("Cleared passcode!");
        ChangeLightColor(LightState.OFF);
    }

    IEnumerator waiter()
    {
        if (inputCode == correctCode)
        {
            Debug.Log("Correct passcode!");
            ChangeLightColor(LightState.PASS);

            if (rounds > 0)
            {
                rounds--;
                yield return new WaitForSeconds(3);
                Reset();
            }
        }
        else
        {
            Debug.Log("Wrong passcode!");
            yield return new WaitForSeconds(3);
            ChangeLightColor(LightState.FAILED);
        }
    }

    public void CheckCode()
    {
        StartCoroutine(waiter());
    }

    private void ChangeLightColor(LightState state)
    {
        Color newColor;
        Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        switch (state) {
            case LightState.OFF:
                newColor = Color.grey;
                Debug.Log("Semaphore is off");
                break;
            case LightState.FAILED:
                newColor = Color.red;
                Debug.Log("Semaphore is red");
                break;
            case LightState.PASS:
                newColor = Color.green;
                Debug.Log("Semaphore is green");
                break;
            default:
                newColor = Color.grey;
                break;
        }
        newMaterial.color = newColor;
        MeshRenderer gameObjectRenderer = lightBulb.GetComponent<MeshRenderer>();
        gameObjectRenderer.material = newMaterial;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int randomCode = Random.Range(1000, 10000);
        correctCode = randomCode.ToString();

        GameObject semaphore = GameObject.Find("Semaphore");
        if (semaphore == null)
        {
            Debug.Log("Couldn't find Semaphore Object!");
            return;
        }
        else
            {
                lightBulb = semaphore.transform.GetChild(1).gameObject;
                if (lightBulb == null) {
                    Debug.Log("Couldn't find Light component of Semaphore!");
                    return;
                }
                else
                {
                    ChangeLightColor(LightState.OFF);
                }
            }

        GameObject stickyNote = GameObject.Find("StickyNote");
        if (stickyNote == null)
        {
            Debug.Log("Couldn't find Sticky Note Object!");
            return;
        }
        else
        {
            stickyNoteText = stickyNote.GetComponentInChildren<TextMeshProUGUI>();
            stickyNoteText.text = correctCode;
        }
    }

    private void Reset()
{
    int randomCode = Random.Range(1000, 10000);
    correctCode = randomCode.ToString();

    stickyNoteText.text = correctCode;
    ClearCode();
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
