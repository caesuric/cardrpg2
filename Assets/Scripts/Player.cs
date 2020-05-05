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
        for (int i = 0; i < 10; i++) deck.Add(new Card { template = energyBoost });
        DrawCards(5);
    }

    public void DrawCard() {
        if (deck.Count == 0) {
            while (discard.Count > 0) {
                int roll = Random.Range(0, discard.Count);
                deck.Add(discard[roll]);
                discard.RemoveAt(roll);
            }
        }
        hand.Add(deck[0]);
        deck.RemoveAt(0);
    }

    public void DrawCards(int n) {
        for (int i = 0; i < n; i++) DrawCard();
    }
}
