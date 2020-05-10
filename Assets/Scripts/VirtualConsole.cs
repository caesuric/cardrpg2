using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualConsole : MonoBehaviour {
    public GameObject textPrefab;
    public GameObject bgPrefab;
    public GameObject letterCollection;
    public int width;
    public int height;
    public float xOffset;
    public float yOffset;
    public SpriteRenderer[,] backgroundBlocks;
    public TextMesh[,] letters;
    public static VirtualConsole instance = null;
    // Start is called before the first frame update
    void Start() {
        instance = this;
        backgroundBlocks = new SpriteRenderer[width, height];
        letters = new TextMesh[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float xPos = (xOffset * 2 * (x + 1) / (width + 1)) - xOffset;
                float yPos = (yOffset * 2 * (y + 1) / (height + 1)) - yOffset;
                var go = Instantiate(bgPrefab);
                go.GetComponent<MapBlockMouseControls>().x = x;
                go.GetComponent<MapBlockMouseControls>().y = y;
                go.transform.position = new Vector3(xPos, yPos, -0.25f);
                go.transform.SetParent(letterCollection.transform);
                backgroundBlocks[x, y] = go.GetComponent<SpriteRenderer>();
                //float randomR = Random.Range(0f, 1f);
                //float randomG = Random.Range(0f, 1f);
                //float randomB = Random.Range(0f, 1f);
                //backgroundBlocks[x, y].color = new Color(randomR, randomG, randomB);
                backgroundBlocks[x, y].color = new Color(0, 0, 0);
                var go2 = Instantiate(textPrefab);
                go2.transform.position = new Vector3(xPos, yPos, -0.25f);
                go2.transform.SetParent(letterCollection.transform);
                letters[x, y] = go2.GetComponent<TextMesh>();
                //int randomLetter = Random.Range(0, 52);
                //if (randomLetter < 26) letters[x, y].text = ((char)('a' + randomLetter)).ToString();
                //else letters[x, y].text = ((char)('A' + randomLetter - 26)).ToString();
                letters[x, y].text = " ";
                //randomR = Random.Range(0f, 1f);
                //randomG = Random.Range(0f, 1f);
                //randomB = Random.Range(0f, 1f);
                //letters[x, y].color = new Color(randomR, randomG, randomB);
                letters[x, y].color = new Color(1, 1, 1);
            }
        }
    }

    public static void Set(int x, int y, string character, float textR = 1, float textG = 1, float textB = 1, float bgR = 0, float bgG = 0, float bgB = 0) {
        if (x < 0 || y < 0 || x >= instance.width || y >= instance.height) return;
        instance.letters[x, y].text = character;
        instance.letters[x, y].color = new Color(textR, textG, textB);
        instance.backgroundBlocks[x, y].color = new Color(bgR, bgG, bgB);
    }

    public static void ColorBlock(int x, int y, float r, float g, float b) {
        if (x < 0 || y < 0 || x >= instance.width || y >= instance.height) return;
        instance.backgroundBlocks[x, y].color = new Color(r, g, b);
    }

    public static void Write(string text, int x, int y, int width, int height, float textR = 1, float textG = 1, float textB = 1, float bgR = 0, float bgG = 0, float bgB = 0) {
        int cursorX = x;
        int cursorY = y + height;
        var words = text.Split(' ');
        foreach (var word in words) {
            if (cursorX + word.Length - 1 > x + width) {
                cursorX = x;
                cursorY--;
                if (cursorY > y + height) return;
            }
            foreach (var letter in word) {
                if (letter == '\n') {
                    cursorX = x;
                    cursorY--;
                }
                else {
                    Set(cursorX, cursorY, letter.ToString(), textR, textG, textB, bgR, bgG, bgB);
                    cursorX++;
                }
            }
            if (cursorX <= x + width) {
                Set(cursorX, cursorY, " ", textR, textG, textB, bgR, bgG, bgB);
                cursorX++;
            }
        }
    }

    public static void DrawBar(string text, int current, int maximum, int x, int y, int width, float textR = 1, float textG = 1, float textB = 1, float bgR = 0, float bgG = 0, float bgB = 0, float bgR2 = 0, float bgG2 = 0, float bgB2 = 0) {
        float percent = (float)current / maximum;
        int amount = (int)(percent * width);
        Write(text, x, y, width, 1, textR, textG, textB);
        for (int xPos = x; xPos < x + width; xPos++) {
            if (xPos - x < amount) instance.backgroundBlocks[xPos, y + 1].color = new Color(bgR, bgG, bgB);
            else instance.backgroundBlocks[xPos, y + 1].color = new Color(bgR2, bgG2, bgB2);
        }
    }

    public static void DrawBox(int x0, int y0, int width, int height, bool active = false) {
        var r = 0.1f;
        var g = 0.1f;
        var b = 0.1f;
        if (active) {
            r = 0;
            g = 0.5f;
            b = 0;
        }
        for (int x = x0; x < x0 + width; x++) {
            for (int y = y0; y < y0 + height; y++) {
                if (!active) Set(x, y, " ");
                else Set(x, y, " ", 1, 1, 1, 0, 0.5f, 0);
            }
        }
        for (int x = x0; x < x0 + width; x++) {
            Set(x, y0, "\u2550", 1, 1, 1, r, g, b);
            Set(x, y0 + height, "\u2550", 1, 1, 1, r, g, b);
        }

        for (int y = y0; y < y0 + height; y++) {
            Set(x0, y, "\u2551", 1, 1, 1, r, g, b);
            Set(x0 + width, y, "\u2551", 1, 1, 1, r, g, b);
        }
        Set(x0, y0, "\u255a", 1, 1, 1, r, g, b);
        Set(x0, y0 + height, "\u2554", 1, 1, 1, r, g, b);
        Set(x0 + width, y0, "\u255d", 1, 1, 1, r, g, b);
        Set(x0 + width, y0 + height, "\u2557", 1, 1, 1, r, g, b);
    }
}
