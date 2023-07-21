namespace RoguelikeEngine {
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages animations and visual effects.
    /// </summary>
    public class AnimationManager : MonoBehaviour {
        /// <summary>
        /// Called every frame.
        /// </summary>
        protected void Update() {
            if (Inputs.instance.mouseMode != MouseMode.Animating) return;
            AnimateProjectiles();
            AnimateAoes();
            if (DoneAnimating()) Inputs.instance.mouseMode = MouseMode.Default;
        }

        private void AnimateProjectiles() {
            foreach (var projectile in Projectile.instances) {
                if (projectile.blastStage > -1) continue;
                if (projectile.range == 0 || (projectile.x == projectile.xDest && projectile.y == projectile.yDest) || Map.instance.currentFloor.monsters[projectile.x, projectile.y] != null) {
                    ProjectileHit(projectile);
                    break;
                }
            }
            foreach (var projectile in Projectile.instances) {
                if (projectile.blastStage > -1) continue;
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
                var denom = Mathf.Sqrt(((float)dx * dx) + ((float)dy * dy));
                if (Mathf.Abs((dy * (xnext - x0 + sx)) - (dx * (ynext - y0))) / denom < 0.5f) xnext += sx;
                else if (Mathf.Abs((dy * (xnext - x0)) - (dx * (ynext - y0 + sy))) / denom < 0.5f) ynext += sy;
                else {
                    xnext += sx;
                    ynext += sy;
                }
                projectile.x = xnext;
                projectile.y = ynext;
                projectile.range--;
                Map.instance.currentFloor.projectiles[x0, y0] = null;
                Map.instance.currentFloor.projectiles[xnext, ynext] = projectile;
            }
            Map.instance.Draw();
        }

        private void AnimateAoes() {
            var cullList = new List<Projectile>();
            foreach (var projectile in Projectile.instances) {
                if (projectile.blastStage > -1) projectile.blastStage++;
                if (projectile.blastStage > projectile.radius) cullList.Add(projectile);
            }
            foreach (var projectile in cullList) {
                Projectile.instances.Remove(projectile);
                Map.instance.currentFloor.projectiles[projectile.x, projectile.y] = null;
            }
            Map.instance.Draw();
        }

        private void ProjectileHit(Projectile projectile) {
            if (projectile.radius == 0) {
                Projectile.instances.Remove(projectile);
                Map.instance.currentFloor.projectiles[projectile.x, projectile.y] = null;
            }
            else {
                projectile.blastStage = 0;
            }
            Player.instance.ResolveTargetedCard(projectile);
            Map.instance.Draw();
        }

        private bool DoneAnimating() {
            if (Projectile.instances.Count == 0) return true;
            return false;
        }
    }
}
