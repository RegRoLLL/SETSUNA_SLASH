using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hanshin : SetsunaSlashScript
{
    [SerializeField] private List<GameObject> predefinedObjects; // 事前に設定する表示用オブジェクトリスト
    [SerializeField] private float bladeTimer = 0f; // Blade が触れている時間
    [SerializeField] private float mincount;
    private int objectIndex = 0; // 次に表示するオブジェクトのインデックス

    private void Start()
    {
        // 事前に設定されたオブジェクトを全て非表示にする
        foreach (var obj in predefinedObjects)
        {
            obj.SetActive(false);
        }
    }

    private void Update()
    {       
            bladeTimer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == bladeLayer)
        {
            bladeTimer = 0f; // タイマーをリセット

        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == bladeLayer)
        {   

            if (bladeTimer >= mincount) 
            {
                ShowNextObject();
            }

        }
    }


    private void ShowNextObject()
    {
        if (predefinedObjects.Count == 0) return; // オブジェクトリストが空なら何もしない

        // 次のオブジェクトを表示
        predefinedObjects[objectIndex].SetActive(true);

        objectIndex = (objectIndex + 1) % predefinedObjects.Count;
    }
}
