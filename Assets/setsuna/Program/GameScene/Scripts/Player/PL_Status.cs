using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PL_Status : Status
{
    public override void MP_damage(float value)
    {
        var mp_Cache = MP;

        base.MP_damage(value);

        if (MP > 0) return;

        HP_damage(value - mp_Cache);
    }
}
