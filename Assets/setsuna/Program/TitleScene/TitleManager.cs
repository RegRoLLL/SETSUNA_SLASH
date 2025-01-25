using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using RegUtility;

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

        string easy = (data[6] == "0") ? "�m�[�}��" : "�C�[�W�[";
        saveDataText.text = $"�p�X���[�h�F{data[0]}\r\nHP�F{data[1]}�@�@ MP�F{data[2]}\r\n�Z�[�u�|�C���g�F({data[3]}, {data[4]})\r\n�G���A�F{Convert.ToInt32(data[5])+1}�@�@{easy}";

        continueCheckPrimeSelect.Select();
    }

    public void ContinueDecide()
    {
        config.loadedSaveData.pass = selectedSaveData[0];
        config.loadedSaveData.maxPart = Convert.ToInt32(selectedSaveData[1]);
        config.loadedSaveData.maxJewel = Convert.ToInt32(selectedSaveData[2]);
        config.loadedSaveData.collectedJewel = Convert.ToInt32(selectedSaveData[3]);
        config.loadedSaveData.partScores.Clear();
        for (int i = 4; i < config.loadedSaveData.partScores.Count; i+=2)
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
