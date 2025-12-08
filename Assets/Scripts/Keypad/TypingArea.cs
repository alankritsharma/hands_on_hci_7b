using UnityEngine;

public class TypingArea : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject leftTypingHand;
    public GameObject rightTypingHand;

    public void OnTriggerEnter(Collider other)
    {
        GameObject hand = other.GetComponentInParent<OVRGrabber>().gameObject;
        if (hand == null)
            return;
        if (hand == leftHand)
        {
            leftTypingHand.SetActive(true);
        }
        else if (hand == rightHand)
        {
            rightTypingHand.SetActive(true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        GameObject hand = other.GetComponentInParent<OVRGrabber>().gameObject;
        if (hand == null)
            return;
        if (hand == leftHand)
        {
            leftTypingHand.SetActive(false);
        }
        else if (hand == rightHand)
        {
            rightTypingHand.SetActive(false);
        }
    }
}
