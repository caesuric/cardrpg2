using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour {
    public bool playerTurn = true;
    public bool inCombat = false;
    public static CombatManager instance = null;

    void Start() {
        instance = this;
    }
}
