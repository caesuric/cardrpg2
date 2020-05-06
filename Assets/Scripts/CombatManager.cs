using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour {
    public bool playerTurn = true;
    public bool inCombat = false;
    public static CombatManager instance = null;

    void Start() {
        instance = this;
    }

    public void CheckIfInCombat() {
        if (InCombat()) {
            if (!inCombat) {
                playerTurn = true;
                Player.instance.actions = 4;
                Player.instance.energy = 5;
            }
            inCombat = true;
        }
        else {
            if (inCombat) {
                Inputs.instance.mouseMode = MouseMode.Default;
                while (Player.instance.hand.Count < 5) Player.instance.DrawCard();
                UserInterface.instance.SetUpCardPositions();
            }
            inCombat = false;
        }
    }

    private bool InCombat() {
        for (int x=0; x<Map.instance.monsters.GetLength(0); x++) {
            for (int y=0; y<Map.instance.monsters.GetLength(1); y++) {
                if (Map.instance.monsters[x, y] == null) continue;
                if (Map.instance.Visible(x, y)) return true;
            }
        }
        return false;
    }

    public void TriggerMonsterTurn() {
        foreach (var monster in Monster.instances) {
            monster.Act();
        }
        Player.instance.actions = 4;
        Map.instance.Draw();
    }
}
