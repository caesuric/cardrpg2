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
    public int energy = 5;
    public int actions = 4;
    public int hp = 10;
    public int maxHp = 10;

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
            text = "Deal 5 damage to an enemy at range 5.",
            effects = new List<CardEffect>() {
                new CardEffect() {
                    type="range",
                    value=5,
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
}
