using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour {
    public bool playerTurn = true;
    public bool inCombat = false;
    public static CombatManager instance = null;
    public List<Monster> initiativeOrder = new List<Monster>();
    public int playerInitiative = 0;

    void Start() {
        instance = this;
    }

    public void CheckIfInCombat() {
        if (InCombat() && !inCombat) {
            RollInitiative();
            ActivateSurpriseRound();
            playerTurn = true;
            Player.instance.actions = 4;
            Player.instance.energy = 5;
            inCombat = true;
            Map.instance.Draw();
        }
        else if (!InCombat() && inCombat) {
            Inputs.instance.mouseMode = MouseMode.Default;
            while (Player.instance.hand.Count < 5) Player.instance.DrawCard();
            Player.instance.actions = 4;
            Player.instance.energy = 5;
            UserInterface.instance.SetUpCardPositions();
            inCombat = false;
        }
    }

    private bool InCombat() {
        for (int x = 0; x < Map.instance.currentFloor.monsters.GetLength(0); x++) {
            for (int y = 0; y < Map.instance.currentFloor.monsters.GetLength(1); y++) {
                if (Map.instance.currentFloor.monsters[x, y] == null) continue;
                if (Map.instance.Visible(x, y)) return true;
            }
        }
        return false;
    }

    public void TriggerMonsterTurn() {
        playerTurn = false;
        foreach (var monster in initiativeOrder) {
            monster.Act();
        }
        Player.instance.actions = 4;
        playerTurn = true;
        Map.instance.Draw();
    }

    private void RollInitiative() {
        foreach (var monster in Monster.instances) if (monster.floor==Map.instance.currentFloorNumber) monster.initiative = Random.Range(0, 100);
        playerInitiative = Random.Range(0, 100);
        initiativeOrder.Clear();
        foreach (var monster in Monster.instances) if (monster.floor==Map.instance.currentFloorNumber) initiativeOrder.Add(monster);
        initiativeOrder.Sort(new MonsterInitiativeComparer());
    }

    private void ActivateSurpriseRound() {
        var tempEnd = new List<Monster>();
        foreach (var monster in initiativeOrder) {
            if (monster.initiative < playerInitiative) {
                monster.Act();
                tempEnd.Add(monster);
            }
        }
        foreach (var monster in tempEnd) {
            initiativeOrder.Remove(monster);
            initiativeOrder.Add(monster);
        }
    }
}

public class MonsterInitiativeComparer : IComparer<Monster> {
    public int Compare(Monster m1, Monster m2) {
        if (m1.initiative < m2.initiative) return -1;
        else if (m1.initiative > m2.initiative) return 1;
        else return 0;
    }
}