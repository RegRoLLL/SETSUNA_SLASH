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
            return; //�O��t���[���ƕω����Ȃ���Ύ��s���Ȃ�
        }
        lastFrameAspect = currentAspect;

        Fix();
    }

    private void Fix()
    {
        //��������{��
        float targetAspect = baseHeight / baseWidth;

        if (currentAspect > targetAspect)//�傫���قǏc��
            cam.orthographicSize = 5 * (currentAspect / targetAspect);//���̂��܂��������͂킩���ĂȂ��B�v�����B
        else
            cam.orthographicSize = baseOrthoGraphicSize;

        Debug.Log("viewport fixed.");
    }
}

