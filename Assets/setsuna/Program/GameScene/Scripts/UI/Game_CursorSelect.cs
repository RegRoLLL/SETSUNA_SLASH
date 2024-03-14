using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selectable))]
public class Game_CursorSelect : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] bool onPointerExitDeselect = true;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        this.GetComponent<Selectable>().Select();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if(onPointerExitDeselect)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
