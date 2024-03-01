using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class LE_markerScript : MonoBehaviour
{
    [SerializeField] bool isSelected;
    [SerializeField] float adjustSens, minScale, maxScale;
    [SerializeField] SpriteRenderer selectedMark;

    LE_terraPartScript terra;

    void Start()
    {
        terra = GetComponentInParent<LE_terraPartScript>();
    }

    void Update()
    {
        if (Mouse.current.scroll.IsActuated() && Keyboard.current.ctrlKey.isPressed)
        {
            AdjustScale();
        }
    }

    public void OnMouseDown()
    {
        terra.SetSelectedMarker(gameObject);
        Debug.Log("OnClick");
    }

    public void SetSelect(bool status)
    {
        selectedMark.gameObject.SetActive(status);
        isSelected = status;
    }


    void AdjustScale()
    {
        var value = Mouse.current.scroll.value.y;

        var ratio = transform.localScale.magnitude / transform.lossyScale.magnitude;

        value = value * ratio * adjustSens;

        transform.localScale += Vector3.one * value * Time.deltaTime;

        float root3 = Mathf.Sqrt(3f);

        if(transform.localScale.x < 0)
            transform.localScale = Vector3.one * minScale;
        else if (transform.localScale.magnitude/root3 < minScale)
            transform.localScale = Vector3.one * minScale;
        else if (transform.localScale.magnitude/root3 > maxScale)
            transform.localScale = Vector3.one * maxScale;
    }
}
