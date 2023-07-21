namespace RoguelikeEngine {
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages combat order.
    /// </summary>
    public class CombatManager : MonoBehaviour {
        /// <summary>
        /// Current singleton instance of the combat manager.
        /// </summary>
        public static CombatManager Instance = null;

        /// <summary>
        /// If it is the player's turn, this will be true.
        /// </summary>
        public bool PlayerTurn = true;

        /// <summary>
        /// If in combat, this will be true.
        /// </summary>
        public bool InCombatValue = false;

        /// <summary>
        /// Initiative order for monsters.
        /// </summary>
        public List<Monster> InitiativeOrder = new List<Monster>();

        /// <summary>
        /// The player's initiative count.
        /// </summary>
        public int PlayerInitiative = 0;

        /// <summary>
        /// Runs on game start.
        /// </summary>
        protected void Start() {
            Instance = this;
        }

        /// <summary>
        /// Checks if the player is in combat.
        /// </summary>
        public void CheckIfInCombat() {
            if (InCombat() && !InCombatValue) {
                RollInitiative();
                ActivateSurpriseRound();
                PlayerTurn = true;
                Player.instance.actions = 4;
                Player.instance.energy = 5;
                InCombatValue = true;
                Map.instance.Draw();
            }
            else if (!InCombat() && InCombatValue) {
                if (Inputs.instance.mouseMode != MouseMode.Animating) Inputs.instance.mouseMode = MouseMode.Default;
                while (Player.instance.hand.Count < 5) Player.instance.DrawCard();
                Player.instance.actions = 4;
                Player.instance.energy = 5;
                UserInterface.instance.SetUpCardPositions();
                InCombatValue = false;
            }
        }

        /// <summary>
        /// Triggers turns for all monsters.
        /// </summary>
        public void TriggerMonsterTurn() {
            PlayerTurn = false;
            foreach (var monster in InitiativeOrder) {
                monster.Act();
            }
            Player.instance.actions = 4;
            PlayerTurn = true;
            Map.instance.Draw();
        }

        private bool InCombat() {
            if (Map.instance.currentFloor == null) return false;
            for (int x = 0; x < Map.instance.currentFloor.monsters.GetLength(0); x++) {
                for (int y = 0; y < Map.instance.currentFloor.monsters.GetLength(1); y++) {
                    if (Map.instance.currentFloor.monsters[x, y] == null) continue;
                    if (Map.instance.Visible(x, y)) return true;
                }
            }
            return false;
        }

        private void RollInitiative() {
            foreach (var monster in Monster.instances) if (monster.floor == Map.instance.currentFloorNumber) monster.initiative = Random.Range(0, 100);
            PlayerInitiative = Random.Range(0, 100);
            InitiativeOrder.Clear();
            foreach (var monster in Monster.instances) if (monster.floor == Map.instance.currentFloorNumber) InitiativeOrder.Add(monster);
            InitiativeOrder.Sort(new MonsterInitiativeComparer());
        }

        private void ActivateSurpriseRound() {
            var tempEnd = new List<Monster>();
            foreach (var monster in InitiativeOrder) {
                if (monster.initiative < PlayerInitiative) {
                    monster.Act();
                    tempEnd.Add(monster);
                }
            }
            foreach (var monster in tempEnd) {
                InitiativeOrder.Remove(monster);
                InitiativeOrder.Add(monster);
            }
        }
    }
}