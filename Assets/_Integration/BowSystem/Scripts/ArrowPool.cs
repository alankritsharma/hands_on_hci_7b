using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPool : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;

    private List<GameObject> availableArrows = new List<GameObject>();

    public GameObject GetArrow()
    {
        GameObject arrow;

        // pool still has an arrow that can be returned
        if (this.availableArrows.Count > 0)
        {
            arrow = this.availableArrows[0];
            arrow.SetActive(true);
            this.availableArrows.RemoveAt(0);
        }
        // no arrow left, instantiate a new one
        else
        {
            arrow = Instantiate(this.arrowPrefab);
            arrow.GetComponent<Arrow>().Init(this);
        }
        
        return arrow;
    }

    public void ReturnArrow(GameObject arrow)
    {
        arrow.SetActive(false);
        this.availableArrows.Add(arrow);
    }
}
