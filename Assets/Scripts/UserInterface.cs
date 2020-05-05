using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInterface : MonoBehaviour {
    public static UserInterface instance;
    public int framesToCount = 300;
    private List<float> counts = new List<float>();
    public List<Card> cards = new List<Card>();
    // Start is called before the first frame update
    void Start() {
        instance = this;
    }

    void Update() {
        //fps tracking
        counts.Add(Mathf.Floor(1f / Time.unscaledDeltaTime));
        if (counts.Count > framesToCount) counts.RemoveAt(0);
    }

    public static void Draw() {
        instance.DrawCards();
        instance.DrawFPS();
    }

    private int AverageCount() {
        var total = 0f;
        foreach (var item in counts) total += item;
        total /= counts.Count;
        return (int)Mathf.Floor(total);
    }

    private void DrawCards() {
        for (int x = 0; x < VirtualConsole.instance.width; x++) {
            for (int y = 0; y < 15; y++) {
                VirtualConsole.Set(x, y, " ");
            }
        }
        for (int x = 0; x < VirtualConsole.instance.width; x++) {
            VirtualConsole.Set(x, 0, "\u2550");
            VirtualConsole.Set(x, 15, "\u2550");
        }
        for (int y = 0; y < 15; y++) {
            VirtualConsole.Set(0, y, "\u2551");
            VirtualConsole.Set(VirtualConsole.instance.width - 1, y, "\u2551");
        }
        VirtualConsole.Set(0, 0, "\u255a");
        VirtualConsole.Set(0, 15, "\u2554");
        VirtualConsole.Set(VirtualConsole.instance.width - 1, 0, "\u255d");
        VirtualConsole.Set(VirtualConsole.instance.width - 1, 15, "\u2557");
        if (cards.Count == 0) SetUpCardPositions(5);
        DrawIndividualCards();
    }

    private void SetUpCardPositions(int count) {
        int cardSize = (VirtualConsole.instance.width - 2) / count;
        int sidePadding = (VirtualConsole.instance.width - 1) % count / 2;
        for (int i = 0; i < count; i++) {
            Card.size = new Vector2(cardSize, 14);
            cards.Add(new Card {
                position = new Vector2(sidePadding + (cardSize * i) + 1, 1),
                originalPosition = new Vector2(sidePadding + (cardSize * i) + 1, 1),
                beingDragged = false
            });
        }
    }

    private void DrawIndividualCards() {
        int cardSize = (int)Card.size.x;
        foreach (var card in cards) {
            var active = card.beingDragged;
            var position = card.position;
            for (int x = (int)position.x; x < (int)position.x + cardSize; x++) {
                for (int y = (int)position.y; y < (int)position.y + 14; y++) {
                    if (!active) VirtualConsole.Set(x, y, " ");
                    else VirtualConsole.Set(x, y, " ", 1, 1, 1, 0, 0.5f, 0);
                }
            }
            for (int x = (int)position.x; x < (int)position.x + cardSize; x++) {
                if (!active) {
                    VirtualConsole.Set(x, (int)position.y, "\u2550");
                    VirtualConsole.Set(x, (int)position.y + 13, "\u2550");
                }
                else {
                    VirtualConsole.Set(x, (int)position.y, "\u2550", 1, 1, 1, 0, 0.5f, 0);
                    VirtualConsole.Set(x, (int)position.y + 13, "\u2550", 1, 1, 1, 0, 0.5f, 0);
                }
            }
            for (int y = (int)position.y; y < (int)position.y + 14; y++) {
                if (!active) {
                    VirtualConsole.Set((int)position.x, y, "\u2551");
                    VirtualConsole.Set((int)position.x + cardSize - 1, y, "\u2551");
                }
                else {
                    VirtualConsole.Set((int)position.x, y, "\u2551", 1, 1, 1, 0, 0.5f, 0);
                    VirtualConsole.Set((int)position.x + cardSize - 1, y, "\u2551", 1, 1, 1, 0, 0.5f, 0);
                }
            }
            if (!active) {
                VirtualConsole.Set((int)position.x, (int)position.y, "\u255a");
                VirtualConsole.Set((int)position.x, (int)position.y + 13, "\u2554");
                VirtualConsole.Set((int)position.x + cardSize - 1, (int)position.y, "\u255d");
                VirtualConsole.Set((int)position.x + cardSize - 1, (int)position.y + 13, "\u2557");
                VirtualConsole.Write("Title", (int)position.x + 1, (int)position.y + 11, cardSize - 4, 1);
                VirtualConsole.Write("The quick red fox jumped over the lazy dogs. Black sphinx of quartz, heed my vow.", (int)position.x + 1, (int)position.y + 2, cardSize - 4, 8);
            }
            else {
                VirtualConsole.Set((int)position.x, (int)position.y, "\u255a", 1, 1, 1, 0, 0.5f, 0);
                VirtualConsole.Set((int)position.x, (int)position.y + 13, "\u2554", 1, 1, 1, 0, 0.5f, 0);
                VirtualConsole.Set((int)position.x + cardSize - 1, (int)position.y, "\u255d", 1, 1, 1, 0, 0.5f, 0);
                VirtualConsole.Set((int)position.x + cardSize - 1, (int)position.y + 13, "\u2557", 1, 1, 1, 0, 0.5f, 0);
                VirtualConsole.Write("Title", (int)position.x + 1, (int)position.y + 11, cardSize - 4, 1, 1, 1, 1, 0, 0.5f, 0);
                VirtualConsole.Write("The quick red fox jumped over the lazy dogs. Black sphinx of quartz, heed my vow.", (int)position.x + 1, (int)position.y + 2, cardSize - 4, 8, 1, 1, 1, 0, 0.5f, 0);
            }
        }
    }

    private void DrawFPS() {
        var fps = AverageCount().ToString();
        fps += " FPS";
        VirtualConsole.Write(fps, VirtualConsole.instance.width - 7, VirtualConsole.instance.height - 2, 7, 1, 0, 1, 0, 0.5f, 0.5f, 0.5f);
    }
}
