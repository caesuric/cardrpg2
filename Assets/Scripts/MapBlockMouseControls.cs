using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlockMouseControls : MonoBehaviour
{
    public int x = 0;
    public int y = 0;

    private void OnMouseOver() {
        Inputs.instance.mouseX = x;
        Inputs.instance.mouseY = y;
    }
}
