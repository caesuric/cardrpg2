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

    public Monster() {
        instances.Add(this);
    }

    public void Act() {
        Move();
        Attack();
    }

    private void Move() {
        for (int i=0; i<4; i++) {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer() {
        Map.instance.monsters[x, y] = null;
        if (x < Map.instance.posX - 1 && Map.instance.layout[x + 1, y].character == ".") x++;
        if (x > Map.instance.posX + 1 && Map.instance.layout[x - 1, y].character == ".") x--;
        if (y < Map.instance.posY - 1 && Map.instance.layout[x, y + 1].character == ".") y++;
        if (y > Map.instance.posY + 1 && Map.instance.layout[x, y - 1].character == ".") y--;
        Map.instance.monsters[x, y] = this;
    }

    private void Attack() {
        var dx = Mathf.Abs(x - Map.instance.posX);
        var dy = Mathf.Abs(y - Map.instance.posY);
        if (dx <= 1 && dy <= 1) Player.instance.hp -= 2;
    }
}
