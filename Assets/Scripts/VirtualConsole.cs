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
                Set(cursorX, cursorY, letter.ToString(), textR, textG, textB, bgR, bgG, bgB);
                cursorX++;
            }
            if (cursorX <= x + width) {
                Set(cursorX, cursorY, " ", textR, textG, textB, bgR, bgG, bgB);
                cursorX++;
            }
        }
    }
}
