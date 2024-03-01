using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetsunaPlayerCamera : SetsunaSlashScript
{
    public Game_HubScript hub;

    public Transform player;
    public PlayerController_main plctrl;
    public float yFixValue, minSPD;
    public Vector2 distanceRatio, directionMaxAbsolute;
    public bool setDistanceFromAverage;

    public Vector2 directionToPL;
    [SerializeField] float dTime, interval;
    [SerializeField] float zero_dTime, zero_waitTime;
    [SerializeField] int listMax;
    public List<Vector2> velocityList = new List<Vector2>();

    Camera camera_;

    void Start()
    {
        camera_ = this.GetComponent<Camera>();
        transform.position = player.position + (Vector3.up * yFixValue);
        dTime = -1;
        velocityList.Add(Vector2.zero);
        setDistanceFromAverage = true;
    }

    void LateUpdate()
    {
        if (dTime >= interval || dTime<0)
        {
            if(setDistanceFromAverage)AddList(plctrl.rb.velocity);
            SetDistance();
            dTime = 0;
        }
        dTime += Time.deltaTime;

        SetPos();
    }

    public void AddList(Vector2 vel)
    {
        if (vel.magnitude > minSPD)
        {
            vel.x *= distanceRatio.x;
            vel.y *= distanceRatio.y;
            velocityList.Add(vel);
            zero_dTime = 0;
        }
        else if(zero_waitTime >= 0)
        {
            if (zero_dTime < zero_waitTime)
            {
                zero_dTime += interval;
            }
            else
            {
                velocityList.Add(vel);
            }
        }


        if (velocityList.Count > listMax)
        {
            velocityList.RemoveAt(0);
        }
    }

    public void ResetList()
    {
        ResetList(Vector2.zero);
    }
    public void ResetList(Vector2 vec)
    {
        velocityList.Clear();
        for (int i = 0; i < listMax; i++)
        {
            velocityList.Add(vec);
        }
    }

    void SetDistance()
    {
        if (!setDistanceFromAverage)
        {
            ResetList(directionToPL);
        }
        else
        {
            Vector2 sum = Vector2.zero;

            foreach (var vec in velocityList)
            {
                sum += vec;
            }

            directionToPL = (sum / velocityList.Count);
        }
    }

    void SetPos()
    {
        directionToPL.x = Mathf.Clamp(directionToPL.x, -directionMaxAbsolute.x, directionMaxAbsolute.x);
        directionToPL.y = Mathf.Clamp(directionToPL.y, -directionMaxAbsolute.y - yFixValue, directionMaxAbsolute.y - yFixValue);

        Vector2 pos = (Vector2)player.position + directionToPL + (Vector2.up * yFixValue);

        transform.position = (Vector3)hub.CameraPosClamp(pos, camera_) + (Vector3.forward * -10f);
    }
}
