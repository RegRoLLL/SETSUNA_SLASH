using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PL_normalAttackCol : MonoBehaviour
{
    [SerializeField] float inactiveTime, dt;

    private void OnEnable()
    {
        dt = 0;
    }

    void Update()
    {
        if (dt < inactiveTime)
        {
            dt += Time.deltaTime;
            return;
        }

        gameObject.SetActive(false);
    }
}
