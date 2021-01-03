using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

public class Map : MonoBehaviour {
    public int posX = 0;
    public int posY = 0;
    public int currentFloorNumber = 0;
    public MapFloor currentFloor = null;
    public List<MapFloor> floors = new List<MapFloor>();
    public int numFloors = 5;
    private bool initialized = false;
    private bool loaded = false;
    public static Map instance = null;

    void Start() {
        instance = this;
        new Player();
    }

    void Update() {
        if (!initialized && Inputs.instance.mouseMode == MouseMode.Default) {
            initialized = true;
            if (!loaded) {
                BuildFloors();
                posX = currentFloor.startingX;
                posY = currentFloor.startingY;
            }
            UserInterface.instance.SetUpCardPositions();
            if (!loaded) Save();
        }
    }

    private void OnApplicationQuit() {
        if (Player.instance.hp > 0) Save();
    }

    public void Save() {
        var save = new Dictionary<string, object> {
            { "posX", posX },
            { "posY", posY },
            { "currentFloorNumber", currentFloorNumber },
            { "floors", SerializeFloors() },
            { "monsters", SerializeMonsters() },
            { "numFloors", numFloors },
            { "player", SerializePlayer() }
        };
        var output = JsonConvert.SerializeObject(save, Formatting.Indented);
        var saveDirectory = Application.persistentDataPath;
        File.WriteAllText(Path.Combine(saveDirectory, Player.instance.name + ".json"), output);
    }

