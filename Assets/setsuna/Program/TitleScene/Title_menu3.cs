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
    public GameObject primeSelect, menu1Prime;
    public float fadeStartTime, fadeTime, whiteBackAlpha;
    public string sentence;

    void OnEnable()
    {
        
    }

    void SetPrime(bool easyMode)
    {
        whiteback.color = Color.clear;
        buttons.alpha = 0;
        vp.targetTexture.Release();

        text.text = sentence;
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
            EventSystem.current.SetSelectedGameObject(menu1Prime);
            //Debug.Log("setSelection: " + EventSystem.current.currentSelectedGameObject.name);
        }

        gameObject.SetActive(false);
    }
}
