using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableTrigger : MonoBehaviour
{
    [SerializeField] UnityEvent onEnable = new();

    private void OnEnable()
    {
        onEnable.Invoke();
    }
}
