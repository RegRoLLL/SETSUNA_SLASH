using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : SetsunaSlashScript
{
    public float groundAngle, wallAngle;
    public Vector2 normalAverage, wallNormal;
    public bool isGrounded, isWall_R, isWall_L, isHighSlope, jumped;

    private bool lastFrameGrounded;

    [SerializeField] PlayerController_main plctrl;

    public HashSet<GameObject> collisionList = new HashSet<GameObject>(),
                                groundCollisions = new HashSet<GameObject>(),
                                wallCollisions_R = new HashSet<GameObject>(),
                                wallCollisions_L = new HashSet<GameObject>();

    public HashSet<Vector2> groundNormals = new HashSet<Vector2>(),
                            wallNormals = new HashSet<Vector2>();

    void Start()
    {
        jumped = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (groundNormals.Count > 0)
        {
            normalAverage = Vector2.zero;

            foreach (var normal in groundNormals) normalAverage += normal;
            normalAverage /= groundNormals.Count;
            normalAverage.Normalize();

        }//è’ìÀèÛãµÇ™ïœâªÇµÇ»Ç¢èÍçáOnCollisionStay2DÇ™åƒÇŒÇÍÇ»Ç¢ÇÃÇ≈ÅB

        if (wallNormals.Count > 0)
        {
            wallNormal = Vector2.zero;

            foreach (var normal in wallNormals) wallNormal += normal;
            wallNormal /= wallNormals.Count;
            wallNormal.Normalize();

        }//è’ìÀèÛãµÇ™ïœâªÇµÇ»Ç¢èÍçáOnCollisionStay2DÇ™åƒÇŒÇÍÇ»Ç¢ÇÃÇ≈

        isHighSlope = ((Vector2.Angle(transform.up, wallNormal) < wallAngle) && (wallNormals.Count > 0));

        isWall_R = wallCollisions_R.Count > 0;
        isWall_L = wallCollisions_L.Count > 0;

        lastFrameGrounded = isGrounded;
        isGrounded = groundCollisions.Count > 0 || (isWall_R && isWall_L);

        if (!lastFrameGrounded && (isGrounded)) plctrl.Chakuchi();
    }


    void LateUpdate()
    {
        groundNormals.Clear();
        wallNormals.Clear();

        /*
        collisionList.Clear();
        groundCollisions.Clear();
        wallCollisions_R.Clear();
        wallCollisions_L.Clear();
        */

        
        collisionList.RemoveWhere((col) => col.gameObject == null);
        groundCollisions.RemoveWhere((col) => col.gameObject == null);
        wallCollisions_R.RemoveWhere((col) => col.gameObject == null);
        wallCollisions_L.RemoveWhere((col) => col.gameObject == null);
        
    }

    void OnCollisionStay2D(Collision2D col)
    {
        var obj = col.gameObject;

        collisionList.Add(obj);

        bool ground = false, wall_R = false, wall_L = false;

        foreach (var contact in col.contacts)
        {
            var angle = Vector2.Angle(transform.up, contact.normal);

            if (angle <= groundAngle)
            {
                ground = true;
                groundNormals.Add(contact.normal);
            }
            else
            {
                bool isRight = (contact.point.x - transform.position.x) > 0;

                if (angle != 180)
                {
                    if (isRight) wall_R = true;
                    else wall_L = true;

                    wallNormals.Add(contact.normal);

                    Debug.DrawRay(contact.point, contact.normal, Color.yellow) ;
                }
            }
        }

        collisionList.Remove(obj);
        groundCollisions.Remove(obj);
        wallCollisions_R.Remove(obj);
        wallCollisions_L.Remove(obj);

        if (ground) groundCollisions.Add(obj);

        if (wall_R) wallCollisions_R.Add(obj);

        if (wall_L) wallCollisions_L.Add(obj);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        var obj = col.gameObject;

        collisionList.Remove(obj);
        groundCollisions.Remove(obj);
        wallCollisions_R.Remove(obj);
        wallCollisions_L.Remove(obj);
    }
}
