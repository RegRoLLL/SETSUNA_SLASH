using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;

[RequireComponent(typeof(PolygonCollider2D))]
public class NotSlashablePropertySetter : SetsunaSlashScript
{
    [SerializeField] bool fixScales = false;
    [SerializeField] bool reverseLineCoefficient = false;
    [SerializeField] bool overrideLineSettings = false;
    [SerializeField] float overrideLineDepth;
    [SerializeField] float overrideLineThickness;

    ComputeBuffer vertexBuffer;
    PolygonCollider2D polCol;
    MeshRenderer mr;
    MeshFilter mf;

    void Start()
    {
        DataInit();
        if(fixScales) FixScales();
        SetProps();
    }

    void DataInit()
    {
        if (vertexBuffer != null) vertexBuffer?.Release();
        if (polCol == null) polCol = GetComponent<PolygonCollider2D>();
        if (mr == null) mr = GetComponent<MeshRenderer>();
        if (mf == null) mf = GetComponent<MeshFilter>();
    }

    void FixScales()
    {
        var mesh = mf.mesh;

        var newVertices = new Vector3[mesh.vertexCount];
        var newPolygon = new Vector2[polCol.points.Length];

        //頂点位置の修正
        for (int i=0;i< mesh.vertexCount; i++)
        {
            newVertices[i] = GetFixedVec(mesh.vertices[i]);
        }
        mesh.vertices = newVertices;
        mesh.RecalculateBounds();

        //コライダーの修正
        for (int i = 0; i < polCol.points.Length; i++)
        {
            newPolygon[i] = GetFixedVec(polCol.points[i]);
        }
        polCol.points = newPolygon;

        transform.localScale = Vector3.one;
    }
    Vector2 GetFixedVec(Vector2 vec)
    {
        var scale = transform.localScale;

        vec.x *= scale.x;
        vec.y *= scale.y;

        return vec;
    }
    Vector3 GetFixedVec(Vector3 vec)
    {
        var scale = transform.localScale;

        vec.x *= scale.x;
        vec.y *= scale.y;
        vec.z *= scale.z;

        return vec;
    }

    void SetProps()
    {
        gameObject.layer = not_slashableLayer;

        var polygon = polCol.points;
        vertexBuffer = new(polygon.Length, sizeof(float) * 2);
        vertexBuffer.SetData(polygon);

        var mat = mr.material;
        mat.SetBuffer("_Verticles", vertexBuffer);
        mat.SetInt("_VertexCount", polCol.points.Length);

        if (reverseLineCoefficient)
        {
            if (overrideLineSettings)
            {
                overrideLineDepth *= -1;
                overrideLineThickness *= -1;
            }
            else
            {
                overrideLineDepth = -mat.GetFloat("_LineDepth");
                overrideLineThickness = -mat.GetFloat("_LineThickness");
                overrideLineSettings = true;
            }
        }

        if (overrideLineSettings)
        {
            mat.SetFloat("_LineDepth", overrideLineDepth);
            mat.SetFloat("_LineThickness", overrideLineThickness);
        }
    }

    private void OnDestroy()
    {
        vertexBuffer?.Release();
    }
}
