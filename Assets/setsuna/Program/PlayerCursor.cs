using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    [SerializeField] Sprite allow, scope;

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        SetCursor(CursorType.allow);
    }

    void Update()
    {
        Cursor.visible = false;
        transform.position = GetCursorPos(VectorSpace.world);
    }

    public enum VectorSpace { world, screen }
    public Vector2 GetCursorPos(VectorSpace space)
    {
        var result = Input.mousePosition;

        if (space == VectorSpace.world)
        {
            return Camera.main.ScreenToWorldPoint(result);
        }
        else
        {
            return result;
        }
    }

    public enum CursorType { allow,scope }
    public void SetCursor(CursorType cursorType)
    {
        sr.sprite = cursorType switch
        {
            CursorType.allow => allow,
            CursorType.scope => scope,
            _ => allow,
        };
    }
}
