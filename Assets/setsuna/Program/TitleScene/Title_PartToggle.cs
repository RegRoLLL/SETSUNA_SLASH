using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Title_PartToggle : SetsunaSlashScript
{
    [SerializeField] TextMeshProUGUI partTMP, scoreTMP;

    Toggle toggle;
    int part;
    TitleManager tm;

    public void SetDatas(int part, int score, int scoreMax, TitleManager tm)
    {
        this.part = part;
        partTMP.text = $"{part}";
        scoreTMP.text = $"{score}/{scoreMax}";
        this.tm = tm;
    }

    public Toggle GetToggle()
    {
        if (toggle == null) toggle = GetComponentInChildren<Toggle>();

        return toggle;
    }

    public void OnToggleChanged()
    {
        if (toggle == null) toggle = GetComponentInChildren<Toggle>();

        Debug.Log(toggle.isOn);

        if (!toggle.isOn) return;

        config.loadedSaveData.latestPart = part;
        tm.SetContinueCheckButtonsNav(toggle);
    }
}
