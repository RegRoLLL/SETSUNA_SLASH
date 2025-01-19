using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PL_Dead : MonoBehaviour
{
    [SerializeField] float fadeTime;
    [SerializeField] Image deathBlackOut;

    Player pl;
    PlayerController_main player;
    PL_Status stat;
    PlayerAnimationCaller plAnim;

    void Start()
    {
        pl=GetComponent<Player>();
        player = GetComponent<PlayerController_main>();
        stat = GetComponent<PL_Status>();
        plAnim = GetComponent<PlayerAnimationCaller>();
        deathBlackOut.gameObject.SetActive(false);
    }

    public void Death()
    {
        StartCoroutine(DeathRevive());
    }

    IEnumerator DeathRevive()
    {
        pl.SetIsDead(true);

        float dt = 0;
        Color color = deathBlackOut.color;

        deathBlackOut.gameObject.SetActive(true);

        while (dt <= fadeTime)
        {
            color.a = (dt / fadeTime);
            deathBlackOut.color = color;

            dt += Time.deltaTime;
            yield return null;
        }
        color.a = 1f;
        deathBlackOut.color = color;



        yield return StartCoroutine(pl.ReturnPlayerPos());

        pl.SetIsDead(false);
        stat.ResetCount();




        while (dt >= 0)
        {
            color.a = (dt / fadeTime);
            deathBlackOut.color = color;

            dt -= Time.deltaTime;
            yield return null;
        }
        color.a = 0f;
        deathBlackOut.color = color;

        deathBlackOut.gameObject.SetActive(true);
    }
}
