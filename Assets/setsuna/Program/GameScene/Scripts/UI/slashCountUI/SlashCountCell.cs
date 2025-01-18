using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SlashCountCell : MonoBehaviour
{
    public SlashCountCellState state = new();

    [SerializeField] Image foreCellImg;
    Image backCellImg;

    public void Initialize()
    {
        backCellImg = GetComponent<Image>();
        
        foreCellImg.enabled = false;
        state.fore = false;
        backCellImg.enabled = false;
        state.back = false;
    }

    public void SetCell(bool enable)
    {
        SetBack(enable);
        SetFore(enable);
    }

    public void Consume()
    {
        SetFore(false);
    }

    public void Recover()
    {
        SetFore(true);
    }



    void SetFore(bool enable)
    {
        foreCellImg.enabled = enable;
        state.fore = enable;
    }
    public void SetBack(bool enable)
    {
        backCellImg.enabled = enable;
        state.back = enable;
    }

    public class SlashCountCellState
    {
        public bool back, fore;
    }
}
