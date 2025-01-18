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
    
    readonly List<SlashCountCell> cells = new();

    void Start()
    {
        ListCells();

        foreach (var cell in cells) cell.Initialize();

        StartCoroutine(ResetUICoroutine());
    }

    void ListCells()
    {
        cells.Clear();

        foreach(Transform t in score_container)
            cells.Add(t.GetComponent<SlashCountCell>());

        foreach (Transform t in slashL_container)
            cells.Add(t.GetComponent<SlashCountCell>());

        foreach (Transform t in slashS_container)
            cells.Add(t.GetComponent<SlashCountCell>());
    }


    public IEnumerator ResetUICoroutine()
    {
        var wait = new WaitForSeconds(settings.cellSetInterval);

        foreach (var cell in cells)
        {
            if (!cell.state.back) continue;
            cell.SetCell(false);
        }

        foreach (var cell in cells)
        {
            yield return wait;
            cell.SetBack(true);
        }
        foreach (var cell in cells)
        {
            if (cell.state.fore) continue;
            cell.Recover();
            yield return wait;
        }
    }
    [Serializable] class SlashCountUISettings
    {
        public float cellSetInterval;
    }
}
