using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Projectile {
    public DisplayCharacter display = null;
    public int x = 0;
    public int y = 0;
    public int xDest = 0;
    public int yDest = 0;
    public int range = 0;
    public int radius = 0;
    public int blastStage = -1;
    public static List<Projectile> instances = new List<Projectile>();

    public Projectile() {
        instances.Add(this);
    }
}
