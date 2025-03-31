using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image)), DefaultExecutionOrder(-1)]
public class ScrollBG : MonoBehaviour,IDisposable
{
    /*
     * アタッチするImageのシェーダーはUnlit/Transparentにすること。
     */

    [SerializeField] BG_type bg_type;
    BG_type lastFrameBG_Type;

    [SerializeField] FarBackGround far;
    [SerializeField] NearBackGround near;

    Transform traceTarget;
    Image image;
    Canvas canvas;
    Vector2 lastFramePosition, texturePos;
    Material mat;
    Vector2 defaultObjScale, defaultTextureScale;

    [SerializeField]string texPropName = "_MainTex";

    Game_HubScript hub;

    [SerializeField] enum BG_type { far,near }



    private void Start()
    {
        image = GetComponent<Image>();
        canvas = image.canvas;

        traceTarget = Camera.main.transform;

        texturePos = Vector2.zero;
        mat = new Material(image.material);
        image.material = mat;
        mat.SetTextureOffset(texPropName, texturePos);
        defaultTextureScale = mat.GetTextureScale(texPropName);
        defaultObjScale = transform.localScale;

        hub = EventSystem.current.GetComponent<Game_HubScript>();

        Initialize();
    }

    public void Initialize()
    {
        //Debug.Log($"{gameObject.name}.Initialize()");

        if (bg_type == BG_type.far) far.Initialize(this);
        else if (bg_type == BG_type.near) near.Initialize(this);

        lastFrameBG_Type = bg_type;


        if (bg_type != BG_type.far) return;

        if (hub.currentPart is StagePart part and not null)
        {
            far.SetOffsetOverwrite(part.bgFarOffsetOverwrite, part.overwriteFarOffset);
            lastFramePosition = part.savePoints.GetSavePoints()[0].transform.position;
            //Debug.DrawLine(lastFramePosition, traceTarget.position, Color.red, 5f);
            //Debug.Log($"set offset:{(lastFramePosition-(Vector2)traceTarget.position).magnitude}");
        }
        else
        {
            lastFramePosition = traceTarget.transform.position;
        }

        texturePos = Vector2.zero;
        far.Scroll();
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
        [SerializeField] bool offsetOverwrite;
        [SerializeField] Vector2 overwritedOffset;

        public void Initialize(ScrollBG outer)
        {
            this.outer = outer;
            outer.canvas.worldCamera = Camera.main;
            outer.canvas.renderMode = RenderMode.ScreenSpaceCamera;

            outer.mat.SetTextureScale(outer.texPropName, outer.defaultTextureScale);
            outer.transform.localScale = outer.defaultObjScale;
        }

        public void SetOffsetOverwrite(bool overwrite, Vector2 _offset)
        {
            offsetOverwrite = overwrite;
            overwritedOffset = _offset;
        }

        public void Scroll(){
            outer.ScrollMat(scrollSPD, offsetOverwrite? overwritedOffset : offset);
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

            if (outer.canvas.worldCamera == null)
            {
                outer.canvas.worldCamera = Camera.main;
            }
            outer.canvas.renderMode = RenderMode.WorldSpace;

            startPos = outer.transform.position;
            rect = outer.transform.TransformVector(outer.image.rectTransform.sizeDelta);

            rect.x /= outer.mat.mainTextureScale.x;
            rect.y /= outer.mat.mainTextureScale.y;

            outer.mat.SetTextureScale(outer.texPropName, outer.defaultTextureScale * 3);
            outer.transform.localScale = outer.defaultObjScale * 3;
        }


        public void Scroll(){
            Vector2 targetPos = outer.traceTarget.position;
            var dir = Vector3.zero;

            if (targetPos.x > startPos.x + rect.x){
                dir.x += rect.x;
                startPos.x += rect.x;
            }
            else if (targetPos.x < startPos.x - rect.x)
            {
                dir.x -= rect.x;
                startPos.x -= rect.x;
            }

            if (targetPos.y > startPos.y + rect.y)
            {
                dir.y += rect.y;
                startPos.y += rect.y;
            }
            else if (targetPos.y < startPos.y - rect.y)
            {
                dir.y -= rect.y;
                startPos.y -= rect.y;
            }

            //Debug.Log($"nearScroll:{dir}");
            outer.transform.position += dir;
        }
    }

    /// <summary>
    /// far側でのみ使用
    /// </summary>
    public void ScrollMat(Vector2 scrollSPD, Vector2 offset)
    {
        Vector2 pos = traceTarget.position;

        if (pos == lastFramePosition) return;

        var direction = pos - lastFramePosition;
        var spd = scrollSPD * 1e-2f;
        direction.x *= spd.x;
        direction.y *= spd.y;

        texturePos += direction;
        mat.SetTextureOffset(texPropName, texturePos + offset);

        lastFramePosition = pos;
    }

    //===========================

    void ResetMat()
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

    void IDisposable.Dispose()
    {
        ResetMat();
    }
}
