using UnityEngine;

public class Keypad : MonoBehaviour
{

    public string correctCode = "0000";
    public int setLen = 4;
    private string inputCode;

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
    }

    public void CheckCode()
    {
        if (inputCode == correctCode)
        {
            Debug.Log("Correct passcode!");
        }
        else
        {
            Debug.Log("Wrong passcode!");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
