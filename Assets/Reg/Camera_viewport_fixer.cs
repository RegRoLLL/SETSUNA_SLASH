using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_viewport_fixer : MonoBehaviour
{
    public float pixelPerUnit = 100,
                 baseHeight = 1080,
                 baseWidth = 1920,
                 baseOrthoGraphicSize = 5;
    Camera cam;
    float lastFrameAspect, currentAspect;

    public bool isAlwaysUpdate = true;

    void Start()
    {
        cam = this.GetComponent<Camera>();
        lastFrameAspect = 0;
        Fix();
    }

    [ExecuteAlways]
    void Update()
    {
        if (!isAlwaysUpdate) return;

        currentAspect = (float)Screen.height / (float)Screen.width;

        if (Mathf.Approximately(currentAspect, lastFrameAspect))
        {
            return; //前回フレームと変化がなければ実行しない
        }
        lastFrameAspect = currentAspect;

        Fix();
    }

    private void Fix()
    {
        //こっから本番
        float targetAspect = baseHeight / baseWidth;

        if (currentAspect > targetAspect)//大きいほど縦長
            cam.orthographicSize = 5 * (currentAspect / targetAspect);//何故うまくいくかはわかってない。要研究。
        else
            cam.orthographicSize = baseOrthoGraphicSize;

        Debug.Log("viewport fixed.");
    }
}

