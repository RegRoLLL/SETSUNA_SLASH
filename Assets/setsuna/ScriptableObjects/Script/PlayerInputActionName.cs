using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerInputAction", menuName = "ScriptableObject/PlayerInputNames")]
public class PlayerInputActionName : ScriptableObject
{
    public string
            move, crouch, sprint, jump, attack,
            charge_mouse, charge_pad, chargeAdjust_pad, chargeSlash_pad, chargeCancel,
            charge_touch,
            reset, pose, interact;
}
