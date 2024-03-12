using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Title_menu3 : SetsunaSlashScript
{
    [SerializeField] VideoPlayer vp;
    [SerializeField] Image whiteback;
    [SerializeField] CanvasGroup buttons;
    [SerializeField] AudioSource seAS;
    [SerializeField] AudioClip openSE;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject primeSelect, menu2Prime;
    public float fadeStartTime, fadeTime, whiteBackAlpha;
    public string temp, easy, normal;

    void OnEnable()
    {
        
    }

    void SetPrime(bool easyMode)
    {
        whiteback.color = Color.clear;
        buttons.alpha = 0;
        vp.targetTexture.Release();

        text.text = string.Format(temp.Replace("@@","\n\r"), easyMode ? easy : normal);
    }

    public IEnumerator Open(bool easyMode)
    {
        SetPrime(easyMode);

        vp.Prepare();
        vp.GetComponent<RawImage>().color = Color.clear;
        while (!vp.isPrepared)
        {
            yield return null;
        }
        vp.GetComponent<RawImage>().color = Color.white;
        vp.Play();

        yield return new WaitForSeconds(fadeStartTime);

        seAS.PlayOneShot(openSE);

        if (config.controllMode != ConfigDatas.ControllMode.touch)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primeSelect);
        }  

        float dTime = 0, ratio;
        Color color = Color.white;
        color.a = 0;


        while (dTime <= fadeTime)
        {
            ratio = dTime / fadeTime;

            color.a = ratio * (whiteBackAlpha/255);
            whiteback.color = color;
            buttons.alpha = ratio;

            dTime += Time.deltaTime;
            yield return null;
        }

        color.a = whiteBackAlpha/255;
        whiteback.color = color;
        buttons.alpha = 1;
    }

    public void GameStart()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void Cancel()
    {
        //if ((Gamepad.current != null) && Gamepad.current.enabled)
        if (config.controllMode != ConfigDatas.ControllMode.touch)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(menu2Prime);
            //Debug.Log("setSelection: " + EventSystem.current.currentSelectedGameObject.name);

            foreach (var button in Object.FindObjectsOfType<Button>())
            {
                var colors = button.colors;
                colors.normalColor = button.colors.selectedColor;
                colors.selectedColor = button.colors.normalColor;
                colors.pressedColor = button.colors.normalColor;

                button.colors = colors;
            }
        }

        gameObject.SetActive(false);
    }
}
