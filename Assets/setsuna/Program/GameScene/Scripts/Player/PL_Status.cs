using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PL_Status:SetsunaSlashScript
{
    [SerializeField] PartStat partStat;
    [SerializeField] int recommendCount, currentCount, currentScoreAdditional;
    [SerializeField] int hintUsed;
    [SerializeField] bool inPuzzle;
    public bool InAnotherPart { get => partStat == PartStat.another; }
    public bool IsGoaled { get => partStat == PartStat.goaled; }

    Player pl;
    SlashCountUI ui;

    public enum PartStat { common, goaled, another }

    void Start()
    {
        pl = GetComponent<Player>();
        ui = pl.ui.SlashCountUI;

        if(!config.debugMode){
            hintUsed = 3;
            recommendCount = 0;
            currentCount = 0;
            currentScoreAdditional = 0;
            inPuzzle = false;
        }
    }

    public void Damage() => TryConsumeCount();
    /// <summary>
    /// �a���񐔂������
    /// </summary>
    /// <returns>����ɐ����������ǂ���(0�������Ȃ�)</returns>
    public bool TryConsumeCount()
    {
        if(partStat != PartStat.common || config.easyMode) return true;

        if(currentCount + currentScoreAdditional <= 0) return false;
        else if (currentCount > 0)
        {
            currentCount--;
            ui.ConsumeSlashCell();
        }
        else
        {
            currentScoreAdditional--;
            ui.ConsumeScoreCell();
        }

        return true;
    }

    /// <summary>
    /// �q���g���g��
    /// </summary>
    /// <returns>�]���_������Ȃ���false��Ԃ�</returns>
    public bool TryUseHint()
    {
        if (currentScoreAdditional <= 0) return false;
        else
        {
            currentScoreAdditional--;
            hintUsed++;
            ui.ConsumeScoreCell();
            return true;
        }
    }

    public void SetAnotherPart()
    {
        partStat = PartStat.another;
        ui.SetInfinity(true);
    }

    public void SetGoaled()
    {
        partStat = PartStat.goaled;
        ui.SetInfinity(false);
    }

    public void SetRecommendCount(int max)
    {
        inPuzzle = (max >= 0);
        partStat = PartStat.common;

        this.recommendCount = max;
        currentCount = max;
        currentScoreAdditional = 3;
        hintUsed = 0;
        ui.SetCells(recommendCount, hintUsed, true, inPuzzle);
    }
    public void ResetCount()
    {
        if (partStat != PartStat.common) return;

        currentCount = recommendCount;
        currentScoreAdditional = 3 - hintUsed;
        ui.SetCells(recommendCount, hintUsed, false, inPuzzle);
    }

    public int CalcScore()
    {
        return currentScoreAdditional + currentCount;
    }
}
