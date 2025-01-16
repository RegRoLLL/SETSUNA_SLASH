using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird_normal : MonoBehaviour
{
    Game_HubScript hub;

    [SerializeField] Transform wing;
    [SerializeField] float wingSpeed, moveSpeed;

    [SerializeField] PlayerDetectArea area;

    [SerializeField] bool isMoving, wasThroughedPlayer;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (area.detected) isMoving = true;

        if (!isMoving) return;

        if(hub==null)
            hub = GetComponentInParent<StageManager>().hub;

        wing.Rotate(Vector3.right * 360f *wingSpeed * Time.deltaTime);
        rb.velocity = Vector2.left * moveSpeed;

        if (wasThroughedPlayer) return;
        if (hub.PL_Ctrler.transform.position.x > transform.position.x) wasThroughedPlayer = true;
    }

    private void OnBecameVisible()
    {
        if (isMoving) return;
        isMoving = true;
    }

    private void OnBecameInvisible()
    {
        if (wasThroughedPlayer)
        {
            Destroy(gameObject);
        }
    }
}
