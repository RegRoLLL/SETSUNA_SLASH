using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetsunaPlayerCamera : SetsunaSlashScript
{
    Game_HubScript hub;

    public Transform player;
    public PlayerController_main plctrl;
    public float yFixValue, minSPD;
    public Vector2 distanceRatio, directionMaxAbsolute;
    public bool setDistanceFromAverage;

    public Vector2 directionToPL;
    [SerializeField] float dTime, interval;
    [SerializeField] int listMax;
    public List<Vector2> velocityList = new List<Vector2>();

    Camera camera_;

    void Start()
    {
        hub = EventSystem.current.GetComponent<Game_HubScript>();

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
            if(setDistanceFromAverage && (plctrl.rb.velocity.magnitude > minSPD)) AddList(plctrl.rb.velocity);
            SetDistance();
            dTime = 0;
        }
        dTime += Time.deltaTime;

        SetPos();
    }

    public void AddList(Vector2 vel)
    {
        vel.x *= distanceRatio.x;
        vel.y *= distanceRatio.y;
        velocityList.Add(vel);

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

    public void SetPos()
    {
        if (hub.currentPart == null) return;

        directionToPL.x = Mathf.Clamp(directionToPL.x, -directionMaxAbsolute.x, directionMaxAbsolute.x);
        directionToPL.y = Mathf.Clamp(directionToPL.y, -directionMaxAbsolute.y - yFixValue, directionMaxAbsolute.y - yFixValue);

        Vector2 pos = (Vector2)player.position + directionToPL + (Vector2.up * yFixValue);

        transform.position = (Vector3)hub.CameraPosClamp(pos, camera_) + (Vector3.forward * -10f);
    }
}
