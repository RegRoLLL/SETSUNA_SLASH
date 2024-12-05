using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoloDisplayer : MonoBehaviour
{
    public float openTime;

    [SerializeField] CanvasGroup canvas;
    [SerializeField] ParticleSystem particle;
    [SerializeField] PlayerDetectArea area;

    [Header("InternalData")]
    public bool isActivated;

    Coroutine coroutine;

    void Start()
    {
        canvas.alpha = 0;
        canvas.gameObject.SetActive(false);
        isActivated = false;
    }

    
    void Update()
    {
        if (area.detected == isActivated) return;

        if (coroutine != null) StopCoroutine(coroutine);

        coroutine = StartCoroutine(OpenClose(area.detected));
    }

    IEnumerator OpenClose(bool open)
    {
        //Debug.Log(open ? "open" : "close");

        isActivated = open;

        if (open) canvas.gameObject.SetActive(true);

        float dTime = 0,
              start = canvas.alpha,
              target = open ? 1 : 0;

        if (open) particle.Play();
        else particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        while (dTime <= openTime)
        {
            //Debug.Log((dTime / openTime));
            canvas.alpha = Mathf.Lerp(start, target, (dTime / openTime));
            dTime += Time.deltaTime;
            yield return null;
        }
        canvas.alpha = target;

        if (!open) canvas.gameObject.SetActive(false);
    }
}
