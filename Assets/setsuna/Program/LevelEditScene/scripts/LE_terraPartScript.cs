using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System;

public class LE_terraPartScript : SetsunaSlashScript
{
    [SerializeField] Key deleteMarkerKey;

    [SerializeField] LineRenderer line;
    [SerializeField] GameObject markerPrefab;
    [SerializeField] Transform container;
    [SerializeField] GameObject currentSelected;
    public List<Transform> markersT = new();

    int i = 0;

    void Start()
    {
        
    }

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            InsertVerticle(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log("right clicked!");
        }

        if (Keyboard.current[deleteMarkerKey].wasPressedThisFrame)
        { 
            if(currentSelected) DeleteMarker();
        }
    }

    void LateUpdate()
    {
        markersT = markersT.Where((t) => t != null).ToList();

        if (markersT.Count <= 1) return;

        SetLine();
        SetMarkerAngles();
    }

    public void AddVerticle()
    {
        if (markersT.Count == 0)
        {
            AddVerticle(Vector2.zero);
        }
        else
        {
            var last = markersT.Last();
            AddVerticle(last.position + last.rotation * Vector2.up);
        }
    }
    public void AddVerticle(Vector2 pos)
    {
        var m = Instantiate(markerPrefab, container);
        m.transform.position = pos;
        markersT.Add(m.transform);
        m.name += $"({i++})";

        if (markersT.Count == 1)
        {
            //m.GetComponent<SpriteRenderer>().color = Color.red;
            SetSelectedMarker(m);
        }
        else
        {
            SetSelectedMarker(currentSelected);
        }
    }

    public void InsertVerticle(Vector2 pos)
    {
        int index = markersT.FindIndex((t) => (t.gameObject == currentSelected));
        //Debug.Log(index);
        InsertVerticle(index + 1, pos);
    }
    public void InsertVerticle(int index, Vector2 pos)
    {
        var m = Instantiate(markerPrefab, container);
        m.transform.position = pos;
        m.name += $"({i++})";

        if (markersT.Count==0 || markersT.Count<=index)
        {
            markersT.Add(m.transform);
        }
        else
        {
            markersT.Insert(index, m.transform);
        }
        
        SetSelectedMarker(m);
    }

    void SetLine()
    {
        line.positionCount = markersT.Count() + 1;
        line.SetPositions(markersT.Select((t) => t.position).ToArray());
        line.SetPosition(line.positionCount - 1, markersT[0].position);
    }

    void SetMarkerAngles()
    {
        Action<Transform, Transform> Look = (from, to) =>
        {
            from.rotation = Quaternion.FromToRotation(Vector3.up, (to.position - from.position));
        };

        for (int i = 0; i < markersT.Count-1; i++)
        {
            Look(markersT[i], markersT[i + 1]);
        }

        if (markersT.Count > 2)
            Look(markersT.Last(), markersT[0]);
    }


    public void SetSelectedMarker(GameObject marker)
    {
        currentSelected = markersT.Find((n) => (n.gameObject == marker)).gameObject;

        foreach (var m in markersT)
        {
            m.GetComponent<LE_markerScript>().SetSelect((m.gameObject == currentSelected));
        }
    }


    void DeleteMarker()
    {
        var index = markersT.FindIndex((n) => (n.gameObject == currentSelected));
        var lastMarker = ((index == 0) ? markersT.Last() : markersT[index - 1]).gameObject;

        var deleteMarker = currentSelected;
        SetSelectedMarker(lastMarker);

        //Debug.Log($"{index} | {lastMarker.name}");

        Destroy(deleteMarker);
    }
}
