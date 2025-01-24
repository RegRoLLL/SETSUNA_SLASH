using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SavePointManager : MonoBehaviour
{
    [SerializeField] bool drawGizmoLine = true;
    [SerializeField, Header("˜A”Ô‚ğ“ü‚ê‚éêŠ‚É{0}‚ğ‘‚­")] string nameFormat = "1-{0}";
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

    public List<SavePoint> GetSavePoints() => savePointList;

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
            index++;
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
            string label;

            if (point.isGoalSave)
            {
                label = "ƒS[ƒ‹";
            }
            else if (point.isAnotherPart){
                label = $"¬•”‰® {point.gameObject.name}";
            }
            else{
                label = $"„§: {data.recommendSlashCount}";
            }

            Color labelColor;

            if (point.isGoalSave)
            {
                labelColor = Color.cyan;
            }
            else if (point.isAnotherPart)
            {
                labelColor = Color.magenta;
            }
            else
            {
                labelColor = Color.green;
            }

            Handles.Label(point.transform.position + Vector3.up,
                          label,
                          new GUIStyle { 
                              fontSize = point.isGoalSave ? 30 : 20,
                              normal={ textColor=labelColor },
                              fontStyle = FontStyle.Bold,
                              alignment = TextAnchor.MiddleCenter
                          });
#endif

            var next = data.nextSave;
            if (next == null) return;
            Gizmos.DrawLine(point.transform.position, next.transform.position);
        }
    }

    public void ConnectSavePoints_InRuntime()
    {
        foreach (var (point, index) in savePointList.Select((point, index) => (point, index)))
        {
            if (savePointList.Count <= index + 1) break;

            point.SetNext(savePointList[index + 1]);
        }
    }

#if UNITY_EDITOR
    public void ConnectSavePoints_InEditor()
    {
        foreach (var (point,index) in savePointList.Select((point,index)=>(point,index)))
        {
            if (savePointList.Count <= index + 1) break;

            var so = new SerializedObject(point);
            so.Update();

            var sp = so.FindProperty("status");
            var spr = sp.FindPropertyRelative("nextSave");
            spr.objectReferenceValue = savePointList[index + 1];

            so.ApplyModifiedProperties();
            Undo.RecordObject(point, "SetComponent Reference");
        }
    }

    public void NameSavePoints_InEditor()
    {
        Undo.RecordObjects(savePointList.Select((point) => point.gameObject).ToArray(), "modify name");

        foreach(var (point, index) in savePointList.Select((point, index) => (point, index)))
        {
            if (savePointList.Last() == point)
            {
                point.gameObject.name = string.Format(nameFormat, "goal");
            }
            else
            {
                point.gameObject.name = string.Format(nameFormat, index + 1);
            }
        }
    }
#endif
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

        if (instance.GetSavePoints().Count > 0)
        {
            if (GUILayout.Button("ConnectSavePoints"))
            {
                instance.ConnectSavePoints_InEditor();
            }

            if (GUILayout.Button("NameSavePointsFromIndex"))
            {
                instance.NameSavePoints_InEditor();
            }
        }
    }
}
#endif