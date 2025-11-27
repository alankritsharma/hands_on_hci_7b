using UnityEngine;

public class TargetZone : MonoBehaviour
{
    public Transform center;     // The bullseye center

    public void OnArrowHit(Vector3 hitPoint)
    {
        // Optional: Play animation, sound, visual feedback
    }
}
