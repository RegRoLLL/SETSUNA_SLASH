using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PL_Dead : MonoBehaviour
{
    [SerializeField] float fadeTime;
    [SerializeField] Image deathBlackOut;

    Game_HubScript hub;
    PlayerController_main player;
    PL_Status stat;
    PlayerAnimationCaller plAnim;

    void Start()
    {
        hub = EventSystem.current.GetComponent<Game_HubScript>();

        player = GetComponent<PlayerController_main>();
        stat = GetComponent<PL_Status>();
        plAnim = GetComponent<PlayerAnimationCaller>();
        deathBlackOut.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player.isDead) return;

        if(stat.hp <= 0)StartCoroutine(DeathRevive());
    }

    IEnumerator DeathRevive()
    {
        player.isDead = true;

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



        yield return hub.gm.StartCoroutine(hub.gm.ReturnPlayerPos());

        player.isDead = false;
        stat.HP_heal(stat.hp_max);
        stat.MP_heal(stat.mp_max);




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
