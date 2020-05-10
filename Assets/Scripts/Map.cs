using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {
    public int posX = 0;
    public int posY = 0;
    public int currentFloorNumber = 0;
    public MapFloor currentFloor = null;
    public List<MapFloor> floors = new List<MapFloor>();
    public int numFloors = 5;
    private bool initialized = false;
    public static Map instance = null;

    void Start() {
        instance = this;
        new Player();
    }

    void Update() {
        if (!initialized) {
            initialized = true;
            BuildFloors();
            posX = currentFloor.startingX;
            posY = currentFloor.startingY;
            UserInterface.instance.SetUpCardPositions();
        }
    }

    private void BuildFloors() {
        for (int i = 0; i < 5; i++) floors.Add(new MapFloor(i));
        currentFloor = floors[0];
    }

    public void Draw() {
        if (currentFloor!=null) {
            DrawMap();
            DrawMonsters();
            DrawProjectiles();
        }
        UserInterface.Draw();
        CombatManager.instance.CheckIfInCombat();
    }

    private void DrawMap() {
        var halfWidth = VirtualConsole.instance.width / 2;
        var halfHeight = ((VirtualConsole.instance.height - 15) / 2) + 15;
        for (int x = posX - halfWidth; x < posX + halfWidth; x++) {
            for (int y = posY - halfHeight; y < posY + halfHeight; y++) {
                if (x == posX && y == posY) continue;
                DisplayCharacter dc = null;
                if ((Visible(x, y) || Seen(x, y)) && GetMonsters(x, y) != "") continue;
                if ((Visible(x, y) || Seen(x, y)) && GetProjectiles(x, y) != "") continue;
                if (y - posY + halfHeight <= 15) continue;
                if (x >= 0 && y >= 0 && x < VirtualConsole.instance.width && y < VirtualConsole.instance.height) dc = currentFloor.layout[x, y];
                if (dc != null && Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
                else if (Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, Get(x, y));
                else if (Seen(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r / 4f, dc.color.g / 4f, dc.color.b / 4f, dc.bgColor.r / 4f, dc.bgColor.g / 4f, dc.bgColor.b / 4f);
                else VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, " ");
            }
        }
        VirtualConsole.Set(halfWidth, halfHeight, "@");
    }

    private void DrawMonsters() {
        var halfWidth = VirtualConsole.instance.width / 2;
        var halfHeight = ((VirtualConsole.instance.height - 15) / 2) + 15;
        for (int x = posX - halfWidth; x < posX + halfWidth; x++) {
            for (int y = posY - halfHeight; y < posY + halfHeight; y++) {
                if ((Visible(x, y) || Seen(x, y)) && GetProjectiles(x, y) != "") continue;
                if (GetMonsters(x,y) == "") continue;
                DisplayCharacter dc = null;
                if (x >= 0 && y >= 0 && x < VirtualConsole.instance.width && y < VirtualConsole.instance.height) dc = currentFloor.monsters[x, y].display;
                if (Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
            }
        }
    }

    private void DrawProjectiles() {
        var halfWidth = VirtualConsole.instance.width / 2;
        var halfHeight = ((VirtualConsole.instance.height - 15) / 2) + 15;
        for (int x = posX - halfWidth; x < posX + halfWidth; x++) {
            for (int y = posY - halfHeight; y < posY + halfHeight; y++) {
                if (GetProjectiles(x, y) == "") continue;
                DisplayCharacter dc = null;
                if (x >= 0 && y >= 0 && x < VirtualConsole.instance.width && y < VirtualConsole.instance.height) dc = currentFloor.projectiles[x, y].display;
                if (Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
                if (currentFloor.projectiles[x, y].blastStage > -1) {
                    var proj = currentFloor.projectiles[x, y];
                    for (int x2 = proj.x - proj.blastStage; x2 <= proj.x + proj.blastStage; x2++) {
                        for (int y2 = proj.y - proj.blastStage; y2 <= proj.y + proj.blastStage; y2++) {
                            if (Visible(x2, y2)) VirtualConsole.Set(x2 - posX + halfWidth, y2 - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
                        }
                    }
                }
            }
        }
    }

    private string Get(int x, int y) {
        return currentFloor.Get(x, y);
    }

    private string GetMonsters(int x, int y) {
        return currentFloor.GetMonsters(x, y);
    }

    private string GetProjectiles(int x, int y) {
        return currentFloor.GetProjectiles(x, y);
    }

    public bool Visible(int x, int y) {
        if (currentFloor == null) return false;
        return currentFloor.Visible(x, y);
    }

    public bool BlocksSight(int x, int y) {
        return currentFloor.BlocksSight(x, y);
    }

    public bool BlocksProjectile(int x, int y) {
        return currentFloor.BlocksProjectile(x, y);
    }

    public bool Seen(int x, int y) {
        if (currentFloor == null) return false;
        return currentFloor.Seen(x, y);
    }

    public void ColorBlock(int x, int y, float r, float g, float b) {
        VirtualConsole.ColorBlock(x - instance.posX + VirtualConsole.instance.width / 2, y - instance.posY + ((VirtualConsole.instance.height - 15) / 2) + 15, r, g, b);
    }
}
