using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Title_CursorSelect : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    void IPointerEnterHandler.OnPointerEnter(PointerEventData e)
    {
        this.GetComponent<Button>().Select();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData e)
    {
        if (this.GetComponentInParent<Title_menu1>() is Title_menu1 menu1)
        {
            menu1.primeSelect.GetComponent<Button>().Select();
        }
        else if (this.GetComponentInParent<Title_menu2>() is Title_menu2 menu2)
        {
            menu2.primeSelect.GetComponent<Button>().Select();
        }
        else if(this.GetComponentInParent<Title_menu3>() is Title_menu3 menu3)
        {
            menu3.primeSelect.GetComponent<Button>().Select();
        }
    }
}
