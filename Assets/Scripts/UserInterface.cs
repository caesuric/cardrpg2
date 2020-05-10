using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInterface : MonoBehaviour {
    public static UserInterface instance;
    public int framesToCount = 300;
    private List<float> counts = new List<float>();
    private List<string> logMessages = new List<string>();
    private static readonly Dictionary<string, string> mapBlockDescriptions = new Dictionary<string, string> {
        { ".", "Stone floor" },
        { "#", "Stone wall" },
        { "+", "Wooden door" },
        { ">", "Stairs leading down" },
        { "<", "Stairs leading up" }
    };
    private static readonly Dictionary<string, string> monsterDescriptions = new Dictionary<string, string> {
        { "g", "Goblin" }
    };

    void Start() {
        instance = this;
    }

    void Update() {
        //fps tracking
        counts.Add(Mathf.Floor(1f / Time.unscaledDeltaTime));
        if (counts.Count > framesToCount) counts.RemoveAt(0);
    }

    public static void Draw() {
        instance.DrawLog();
        instance.DrawFPS();
        instance.DrawHUD();
        if (Inputs.instance.cardDragged < 0) instance.DrawTooltip();
        instance.DrawCards();
    }

    public static void Log(string message) {
        instance.logMessages.Add(message);
    }

    private void DrawTooltip() {
        var mouseX = Inputs.instance.mouseX;
        var mouseY = Inputs.instance.mouseY;
        if (mouseY <= 15) return;
        var halfWidth = VirtualConsole.instance.width / 2;
        var halfHeight = ((VirtualConsole.instance.height - 15) / 2) + 15;
        var mapX = mouseX + Map.instance.posX - halfWidth;
        var mapY = mouseY + Map.instance.posY - halfHeight;
        if (!Map.instance.Visible(mapX, mapY) && !Map.instance.Seen(mapX, mapY)) return;
        VirtualConsole.DrawBox(mouseX + 1, mouseY + 1, 20, 4);
        WriteTooltipDescription(mapX, mapY, mouseX + 2, mouseY + 1, 18, 3);
    }

    private void WriteTooltipDescription(int mapX, int mapY, int x, int y, int width, int height) {
        var output = "";
        var mapBlock = Map.instance.currentFloor.layout[mapX, mapY];
        if (mapBlockDescriptions.ContainsKey(mapBlock.character)) {
            output += mapBlockDescriptions[mapBlock.character] + "\n";
        }
        if (Map.instance.Visible(mapX,mapY)) {
            var monster = Map.instance.currentFloor.monsters[mapX, mapY];
            if (monster != null && monsterDescriptions.ContainsKey(monster.display.character)) {
                output += monsterDescriptions[monster.display.character] + "\n";
            }
        }
        if (Map.instance.posX == mapX && Map.instance.posY == mapY) output += "You";
        VirtualConsole.Write(output, x, y, width, height);
    }

    private int AverageCount() {
        var total = 0f;
        foreach (var item in counts) total += item;
        total /= counts.Count;
        return (int)Mathf.Floor(total);
    }

    private void DrawLog() {
        VirtualConsole.DrawBox(47, 16, VirtualConsole.instance.width - 48, 7);
        DrawLogMessages();
    }

    private void DrawLogMessages() {
        while (logMessages.Count > 6) logMessages.RemoveAt(0);
        var concat = "";
        foreach (var logMessage in logMessages) concat += logMessage + "\n";
        if (concat!="") concat = concat.Substring(0, concat.Length - 1);
        concat = PruneLinesToFit(concat);
        VirtualConsole.Write(concat, 48, 16, VirtualConsole.instance.width - 50, 6);
    }

    private string PruneLinesToFit(string original) {
        var output = original;
        int lines = 1;
        int cursor = 0;
        foreach (var letter in original) {
            if (letter=='\n') {
                cursor = 0;
                lines++;
            }
            else {
                cursor++;
                if (cursor>VirtualConsole.instance.width-50) {
                    cursor = 0;
                    lines++;
                }
            }
        }
        if (lines > 6) {
            output = output.Substring(output.IndexOf('\n') + 1);
            return PruneLinesToFit(output);
        }
        else return output;
    }

    private void DrawCards() {
        VirtualConsole.DrawBox(0, 0, VirtualConsole.instance.width - 1, 15);
        DrawIndividualCards();
    }

    public void SetUpCardPositions() {
        int cardSize = (VirtualConsole.instance.width - 2) / 5;
        int sidePadding = (VirtualConsole.instance.width - 1) % 5 / 2;
        Card.size = new Vector2(cardSize, 14);

        int i = 0;
        foreach (var card in Player.instance.hand) {
            card.position = new Vector2(sidePadding + (cardSize * i) + 1, 1);
            card.originalPosition = card.position;
            card.beingDragged = false;
            i++;
        }
    }

    private void DrawIndividualCards() {
        int cardSize = (int)Card.size.x;
        foreach (var card in Player.instance.hand) {
            var active = card.beingDragged;
            var position = card.position;
            VirtualConsole.DrawBox((int)position.x, (int)position.y, cardSize - 1, 13, active);
            if (!active) {
                VirtualConsole.Write(card.template.name, (int)position.x + 1, (int)position.y + 11, cardSize - 4, 1);
                VirtualConsole.Write("Cost: " + card.template.cost.ToString(), (int)position.x + 1, (int)position.y + 10, cardSize - 4, 1);
                VirtualConsole.Write(card.template.text, (int)position.x + 1, (int)position.y + 2, cardSize - 4, 8);
            }
            else {
                VirtualConsole.Write(card.template.name, (int)position.x + 1, (int)position.y + 11, cardSize - 4, 1, 1, 1, 1, 0, 0.5f, 0);
                VirtualConsole.Write("Cost: " + card.template.cost.ToString(), (int)position.x + 1, (int)position.y + 10, cardSize - 4, 1, 1, 1, 1, 0, 0.5f, 0);
                VirtualConsole.Write(card.template.text, (int)position.x + 1, (int)position.y + 2, cardSize - 4, 8, 1, 1, 1, 0, 0.5f, 0);
            }
        }
    }

    private void DrawFPS() {
        var fps = AverageCount().ToString();
        fps += " FPS";
        VirtualConsole.Write(fps, VirtualConsole.instance.width - 7, VirtualConsole.instance.height - 2, 7, 1, 0, 1, 0, 0.5f, 0.5f, 0.5f);
    }

    private void DrawHUD() {
        var level = "Level " + Player.instance.level.ToString();
        var hp = "HP: " + Player.instance.hp.ToString() + "/" + Player.instance.maxHp.ToString();
        var actions = "Actions: " + Player.instance.actions.ToString();
        var energy = "Energy: " + Player.instance.energy.ToString();
        var deck = "Cards in Deck: " + Player.instance.deck.Count.ToString();
        var discard = "Cards in Discard: " + Player.instance.discard.Count.ToString();
        var floor = "Floor: " + (Map.instance.currentFloorNumber + 1).ToString();
        var experience = "Experience: " + Player.instance.experience.ToString() + "/" + Player.instance.experienceToLevel.ToString();
        VirtualConsole.Write(floor, 0, VirtualConsole.instance.height - 2, 9, 1, 1, 1, 1, 0.25f, 0.25f, 0.25f);
        VirtualConsole.Write(level, 0, VirtualConsole.instance.height - 3, 10, 1, 1, 1, 1, 0.25f, 0.25f, 0.25f);
        VirtualConsole.DrawBar(hp, Player.instance.hp, Player.instance.maxHp, 0, VirtualConsole.instance.height - 4, 23, 1, 1, 1, 1, 0, 0, 0.5f, 0, 0);
        VirtualConsole.DrawBar(experience, Player.instance.experience, Player.instance.experienceToLevel, 0, VirtualConsole.instance.height - 5, 23, 1, 1, 1, 0, 0, 1, 0, 0, 0.5f);
        VirtualConsole.DrawBar(actions, Player.instance.actions, 4, 0, VirtualConsole.instance.height - 6, 23, 0, 0, 0, 0, 1, 0, 0, 0.5f, 0);
        VirtualConsole.Write(energy, 0, VirtualConsole.instance.height - 7, 10, 1, 1, 1, 1, 0.25f, 0.25f, 0.25f);
        VirtualConsole.Write(deck, 0, VirtualConsole.instance.height - 8, 18, 1, 1, 1, 1, 0.25f, 0.25f, 0.25f);
        VirtualConsole.Write(discard, 0, VirtualConsole.instance.height - 9, 21, 1, 1, 1, 1, 0.25f, 0.25f, 0.25f);
    }
}
