using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;

[RequireComponent(typeof(PolygonCollider2D))]
public class NotSlashablePropertySetter : SetsunaSlashScript
{
    ComputeBuffer vertexBuffer;
    PolygonCollider2D polCol;

    void Start()
    {
        if (vertexBuffer != null) vertexBuffer?.Release();
        if (polCol == null) polCol = GetComponent<PolygonCollider2D>();

        gameObject.layer = not_slashableLayer;

        vertexBuffer = new(polCol.points.Length, sizeof(float) * 2);
        vertexBuffer.SetData(polCol.points);
        var mat = GetComponent<MeshRenderer>().material;
        mat.SetBuffer("_Verticles", vertexBuffer);
        mat.SetInt("_VertexCount", polCol.points.Length);
    }

    private void OnDestroy()
    {
        vertexBuffer?.Release();
    }
}
