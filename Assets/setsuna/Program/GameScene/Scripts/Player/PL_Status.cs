using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PL_Status:SetsunaSlashScript
{
    [SerializeField] int recommendCount, currentCount, currentScoreAdditional;
    [SerializeField] int hintUsed;

    Player pl;
    SlashCountUI ui;

    void Start()
    {
        pl = GetComponent<Player>();
        ui = pl.ui.SlashCountUI;

        if(!config.debugMode){
            hintUsed = 0;
            recommendCount = 0;
            currentCount = 0;
            currentScoreAdditional = 0;
        }
    }

    /// <summary>
    /// éaåÇâÒêîÇè¡îÔÇ∑ÇÈ
    /// </summary>
    /// <returns>è¡îÔÇ…ê¨å˜ÇµÇΩÇ©Ç«Ç§Ç©(0Çâ∫âÒÇÍÇ»Ç¢)</returns>
    public bool ConsumeCount()
    {
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

    public void SetRecomendCount(int max)
    {
        this.recommendCount = max;
        currentCount = max;
        currentScoreAdditional = 3;
        hintUsed = 0;
        ui.SetCells(recommendCount, hintUsed, true);
    }
    public void ResetCount()
    {
        currentCount = recommendCount;
        currentScoreAdditional = 3 - hintUsed;
        ui.SetCells(recommendCount, hintUsed, false);
    }

    public int CalcScore()
    {
        return currentScoreAdditional + currentCount;
    }
}
