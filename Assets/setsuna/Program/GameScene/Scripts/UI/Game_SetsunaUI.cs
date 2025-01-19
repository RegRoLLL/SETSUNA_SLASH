using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Game_SetsunaUI : MonoBehaviour
{
    [HideInInspector] public GameManager gm;

    [SerializeField] Game_menuUI menuUI;
    [SerializeField] SlashCountUI slashCountUI;
    public SlashCountUI SlashCountUI { get => slashCountUI; }

    [SerializeField] GameObject touchController;
    [Space]
    [SerializeField] Image whiteOut;
    [SerializeField] float flashTime;
    [Space]
    [SerializeField] Image titlePanel;
    [SerializeField] TextMeshProUGUI titleTMP;
    [SerializeField] float titleOpenTime, titleStayTime, titleCloseTime;

    void Start()
    {
        menuUI.gameObject.SetActive(false);
        whiteOut.gameObject.SetActive(false);
        gm = EventSystem.current.gameObject.GetComponent<GameManager>();
    }

    public void TogglePauseMenu(bool pause)
    {
        menuUI.gameObject.SetActive(pause);
    }

    public void ToggleTouchController(bool active)
    {
        touchController.SetActive(active);
    }

    public IEnumerator Flash()
    {
        FlashStart();
        yield return StartCoroutine(FlashEnd());
    }
    public IEnumerator Flash(Action whileFlashAction)
    {
        FlashStart();
        whileFlashAction();
        yield return StartCoroutine(FlashEnd());
    }

    void FlashStart()
    {
        whiteOut.gameObject.SetActive(true);

        whiteOut.color = Color.white;
    }
    IEnumerator FlashEnd()
    {
        var color = whiteOut.color;

        float dTime = 0;
        while (dTime <= flashTime)
        {
            color.a = 1 - (dTime / flashTime);

            whiteOut.color = color;

            dTime += Time.unscaledDeltaTime;
            yield return null;
        }

        whiteOut.color = Color.clear;

        whiteOut.gameObject.SetActive(false);
    }

    public IEnumerator TitleCall(string titleName)
    {
        titlePanel.gameObject.SetActive(true);


        titleTMP.text = titleName;

        float dTime = 0;

        titlePanel.fillOrigin = 0;
        while (dTime <= titleOpenTime)
        {
            float ratio = dTime / titleOpenTime;
            float value = Mathf.Sin(ratio * 90 * Mathf.Deg2Rad);

            titlePanel.fillAmount = value;

            dTime += Time.unscaledDeltaTime;
            yield return null;
        }
        titlePanel.fillAmount = 1;



        yield return new WaitForSecondsRealtime(titleStayTime);



        dTime = 0;
        titlePanel.fillOrigin = 1;
        while (dTime <= titleOpenTime)
        {
            float ratio = dTime / titleOpenTime;
            float value = -Mathf.Pow(ratio, 4) + 1;

            titlePanel.fillAmount = value;

            dTime += Time.unscaledDeltaTime;
            yield return null;
        }
        titlePanel.fillAmount = 0;


        titlePanel.gameObject.SetActive(false);
    }
}
