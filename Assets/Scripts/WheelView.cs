using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;
using Best.HTTP.Request.Settings;
using System.Net.WebSockets;
public class WheelView : MonoBehaviour
{
    public int segment = 24;
    Tween rotationTween;
    public int targetIndex;
    // internal int type = 0;
    [SerializeField] internal WheelItem[] wheelItems;
    public bool isStatic = true;
    [SerializeField] float startOffsetAngle = 25f;
    [SerializeField] int extraRotations = 5;
    bool canStopOnHit = false;
    bool hasStopped = false;

    void Start()
    {
        // if (isStatic)
        // {
        //     rotationTween ??= transform.DOLocalRotate(new Vector3(0, 0, -360), 7f, RotateMode.FastBeyond360)
        //   .SetLoops(-1, LoopType.Incremental)
        //   .SetEase(Ease.Linear);
        // }
    }

    internal void PopulateValues(GoldSpin values)
    {

        // ashu change here
        for (int i = 0; i < values.multiplierValues.Count; i++)
        {
            for (int j = 0; j < wheelItems.Length; j++)
            {
                if (wheelItems[j].type == "MULTIPLIERS" && wheelItems[j].value == 0)
                {
                    wheelItems[j].value = values.multiplierValues[i];
                    if (wheelItems[j].valueText) wheelItems[j].valueText.text = values.multiplierValues[i].ToString() + "x";
                    break;
                }
            }
        }
        for (int i = 0; i < values.wheelValues.Count; i++)
        {
            for (int j = 0; j < wheelItems.Length; j++)
            {
                if (wheelItems[j].type == "COINS" && wheelItems[j].value == 0)
                {
                    wheelItems[j].value = values.wheelValues[i];
                    if (wheelItems[j].valueText) wheelItems[j].valueText.text = values.wheelValues[i].ToString() + "x";
                    break;
                }
            }
        }
        // for (int i = 0; i < values.wheelValues.coins.Count; i++)
        // {
        //     for (int j = 0; j < wheelItems.Length; j++)
        //     {
        //         if (wheelItems[j].type == "COINS" && wheelItems[j].value == 0)
        //         {
        //             wheelItems[j].value = values.wheelValues.coins[i];
        //             if (wheelItems[j].valueText) wheelItems[j].valueText.text = values.wheelValues.coins[i].ToString();
        //             break;
        //         }
        //     }
        // }


    }
    public void OnSegmentHit(WheelItem hitItem)
    {
        if (hasStopped) return;

        hasStopped = true;

        transform.DOKill(true);


    }


    void EnableOnlyTargetCollider()
    {
        foreach (var item in wheelItems)
        {
            if (item?.collider == null) continue;

            var col = item.collider.GetComponent<Collider2D>();
            col.enabled = (item.index == targetIndex);
        }

        Debug.Log($"Collider enabled ONLY for index {targetIndex}");
    }
    internal IEnumerator StopWheel()
    {
        hasStopped = false;

        // Kill infinite spin
        transform.DOKill(true);

        // 🔒 No collider can stop wheel yet
        DisableAllColliders();

        // 🔄 Spin EXACTLY 2 rounds
        yield return transform.DOLocalRotate(
            new Vector3(0, 0, -360f * 2),
            2.2f,
            RotateMode.FastBeyond360
        ).SetEase(Ease.Linear)
         .WaitForCompletion();

        // 🎯 Enable ONLY winning collider
        EnableOnlyTargetCollider();

        // 🔄 Keep spinning until hit
        rotationTween = transform.DOLocalRotate(
            new Vector3(0, 0, -360),
            1.2f,
            RotateMode.FastBeyond360
        ).SetLoops(-1, LoopType.Incremental)
         .SetEase(Ease.Linear);

        Debug.Log("Waiting for target collider hit...");
    }

    void DisableAllColliders()
    {
        foreach (var item in wheelItems)
        {
            if (item?.collider == null) continue;
            item.collider.GetComponent<Collider2D>().enabled = false;
        }
    }



}

[Serializable]
public class WheelItem
{
    public string type;
    public int index = 0;
    public double value;
    public TMP_Text valueText;
    public GameObject collider;

}