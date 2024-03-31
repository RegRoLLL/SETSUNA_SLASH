using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClackGayser : Clack
{
    [SerializeField] WindBlower gayser;
    [SerializeField] ParticleSystem clackEffect;

    void Start()
    {
        gayser.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    protected override void OnClackHit()
    {
        Debug.Log("overrode OnClack");

        clackEffect.Play();
        gayser.gameObject.SetActive(true);
    }
}
