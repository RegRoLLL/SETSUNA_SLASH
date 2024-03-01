using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class GroundChecker_v2 : SetsunaSlashScript
{
    public CheckerRays checkRays = new();
    public float groundAngle, wallAngle;
    public Vector2 normalAverage, wallNormal;
    public bool isGrounded, isWall_R, isWall_L, isHighSlope;

    private bool lastFrameGrounded;

    [SerializeField] PlayerController_main plctrl;

    Rigidbody2D rb;
    [HideInInspector] public Collider2D col;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        checkRays.col = col;
    }

    // Update is called once per frame
    void Update()
    {
        lastFrameGrounded = isGrounded;

        var rayResults = checkRays.GetRayResults();

        ResetPropaties();
        foreach (var hit in rayResults)
        {
            if (hit.collider == null) continue;

            var angle = Vector2.SignedAngle(Vector2.up, hit.normal);
            if (MathF.Abs(angle) <= groundAngle)
            {
                isGrounded = true;
                normalAverage += hit.normal;
            }
            else
            {
                ((angle > 0) ? ref isWall_R : ref isWall_L) = true;
                wallNormal += hit.normal;
            }

            //Debug.Log(angle);
        }
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -90) * (normalAverage * 0.5f), Color.magenta,0.5f);
        //Debug.Log(normalAverage);

        normalAverage.Normalize();
        wallNormal.Normalize();

        if (!lastFrameGrounded && (isGrounded)) plctrl.Chakuchi();
        else if (plctrl.jumped && plctrl.rb.velocity.y <= 0) plctrl.Chakuchi();
    }

    void ResetPropaties()
    {
        isGrounded = false;
        isWall_R = false;
        isWall_L = false;
        isHighSlope = false;
        normalAverage = Vector2.zero;
        wallNormal = Vector2.zero;
    }


    [Serializable]
    public class CheckerRays
    {
        public Vector2 offsetLocal;
        public int rayCount;
        public float range;
        public LayerMask mask;

        [HideInInspector] public Collider2D col;

        public List<RaycastHit2D> GetRayResults()
        {
            var results = new List<RaycastHit2D>();
            var originCenter = (Vector2)col.bounds.center - Vector2.up * col.bounds.extents.y
                                + (Vector2)col.transform.TransformDirection(offsetLocal);

            if (rayCount == 1)
            {
                results.Add(RayCast(originCenter));
            }
            else if (rayCount > 1)
            {
                originCenter -= Vector2.right * col.bounds.extents.x;//左端にセンターを移動
                var step = Vector2.right * col.bounds.size.x / (rayCount - 1);//ray同士の間隔

                for (int i = 0; i < rayCount; i++)
                {
                    results.Add(RayCast(originCenter));
                    originCenter += step;
                }
            }

            return results;
        }

        RaycastHit2D RayCast(Vector2 pos)
        {
            var hit = Physics2D.Raycast(pos, Vector2.down,range,mask);

            //Debug.Log(hit.collider);


            Debug.DrawRay(pos, Vector2.down * range, Color.red);
            Debug.DrawRay(pos, Vector2.down * hit.distance, Color.cyan);

            if (hit.collider != null)
            {
                var direction = Quaternion.Euler(0, 0, -90) * (hit.normal * 0.5f);
                Debug.DrawRay(hit.point, direction, Color.yellow);
            }

            return hit;
        }
    }
}
