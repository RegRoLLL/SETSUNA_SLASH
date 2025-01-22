using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : SetsunaSlashScript
{
    [field:SerializeField] public float HP { get; private set; }
    [field: SerializeField] public float HP_max { get; set; }
    [field: SerializeField] public float HP_regene { get; private set; }

    [field: SerializeField] public float MP { get; private set; }
    [field: SerializeField] public float MP_max { get; set; }
    [field: SerializeField] public float MP_regene { get; private set; }

    void Update()
    {
        Regene();
    }

    protected virtual void Regene()
    {
        if (HP_regene != 0 && HP > 0)
        {
            HP_heal(HP_regene * Time.deltaTime);
        }

        if (MP_regene != 0)
        {
            MP_heal(MP_regene * Time.deltaTime);
        }
    }

    public virtual void HP_damage(float value)
    {
        HP -= value;
        HP = Mathf.Clamp(HP, 0, HP_max);
    }
    public virtual void HP_heal(float value)
    {
        HP += value;
        HP = Mathf.Clamp(HP, 0, HP_max);
    }
    public virtual void SetHP(float value)
    {
        HP_heal(HP_max);
        HP_damage(HP_max - value);
    }

    public virtual void MP_damage(float value)
    {
        MP -= value;
        MP = Mathf.Clamp(MP, 0, MP_max);
    }
    public virtual void MP_heal(float value)
    {
        MP += value;
        MP = Mathf.Clamp(MP, 0, MP_max);
    }
    public virtual void SetMP(float value)
    {
        MP_heal(MP_max);
        MP_damage(MP_max - value);
    }
}
