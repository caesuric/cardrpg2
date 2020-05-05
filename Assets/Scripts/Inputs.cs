using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs : MonoBehaviour
{
    private bool moved = false;
    private float moveTimer = 0f;
    public static Inputs instance = null;
    public int oldMouseX = 0;
    public int oldMouseY = 0;
    public int mouseX = 0;
    public int mouseY = 0;
    public bool mouseDown = false;
    public int cardDragged = -1;
    public int cardDragCoordsX = -1;
    public int cardDragCoordsY = -1;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (cardDragged == -2) SlideCardBack();

        var prevMouseDown = mouseDown;
        if (Input.GetMouseButtonDown(0)) mouseDown = true;
        if (Input.GetMouseButtonUp(0)) mouseDown = false;
        if (mouseDown && !prevMouseDown && OverCard()) DragCard();
        if (cardDragged > -1 && (oldMouseX != mouseX || oldMouseY != mouseY)) MoveCard();
        if (prevMouseDown && !mouseDown) StopDragCard();
        oldMouseX = mouseX;
        oldMouseY = mouseY;

        if (moved) {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0) {
                moveTimer = 0;
                moved = false;
            }
        }
        var horiz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        if (horiz == 0 && vert == 0) moved = false;
        if (moved) return;
        if (horiz > 0) MoveRight();
        else if (horiz < 0) MoveLeft();
        else if (vert < 0) MoveUp();
        else if (vert > 0) MoveDown();

    }

    private void SlideCardBack() {
        var moved = false;
        foreach (var card in Player.instance.hand) {
            var position = card.position;
            var originalPosition = card.originalPosition;
            if (position == originalPosition) continue;
            moved = true;
            for (int j = 0; j < 4; j++) {
                if (position.x > originalPosition.x) position = new Vector2(position.x - 1, position.y);
                else if (position.x < originalPosition.x) position = new Vector2(position.x + 1, position.y);
                if (position.y > originalPosition.y) position = new Vector2(position.x, position.y - 1);
                else if (position.y < originalPosition.y) position = new Vector2(position.x, position.y + 1);
            }
            card.position = position;
        }
        if (moved) Map.instance.Draw();
        else cardDragged = -1;
    }

    private bool OverCard() {
        foreach (var card in Player.instance.hand) {
            var position = card.position;
            var size = Card.size;
            var x = (int)position.x;
            var y = (int)position.y;
            var sizeX = (int)size.x;
            var sizeY = (int)size.y;
            if (mouseX >= x && mouseX < x + sizeX && mouseY >= y && mouseY < y + sizeY) return true;
        }
        return false;
    }

    private void DragCard() {
        foreach (var card in Player.instance.hand) {
            var position = card.position;
            var size = Card.size;
            var x = (int)position.x;
            var y = (int)position.y;
            var sizeX = (int)size.x;
            var sizeY = (int)size.y;
            if (mouseX >= x && mouseX < x + sizeX && mouseY >= y && mouseY < y + sizeY) {
                cardDragged = Player.instance.hand.IndexOf(card);
                cardDragCoordsX = mouseX - x;
                cardDragCoordsY = mouseY - y;
                card.beingDragged = true;
                Map.instance.Draw();
                return;
            }
        }
    }

    private void MoveCard() {
        Player.instance.hand[cardDragged].position = new Vector2(mouseX - cardDragCoordsX, mouseY - cardDragCoordsY);
        Map.instance.Draw();
    }

    private void StopDragCard() {
        if (cardDragged < 0) return;
        Player.instance.hand[cardDragged].beingDragged = false;
        if (CardPlayed()) {
            var card = Player.instance.hand[cardDragged];
            card.position = card.originalPosition;
            Player.instance.discard.Add(card);
            Player.instance.hand.Remove(card);
        }
        cardDragged = -2;
        UserInterface.instance.SetUpCardPositions();
        Map.instance.Draw();
    }

    private bool CardPlayed() {
        var card = Player.instance.hand[cardDragged];
        if (card.position.y > 14) return true;
        return false;
    }


    private void MoveRight() {
        if (!MoveValid(Map.instance.posX + 1, Map.instance.posY)) return;
        Map.instance.posX++;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private void MoveLeft() {
        if (!MoveValid(Map.instance.posX - 1, Map.instance.posY)) return;
        Map.instance.posX--;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private void MoveUp() {
        if (!MoveValid(Map.instance.posX, Map.instance.posY - 1)) return;
        Map.instance.posY--;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private void MoveDown() {
        if (!MoveValid(Map.instance.posX, Map.instance.posY + 1)) return;
        Map.instance.posY++;
        Map.instance.Draw();
        moveTimer = 0.25f;
        moved = true;
    }

    private bool MoveValid(int x, int y) {
        if (Map.instance.monsters[x, y] != "") return false;
        if (Map.instance.layout[x, y] == "." || Map.instance.layout[x, y] == "+") {
            Map.instance.layout[x, y] = ".";
            return true;
        }
        return false;
    }
}