    public void DeleteSave() {
        var saveDirectory = Application.persistentDataPath;
        var filePath = Path.Combine(saveDirectory, Player.instance.name + ".json");
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    private List<Dictionary<string, object>> SerializeFloors() {
        var output = new List<Dictionary<string, object>>();
        foreach (var floor in floors) {
            var floorOutput = new Dictionary<string, object>();
            var layout = "";
            var seen = "";
            for (int x=0; x<floor.layout.GetLength(0); x++) {
                for (int y=0; y<floor.layout.GetLength(1); y++) {
                    layout += floor.layout[x, y].character;
                    if (floor.seen[x, y]) seen += "1";
                    else seen += "0";
                }
            }
            floorOutput["layout"] = layout;
            floorOutput["seen"] = seen;
            floorOutput["width"] = floor.layout.GetLength(0);
            floorOutput["height"] = floor.layout.GetLength(1);
            output.Add(floorOutput);
        }
        return output;
    }

    private void DeserializeFloors(object rawData) {
        var data = (JArray)rawData;
        floors.Clear();
        int floorNumber = 0;
        foreach (var floor in data) {
            var floorOutput = new MapFloor(floorNumber);
            int cursor = 0;
            for (int x = 0; x<(int)(long)floor["width"]; x++) {
                for (int y=0; y<(int)(long)floor["height"]; y++) {
                    floorOutput.layout[x, y].character = ((string)floor["layout"])[cursor].ToString();
                    floorOutput.seen[x, y] = ((string)floor["layout"])[cursor].ToString() == "1";
                    cursor++;
                }
            }
            floorNumber++;
            floors.Add(floorOutput);
        }
    }

    private List<Dictionary<string, object>> SerializeMonsters() {
        var output = new List<Dictionary<string, object>>();
        foreach (var monster in Monster.instances) {
            var displayChar = new Dictionary<string, object> {
                ["character"] = monster.display.character,
                ["colorR"] = monster.display.color.r,
                ["colorG"] = monster.display.color.g,
                ["colorB"] = monster.display.color.b,
                ["bgColorR"] = monster.display.bgColor.r,
                ["bgColorG"] = monster.display.bgColor.g,
                ["bgColorB"] = monster.display.bgColor.b
            };
            var monsterOutput = new Dictionary<string, object> {
                ["hp"] = monster.hp,
                ["maxHp"] = monster.maxHp,
                ["displayCharacter"] = displayChar,
                ["x"] = monster.x,
                ["y"] = monster.y,
                ["floor"] = monster.floor,
                ["initiative"] = monster.initiative
            };
            output.Add(monsterOutput);
        }
        return output;
    }

    private void DeserializeMonsters(object rawData) {
        var data = (JArray)rawData;
        Monster.instances.Clear();
        foreach (var monster in data) {
            var displayCharacter = (JObject)monster["displayCharacter"];
            var monsterOutput = new Monster {
                hp = (int)(long)monster["hp"],
                maxHp = (int)(long)monster["maxHp"],
                x = (int)(long)monster["x"],
                y = (int)(long)monster["y"],
                floor = (int)(long)monster["floor"],
                initiative = (int)(long)monster["initiative"],
                display = new DisplayCharacter {
                    character = (string)displayCharacter["character"],
                    color = new Color((float)displayCharacter["colorR"], (float)displayCharacter["colorG"], (float)displayCharacter["colorB"]),
                    bgColor = new Color((float)displayCharacter["bgColorR"], (float)displayCharacter["bgColorG"], (float)displayCharacter["bgColorB"])
                }
            };
            Monster.instances.Add(monsterOutput);
        }

        foreach (var monster in Monster.instances) {
            floors[monster.floor].monsters[monster.x, monster.y] = monster;
        }
    }

    private Dictionary<string, object> SerializePlayer() {
        var output = new Dictionary<string, object> {
            ["energy"] = Player.instance.energy,
            ["actions"] = Player.instance.actions,
            ["hp"] = Player.instance.hp,
            ["maxHp"] = Player.instance.maxHp,
            ["level"] = Player.instance.level,
            ["experience"] = Player.instance.experience,
            ["experienceToLevel"] = Player.instance.experienceToLevel,
            ["name"] = Player.instance.name,
            ["cardTemplates"] = SerializeCardTemplates(),
            ["deck"] = SerializeCards(Player.instance.deck),
            ["hand"] = SerializeCards(Player.instance.hand),
            ["discard"] = SerializeCards(Player.instance.discard),
            ["inPlay"] = SerializeCards(Player.instance.inPlay),
            ["justPlayed"] = SerializeCard(Player.instance.justPlayed)
        };
        return output;
    }

    private void DeserializePlayer(object rawData) {
        var data = (JObject)rawData;
        Player.instance.energy = (int)(long)data["energy"];
        Player.instance.actions = (int)(long)data["actions"];
        Player.instance.hp = (int)(long)data["hp"];
        Player.instance.maxHp = (int)(long)data["maxHp"];
        Player.instance.level = (int)(long)data["level"];
        Player.instance.experience = (int)(long)data["experience"];
        Player.instance.experienceToLevel = (int)(long)data["experienceToLevel"];
        Player.instance.name = (string)data["name"];
        DeserializeCardTemplates(data["cardTemplates"]);
        Player.instance.deck = DeserializeCards(data["deck"]);
        Player.instance.hand = DeserializeCards(data["hand"]);
        Player.instance.discard = DeserializeCards(data["discard"]);
        Player.instance.inPlay = DeserializeCards(data["inPlay"]);
        Player.instance.justPlayed = DeserializeCard(data["justPlayed"]);
    }

    private List<Dictionary<string, object>> SerializeCardTemplates() {
        var output = new List<Dictionary<string, object>>();
        foreach (var template in CardTemplate.instances) {
            var effects = new List<Dictionary<string, object>>();
            foreach (var effect in template.effects) {
                effects.Add(new Dictionary<string, object> {
                    ["type"] = effect.type,
                    ["value"] = effect.value
                });
            }
            var templateOutput = new Dictionary<string, object> {
                ["name"] = template.name,
                ["cost"] = template.cost,
                ["text"] = template.text,
                ["effects"] = effects
            };
            output.Add(templateOutput);
        }
        return output;
    }

    private void DeserializeCardTemplates(object rawData) {
        var data = (JArray)rawData;
        CardTemplate.instances.Clear();
        foreach (var template in data) {
            var effects = (JArray)template["effects"];
            var cardOutput = new CardTemplate {
                name = (string)template["name"],
                cost = (int)(long)template["cost"],
                text = (string)template["text"]
            };
            cardOutput.effects = new List<CardEffect>();
            foreach (var effect in effects) {
                cardOutput.effects.Add(new CardEffect {
                    type = (string)effect["type"],
                    value = (float)effect["value"]
                });
            }
        }
    }

    private List<int> SerializeCards(List<Card> cards) {
        var output = new List<int>();
        foreach (var card in cards) output.Add(SerializeCard(card));
        return output;
    }

    private List<Card> DeserializeCards(object rawData) {
        var data = (JArray)rawData;
        var output = new List<Card>();
        foreach (var card in data) {
            output.Add(new Card {
                template = CardTemplate.instances[(int)(long)card]
            });
        }
        return output;
    }

    private int SerializeCard(Card card) {
        if (card == null) return -1;
        return CardTemplate.instances.IndexOf(card.template);
    }

    private Card DeserializeCard(object rawData) {
        int data = (int)((JValue)rawData).ToObject(typeof(int));
        if (data == -1) return null;
        return new Card {
            template = CardTemplate.instances[data]
        };
    }

    public void Load(string filename) {
        //todo: load
        var data = File.ReadAllText(filename);
        var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
        posX = (int)(long)json["posX"];
        posY = (int)(long)json["posY"];
        currentFloorNumber = (int)(long)json["currentFloorNumber"];
        numFloors = (int)(long)json["numFloors"];
        DeserializeFloors(json["floors"]);
        currentFloor = floors[currentFloorNumber];
        DeserializeMonsters(json["monsters"]);
        DeserializePlayer(json["player"]);
        loaded = true;
        UserInterface.instance.SetUpCardPositions();
        //tint each map floor
        foreach (var floor in floors) floor.TintMap();
    }

    private void BuildFloors() {
        for (int i = 0; i < 5; i++) floors.Add(new MapFloor(i));
        currentFloor = floors[0];
    }

    public void Draw() {
        if (currentFloor!=null) {
            DrawMap();
            DrawMonsters();
            DrawProjectiles();
        }
        UserInterface.Draw();
        CombatManager.instance.CheckIfInCombat();
    }

    private void DrawMap() {
        var halfWidth = VirtualConsole.instance.width / 2;
        var halfHeight = ((VirtualConsole.instance.height - 15) / 2) + 15;
        for (int x = posX - halfWidth; x < posX + halfWidth; x++) {
            for (int y = posY - halfHeight; y < posY + halfHeight; y++) {
                if (x == posX && y == posY) continue;
                DisplayCharacter dc = null;
                if (Visible(x, y) && GetMonsters(x, y) != "") continue;
                if (Visible(x, y) && GetProjectiles(x, y) != "") continue;
                if (y - posY + halfHeight <= 15) continue;
                if (x >= 0 && y >= 0 && x < MapFloor.xSize && y < MapFloor.ySize) dc = currentFloor.layout[x, y];
                if (dc != null && Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
                else if (Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, Get(x, y));
                else if (dc != null && Seen(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r / 4f, dc.color.g / 4f, dc.color.b / 4f, dc.bgColor.r / 4f, dc.bgColor.g / 4f, dc.bgColor.b / 4f);
                else VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, " ");
            }
        }
        VirtualConsole.Set(halfWidth, halfHeight, "@");
    }

    private void DrawMonsters() {
        var halfWidth = VirtualConsole.instance.width / 2;
        var halfHeight = ((VirtualConsole.instance.height - 15) / 2) + 15;
        for (int x = posX - halfWidth; x < posX + halfWidth; x++) {
            for (int y = posY - halfHeight; y < posY + halfHeight; y++) {
                if (Visible(x, y) && GetProjectiles(x, y) != "") continue;
                if (GetMonsters(x, y) == "") continue;
                DisplayCharacter dc = null;
                if (x >= 0 && y >= 0 && x < MapFloor.xSize && y < MapFloor.ySize) dc = currentFloor.monsters[x, y].display;
                if (dc != null && Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
            }
        }
    }

    private void DrawProjectiles() {
        var halfWidth = VirtualConsole.instance.width / 2;
        var halfHeight = ((VirtualConsole.instance.height - 15) / 2) + 15;
        for (int x = posX - halfWidth; x < posX + halfWidth; x++) {
            for (int y = posY - halfHeight; y < posY + halfHeight; y++) {
                if (GetProjectiles(x, y) == "") continue;
                DisplayCharacter dc = null;
                if (x >= 0 && y >= 0 && x < MapFloor.xSize && y < MapFloor.ySize) dc = currentFloor.projectiles[x, y].display;
                if (dc != null && Visible(x, y)) VirtualConsole.Set(x - posX + halfWidth, y - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
                if (currentFloor.projectiles[x, y].blastStage > -1) {
                    var proj = currentFloor.projectiles[x, y];
                    for (int x2 = proj.x - proj.blastStage; x2 <= proj.x + proj.blastStage; x2++) {
                        for (int y2 = proj.y - proj.blastStage; y2 <= proj.y + proj.blastStage; y2++) {
                            if (Visible(x2, y2)) VirtualConsole.Set(x2 - posX + halfWidth, y2 - posY + halfHeight, dc.character, dc.color.r, dc.color.g, dc.color.b, dc.bgColor.r, dc.bgColor.g, dc.bgColor.b);
                        }
                    }
                }
            }
        }
    }

    private string Get(int x, int y) {
        return currentFloor.Get(x, y);
    }

    private string GetMonsters(int x, int y) {
        return currentFloor.GetMonsters(x, y);
    }

    private string GetProjectiles(int x, int y) {
        return currentFloor.GetProjectiles(x, y);
    }

    public bool Visible(int x, int y) {
        if (currentFloor == null) return false;
        return currentFloor.Visible(x, y);
    }

    public bool BlocksSight(int x, int y) {
        return currentFloor.BlocksSight(x, y);
    }

    public bool BlocksProjectile(int x, int y) {
        return currentFloor.BlocksProjectile(x, y);
    }

    public bool Seen(int x, int y) {
        if (currentFloor == null) return false;
        return currentFloor.Seen(x, y);
    }

    public void ColorBlock(int x, int y, float r, float g, float b) {
        VirtualConsole.ColorBlock(x - instance.posX + VirtualConsole.instance.width / 2, y - instance.posY + ((VirtualConsole.instance.height - 15) / 2) + 15, r, g, b);
    }
}
