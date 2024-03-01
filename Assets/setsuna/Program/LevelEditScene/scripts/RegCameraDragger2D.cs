using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Camera))]
public class RegCameraDragger2D : MonoBehaviour
{
    public bool requireCtrlKey;

    [SerializeField] float zoomSens, zoomTranslateSens,
                           dragSens, dragMin;

    Func<Camera> cam = () => Camera.main;

    void Start()
    {
        
    }

    void Update()
    {
        if (Mouse.current.scroll.IsActuated())
        {
            if (requireCtrlKey ? !Keyboard.current.ctrlKey.isPressed : true) Zoom();
        }

        if (Mouse.current.middleButton.isPressed)
        {
            Drag();
        }
    }

    void Zoom()
    {
        float value = 0f;
        value += Mouse.current.scroll.down.ReadValue();
        value -= Mouse.current.scroll.up.ReadValue();
        value *= Time.deltaTime * zoomSens;

        if (cam().orthographicSize + value > 0) cam().orthographicSize += value;


        Vector2 distance = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - cam().transform.position);
        distance *= (value > 0) ? -1 : 1;

        cam().transform.Translate(distance * Time.deltaTime * zoomTranslateSens);
    }


    void Drag()
    {
        var delta = Mouse.current.delta.value;

        if (Mathf.Approximately(0, delta.magnitude)) return;

        if (delta.magnitude <= dragMin) return;

        Debug.Log(delta);

        cam().transform.Translate(delta * -dragSens * Time.deltaTime);
    }
}
