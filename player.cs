using System;
using Cairo;
public delegate void Notify();
namespace hardestgame
{
    public class Player
    {
        public PointD pixPos { get; set; }
        public PointD size { get; set; }
        public double speed = 2.6;
        public bool[] canNotMove;
        int width = 35, height = 35;
        public event Notify changed;
        public bool[] dirs;

        public Player()
        {
            size = new PointD(width, height);
            canNotMove = new bool[4]; //left, right. up, down
            dirs = new bool[4]; //left, right, up, down

        }

        public void changePixPos()
        {
            PointD pos = pixPos;
            int x, y;
            bool a = canNotMove[0], b = canNotMove[1], c = canNotMove[2], d = canNotMove[3];
            y = (dirs[2] && !c) ? -1 : (dirs[3] && !d)? 1 : 0;
            x = (dirs[0] && !a)? -1 : (dirs[1] && !b) ? 1 : 0;
            pos.X += speed * x;
            pos.Y += speed * y;
            pixPos = pos;
            changed?.Invoke();
        }

    }
}
