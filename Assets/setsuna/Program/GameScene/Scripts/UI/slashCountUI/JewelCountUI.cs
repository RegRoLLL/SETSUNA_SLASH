using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JewelCountUI : MonoBehaviour
{
    [SerializeField] GameObject jewelCellPrefab;
    [SerializeField] float showHideTime;
    readonly List<SlashCountCell> jewels = new();

    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void GenerateJewelCells(int count)
    {
        jewels.ForEach(j => Destroy(j.gameObject));

        jewels.Clear();

        for (int i = 0; i < count; i++)
        {
            var jewel = Instantiate(jewelCellPrefab, this.transform).GetComponent<SlashCountCell>();
            jewel.Initialize();
            jewel.SetCell(true);
            jewel.Consume();
            jewels.Add(jewel);
        }
    }

    public void GetJewel(int index)
    {
        jewels[index].Recover();
    }

    public void Show(bool animate)
    {
        StartCoroutine(ShowCoroutine(animate));
    }
    IEnumerator ShowCoroutine(bool animate)
    {
        var startX = rectTransform.localEulerAngles.x;
        var targetX = 0f;
        var angle = rectTransform.localEulerAngles;
        var dt = 0f;

        if (animate)
        {
            while (dt < showHideTime)
            {
                angle.x = Mathf.Lerp(startX, targetX, dt / showHideTime);
                rectTransform.localEulerAngles = angle;
                dt += Time.deltaTime;
                yield return null;
            }
        }

        angle.x = targetX;
        rectTransform.localEulerAngles = angle;
    }

    public void Hide(bool animate)
    {
        StartCoroutine(HideCoroutine(animate));
    }
    IEnumerator HideCoroutine(bool animate)
    {
        if (rectTransform == null) Start();

        var startX = rectTransform.localEulerAngles.x;
        var targetX = 90f;
        var angle = rectTransform.localEulerAngles;
        var dt = 0f;

        if (animate)
        {
            while (dt < showHideTime)
            {
                angle.x = Mathf.Lerp(startX, targetX, dt / showHideTime);
                rectTransform.localEulerAngles = angle;
                dt += Time.deltaTime;
                yield return null;
            }
        }

        angle.x = targetX;
        rectTransform.localEulerAngles = angle;
    }

    /// <summary>
    /// 宝石の獲得状況
    /// </summary>
    /// <returns>01のデータ列で返す。小さい位から読む。トップに1を足している</returns>
    public string GetJewelsCollecting()
    {
        string result = "";
        foreach (var jewel in jewels)
        {
            if (jewel.state.fore) result += "1";
            else result += "0";
        }
        result += "1";
        return result;
    }
}
