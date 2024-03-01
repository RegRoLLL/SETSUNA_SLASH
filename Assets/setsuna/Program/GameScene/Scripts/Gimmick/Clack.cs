using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Clack : MonoBehaviour
{
    public float breakPower;

    void Start()
    {
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            //Debug.Log($"inpulse:{contact.tangentImpulse}");

            if (contact.tangentImpulse >= breakPower)
            {
                OnClackHit();
                break;
            }
        }
    }

    protected abstract void OnClackHit();
}
