using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerCollisionChecker : SetsunaSlashScript
{
    [SerializeField] protected Vector2 defaultSize, defaultExistents;

    [SerializeField] CheckerRays checkRays;
    public float groundAngle, wallAngle;

    Rigidbody2D rb;
    [HideInInspector]public Collider2D col;

    private List<RaycastHit2D> groundresults, wallresults_R, wallresults_L;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        checkRays.Initialize(this);
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        GetResults();
    }

    public void Initialize()
    {
        defaultSize = col.transform.localScale;
        defaultExistents = col.bounds.extents;

        GetResults();
    }

    private void GetResults()
    {
        groundresults = checkRays.ground.GetRayResults();
        wallresults_R = checkRays.wall.GetRayResults(right: true);
        wallresults_L = checkRays.wall.GetRayResults(right: false);
    }


    public void SizeChange(float ratio_x, float ratio_y)
    {
        var bottomPos = (Vector2)col.transform.position - Vector2.up * col.bounds.extents.y;

        var size = defaultSize;
        size.x *= ratio_x;
        size.y *= ratio_y;
        col.transform.localScale = size;

        col.transform.position = bottomPos + Vector2.up * defaultExistents.y * ratio_y;
    }

    public Vector2 GetGroundNormal(bool addAngle90)
    {
        var rayResults = groundresults;
        Vector2 normals = Vector2.zero;

        foreach(var hit in rayResults)
        {
            if (hit.collider == null) continue;

            var angle = Vector2.SignedAngle(Vector2.up, hit.normal);
            if (MathF.Abs(angle) <= groundAngle)
            {
                normals += hit.normal;
            }
        }

        Debug.DrawRay(transform.position - Vector3.up*col.bounds.extents.y, Quaternion.Euler(0, 0, -90) * (normals * 0.3f), Color.magenta);

        if (addAngle90)
        {
            return Quaternion.Euler(0, 0, -90) * normals.normalized;
        }
        else
        {
            return normals.normalized;
        }
    }

    public Vector2 GetWallNormal(bool right)
    {
        var xPos = col.bounds.center.x;
        var closeHitXdistance = float.MaxValue;
        var results = right ? wallresults_R : wallresults_L;
        Vector2 normal = Vector2.zero;

        foreach (var hit in results)
        {
            if (hit.collider == null) continue;

            var angle = Vector2.SignedAngle(Vector2.up, hit.normal);
            if (MathF.Abs(angle) < wallAngle) continue;
            if (MathF.Abs(angle) > 90) continue;

            var hitXdistance = Math.Abs(hit.point.x - xPos);
            if (hitXdistance >= closeHitXdistance) continue;

            normal = hit.normal;
            closeHitXdistance = hitXdistance;
        }

        return normal.normalized;
    }

    public Vector2 GetHighSlopeNormal()
    {
        bool groundhit, wallhitR, wallhitL;
        Vector2 result = Vector2.zero;

        //groundのrayがhitしているか
        groundhit = (groundresults.Where((h) => (h.collider == null)).ToList().Count != 0);

        //wallのrayがhitしているか
        wallhitR = (wallresults_R.Where((h) => (h.collider == null)).ToList().Count == 0);
        wallhitL = (wallresults_L.Where((h) => (h.collider == null)).ToList().Count == 0);


        List<RaycastHit2D> rayresults;

        if (groundhit){
            rayresults = groundresults;
        }
        else if (wallhitR){
            rayresults = wallresults_R;
        }
        else if (wallhitL){
            rayresults = wallresults_L;
        }
        else return result;


        foreach (var hit in rayresults)
        {
            if (hit.collider == null) continue;

            var angle = Vector2.SignedAngle(Vector2.up, hit.normal);

            if (Mathf.Abs(angle) <= groundAngle) continue;
            if (MathF.Abs(angle) >= wallAngle) continue;

            result += hit.normal;
        }

        return result.normalized;
    }


    public bool IsGrounded()
    {
        bool isGrounded = false;

        var rayResults = groundresults;

        foreach (var hit in rayResults)
        {
            if (hit.collider == null) continue;

            var angle = Vector2.SignedAngle(Vector2.up, hit.normal);
            //Debug.Log($"angle = {angle}");
            if (MathF.Abs(angle) <= groundAngle)
            {
                isGrounded = true;
                break;
            }
        }

        //Debug.Log($"isGrounded = {isGrounded}");

        return isGrounded;
    }

    public bool IsWall(bool right)
    {
        bool isWall = false;

        var results = right ? wallresults_R : wallresults_L;

        foreach (var hit in results)
        {
            if (hit.collider != null)
            {
                var angle = MathF.Abs(Vector2.SignedAngle(Vector2.up, hit.normal));
                if ((angle >= wallAngle) && angle <= 90)
                {
                    isWall = true;
                }
                //Debug.Log($"{(right ? "R" : "L")}_angle = {angle}");
            }
        }
        

        //Debug.Log($"isWall_{(right ? "R" : "L")} = {isWall}");

        return isWall;
    }

    public bool IsHighSlope()
    {
        bool groundhit, wallhitR, wallhitL;

        //groundのrayがhitしているか
        groundhit = (groundresults.Where((h) => (h.collider == null)).ToList().Count != 0);

        //wallのrayがhitしているか
         wallhitR = (wallresults_R.Where((h) => (h.collider == null)).ToList().Count == 0);
         wallhitL = (wallresults_L.Where((h) => (h.collider == null)).ToList().Count == 0);

        //hitしているのにgroundもwallもtrue判定でない場合にtrueを返す
        return ((groundhit || wallhitR || wallhitL) && !(IsGrounded() || IsWall(true) || IsWall(false)));
    }


    [Serializable]
    public class CheckerRays
    {
        public Ground ground;
        public Wall wall; 
        public LayerMask mask;
        [HideInInspector]public Collider2D col;

        public void Initialize(PlayerCollisionChecker pcc)
        {
            col = pcc.col;
            ground.Initialize(this);
            wall.Initialize(this);
        }


        [Serializable]
        public class Ground
        {
            [NonSerialized] CheckerRays outer;

            public Vector2 offsetLocal;
            public int rayCount;
            public float range;
            [HideInInspector] public Collider2D col;

            public void Initialize(CheckerRays outer)
            {
                this.outer = outer;
                col = outer.col;
            }


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
                var hit = Physics2D.Raycast(pos, Vector2.down, range, outer.mask);

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

        [Serializable]
        public class Wall
        {
            [NonSerialized]CheckerRays outer;

            public Vector2 offsetLocal;
            public int rayCount;
            public float range;
            [HideInInspector] public Collider2D col;

            public void Initialize(CheckerRays outer)
            {
                this.outer = outer;
                col = outer.col;
            }


            public List<RaycastHit2D> GetRayResults(bool right)
            {
                var results = new List<RaycastHit2D>();
                var originCenter = (Vector2)col.bounds.center
                                    //+ Vector2.right * col.bounds.extents.x * (right ? 1 : -1)
                                    + (Vector2)col.transform.TransformDirection(offsetLocal);

                if (rayCount == 1)
                {
                    results.Add(RayCast(originCenter, right));
                }
                else if (rayCount > 1)
                {
                    originCenter -= Vector2.up * col.bounds.extents.y;//下端にセンターを移動
                    var step = Vector2.up * col.bounds.size.y / (rayCount - 1);//ray同士の間隔

                    for (int i = 0; i < rayCount; i++)
                    {
                        results.Add(RayCast(originCenter, right));
                        originCenter += step;
                    }
                }

                return results;
            }

            RaycastHit2D RayCast(Vector2 pos, bool right)
            {
                var dir = (right ? 1:-1) * Vector2.right;

                var hit = Physics2D.Raycast(pos, dir, range, outer.mask);
                    
                Color noHit, onHit;
                (noHit, onHit) = right ? (Color.magenta, Color.green) : (Color.yellow, Color.cyan);
                Debug.DrawRay(pos, dir.normalized * range, noHit);
                Debug.DrawRay(pos, dir.normalized * hit.distance, onHit);

                if (hit.collider != null)
                {
                    var direction = (hit.normal * 0.5f);
                    Debug.DrawRay(hit.point, direction, onHit);
                    //Debug.Log(hit.collider.name);
                }

                return hit;
            }
        }
    }
}
