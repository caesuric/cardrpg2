using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Monster {
    public int hp = 5;
    public int maxHp = 5;
    public DisplayCharacter display = null;
    public static List<Monster> instances = new List<Monster>();
    public int x = 0;
    public int y = 0;
    public int floor = 0;
    public int initiative = 0;

    public Monster() {
        instances.Add(this);
    }

    public void Act() {
        if (floor != Map.instance.currentFloorNumber) return;
        Move();
        Attack();
    }

    private void Move() {
        for (int i=0; i<4; i++) {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer() {
        Map.instance.currentFloor.monsters[x, y] = null;
        if (x < Map.instance.posX - 1 && MoveOkay(x + 1, y)) x++;
        if (x > Map.instance.posX + 1 && MoveOkay(x - 1, y)) x--;
        if (y < Map.instance.posY - 1 && MoveOkay(x, y + 1)) y++;
        if (y > Map.instance.posY + 1 && MoveOkay(x, y - 1)) y--;
        Map.instance.currentFloor.monsters[x, y] = this;
    }

    private bool MoveOkay(int x, int y) {
        if (Map.instance.currentFloor.layout[x, y].character != ".") return false;
        if (Map.instance.currentFloor.monsters[x, y] != null) return false;
        return true;
    }

    private void Attack() {
        var dx = Mathf.Abs(x - Map.instance.posX);
        var dy = Mathf.Abs(y - Map.instance.posY);
        if (dx <= 1 && dy <= 1) {
            Player.instance.hp -= 2;
            UserInterface.Log("The goblin hits you with a club, dealing 2 damage.");
        }
    }
}
