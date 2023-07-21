namespace RoguelikeEngine {
    using System.Collections.Generic;

    /// <summary>
    /// Compares initiative for a pair of monsters.
    /// </summary>
    public class MonsterInitiativeComparer : IComparer<Monster> {
        /// <summary>
        /// Compares initiative for a pair of monsters.
        /// </summary>
        /// <param name="m1">First monster.</param>
        /// <param name="m2">Second monster.</param>
        /// <returns>-1 if first monster initiative is lower, 0 if equal, 1 if higher.</returns>
        public int Compare(Monster m1, Monster m2) {
            if (m1.initiative < m2.initiative) return -1;
            else if (m1.initiative > m2.initiative) return 1;
            else return 0;
        }
    }
}
