using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyToTeleport : SetsunaSlashScript
{
    [SerializeField] private GameObject flyobject; 
    [SerializeField] private GameObject targetobject; 

    private bool touchedBlade = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == bladeLayer)
        {
            touchedBlade = true;
            Debug.Log("slashed");
            StartCoroutine(ResetTouchedBlade());
        }
    }

    private IEnumerator ResetTouchedBlade()
    {
        yield return new WaitForSeconds(3f);
        touchedBlade = false;
    }

    private void OnDestroy()
    {
        if (touchedBlade)
        {
            TeleportObject();
        }
    }

    private void TeleportObject()
    {
        if (flyobject != null && targetobject != null)
        {
            flyobject.transform.position = targetobject.transform.position;
            
        }
    }
}
