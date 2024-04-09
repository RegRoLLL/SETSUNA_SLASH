using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BackGroundGroup : MonoBehaviour
{
    [SerializeField] bool isOn;

    [Header("Internal Datas")]
    [SerializeField] List<ScrollBG> bg_list = new();

    void OnEnable()
    {
        bg_list = GetComponentsInChildren<ScrollBG>(true).ToList();
    }

    public void SetParent(Transform parent)
    {
        GetComponent<RectTransform>().SetParent(parent);
    }

    public void SetEnable(bool on)
    {
        foreach(var sbg in bg_list) {
            sbg.enabled = on;

            if(on)sbg.Initialize();
        }

        isOn = on;
    }
}
