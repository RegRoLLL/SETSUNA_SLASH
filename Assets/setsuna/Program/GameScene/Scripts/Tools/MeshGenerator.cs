using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using RSE = RegUtil.RegSpriteEditing;

public class MeshGenerator : SetsunaSlashScript
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] Material material;
    [SerializeField] string objectname = "meshObject";
    [SerializeField] string SaveFolderPath = "Assets/";
    [SerializeField] string filename = "meshAsset";

    [SerializeField][ContextMenuItem("clear","ClearList")] List<MeshFilter> meshObjects = new List<MeshFilter>();

    [ContextMenu("Generate Mesh")]
    void GenerateMeshObject()
    {
        int i = 0;

        foreach (var sprite in sprites)
        {
            var objects = RSE.Convert.SpriteToMeshObjects(sprite, material, transform.position);

            foreach (var obj in objects)
            {
                obj.name = objectname + "_" + i.ToString();
                i++;
            }

            meshObjects.AddRange(objects.ConvertAll((obj) => obj.GetComponent<MeshFilter>()));
        }
    }

#if UNITY_EDITOR

    [ContextMenu("SaveMesh")]
    void SaveMeshAsAsset()
    {
        string path;
        int i = 0;

        foreach (var meshFilter in meshObjects)
        {
            path = SaveFolderPath + filename + "_" + i.ToString() + ".asset";

            AssetDatabase.CreateAsset(meshFilter.mesh, path);
            AssetDatabase.SaveAssets();

            i++;
        }
    }

#endif

    void ClearList()
    {
        meshObjects.Clear();
    }
}
