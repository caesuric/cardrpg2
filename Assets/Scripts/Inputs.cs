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
    public MouseMode mouseMode = MouseMode.Default;
    public int mouseRange = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (Player.instance.hp <= 0) return;
        if (cardDragged == -2) SlideCardBack();
        if (mouseMode == MouseMode.Animating) Animate();

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
            oldMouseX = mouseX;
            oldMouseY = mouseY;
        }

        //keyboard
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
        if (CombatManager.instance.inCombat && (horiz != 0 || vert != 0)) Player.instance.actions--;
        if (horiz > 0) MoveRight();
        else if (horiz < 0) MoveLeft();
        else if (vert < 0) MoveUp();
        else if (vert > 0) MoveDown();
        if (Player.instance.actions <= 0) CombatManager.instance.TriggerMonsterTurn();
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
        if (card.position.y > 14) return true;
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
            if (xnext >= 0 && ynext >= 0 && xnext < Map.instance.layout.GetLength(0) && ynext < Map.instance.layout.GetLength(1)) {
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
        if (Map.instance.monsters[x, y] != null) return false;
        if (Map.instance.layout[x, y].character == "." || Map.instance.layout[x, y].character == "+") {
            Map.instance.layout[x, y].character = ".";
            return true;
        }
        return false;
    }

    private void Animate() {
        foreach (var projectile in Projectile.instances) {
            if (projectile.range == 0 || (projectile.x == projectile.xDest && projectile.y == projectile.yDest) || Map.instance.monsters[projectile.x, projectile.y] != null) {
                RemoveProjectile(projectile);
                break;
            }
        }
        foreach (var projectile in Projectile.instances) {
            int x = projectile.xDest;
            int y = projectile.yDest;
            var x0 = projectile.x;
            var y0 = projectile.y;
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
            if (Mathf.Abs(dy * (xnext - x0 + sx) - dx * (ynext - y0)) / denom < 0.5f) xnext += sx;
            else if (Mathf.Abs(dy * (xnext - x0) - dx * (ynext - y0 + sy)) / denom < 0.5f) ynext += sy;
            else {
                xnext += sx;
                ynext += sy;
            }
            projectile.x = xnext;
            projectile.y = ynext;
            projectile.range--;
            Map.instance.projectiles[x0, y0] = null;
            Map.instance.projectiles[xnext, ynext] = projectile;
        }
        Map.instance.Draw();
    }

    private void RemoveProjectile(Projectile projectile) {
        Projectile.instances.Remove(projectile);
        Map.instance.projectiles[projectile.x, projectile.y] = null;
        mouseMode = MouseMode.Default;
        Player.instance.ResolveTargetedCard(projectile);
        Map.instance.Draw();
    }
}
