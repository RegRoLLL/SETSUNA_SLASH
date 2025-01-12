using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RME = RegUtil.RegMeshEdit_v3;

public class SlashEffect : SetsunaSlashScript
{
    [SerializeField] SlashType slashType;
    [SerializeField] AudioSource seAS;

    public float speed, density, gravityScale;
    public PhysicsMaterial2D physicsMaterial;

    public float twiceCutErrorRatio;

    bool isDataAlreadySet = false;
    Vector2 startPos, endPos, direction;

    Rigidbody2D rb;
    bool vanishFlag;

    enum SlashType
    {
        fast,charge
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        vanishFlag = false;
    }

    public void SetData(Vector2 start, Vector2 end)
    {
        startPos = start;
        endPos = end;
        direction = (end - start).normalized;
        transform.SetPositionAndRotation(start, Quaternion.FromToRotation(Vector3.up, (end - start)));
        isDataAlreadySet = true;
    }

    
    void Update()
    {
        if (!isDataAlreadySet) return;

        if (slashType == SlashType.fast) FastSlash();
        else if (slashType == SlashType.charge) ChargeSlash();
    }

    void FastSlash()
    {

    }

    void ChargeSlash()
    {
        rb.velocity = direction * speed;

        var delta = (Vector2)transform.position - startPos;

        if (delta.magnitude >= (endPos - startPos).magnitude) Vanish();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (slashType == SlashType.charge)
        {
            if (col.gameObject.layer == not_slashableLayer)
            {
                Vanish();
            }

            //Debug.Log("trigger enter: " + col.gameObject.name + " | " + col.gameObject.layer + " | vanishFrag: " + vanishFlag);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (slashType != SlashType.charge) return;

        if (col.gameObject.layer == slashableLayer) SlashCut(col.gameObject, startPos, transform.position);
    }

    private void OnBecameInvisible()
    {
        //Debug.Log("invisivble");
        Vanish();
    }

    public void Vanish()
    {
        if (vanishFlag) return;

        Destroy(gameObject);
        vanishFlag = true;
    }


    void SlashCut(GameObject obj, Vector2 start, Vector2 end)
    {
        bool isStatic = !obj.GetComponent<Rigidbody2D>();
        Vector2 velocity = isStatic ? Vector2.zero : obj.GetComponent<Rigidbody2D>().velocity;
        Vector3 scale = obj.transform.lossyScale;
        GameObject container = obj.transform.parent.gameObject;

        var results = RME.MeshCut.Cut(obj, start, end);
        if (results == null) return;

        var areaMax = 0f;
        GameObject largestPart = results[0];

        if (results == null) return;

        foreach (var part in results)//切断
        {
            part.transform.localScale = scale;
            part.transform.parent = container.transform;
            part.layer = slashableLayer;

            float area = GetAreaOfPolygon(part.GetComponent<PolygonCollider2D>().points);
            if (area > areaMax)
            {
                areaMax = area;
                largestPart = part;
            }

            part.GetComponent<PolygonCollider2D>().sharedMaterial = physicsMaterial;
        }

        //元オブジェクトが物理じゃない場合
        if (isStatic) results.Remove(largestPart);

        void addPhysics(GameObject part)
        {
            part.AddComponent(typeof(Rigidbody2D));

            var partRB = part.GetComponent<Rigidbody2D>();

            partRB.useAutoMass = true;
            partRB.bodyType = RigidbodyType2D.Dynamic;
            partRB.velocity = velocity;
            partRB.gravityScale = gravityScale;
            StartCoroutine(SetDensityAfterFrame(part.GetComponent<PolygonCollider2D>()));
        }

        if (results.Count == 1)
        {
            var area1 = GetAreaOfPolygon(results[0].GetComponent<PolygonCollider2D>().points);
            var area2 = GetAreaOfPolygon(largestPart.GetComponent<PolygonCollider2D>().points);
            float ratio = area1 / area2;

            //2分割のパーツがほぼ同じ大きさの場合
            if (ratio >= (1 - twiceCutErrorRatio) && ratio <= (1 + twiceCutErrorRatio))
            {
                addPhysics(largestPart);
                addPhysics(results[0]);
            }
            else
            {
                addPhysics(results[0]);
            }
        }
        else
        {
            //最も大きいパーツ以外に物理を適用(元オブジェクトも物理が適用されてたら全部)
            foreach (var part in results)
            {
                addPhysics(part);
            }
        }
    }

    float GetAreaOfPolygon(Vector2[] polygon)
    {
        static float CrossFrom2Vec2(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        float sum = 0;

        for (int i = 0; i < polygon.Length - 1; i++)
            sum += CrossFrom2Vec2(polygon[i], polygon[i + 1]);

        sum += CrossFrom2Vec2(polygon[^1], polygon[0]);

        return Math.Abs(sum) / 2;
    }


    IEnumerator SetDensityAfterFrame(PolygonCollider2D col)
    {
        yield return null;

        col.density = density;
    }
}
