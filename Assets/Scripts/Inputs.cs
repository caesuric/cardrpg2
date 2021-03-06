﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs : MonoBehaviour
{
    private static readonly float moveTimeout = 0.2f;
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
    public MouseMode mouseMode = MouseMode.MainMenu;
    public int mouseRange = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (cardDragged == -2) SlideCardBack();
        if (mouseMode == MouseMode.Animating) return;
        if (Player.instance.hp <= 0 && mouseMode == MouseMode.Default) {
            Map.instance.DeleteSave();
            mouseMode = MouseMode.MainMenu;
            MenuManager.instance.Draw();
            return;
        }

        if (mouseMode==MouseMode.MainMenu) {
            if (Input.GetMouseButtonUp(0) && MenuManager.instance.ClickedStart()) {
                mouseMode = MouseMode.Default;
                Map.instance.Draw();
            }
            if (Input.GetMouseButtonUp(0) && MenuManager.instance.ClickedLoad()) {
                mouseMode = MouseMode.LoadMenu;
                MenuManager.instance.DrawLoadMenu();
            }
            return;
        }

        if (mouseMode==MouseMode.LoadMenu) {
            if (Input.GetMouseButtonUp(0)) {
                var characterClicked = MenuManager.instance.CharacterClicked();
                if (characterClicked!=null) {
                    Map.instance.Load(characterClicked);
                    mouseMode = MouseMode.Default;
                    Map.instance.Draw();
                }
            }
            return;
        }

        if (CombatManager.instance.inCombat) {
            var prevMouseDown = mouseDown;
            if (Input.GetMouseButtonDown(0)) mouseDown = true;
            if (Input.GetMouseButtonUp(0)) mouseDown = false;
            if (mouseMode == MouseMode.Default) {
                if (mouseDown && !prevMouseDown && OverCard()) DragCard();
                if (cardDragged > -1 && (oldMouseX != mouseX || oldMouseY != mouseY)) MoveCard();
                if (prevMouseDown && !mouseDown) StopDragCard();
            }
            else if (mouseMode == MouseMode.Targeting) {
                DrawTargetingLine();
                if (!mouseDown && prevMouseDown) {
                    mouseMode = MouseMode.Animating;
                    int x = mouseX + Map.instance.posX - VirtualConsole.instance.width / 2;
                    int y = mouseY + Map.instance.posY - (((VirtualConsole.instance.height - 15) / 2) + 15);
                    Player.instance.FireProjectile(x, y);
                }
            }
        }

        //draw for tooltip
        if (mouseX != oldMouseX || mouseY != oldMouseY) Map.instance.Draw();
        oldMouseX = mouseX;
        oldMouseY = mouseY;

        //draw card
        if (CombatManager.instance.inCombat && Player.instance.actions > 0 && Input.GetKeyDown(KeyCode.Space) && Player.instance.hand.Count < 5) {
            Player.instance.actions--;
            Player.instance.DrawCard();
            UserInterface.Log("You draw a card.");
            UserInterface.instance.SetUpCardPositions();
            Map.instance.Draw();
            if (Player.instance.actions <= 0) CombatManager.instance.TriggerMonsterTurn();
        }
        else if (CombatManager.instance.inCombat && mouseMode == MouseMode.Targeting && Input.GetKeyDown(KeyCode.Escape)) {
            mouseMode = MouseMode.Default;
            Player.instance.actions++;
            Player.instance.energy += Player.instance.justPlayed.template.cost;
            Player.instance.hand.Add(Player.instance.justPlayed);
            Player.instance.discard.Remove(Player.instance.justPlayed);
            UserInterface.instance.SetUpCardPositions();
            Player.instance.justPlayed = null;
            Map.instance.Draw();
        }

        // stairs
        if (Input.GetKeyDown(KeyCode.Period) && Map.instance.currentFloor.layout[Map.instance.posX, Map.instance.posY].character == ">") GoDownStairs();
        else if (Input.GetKeyDown(KeyCode.Comma) && Map.instance.currentFloor.layout[Map.instance.posX, Map.instance.posY].character == "<") GoUpStairs();

        //keyboard movement
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
        if (CombatManager.instance.inCombat && (horiz != 0 || vert != 0 || Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Keypad3))) Player.instance.actions--;
        if (horiz > 0) Move(1, 0);
        else if (horiz < 0) Move(-1, 0);
        else if (vert < 0) Move(0, -1);
        else if (vert > 0) Move(0, 1);
        else if (Input.GetKeyDown(KeyCode.Keypad7)) Move(-1, 1);
        else if (Input.GetKeyDown(KeyCode.Keypad9)) Move(1, 1);
        else if (Input.GetKeyDown(KeyCode.Keypad1)) Move(-1, -1);
        else if (Input.GetKeyDown(KeyCode.Keypad3)) Move(1, -1);
        if (moved && Player.instance.actions <= 0) CombatManager.instance.TriggerMonsterTurn();
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
            Player.instance.PlayCard(card);
            UserInterface.instance.SetUpCardPositions();
        }
        cardDragged = -2;
        Map.instance.Draw();
    }

    private bool CardPlayed() {
        var card = Player.instance.hand[cardDragged];
        if (card.position.y > 14 && Player.instance.energy >= card.template.cost && Player.instance.actions > 0) return true;
        return false;
    }

    private void DrawTargetingLine() {
        Map.instance.Draw();
        int x = mouseX + Map.instance.posX - VirtualConsole.instance.width / 2;
        int y = mouseY + Map.instance.posY - (((VirtualConsole.instance.height - 15) / 2) + 15);
        int range = mouseRange + 1;
        var x0 = Map.instance.posX;
        var y0 = Map.instance.posY;
        var dx = x - x0;
        var dy = y - y0;
        int sx, sy;
        if (x0 < x) sx = 1;
        else sx = -1;
        if (y0 < y) sy = 1;
        else sy = -1;
        int xnext = x0;
        int ynext = y0;
        var denom = Mathf.Sqrt((float)dx * dx + (float)dy * dy);
        while ((xnext != x || ynext != y) && range > 0) {
            range--;
            if (xnext >= 0 && ynext >= 0 && xnext < Map.instance.currentFloor.layout.GetLength(0) && ynext < Map.instance.currentFloor.layout.GetLength(1)) {
                if (!Map.instance.BlocksSight(xnext, ynext)) {
                    if (xnext != x0 || ynext != y0) Map.instance.ColorBlock(xnext, ynext, 0, 1, 0);
                    if (Map.instance.BlocksProjectile(xnext, ynext)) return;
                }
                else return;
                if (Mathf.Abs(dy * (xnext - x0 + sx) - dx * (ynext - y0)) / denom < 0.5f) xnext += sx;
                else if (Mathf.Abs(dy * (xnext - x0) - dx * (ynext - y0 + sy)) / denom < 0.5f) ynext += sy;
                else {
                    xnext += sx;
                    ynext += sy;
                }
            }
        }
        if (range > 0 && (xnext != x0 || ynext != y0) && !Map.instance.BlocksSight(xnext, ynext)) Map.instance.ColorBlock(xnext, ynext, 0, 1, 0);
    }

    private void Move(int x, int y) {
        if (Map.instance.currentFloor.monsters[Map.instance.posX + x, Map.instance.posY + y] != null) {
            Player.instance.DefaultMeleeAttack(Map.instance.currentFloor.monsters[Map.instance.posX + x, Map.instance.posY + y]);
        }
        else {
            if (!MoveValid(Map.instance.posX + x, Map.instance.posY + y)) return;
            Map.instance.posX += x;
            Map.instance.posY += y;
        }
        Map.instance.Draw();
        moveTimer = moveTimeout;
        moved = true;
    }

    private bool MoveValid(int x, int y) {
        if (Map.instance.currentFloor.monsters[x, y] != null) return false;
        if (Map.instance.currentFloor.layout[x, y].character == "." || Map.instance.currentFloor.layout[x, y].character == "+") {
            Map.instance.currentFloor.layout[x, y].character = ".";
            Map.instance.currentFloor.layout[x, y].color = Color.white;
            Map.instance.currentFloor.layout[x, y].bgColor = Color.black;
            return true;
        }
        else if (Map.instance.currentFloor.layout[x, y].character == ">" || Map.instance.currentFloor.layout[x, y].character == "<") return true;
        return false;
    }

    private void GoDownStairs() {
        Map.instance.currentFloorNumber++;
        Map.instance.currentFloor = Map.instance.floors[Map.instance.currentFloorNumber];
        Map.instance.posX = Map.instance.currentFloor.startingX;
        Map.instance.posY = Map.instance.currentFloor.startingY;
        Map.instance.Draw();
    }

    private void GoUpStairs() {
        Map.instance.currentFloorNumber--;
        Map.instance.currentFloor = Map.instance.floors[Map.instance.currentFloorNumber];
        Map.instance.posX = Map.instance.currentFloor.endingX;
        Map.instance.posY = Map.instance.currentFloor.endingY;
        Map.instance.Draw();
    }
}
