using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs : MonoBehaviour
{
    private bool moved = false;
    private float moveTimer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (moved) {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0) {
                moveTimer = 0;
                moved = false;
            }
        }
        var horiz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        if (horiz == 0 && vert == 0) moved = false;
        if (moved) return;
        if (horiz > 0) MoveRight();
        else if (horiz < 0) MoveLeft();
        else if (vert < 0) MoveUp();
        else if (vert > 0) MoveDown();
    }

    private void MoveRight() {
        if (!MoveValid(Map.instance.posX + 1, Map.instance.posY)) return;
        Map.instance.posX++;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private void MoveLeft() {
        if (!MoveValid(Map.instance.posX - 1, Map.instance.posY)) return;
        Map.instance.posX--;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private void MoveUp() {
        if (!MoveValid(Map.instance.posX, Map.instance.posY - 1)) return;
        Map.instance.posY--;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private void MoveDown() {
        if (!MoveValid(Map.instance.posX, Map.instance.posY + 1)) return;
        Map.instance.posY++;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private bool MoveValid(int x, int y) {
        if (Map.instance.layout[x, y] == "." || Map.instance.layout[x, y] == "+") {
            Map.instance.layout[x, y] = ".";
            return true;
        }
        return false;
    }
}
