using System;
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

    public Player() {
        instance = this;
    }
}
