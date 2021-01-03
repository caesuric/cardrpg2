using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Player {
    public static Player instance;
    public List<Card> deck = new List<Card>();
    public List<Card> hand = new List<Card>();
    public List<Card> discard = new List<Card>();
    public List<Card> inPlay = new List<Card>();
    public Card justPlayed = null;
    public int energy = 5;
    public int actions = 4;
    public int hp = 30;
    public int maxHp = 30;
    public int level = 1;
    public int experience = 0;
    public int experienceToLevel = 10;
    public string name = "Bob";

    public Player() {
        instance = this;
        var energyBoost = new CardTemplate() {
            cost = 0,
            name = "Energy Boost",
            text = "Gain 3 energy.",
            effects = new List<CardEffect>() {
                new CardEffect() {
                    type = "gainEnergy",
                    value = 3
                }
            }
        };
        var firebolt = new CardTemplate() {
            cost = 2,
            name = "Firebolt",
            text = "Deal 5 damage to an enemy at range 10.",
            effects = new List<CardEffect>() {
                new CardEffect() {
                    type="range",
                    value=10,
                },
                new CardEffect() {
                    type="damage",
                    value=5
                }
            }
        };
        var fireball = new CardTemplate() {
            cost = 6,
            name = "Fireball",
            text = "Deal 5 damage at range 10 with radius 2.",
            effects = new List<CardEffect>() {
                new CardEffect() {
                    type="range",
                    value=10
                },
                new CardEffect() {
                    type="damage",
                    value=5
                },
                new CardEffect() {
                    type="radius",
                    value=2
                }
            }
        };
        for (int i = 0; i < 4; i++) deck.Add(new Card { template = firebolt });
        for (int i = 0; i < 4; i++) deck.Add(new Card { template = energyBoost });
        for (int i = 0; i < 2; i++) deck.Add(new Card { template = fireball });
        ShuffleDeck();
        DrawCards(5);
    }

    public void ShuffleDeck() {
        var tempDeck = new List<Card>();
        while (deck.Count>0) {
            int roll = Random.Range(0, deck.Count);
            tempDeck.Add(deck[roll]);
            deck.RemoveAt(roll);
        }
        deck = tempDeck;
    }

    public void DrawCard() {
        if (deck.Count == 0) {
            while (discard.Count > 0) {
                int roll = Random.Range(0, discard.Count);
                deck.Add(discard[roll]);
                discard.RemoveAt(roll);
            }
        }
        if (deck.Count > 0) {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }
    }

    public void DrawCards(int n) {
        for (int i = 0; i < n; i++) DrawCard();
    }

    public void PlayCard(Card card) {
        if (energy < card.template.cost) return;
        if (actions < 1) return;
        energy -= card.template.cost;
        actions--;
        justPlayed = card;
        UserInterface.Log("You play " + card.template.name + ".");
        if (card.template.ContainsEffect("damage") && card.template.ContainsEffect("range")) {
            Inputs.instance.mouseMode = MouseMode.Targeting;
            Inputs.instance.mouseRange = (int)card.template.FindEffect("range").value;
        }
        if (card.template.ContainsEffect("gainEnergy")) energy += (int)card.template.FindEffect("gainEnergy").value;
        if (Inputs.instance.mouseMode != MouseMode.Targeting && actions <= 0) CombatManager.instance.TriggerMonsterTurn();
    }

    public void FireProjectile(int x, int y) {
        if (justPlayed.template.ContainsEffect("radius")) UserInterface.Log("You launch a fireball!");
        else UserInterface.Log("You launch a firebolt!");
        var proj = new Projectile {
            display = new DisplayCharacter {
                character = "\u256c",
                color = Color.yellow,
                bgColor = Color.red
            },
            xDest = x,
            yDest = y,
            x = Map.instance.posX,
            y = Map.instance.posY,
            range = Inputs.instance.mouseRange,
        };
        if (justPlayed.template.ContainsEffect("radius")) proj.radius = (int)justPlayed.template.FindEffect("radius").value;
        Map.instance.currentFloor.projectiles[Map.instance.posX, Map.instance.posY] = proj;
    }

    public void ResolveTargetedCard(Projectile projectile) {
        var x = projectile.x;
        var y = projectile.y;
        if (justPlayed.template.ContainsEffect("radius")) {
            var radius = (int)justPlayed.template.FindEffect("radius").value;
            for (int xOf = x - radius; xOf <= x + radius; xOf++) {
                for (int yOf = y - radius; yOf <= y + radius; yOf++) {
                    if (Map.instance.currentFloor.monsters[xOf, yOf] != null) {
                        UserInterface.Log("The fireball strikes the goblin, dealing " + ((int)justPlayed.template.FindEffect("damage").value).ToString() + " damage.");
                        Map.instance.currentFloor.monsters[xOf, yOf].hp -= (int)justPlayed.template.FindEffect("damage").value;
                    }
                    CheckForMonsterDeath(Map.instance.currentFloor.monsters[xOf, yOf]);
                }
            }
        }
        else {
            if (Map.instance.currentFloor.monsters[x, y] != null) {
                UserInterface.Log("The firebolt strikes the goblin, dealing " + ((int)justPlayed.template.FindEffect("damage").value).ToString() + " damage.");
                Map.instance.currentFloor.monsters[x, y].hp -= (int)justPlayed.template.FindEffect("damage").value;
            }
            CheckForMonsterDeath(Map.instance.currentFloor.monsters[x, y]);
        }
        CheckForTurnOver();
    }

    public void DefaultMeleeAttack(Monster monster) {
        UserInterface.Log("You bodyslam the goblin, dealing 2 damage!");
        monster.hp -= 2;
        CheckForMonsterDeath(monster);
        CheckForTurnOver();
    }

    private void CheckForMonsterDeath(Monster monster) {
        if (monster == null) return;
        if (monster.hp <= 0) {
            UserInterface.Log("The goblin dies.");
            GainExperience(1);
            Monster.instances.Remove(monster);
            CombatManager.instance.initiativeOrder.Remove(monster);
            Map.instance.currentFloor.monsters[monster.x, monster.y] = null;
        }
    }

    private void CheckForTurnOver() {
        CombatManager.instance.CheckIfInCombat();
        if (CombatManager.instance.inCombat && actions <= 0) CombatManager.instance.TriggerMonsterTurn();
    }

    private void GainExperience(int amount) {
        experience += amount;
        if (experience>=experienceToLevel) {
            level++;
            maxHp += 10;
            hp = maxHp;
            experienceToLevel *= 2;
            experience = 0;
        }
    }
}
