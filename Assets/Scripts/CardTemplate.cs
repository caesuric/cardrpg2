using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CardTemplate {
    public static List<CardTemplate> instances = new List<CardTemplate>();
    public string name;
    public int cost;
    public string text;
    public List<CardEffect> effects;

    public CardTemplate() {
        instances.Add(this);
    }
}
