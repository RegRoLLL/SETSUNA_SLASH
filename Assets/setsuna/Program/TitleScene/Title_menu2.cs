using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Title_menu2 : SetsunaSlashScript
{
    [SerializeField] List<Image> masks = new List<Image>();
    [SerializeField] GameObject primeSelect;
    public float openCloseTime;
    public Title_menu1 menu1;
    public Title_menu3 menu3;

    void Start()
    {

    }

    public IEnumerator OpenClose(bool open)
    {
        if (open && (config.controllMode != ConfigDatas.ControllMode.touch))
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primeSelect);
        }

        //�B��(open) or �J��(close)
        foreach (var mask in masks)
        {
            mask.fillOrigin = open ? 0 : 1;
            mask.fillAmount = open ? 1 : 0;

            mask.gameObject.SetActive(true);
        }

        float dTime = 0;

        //�X���C�h�I�[�v�� or �N���[�Y
        while (dTime <= openCloseTime)
        {
            if (open)
            {
                foreach (var mask in masks)
                    mask.fillAmount = 1 - (dTime / openCloseTime);
            }
            else
            {
                foreach (var mask in masks)
                    mask.fillAmount = (dTime / openCloseTime);
            }

            dTime += Time.deltaTime;
            yield return null;
        }

        //�}�X�N������(open) or �t���W�J(close)
        foreach (var mask in masks)
        {
            mask.fillAmount = open ? 0 : 1;
            mask.gameObject.SetActive(!open);
        }


        if (!open)
        {
            gameObject.SetActive(false);
            menu1.StartCoroutine(menu1.OpenClose(open: true));
        }
    }


    public void GotoMenu1()
    {
        menu1.gameObject.SetActive(true);
        StartCoroutine(OpenClose(open: false));
    }


    public void GotoMenu3(bool easyMode)
    {
        menu3.gameObject.SetActive(true);
        menu3.StartCoroutine(menu3.Open(easyMode));
    }
}
