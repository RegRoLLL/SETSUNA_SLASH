using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using RegUtility;
using System.Linq;

public class TitleManager : SetsunaSlashScript
{
    [SerializeField] GameObject continueWindow;
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] TextMeshProUGUI passNotFoundCaution;
    [SerializeField] string passNotFoundMessage;
    int passNotFoundCount = 0;

    [SerializeField] GameObject continueCheckWindow;
    [SerializeField] TextMeshProUGUI saveDataText;
    [SerializeField] Button continueCheckPrimeSelect;

    [SerializeField] ToggleGroup partToggleGroup;
    [SerializeField] GameObject partTogglePrefab;
    [SerializeField] Button continueCheckDecide, continueCheckCancel;

    [SerializeField] string[] selectedSaveData;

    void Start()
    {
        continueWindow.SetActive(false);
        continueCheckWindow.SetActive(false);
        Cursor.visible = true;
    }

    public void CheckSaveData()
    {
        CheckSaveData(passwordField.text);
    }
    public void CheckSaveData(string password)
    {
        var data = config.saveDatas.Find((n) => n[0] == password);
        bool found = (data != null);
        passNotFoundCaution.gameObject.SetActive(!found);
        passNotFoundCaution.text = $"{passNotFoundMessage}({passNotFoundCount})";

        if (!found) passNotFoundCount++;
        else ContinueCheck(data);

        //Debug.Log(password + " | " + found);
    }

    public void ContinueCheck(string[] data)
    {
        selectedSaveData = data;
        SetLoadedSaveData();

        continueCheckWindow.SetActive(true);

        var loaded = config.loadedSaveData;

        var text = "";
        text += $"{loaded.pass}\r\n";
        text += $"宝石：{loaded.collectedJewel}/{loaded.maxJewel}\r\n";
        text += $"最新エリア：{loaded.latestPartTitle} (part{loaded.latestPart})\r\n";
        text += $"<align=\"center\"><size=64>ー開始位置を選んでくださいー</size>\r\n";

        saveDataText.text = text;

        var parent = partToggleGroup.transform;
        int part = 1;
        foreach (Transform t in parent) Destroy(t.gameObject);
        List<Toggle> toggles = new();
        foreach(var (maxScore,score) in loaded.partScores)
        {
            var t = Instantiate(partTogglePrefab, parent);

            var component = t.GetComponent<Title_PartToggle>();
            component.SetDatas(part, score, maxScore, this);

            var toggle = component.GetToggle();
            toggle.group = partToggleGroup;
            if (part > loaded.latestPart) toggle.interactable = false;

            toggles.Add(toggle);

            part++;
        }
        toggles[loaded.latestPart - 1].isOn = true;

        if (toggles.Count >= 1)
        {
            foreach (var (toggle, index) in toggles.Select((toggle, index) => (toggle, index)))
            {
                Navigation nav = new()
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnRight = toggles[(index + 1 + toggles.Count) % toggles.Count],
                    selectOnLeft = toggles[(index - 1 + toggles.Count) % toggles.Count],
                    selectOnDown = continueCheckDecide
                };
            }
        }

        continueCheckPrimeSelect.Select();
    }

    public void SetContinueCheckButtonsNav(Toggle toggle)
    {
        Navigation decideNav = continueCheckDecide.navigation;
        decideNav.selectOnUp = toggle;
        continueCheckDecide.navigation = decideNav;

        Navigation cancelNav = continueCheckCancel.navigation;
        decideNav.selectOnUp = toggle;
        continueCheckCancel.navigation = cancelNav;
    }

    void SetLoadedSaveData()
    {
        config.loadedSaveData.pass = selectedSaveData[0];
        config.loadedSaveData.latestPart = Convert.ToInt32(selectedSaveData[1]);
        config.loadedSaveData.latestPartTitle = selectedSaveData[2];
        config.loadedSaveData.maxJewel = Convert.ToInt32(selectedSaveData[3]);
        config.loadedSaveData.collectedJewel = Convert.ToInt32(selectedSaveData[4]);
        config.loadedSaveData.jewelsBit = selectedSaveData[5];
        config.loadedSaveData.partScores.Clear();
        for (int i = 6; i < selectedSaveData.Length; i += 2)
        {
            config.loadedSaveData.partScores.Add(
                (
                    maxScore: Convert.ToInt32(selectedSaveData[i]),
                    score: Convert.ToInt32(selectedSaveData[i + 1])
                )
            );
        }
    }

    public void ContinueDecide()
    {
        config.isContinueStart = true;

        SceneManager.LoadScene(gameScene);
    }
}
