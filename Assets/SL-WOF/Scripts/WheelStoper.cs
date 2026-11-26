using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelStoper : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        WheelView wheelView = other.GetComponentInParent<WheelView>();
        if (wheelView == null) return;

        foreach (var item in wheelView.wheelItems)
        {
            if (item != null && item.collider == other.gameObject)
            {
                wheelView.OnSegmentHit(item);
                break;
            }
        }
    }

}
