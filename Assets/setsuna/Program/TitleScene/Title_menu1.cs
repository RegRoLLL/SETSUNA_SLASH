using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Title_menu1 : SetsunaSlashScript
{
    [SerializeField] Image mask;
    [SerializeField] Title_menu3 menu3;
    public GameObject primeSelect;
    public float openCloseTime;

    void Start()
    {
        mask.fillMethod = Image.FillMethod.Horizontal;
        mask.gameObject.SetActive(false);

        if(config.controllMode != ConfigDatas.ControllMode.touch)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primeSelect);
        }
    }

    public void GotoMenu3()
    {
        menu3.gameObject.SetActive(true);
        menu3.StartCoroutine(menu3.Open(false));
    }

    //�C�[�W�[���[�h�p�~�ɂ�Menu2�p�~
    //public void GotoMenu2()
    //{
    //    StartCoroutine(OpenClose(open: false));
    //}

    public IEnumerator OpenClose(bool open)
    {
        if (open) Start();



        float dTime = 0;
        mask.fillOrigin = open ? 0 : 1;
        mask.fillAmount = open ? 1 : 0;

        mask.gameObject.SetActive(true);


        while (dTime <= openCloseTime)
        {
            if (open) mask.fillAmount = 1 - (dTime / openCloseTime);
            else mask.fillAmount = (dTime / openCloseTime);

            dTime += Time.deltaTime;
            yield return null;
        }
        mask.fillAmount = open ? 0 : 1;
        mask.gameObject.SetActive(!open);


        if (!open)
        {
            gameObject.SetActive(false);

            menu3.gameObject.SetActive(true);
            menu3.StartCoroutine(menu3.Open(false));
        }
    }
}
