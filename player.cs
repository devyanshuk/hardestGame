using System;
using Cairo;
namespace hardestgame
{
    public class Player
    {
        public PointD pixPos { get; set; }
        public PointD size { get; set; }
        public double SPEED = 5;
        public bool[] canNotMove;
        const int WIDTH = 35, HEIGHT = 35;
        public bool[] dirs;

        public Player()
        {
            size = new PointD(WIDTH, HEIGHT);
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
            pos.X += SPEED * x;
            pos.Y += SPEED * y;
            pixPos = pos;
        }

    }
}
