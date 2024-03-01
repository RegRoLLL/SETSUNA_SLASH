using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class ScrollBG : MonoBehaviour
{
    /*
     * アタッチするImageのシェーダーはUnlit/Transparentにすること。
     */
    
    [SerializeField] BG_type bg_type;
    BG_type lastFrameBG_Type;

    [SerializeField] Transform traceTarget;

    [SerializeField] FarBackGround far;
    [SerializeField] NearBackGround near;

    Image image;
    Canvas canvas;
    Vector2 lastFramePosition, texturePos;
    Material mat;
    Vector2 defaultObjScale, defaultTextureScale;
    [SerializeField]string texPropName = "_MainTex";


    [SerializeField] enum BG_type { far,near }



    private void Start()
    {
        image = GetComponent<Image>();
        canvas = image.canvas;

        texturePos = Vector2.zero;
        mat = image.material;
        mat.SetTextureOffset(texPropName, texturePos);
        defaultTextureScale = mat.GetTextureScale(texPropName);
        defaultObjScale = transform.localScale;

        Initialize();

        lastFrameBG_Type = bg_type;
        lastFramePosition = traceTarget.position;
    }

    void Initialize()
    {
        if (bg_type == BG_type.far) far.Initialize(this);
        else if (bg_type == BG_type.near) near.Initialize(this);
    }


    private void LateUpdate()
    {
        if (bg_type != lastFrameBG_Type)
        {
            Initialize();
            lastFrameBG_Type = bg_type;
        }


        if (bg_type == BG_type.far) far.Scroll();
        else if (bg_type == BG_type.near) near.Scroll();
    }

    [Serializable]
    public class FarBackGround
    {
        ScrollBG outer;

        [SerializeField] Vector2 offset, scrollSPD;

        public void Initialize(ScrollBG outer)
        {
            this.outer = outer;
            outer.canvas.worldCamera = Camera.main;
            outer.canvas.renderMode = RenderMode.ScreenSpaceCamera;

            outer.mat.SetTextureScale(outer.texPropName, outer.defaultTextureScale);
            outer.transform.localScale = outer.defaultObjScale;
        }

        public void Scroll(){
            outer.ScrollMat(scrollSPD, offset);
        }
    }


    [Serializable]
    public class NearBackGround
    {
        ScrollBG outer;

        [SerializeField] Vector2 offset;

        [SerializeField] Vector2 rect;
        Vector3 startPos;

        public void Initialize(ScrollBG outer)
        {
            this.outer = outer;

            outer.canvas.worldCamera = Camera.main;
            outer.canvas.renderMode = RenderMode.WorldSpace;

            outer.mat.SetTextureScale(outer.texPropName, outer.defaultTextureScale * 3);
            outer.transform.localScale = outer.defaultObjScale * 3;

            startPos = outer.transform.position;
            rect = outer.transform.TransformVector(outer.image.rectTransform.sizeDelta / 2) / 3f;
        }


        public void Scroll(){
            Vector2 camPos = Camera.main.transform.position;
            var dir = Vector3.zero;

            if (camPos.x > startPos.x + rect.x){
                dir.x += rect.x;
                startPos.x += rect.x;
            }
            else if (camPos.x < startPos.x - rect.x)
            {
                dir.x -= rect.x;
                startPos.x -= rect.x;
            }

            if (camPos.y > startPos.y + rect.y)
            {
                dir.y += rect.y;
                startPos.y += rect.y;
            }
            else if (camPos.y < startPos.y - rect.y)
            {
                dir.y -= rect.y;
                startPos.y -= rect.y;
            }


            outer.transform.position += dir;
        }
    }


    public void ScrollMat(Vector2 scrollSPD, Vector2 offset)
    {
        Vector2 pos = traceTarget.position;

        if (pos == lastFramePosition) return;

        var direction = pos - lastFramePosition;
        var spd = scrollSPD * 1e+0f;
        direction.x *= spd.x;
        direction.y *= spd.y;
        
        //if (Mathf.Abs(direction.x) < minDirection.x) direction.x = 0;
        //if (Mathf.Abs(direction.y) < minDirection.y) direction.y = 0;

        texturePos += direction;
        mat.SetTextureOffset(texPropName, texturePos + offset);

        lastFramePosition = pos;
    }



    [ContextMenu("reset")]
    public void ResetMat()
    {
        texturePos = Vector2.zero;

        if (mat)
        {
            mat.SetTextureOffset(texPropName, Vector2.zero);
            mat.SetTextureScale(texPropName, defaultTextureScale);
        }
    }


    private void OnDestroy()
    {
        ResetMat();
    }
}
