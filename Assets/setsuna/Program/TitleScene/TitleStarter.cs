using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Linq;

public class TitleStarter : SetsunaSlashScript
{
    [SerializeField] VideoPlayer vp;
    [SerializeField] Image whiteMask;
    [SerializeField] GameObject menus;
    [SerializeField] List<GameObject> menuList = new List<GameObject>();
    public float fadeStartTime, fadeTime;

    public AudioSource bgmAS;

    void Start()
    {
        whiteMask.color = Color.clear;
        whiteMask.gameObject.SetActive(true);
        menus.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);

        foreach (var n in menuList) n.SetActive(false);

        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        vp.Play();

        yield return new WaitForSeconds(fadeStartTime);

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        float dTime=0;
        Color color = Color.white;
        whiteMask.color = color;

        menus.SetActive(true);
        menuList[0].SetActive(true);

        while (dTime <= fadeTime)
        {
            color.a = 1-(dTime / fadeTime);
            whiteMask.color = color;
            dTime += Time.deltaTime;
            yield return null;
        }
        whiteMask.color = Color.clear;
        whiteMask.gameObject.SetActive(false);

        bgmAS.Play();
    }
}
