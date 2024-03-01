using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerDetectArea : SetsunaSlashScript 
{
    public bool detected;

   
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == playerLayer) detected = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == playerLayer) detected = false;
    }
}
