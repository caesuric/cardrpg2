using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public void Draw() {
        VirtualConsole.Clear();
        VirtualConsole.DrawBox(20, 45, 40, 2);
        VirtualConsole.Write("Deckbuilding Roguelike", 30, 45, 40, 1);
        VirtualConsole.DrawBox(30, 30, 20, 2);
        VirtualConsole.Write("Start", 38, 30, 6, 1);
        VirtualConsole.DrawBox(30, 26, 20, 2);
        VirtualConsole.Write("Load", 38, 26, 5, 1);
    }

    public void DrawLoadMenu() {
        VirtualConsole.Clear();
        VirtualConsole.DrawBox(20, 45, 40, 2);
        VirtualConsole.Write("Load Menu", 35, 45, 9, 1);
        DrawLoadButtons();
    }

    private void DrawLoadButtons() {
        var saveDirectory = Application.persistentDataPath;
        var filenames = Directory.GetFiles(saveDirectory, "*.json");
        int cursor = 34;
        foreach (var filename in filenames) {
            cursor -= 4;
            VirtualConsole.DrawBox(30, cursor, 20, 2);
            var shortenedName = Path.GetFileNameWithoutExtension(filename);
            VirtualConsole.Write(shortenedName, 30 + (10 - shortenedName.Length / 2), cursor, 20, 1);
        }
    }

   private void GoToGame() {
        Map.instance.Draw();
    }

    public bool ClickedStart() {
        if (MouseInBounds(30, 30, 20, 2)) return true;
        return false;
    }

    public bool ClickedLoad() {
        if (MouseInBounds(30, 26, 20, 2)) return true;
        return false;
    }

    public string CharacterClicked() {
        var saveDirectory = Application.persistentDataPath;
        var filenames = Directory.GetFiles(saveDirectory, "*.json");
        int cursor = 34;
        foreach (var filename in filenames) {
            cursor -= 4;
            if (MouseInBounds(30, cursor, 20, 2)) return filename;
        }
        return null;
    }

    private bool MouseInBounds(int x, int y, int width, int height) {
        var mouseX = Inputs.instance.mouseX;
        var mouseY = Inputs.instance.mouseY;
        if (mouseX >= x && mouseX <= x + width && mouseY >= y && mouseY <= y + height) return true;
        return false;
    }
}
