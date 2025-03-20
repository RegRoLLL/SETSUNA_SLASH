using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Koume : SetsunaSlashScript
{
    public GameObject audioSource;

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
            PlayBladeDestroySound();
        }
    }

    private void PlayBladeDestroySound()
    {
        audioSource.SetActive(true);
        Debug.Log("destroy");
    }
}
