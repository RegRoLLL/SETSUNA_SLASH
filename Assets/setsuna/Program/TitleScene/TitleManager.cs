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

        continueCheckWindow.SetActive(true);

        var text = "";
        text += $"{data[0]}\r\n";
        text += $"宝石：{data[4]}/{data[3]}\r\n";
        text += $"最新エリア：{data[2]} (part{data[1]})\r\n";
        text += $"スコア：\r\n";

        int part = 1;
        for(int i = 6;i<data.Length;i+=2)
        {
            text += $"part{part++} {data[i+1]} / {data[i]}   ";
        }

        saveDataText.text = text;

        continueCheckPrimeSelect.Select();
    }

    public void ContinueDecide()
    {
        config.loadedSaveData.pass = selectedSaveData[0];
        config.loadedSaveData.latestPart = Convert.ToInt32(selectedSaveData[1]);
        config.loadedSaveData.latestPartTitle = selectedSaveData[2];
        config.loadedSaveData.maxJewel = Convert.ToInt32(selectedSaveData[3]);
        config.loadedSaveData.collectedJewel = Convert.ToInt32(selectedSaveData[4]);
        config.loadedSaveData.jewelsBit = selectedSaveData[5];
        config.loadedSaveData.partScores.Clear();
        for (int i = 6; i < config.loadedSaveData.partScores.Count; i+=2)
        {
            config.loadedSaveData.partScores.Add(
                (
                    maxScore: Convert.ToInt32(selectedSaveData[i]),
                    score: Convert.ToInt32(selectedSaveData[i+1])
                )
            );
        }
        config.isContinueStart = true;

        SceneManager.LoadScene(gameScene);
    }
}
