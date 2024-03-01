using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockGenerator : MonoBehaviour
{
    [SerializeField] Rock rockOrigin;
    public float rockScale;
    public float interval;
    private float dt;

    void Start()
    {
        Generate();
    }

    void Update()
    {
        if (dt < interval)
        {
            dt += Time.deltaTime;
            return;
        }

        Generate();
    }

    void Generate()
    {
        var obj = Instantiate(rockOrigin.gameObject, this.transform);
        obj.transform.localScale = Vector2.one * rockScale;
        var rock = obj.GetComponent<Rock>();
        rock.StartCoroutine(rock.Appear());
        rock.StartSimulate(Vector2.zero, true);

        dt = 0;
    }
}
