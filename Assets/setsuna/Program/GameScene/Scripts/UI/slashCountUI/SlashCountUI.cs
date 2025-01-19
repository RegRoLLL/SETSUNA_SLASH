using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System;

public class SlashCountUI : MonoBehaviour
{
    [SerializeField] SlashCountUISettings settings = new();

    [SerializeField] Transform score_container, slashL_container, slashS_container;

    int scoreRemains, slashRemains;
    [SerializeField] List<SlashCountCell> scoreCells = new();
    [SerializeField] List<SlashCountCell> slashCells = new();

    void Start()
    {
        ListCells();

        foreach (var cell in scoreCells) cell.Initialize();
        foreach (var cell in slashCells) cell.Initialize();

        StartCoroutine(ResetUICoroutine(true));
    }

    void ListCells()
    {
        slashCells.Clear();
        scoreCells.Clear();

        foreach(Transform t in score_container)
            scoreCells.Add(t.GetComponent<SlashCountCell>());

        foreach (Transform t in slashL_container)
            slashCells.Add(t.GetComponent<SlashCountCell>());

        foreach (Transform t in slashS_container)
            slashCells.Add(t.GetComponent<SlashCountCell>());
    }

    public void SetCells(int recommend, int hintUsed, bool disableAnimation)
    {
        scoreRemains = 3 - hintUsed;
        slashRemains = recommend;

        StartCoroutine(ResetUICoroutine(disableAnimation));
    }
    public void ConsumeSlashCell()
    {
        slashCells[(slashRemains--) - 1].Consume();
    }
    public void ConsumeScoreCell()
    {
        scoreCells[(scoreRemains--) - 1].Consume();
    }

    public IEnumerator ResetUICoroutine(bool disableAnimation)
    {
        var wait = new WaitForSeconds(settings.cellSetInterval);

        //全セルを非表示
        if(disableAnimation)
        {
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

        //スコアセルを表示
        foreach (var (cell, index) in scoreCells.Select((cell,index)=> (cell, index)))
        {
            if (index + 1 > scoreRemains) continue;

            yield return wait;

            if(disableAnimation || !cell.state.back)
            {
                cell.SetCell(true);
            }
            else{
                cell.Recover();
            }
        }

        //斬撃セルを表示
        foreach ( var(cell,index) in slashCells.Select((cell, index) => (cell, index)))
        {
            if (index + 1 > slashRemains) continue;
            
            yield return wait;

            if(disableAnimation || !cell.state.back)
            {
                cell.SetCell(true);
            }
            else {
                cell.Recover();
            }
        }
    }
    [Serializable] class SlashCountUISettings
    {
        public float cellSetInterval;
    }
}
