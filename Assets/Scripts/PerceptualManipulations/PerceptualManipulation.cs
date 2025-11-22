using UnityEngine;

public abstract class PerceptualManipulation : MonoBehaviour
{
    public abstract void StartPM();
    public abstract void UpdatePM(float gain);
    public abstract void StopPM(bool resolve);
}
