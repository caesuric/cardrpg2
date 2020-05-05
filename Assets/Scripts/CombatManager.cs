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
            if (!inCombat) playerTurn = true;
            inCombat = true;
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
}
