using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selectable))]
public class Game_CursorSelect : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] bool onPointerExitDeselect = true;
    Selectable selectable;

    void Start()
    {
        selectable = GetComponent<Selectable>();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        selectable.Select();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if(onPointerExitDeselect)
            if(EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
    }
}
