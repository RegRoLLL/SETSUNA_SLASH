using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : SetsunaSlashScript
{
    [field: SerializeField] public float hp { get; private set; }
    [field: SerializeField] public float hp_max { get; set; }
    [field: SerializeField] public float hp_regene { get; private set; }

    [field: SerializeField] public float mp { get; private set; }
    [field: SerializeField] public float mp_max { get; set; }
    [field: SerializeField] public float mp_regene { get; private set; }

    void Update()
    {
        Regene();
    }

    protected virtual void Regene()
    {
        if (hp_regene != 0 && hp > 0)
        {
            HP_heal(hp_regene * Time.deltaTime);
        }

        if (mp_regene != 0)
        {
            MP_heal(mp_regene * Time.deltaTime);
        }
    }

    public virtual void HP_damage(float value)
    {
        hp -= value;
        hp = Mathf.Clamp(hp, 0, hp_max);
    }
    public virtual void HP_heal(float value)
    {
        hp += value;
        hp = Mathf.Clamp(hp, 0, hp_max);
    }
    public virtual void SetHP(float value)
    {
        HP_heal(hp_max);
        HP_damage(hp_max - value);
    }

    public virtual void MP_damage(float value)
    {
        mp -= value;
        mp = Mathf.Clamp(mp, 0, mp_max);
    }
    public virtual void MP_heal(float value)
    {
        mp += value;
        mp = Mathf.Clamp(mp, 0, mp_max);
    }
    public virtual void SetMP(float value)
    {
        MP_heal(mp_max);
        MP_damage(mp_max - value);
    }
}
