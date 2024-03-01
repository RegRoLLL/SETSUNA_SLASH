using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Title_Cursor : SetsunaSlashScript
{
    [SerializeField] Vector2 fixDirection;

    [SerializeField] GameObject selectedCache;
    Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == selectedCache) return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(selectedCache);
        }

        selectedCache = EventSystem.current.currentSelectedGameObject;

        image.enabled = (selectedCache != null);
        image.enabled = (config.controllMode != ConfigDatas.ControllMode.touch);

        if (!image.enabled) return;

        var rectT = selectedCache.GetComponent<RectTransform>();
        var dir = (-Vector3.right * rectT.rect.width / 200) + (Vector3)fixDirection;

        transform.position = selectedCache.GetComponent<RectTransform>().position + dir;
    }
}
