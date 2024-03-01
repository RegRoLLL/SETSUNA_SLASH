using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HingeJoint2D))]
public class RotationShaft : MonoBehaviour
{
    HingeJoint2D joint;
    float primeLimit;

    void Start()
    {
        joint = GetComponent<HingeJoint2D>();
        primeLimit = joint.limits.max;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (joint.connectedBody != null) return;

        joint.connectedBody = other.GetComponent<Rigidbody2D>();

        var angle = other.transform.eulerAngles.z;

        //Debug.Log("angle: " + angle);

        var limit = joint.limits;

        if (angle > 180)
        {
            angle -= 360;
            //Debug.Log("fixedangle: " + angle);
        }

        limit.max = -angle + primeLimit;
        limit.min = -angle - primeLimit;

        joint.limits = limit;
    }
}
