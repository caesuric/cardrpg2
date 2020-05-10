using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance = null;
    public bool initialized = false;

    void Start()
    {
        instance = this;
    }

    void Update() {
        if (!initialized) {
            initialized = true;
            Inputs.instance.mouseMode = MouseMode.MainMenu;
            Draw();
        }
    }

    private void Draw() {
        VirtualConsole.DrawBox(20, 45, 40, 2);
        VirtualConsole.Write("Deckbuilding Roguelike", 30, 45, 40, 1);
        VirtualConsole.DrawBox(30, 30, 20, 2);
        VirtualConsole.Write("Start", 38, 30, 6, 1);
    }

   private void GoToGame() {
        Map.instance.Draw();
    }

    public bool ClickedStart() {
        if (MouseInBounds(30, 30, 20, 2)) return true;
        return false;
    }

    private bool MouseInBounds(int x, int y, int width, int height) {
        var mouseX = Inputs.instance.mouseX;
        var mouseY = Inputs.instance.mouseY;
        if (mouseX >= x && mouseX <= x + width && mouseY >= y && mouseY <= y + height) return true;
        return false;
    }
}
