using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SavePointManager : MonoBehaviour
{
    [SerializeField] bool drawGizmoLine = true;
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

    private void OnDrawGizmos()
    {
        if (!drawGizmoLine) return;

        Gizmos.color = Color.cyan;

        foreach (var point in savePointList)
        {
            Gizmos.DrawWireSphere(point.transform.position, 1f);

            var data = point.GetData();

#if UNITY_EDITOR
            var recommend = data.recommendSlashCount;
            Handles.Label(point.transform.position + Vector3.up,
                          $"„§:{recommend}",
                          new GUIStyle { 
                              fontSize = 20,
                              normal={ textColor=Color.green },
                              fontStyle = FontStyle.Bold,
                              alignment = TextAnchor.MiddleCenter
                          });
#endif

            var next = data.nextSave;
            if (next == null) return;
            Gizmos.DrawLine(point.transform.position, next.transform.position);
        }
    }
}

#if UNITY_EDITOR
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
#endif