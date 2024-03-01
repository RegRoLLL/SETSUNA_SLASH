using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "AudioBinds", menuName = "ScriptableObject/AudioBinds")]
public class AudioBinding : ScriptableObject
{
    public Player player;
    public Enemy enemy;
    public Gimmick gimmick;


    //=========================================

    [Serializable]
    public struct Player
    {
        public AudioClip
            jump, walk, dash, chargeSlash, notSlashAbleHit, attack_normal;
    }

    [Serializable]
    public struct Gimmick
    {
        public AudioClip
            gayser, savePoint, levitationStoneAwake;
    }

    [Serializable]
    public struct Enemy
    {
        public Golem golem;
    }

    //============================================

    [Serializable]
    public struct Golem
    {
        public AudioClip
            golem_punch, golem_throw, golem_throwGenerate, golem_stingerAttack, golem_stinger, golem_death;
    }
}
