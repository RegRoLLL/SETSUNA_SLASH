using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HintUI : MonoBehaviour
{
    [SerializeField] float openCloseTime;
    [SerializeField] Color bgDefColor;
    [SerializeField] GameObject hintContainer, hint, hintMenu, closeButton;
    [SerializeField] Button backButton,nextButton;
    [SerializeField] List<Hint> hints = new();

    [Space]
    [SerializeField] TextMeshProUGUI hintText;
    [SerializeField] Image hintImg;
    [SerializeField] TextMeshProUGUI hintNumText;
    [SerializeField] TextMeshProUGUI hintMenuText;

    Image bg;

    int displayingIndex;
    Hint displayingHint;

    public void SetHints(List<Hint> hintsData)
    {
        hints.Clear();
        hints.AddRange(hintsData);
        displayingIndex = 0;
        displayingHint = hints[0];
        Display();
    }

    public void Open()
    {
        if(bg==null) bg = GetComponent<Image>();
        gameObject.SetActive(true);
        StartCoroutine(OpenClose(true));
    }
    public void Close()
    {
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
            closeButton.transform.localEulerAngles = lerp * Vector3.forward * 360;

            dt += Time.deltaTime;
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

        backButton.targetGraphic.enabled = (displayingIndex != 0);
        nextButton.targetGraphic.enabled = (displayingIndex != hints.Count - 1);
    }

    [Serializable]
    public class Hint
    {
        [Multiline] public string text;
        public Sprite image;
    }
}
