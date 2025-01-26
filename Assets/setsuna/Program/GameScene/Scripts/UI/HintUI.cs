using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using RegUtil;

public class HintUI : MonoBehaviour
{
    [SerializeField] float openCloseTime;
    [SerializeField] Color bgDefColor;
    [SerializeField, Multiline] string hintMenuTextFormatt, scoreNotEnough;
    [SerializeField] Hint displayOnNoHints = new();
    [SerializeField] GameObject hintContainer, hint, hintMenu, closeButton;
    [SerializeField] Button backButton,nextButton;
    [SerializeField] List<Hint> hints = new();

    [Space]
    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] Image hintImg;
    [SerializeField] TextMeshProUGUI hintNumText;
    [SerializeField] TextMeshProUGUI hintMenuText;
    [SerializeField] TextMeshProUGUI scoreNotEnoughText;

    Image bg;
    Player player;

    int displayingIndex;
    Hint displayingHint;

    private void Update()
    {
        if (this.gameObject.activeInHierarchy) RegTimeKeeper.Pause();
    }

    public void SetHints(List<Hint> hintsData)
    {
        hints.Clear();
        hints.AddRange(hintsData);
        if (hintsData.Count == 0)
        {
            hints.Add(displayOnNoHints);
        }
        displayingIndex = 0;
        displayingHint = hints[0];
        Display();
    }

    public void Open(Player pl)
    {
        if (player == null) player = pl;
        if (bg == null) bg = GetComponent<Image>();
        pl.ToggleStopPlayerControll(true);
        gameObject.SetActive(true);
        StartCoroutine(OpenClose(true));
    }
    public void Close()
    {
        player.ToggleStopPlayerControll(false);
        StartCoroutine(OpenClose(false));
    }

    IEnumerator OpenClose(bool open)
    {
        var dt = 0f;
        float lerp;
        Color color = bg.color;
        while (dt <= openCloseTime)
        {
            lerp = dt / openCloseTime;
            hintContainer.transform.localScale = Vector3.one * (open ? lerp : 1 - lerp);
            color.a = bgDefColor.a * (open? lerp : 1 - lerp);
            bg.color = color;
            closeButton.transform.localScale = Vector3.one * (open ? lerp : 1 - lerp);
            closeButton.transform.localEulerAngles = 360 * lerp * Vector3.forward;

            dt += Time.unscaledDeltaTime;
            yield return null;
        }
        hintContainer.transform.localScale = open ? Vector3.one : Vector3.zero;
        closeButton.transform.localScale = open ? Vector3.one : Vector3.zero;
        closeButton.transform.localEulerAngles = Vector3.forward * 360;
        bg.color = open ? bgDefColor : Color.clear;
        gameObject.SetActive(open);
    }

    public void NextBack(bool next)
    {
        displayingIndex += (next ? 1 : -1);
        displayingHint = hints[displayingIndex];
        Display();
    }
    void Display()
    {
        hintNumText.text = $"{displayingIndex + 1}";
        hintText.text = displayingHint.text;
        hintImg.sprite = displayingHint.image;

        if (displayingHint == displayOnNoHints) hintNumText.text = "";

        backButton.targetGraphic.enabled = (displayingIndex != 0);
        nextButton.targetGraphic.enabled = (displayingIndex != hints.Count - 1);
        nextButton.interactable = displayingHint.unLocked;

        hintMenuText.text = string.Format(hintMenuTextFormatt, displayingIndex + 1);
        scoreNotEnoughText.text = "";

        hintMenu.SetActive(!displayingHint.unLocked);
        hint.SetActive(displayingHint.unLocked);
    }

    public void Unlock()
    {
        if (player.Status.UseHint())
        {
            displayingHint.unLocked = true;
            Display();
        }
        else
        {
            scoreNotEnoughText.text = scoreNotEnough;
        }
    }

    [Serializable]
    public class Hint
    {
        public bool unLocked;
        [Multiline] public string text;
        public Sprite image;
    }
}
