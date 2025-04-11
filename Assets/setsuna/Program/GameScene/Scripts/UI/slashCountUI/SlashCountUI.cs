using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System;
using TMPro;
using UnityEngine.UI;

public class SlashCountUI : MonoBehaviour
{
    [SerializeField] SlashCountUISettings settings = new();

    [SerializeField] Transform score_container, slashL_container, slashS_container;
    [SerializeField] Image infinityGauge;
    [SerializeField] bool isInfinity;

    [SerializeField] CanvasGroup pointPopupGroup;
    [SerializeField] TextMeshProUGUI pointPopupLabelTMP, pointPopupCommentTMP;
    [SerializeField] string score0, score1, score2, score3, overScore;

    [SerializeField] TextMeshProUGUI partTitleTMP, areaNameTMP;

    public JewelCountUI jewelCounter;

    int scoreRemains, slashRemains;
    [SerializeField] List<SlashCountCell> scoreCells = new();
    [SerializeField] List<SlashCountCell> slashCells = new();

    WaitForSeconds pointPopupStay;
    RectTransform pointPopupRectTransform;
    Vector3 pointPopupOriginPos;

    Coroutine resetUICoroutine,showSlashCellsCoroutine;

    void Start()
    {
        pointPopupStay = new WaitForSeconds(settings.pointPopupStayT);
        pointPopupRectTransform = pointPopupGroup.GetComponent<RectTransform>();
        pointPopupOriginPos = pointPopupRectTransform.localPosition;
        pointPopupRectTransform.localPosition += Vector3.left * settings.pointPopupSlide;

        infinityGauge.enabled = false;
        jewelCounter.Hide(false);

        ListCells();

        foreach (var cell in scoreCells) cell.Initialize();
        foreach (var cell in slashCells) cell.Initialize();

        StartCoroutine(VanishCellsCoroutine());
    }

    void ListCells()
    {
        slashCells.Clear();
        scoreCells.Clear();

        foreach (Transform t in score_container)
            scoreCells.Add(t.GetComponent<SlashCountCell>());

        foreach (Transform t in slashL_container)
            slashCells.Add(t.GetComponent<SlashCountCell>());

        foreach (Transform t in slashS_container)
            slashCells.Add(t.GetComponent<SlashCountCell>());
    }

    public void SetInfinity(bool showJewelCounter)
    {
        infinityGauge.enabled = true;
        isInfinity = true;
        scoreRemains = 3;
        slashRemains = 10;

        if(showJewelCounter)jewelCounter.Show(true);

        ResetUI(false);
    }

    public void SetCells(int recommend, int hintUsed, bool disableAnimation, bool inPuzzle)
    {
        isInfinity = false;
        jewelCounter.Hide(true);

        scoreRemains = 3 - hintUsed;
        slashRemains = recommend;

        if (!inPuzzle)
            StartCoroutine(VanishCellsCoroutine());
        else
            ResetUI(disableAnimation);
    }
    public void ConsumeSlashCell()
    {
        slashCells[(slashRemains--) - 1].Consume();
    }
    public void ConsumeScoreCell()
    {
        scoreCells[(scoreRemains--) - 1].Consume();
    }

    public void ShowPointPopup(StagePart.PartClearStatus stat, int point)
    {
        StartCoroutine(PointPopupCoroutine(stat, point));
    }
    IEnumerator PointPopupCoroutine(StagePart.PartClearStatus stat, int point)
    {
        pointPopupGroup.alpha = 1;

        var slideStartPos = pointPopupRectTransform.localPosition;

        pointPopupLabelTMP.text = $"★ {stat.currentPoint}/{stat.recommendMaxPoint}";

        string comment;

        if (point == 0) comment = score0;
        else if (point == 1) comment = score1;
        else if (point == 2) comment = score2;
        else if (point == 3) comment = score3;
        else if (point >= 4) comment = overScore;
        else comment = "ERROR: OUT OF RANGE!!";

        pointPopupCommentTMP.text = $"{comment} +{point}";

        float dt = 0, lerp;
        while (dt <= settings.pointPopupShowT) {
            lerp = dt / settings.pointPopupShowT;

            pointPopupRectTransform.localPosition
                = Vector3.Lerp(slideStartPos, pointPopupOriginPos, lerp);

            dt += Time.deltaTime;
            yield return null;
        }
        pointPopupRectTransform.localPosition = pointPopupOriginPos;

        yield return pointPopupStay;

        dt = 0;
        while (dt <= settings.pointPopupFadeT)
        {
            lerp = 1 - dt / settings.pointPopupFadeT;

            pointPopupGroup.alpha = lerp;

            dt += Time.deltaTime;
            yield return null;
        }
        pointPopupGroup.alpha = 0;
        pointPopupRectTransform.localPosition = slideStartPos;
    }

    public void SetSubtitleLabel(string partTitle, string areaName)
    {
        partTitleTMP.text = partTitle;
        areaNameTMP.text = areaName;
    }

    void ResetUI(bool disableAnimation)
    {
        if (resetUICoroutine != null)
        {
            StopCoroutine(resetUICoroutine);
            StopCoroutine(showSlashCellsCoroutine);
        }
        resetUICoroutine = StartCoroutine(ResetUICoroutine(disableAnimation));
    }
    IEnumerator ResetUICoroutine(bool disableAnimation)
    {
        var wait = new WaitForSeconds(settings.cellSetInterval);

        //全セルを非表示
        if (disableAnimation)
        {
            yield return StartCoroutine(VanishCellsCoroutine());
        }

        infinityGauge.enabled = isInfinity;

        //スコアセルを表示
        yield return StartCoroutine(ShowCellsCoroutine(scoreCells,scoreRemains , wait));

        //斬撃セルを表示
        yield return showSlashCellsCoroutine = StartCoroutine(ShowCellsCoroutine(slashCells,slashRemains , wait));

        resetUICoroutine = null;
        showSlashCellsCoroutine = null;
    }
    public IEnumerator VanishCellsCoroutine()
    {
        var wait = new WaitForSeconds(settings.cellSetInterval);

        foreach (var cell in Enumerable.Reverse(slashCells))
        {
            if (!cell.state.back) continue;
            yield return wait;
            cell.SetCell(false);
        }
        foreach (var cell in Enumerable.Reverse(scoreCells))
        {
            if (!cell.state.back) continue;
            yield return wait;
            cell.SetCell(false);
        }
    }
    IEnumerator ShowCellsCoroutine(List<SlashCountCell> cells,int count, WaitForSeconds wait)
    {
        foreach (var (cell, index) in cells.Select((cell, index) => (cell, index)))
        {
            if (index + 1 > count) continue;

            if (cell.state.fore) continue;
            yield return wait;

            if (!cell.state.back)
            {
                cell.SetCell(true);
            }
            else
            {
                cell.Recover();
            }
        }
    }
    [Serializable] class SlashCountUISettings
    {
        public float cellSetInterval;

        public float pointPopupSlide;
        public float pointPopupShowT, pointPopupStayT, pointPopupFadeT;
    }
}
