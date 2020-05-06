﻿using UnityEngine;
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
    public int hp = 10;
    public int maxHp = 10;
    public int level = 1;
    public int experience = 0;

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
        for (int i = 0; i < 5; i++) deck.Add(new Card { template = firebolt });
        for (int i = 0; i < 5; i++) deck.Add(new Card { template = energyBoost });
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
        if (card.template.ContainsEffect("damage") && card.template.ContainsEffect("range")) {
            Inputs.instance.mouseMode = MouseMode.Targeting;
            Inputs.instance.mouseRange = (int)card.template.FindEffect("range").value;
        }
        if (card.template.ContainsEffect("gainEnergy")) energy += (int)card.template.FindEffect("gainEnergy").value;
        if (Inputs.instance.mouseMode != MouseMode.Targeting && actions <= 0) CombatManager.instance.TriggerMonsterTurn();
    }

    public void FireProjectile(int x, int y) {
        Map.instance.projectiles[Map.instance.posX, Map.instance.posY] = new Projectile {
            display = new DisplayCharacter {
                character = "\u256c",
                color = Color.yellow,
                bgColor = Color.red
            },
            xDest = x,
            yDest = y,
            x = Map.instance.posX,
            y = Map.instance.posY,
            range = Inputs.instance.mouseRange
        };
    }

    public void ResolveTargetedCard(Projectile projectile) {
        var x = projectile.x;
        var y = projectile.y;
        if (Map.instance.monsters[x, y] != null) Map.instance.monsters[x, y].hp -= (int)justPlayed.template.FindEffect("damage").value;
        if (Map.instance.monsters[x, y] != null && Map.instance.monsters[x, y].hp <= 0) Map.instance.monsters[x, y] = null;
        CombatManager.instance.CheckIfInCombat();
        if (CombatManager.instance.inCombat && actions <= 0) CombatManager.instance.TriggerMonsterTurn();
    }
}
