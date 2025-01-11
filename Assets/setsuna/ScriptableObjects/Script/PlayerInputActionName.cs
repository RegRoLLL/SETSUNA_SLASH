using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerInputAction", menuName = "ScriptableObject/PlayerInputNames")]
public class PlayerInputActionName : ScriptableObject
{
    public string
            move, crouch, sprint, jump, attack, chargeCancel,
            reset, pose, interact;
}
