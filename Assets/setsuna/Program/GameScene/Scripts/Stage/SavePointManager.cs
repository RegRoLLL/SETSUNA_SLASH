using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SavePointManager : MonoBehaviour
{
    [SerializeField] List<SavePoint> savePointList = new();

    public void ListSavePoints()
    {
        savePointList.Clear();
        foreach (Transform t in transform)
        {
            if (t.TryGetComponent<SavePoint>(out var point))
            {
                savePointList.Add(point);
            }
        }
    }

    public void ConnectSavePoints()
    {
        foreach (var (point,index) in savePointList.Select((point,index)=>(point,index)))
        {
            if (savePointList.Count <= index + 1) break;

            Debug.Log(point.gameObject.name);

            point.SetNext(savePointList[index + 1]);
        }
    }

    public List<SavePoint.SavePointStatus> GetSavePointsData()
    {
        List<SavePoint.SavePointStatus> result = new();

        foreach (var point in savePointList)
        {
            result.Add(point.GetData());
        }

        return result;
    }

    public void SetSavePointsData(List<SavePoint.SavePointStatus> dataList)
    {
        int index = 0;

        while (index < dataList.Count)
        {
            savePointList[index].SetData(dataList[index]);
        }
    }
}

[CustomEditor(typeof(SavePointManager))]
public class SavePointeManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var instance = target as SavePointManager;

        if (GUILayout.Button("SetSavePointsList"))
        {
            instance.ListSavePoints();
        }

        if (GUILayout.Button("ConnectSavePoints"))
        {
            instance.ConnectSavePoints();
        }
    }
}