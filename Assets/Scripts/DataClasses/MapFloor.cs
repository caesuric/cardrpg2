﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MapFloor {
    public static readonly int xSize = 160;
    public static readonly int ySize = 120;
    public DisplayCharacter[,] layout = new DisplayCharacter[xSize, ySize];
    public Monster[,] monsters = new Monster[xSize, ySize];
    public Projectile[,] projectiles = new Projectile[xSize, ySize];
    public bool[,] seen = new bool[xSize, ySize];
    public int startingX = 0;
    public int startingY = 0;
    public int endingX = -1;
    public int endingY = -1;
    public int floor = 0;

    public MapFloor(int floor) {
        this.floor = floor;
        FillMap();
        DigStartingRoom();
        AddRooms();
        if (floor > 0) AddEntrance();
        if (floor < 4) AddExit();
        TintMap();
    }

    private void Print() {
        var output = "";
        for (int x = 0; x < layout.GetLength(0); x++) {
            for (int y = 0; y < layout.GetLength(1); y++) {
                output += layout[x, y] + "  ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }

    public void TintMap() {
        for (int x = 0; x < layout.GetLength(0); x++) {
            for (int y = 0; y < layout.GetLength(1); y++) {
                var point = layout[x, y];
                if (point.character == "#") point.bgColor = new Color(0.6f, 0.6f, 0.6f);
                else if (point.character == "+") {
                    point.color = Color.yellow;
                    point.bgColor = new Color(0.65f, 0.16f, 0.16f);
                }
                else {
                    point.color = Color.white;
                    point.bgColor = Color.black;
                }
            }
        }
    }

    private void AddEntrance() {
        layout[startingX, startingY].character = "<";
    }

    private void AddExit() {
        while (endingX == -1 || endingY == -1 || layout[endingX, endingY].character != ".") {
            endingX = Random.Range(0, xSize);
            endingY = Random.Range(0, ySize);
        }
        layout[endingX, endingY].character = ">";
    }

    private void FillMap() {
        for (int x = 0; x < layout.GetLength(0); x++) {
            for (int y = 0; y < layout.GetLength(1); y++) {
                layout[x, y] = new DisplayCharacter {
                    character = "#",
                    color = Color.white,
                    bgColor = Color.black
                };
                monsters[x, y] = null;
                projectiles[x, y] = null;
                seen[x, y] = false;
            }
        }
    }

    private void DigStartingRoom() {
        startingX = Random.Range(3, xSize - 3);
        startingY = Random.Range(3, ySize - 3);
        for (int x = startingX - 2; x < startingX + 3; x++) {
            for (int y = startingY - 2; y < startingY + 3; y++) {
                layout[x, y].character = ".";
            }
        }
    }

    private void AddRooms() {
        for (int i = 0; i < 30; i++) {
            for (int j = 0; j < 1000; j++) {
                if (AddFeature()) break;
            }
        }
    }

    private bool AddFeature() {
        // pick random wall
        int x = 0, y = 0;
        int facing = -1;
        while (facing == -1) {
            x = Random.Range(1, layout.GetLength(0) - 2);
            y = Random.Range(1, layout.GetLength(1) - 2);
            if (Get(x, y) != "#") continue;
            if (Get(x, y - 1) == ".") facing = 2;
            else if (Get(x + 1, y) == ".") facing = 3;
            else if (Get(x, y + 1) == ".") facing = 0;
            else if (Get(x - 1, y) == ".") facing = 1;
        }
        int type = Random.Range(0, 15); // #0 for corridors, 1 for rooms, 2-3 for corridor with door, 4-5 for room with door, 6 for octagon, 7-8 for octagon with door, 9 for cross, 10-11 for cross with door, 12 for diamond, 13-14 for diamond with door
        // get feature parameters
        // get feature door presence
        int width = 0;
        int height = 0;
        bool door = false;
        switch (type) {
            case 0:
                width = Random.Range(4, 16);
                height = Random.Range(2, 3);
                break;
            case 2:
            case 3:
                door = true;
                width = Random.Range(4, 16);
                height = Random.Range(2, 3);
                break;
            case 1:
                width = Random.Range(4, 16);
                height = Random.Range(4, 16);
                break;
            case 4:
            case 5:
                door = true;
                width = Random.Range(4, 16);
                height = Random.Range(4, 16);
                break;
            case 6:
            case 9:
                width = height = Random.Range(1, 4) * 4;
                break;
            case 7:
            case 8:
            case 10:
            case 11:
                door = true;
                width = height = Random.Range(1, 4) * 4;
                break;
            case 12:
                width = height = Random.Range(2, 7) * 2 + 1;
                break;
            case 13:
            case 14:
            default:
                door = true;
                width = height = Random.Range(2, 7) * 2 + 1;
                break;
        }
        if (door && (facing == 0 || facing == 2)) height++;
        else if (door) width++;
        // get feature minor axis offset
        int minorAxisOffset = 0;
        if ((facing == 1 || facing == 3) && type <= 5) minorAxisOffset = Random.Range(0, height - 1);
        else if ((facing == 0 || facing == 2) && type <= 5) minorAxisOffset = Random.Range(0, height - 2);
        else if ((facing == 1 || facing == 3) && type >= 6 && type <= 11) minorAxisOffset = Random.Range(height / 4, (height / 4 * 3) - 1);
        else if ((facing == 0 || facing == 2) && type >= 6 && type <= 11) minorAxisOffset = Random.Range(height / 4, (height / 4 * 3) - 1);
        else if (type >= 12 && type <= 14) minorAxisOffset = height / 2;
        // attempt to dig room
        if (SpaceForRoom(x, y, facing, width, height, minorAxisOffset)) {
            DigRoom(type, x, y, facing, width, height, door, minorAxisOffset);
            int monsterRoll = Random.Range(0, 2);
            if (monsterRoll == 0) AddMonsters(x, y, width, height);
            return true;
        }
        else return false;
    }

    private void AddMonsters(int x, int y, int width, int height) {
        int roll = Random.Range(1, 3);
        for (int i = 0; i < roll; i++) AddMonster(x, y, width, height);
    }

    private void AddMonster(int x, int y, int width, int height) {
        for (int i = 0; i < 1000; i++) {
            int xRoll = Random.Range(x, x + width);
            int yRoll = Random.Range(y, y + width);
            if (Get(xRoll, yRoll) == "." && monsters[xRoll, yRoll] == null && !VisibleFromEntrance(xRoll, yRoll)) {
                monsters[xRoll, yRoll] = new Monster {
                    x = xRoll,
                    y = yRoll,
                    floor = floor,
                    display = new DisplayCharacter {
                        character = "g",
                        color = Color.green,
                        bgColor = Color.black
                    }
                };
                return;
            }
        }
    }

    private void DigRoom(int type, int x, int y, int facing, int width, int height, bool door, int minorAxisOffset) {
        int xStep = 1;
        int yStep = 1;
        // add door if necessary
        if (door) {
            int doorRoll = Random.Range(0, 2);
            if (doorRoll == 0) layout[x, y].character = "+";
            else layout[x, y].character = ".";
            if (facing == 0 || facing == 2) {
                y += yStep;
                height -= yStep;
            }
            else {
                x += xStep;
                width -= xStep;
            }
        }
        //rotate room if necessary
        if (facing == 0 || facing == 2) {
            var temp = width;
            width = height;
            height = temp;
        }
        // flip room if necessary
        if (facing == 0) {
            height = 0 - height;
            yStep = -1;
        }
        if (facing == 3) {
            width = 0 - width;
            xStep = -1;
        }
        // adjust room for offset
        if (facing == 0 || facing == 2) x -= xStep * minorAxisOffset;
        else y -= yStep * minorAxisOffset;

        if (type >= 0 && type <= 5) DigRectangle(x, y, facing, width, height, door, minorAxisOffset, xStep, yStep);
        else if (type >= 6 && type <= 8) DigOctagon(x, y, x + width, y + height);
        else if (type >= 9 && type <= 11) DigCross(x, y, x + width, y + height);
        else if (type >= 12 && type <= 14) DigDiamond(x, y, x + width, y + height);
    }

    private void DigRectangle(int x, int y, int facing, int width, int height, bool door, int minorAxisOffset, int xStep, int yStep) {
        for (int xIn = x; xIn != x + width; xIn += xStep) {
            for (int yIn = y; yIn != y + height; yIn += yStep) {
                layout[xIn, yIn].character = ".";
            }
        }
        int roll = Random.Range(0, 20);
        if (roll == 0) AddCentralPillar(x, y, x + width, y + height, xStep, yStep);
    }

    private void AddCentralPillar(int x1, int y1, int x2, int y2, int xStep, int yStep) {
        int xq = Mathf.Min((x2 - x1) / 4, 2) + 1;
        int yq = Mathf.Min((y2 - y1) / 4, 2) + 1;
        if (x1 + xq >= x2 - xq || y1 + yq >= y2 - yq) return;
        for (int xIn = x1 + xq; xIn < x2 - xq; xIn += xStep) {
            for (int yIn = y1 + yq; yIn < y2 - yq; yIn += yStep) {
                layout[xIn, yIn].character = "#";
            }
        }
    }

    private void DigOctagon(int x1, int y1, int x2, int y2) {
        int qx = (x2 - x1) / 4;
        int qy = (y2 - y1) / 4;
        RemoveLine(x1, y1 + qy, x1 + qx, y1);
        RemoveLine(x1 + qx, y1, x2 - qx, y1);
        RemoveLine(x2 - qx, y1, x2, y1 + qy);
        RemoveLine(x2, y1 + qy, x2, y2 - qy);
        RemoveLine(x2, y2 - qy, x2 - qx, y2);
        RemoveLine(x2 - qx, y2, x1 + qx, y2);
        RemoveLine(x1 + qx, y2, x1, y2 - qy);
        RemoveLine(x1, y2 - qy, x1, y1 + qy);
        FloodRemove(x1 + (x2 - x1) / 2, y1 + (y2 - y1) / 2);
    }

    private void RemoveLine(int x1, int y1, int x2, int y2) {
        int step = -1;
        if (x2 == x1) RemoveVerticalLine(x1, y1, y2);
        else if (x2 > x1) step = 1;
        float prevY = y1;
        for (int xValue = x1; xValue != x2 + step; xValue += step) prevY = RemoveLineStep(xValue, x1, y1, x2, y2, prevY);
    }

    private float RemoveLineStep(int xValue, int x1, int y1, int x2, int y2, float prevY) {
        float yValue = ((float)xValue - x1) / ((float)x2 - x1) * ((float)y2 - y1) + y1;
        if (xValue != x1 && Mathf.Abs((int)yValue - (int)prevY) > 1) RemoveVerticalLine((int)xValue, (int)prevY, (int)yValue);
        if (xValue > 0 && yValue > 0 && xValue < layout.GetLength(0) - 1 && yValue < layout.GetLength(1) - 1) layout[xValue, (int)yValue].character = ".";
        return yValue;
    }

    private void RemoveVerticalLine(int x, int y1, int y2) {
        int step = -1;
        if (y2 == y1) {
            layout[x, y1].character = ".";
            return;
        }
        else if (y2 > y1) step = 1;
        for (int yValue = y1; yValue != y2 + step; yValue += step) layout[x, yValue].character = ".";
    }

    private void FloodRemove(int x, int y) {
        if (x > 0 && y > 0 && x < layout.GetLength(0) - 1 && y < layout.GetLength(1) - 1 && layout[x, y].character == "#") {
            layout[x, y].character = ".";
            FloodRemove(x - 1, y);
            FloodRemove(x + 1, y);
            FloodRemove(x, y - 1);
            FloodRemove(x, y + 1);
        }
    }

    private void DigCross(int x1, int y1, int x2, int y2) {
        int qx = (x2 - x1) / 4;
        int qy = (y2 - y1) / 4;
        for (int xIn = x1 + qx; xIn < x2 - qx; xIn++) {
            for (int yIn = y1; yIn < y2; yIn++) {
                layout[xIn, yIn].character = ".";
            }
        }
        for (int xIn = x1; xIn < x2; xIn++) {
            for (int yIn = y1 + qy; yIn < y2 - qy; yIn++) {
                layout[xIn, yIn].character = ".";
            }
        }
    }

    private void DigDiamond(int x1, int y1, int x2, int y2) {
        int hx = (x2 - x1) / 2;
        int hy = (y2 - y1) / 2;
        RemoveLine(x1 + hx, y1, x2, y1 + hy);
        RemoveLine(x2, y1 + hy, x1 + hx, y2);
        RemoveLine(x1 + hx, y2, x1, y1 + hy);
        RemoveLine(x1, y1 + hy, x1 + hx, y1);
        FloodRemove(x1 + hx, y1 + hy);
    }

    private bool SpaceForRoom(int x, int y, int facing, int width, int height, int minorAxisOffset) {
        int xStep = 1;
        int yStep = 1;
        //rotate room if necessary
        if (facing == 0 || facing == 2) {
            var temp = width;
            width = height;
            height = temp;
        }
        // flip room if necessary
        if (facing == 0) {
            height = 0 - height;
            yStep = -1;
        }
        if (facing == 3) {
            width = 0 - width;
            xStep = -1;
        }
        // adjust room for offset
        if (facing == 0 || facing == 2) x -= xStep * minorAxisOffset;
        else y -= yStep * minorAxisOffset;
        if (x + width >= layout.GetLength(0) - 2 || x + width <= 2 || y + height >= layout.GetLength(1) - 2 || y + height <= 2) return false;
        for (int xIn = x; xIn != x + width + xStep; xIn += xStep) {
            for (int yIn = y; yIn != y + height + yStep; yIn += yStep) {
                if (Get(xIn, yIn) != "#") return false;
            }
        }
        return true;
    }

    public string Get(int x, int y) {
        if (x < 0 || y < 0 || x >= layout.GetLength(0) || y >= layout.GetLength(1)) return " ";
        else return layout[x, y].character;
    }

    public string GetMonsters(int x, int y) {
        if (x < 0 || y < 0 || x >= monsters.GetLength(0) || y >= monsters.GetLength(1)) return "";
        else if (monsters[x, y] == null) return "";
        else return monsters[x, y].display.character;
    }

    public string GetProjectiles(int x, int y) {
        if (x < 0 || y < 0 || x >= projectiles.GetLength(0) || y >= projectiles.GetLength(1)) return "";
        else if (projectiles[x, y] == null) return "";
        else return projectiles[x, y].display.character;
    }

    public bool Visible(int x, int y) {
        if (x < 0 || y < 0 || x >= layout.GetLength(0) || y >= layout.GetLength(1)) return false;
        var x0 = Map.instance.posX;
        var y0 = Map.instance.posY;
        var dx = x - Map.instance.posX;
        var dy = y - Map.instance.posY;
        int sx, sy;
        if (x0 < x) sx = 1;
        else sx = -1;
        if (y0 < y) sy = 1;
        else sy = -1;
        int xnext = x0;
        int ynext = y0;
        var denom = Mathf.Sqrt((float)dx * dx + (float)dy * dy);
        while (xnext != x || ynext != y) {
            if (xnext >= 0 && ynext >= 0 && xnext < layout.GetLength(0) && ynext < layout.GetLength(1)) {
                if (BlocksSight(xnext, ynext)) return false;
                if (Mathf.Abs(dy * (xnext - x0 + sx) - dx * (ynext - y0)) / denom < 0.5f) xnext += sx;
                else if (Mathf.Abs(dy * (xnext - x0) - dx * (ynext - y0 + sy)) / denom < 0.5f) ynext += sy;
                else {
                    xnext += sx;
                    ynext += sy;
                }
            }
            else {
                return false;
            }
        }
        seen[x, y] = true;
        return true;
    }

    public bool VisibleFromEntrance(int x, int y) {
        if (x < 0 || y < 0 || x >= layout.GetLength(0) || y >= layout.GetLength(1)) return false;
        var x0 = startingX;
        var y0 = startingY;
        var dx = x - startingX;
        var dy = y - startingY;
        int sx, sy;
        if (x0 < x) sx = 1;
        else sx = -1;
        if (y0 < y) sy = 1;
        else sy = -1;
        int xnext = x0;
        int ynext = y0;
        var denom = Mathf.Sqrt((float)dx * dx + (float)dy * dy);
        while (xnext != x || ynext != y) {
            if (xnext >= 0 && ynext >= 0 && xnext < layout.GetLength(0) && ynext < layout.GetLength(1)) {
                if (BlocksSight(xnext, ynext)) return false;
                if (Mathf.Abs(dy * (xnext - x0 + sx) - dx * (ynext - y0)) / denom < 0.5f) xnext += sx;
                else if (Mathf.Abs(dy * (xnext - x0) - dx * (ynext - y0 + sy)) / denom < 0.5f) ynext += sy;
                else {
                    xnext += sx;
                    ynext += sy;
                }
            }
            else {
                return false;
            }
        }
        seen[x, y] = true;
        return true;
    }

    public bool BlocksSight(int x, int y) {
        var block = layout[x, y];
        if (block.character == "#" || block.character == "+") return true;
        return false;
    }

    public bool BlocksProjectile(int x, int y) {
        if (BlocksSight(x, y)) return true;
        var block = monsters[x, y];
        if (block != null && block.display.character != "") return true;
        return false;
    }

    public bool Seen(int x, int y) {
        if (x < 0 || y < 0 || x >= layout.GetLength(0) || y >= layout.GetLength(1)) return false;
        return seen[x, y];
    }
}
