using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_mainUI : MonoBehaviour
{
    [SerializeField] PL_Status player;
    [SerializeField] Image hpBar, mpBar;

    void Start()
    {
        
    }

    void Update()
    {
        SetBarFill();
    }

    void SetBarFill()
    {
        hpBar.fillAmount = (player.hp / player.hp_max);
        mpBar.fillAmount = (player.mp / player.mp_max);
    }
}
