using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class Drop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Dropped object was: " + eventData.pointerDrag);
        Drag drag = eventData.pointerDrag.GetComponent <Drag>();
        if (drag != null) {
            if(transform.childCount > 0)
            {
                transform.GetChild(0).SetParent(drag.old);
            }
            drag.transform.SetParent(transform);
        }

    }
}
