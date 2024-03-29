using Cairo;
using Gdk;

namespace hardestgame
{
    public class Player : Rectangle
    {
        public event Notify opacityChanged;

        public PointD pixPos { get; set; }
        public PointD size { get; set; }
        public double speed;
        public bool[] canNotMove;
        const int WIDTH = 35, HEIGHT = 35;
        public bool[] dirs;

        public Player(double speed)
        {
            size = new PointD(WIDTH, HEIGHT);
            canNotMove = new bool[4]; //left, right. up, down
            dirs = new bool[4]; //left, right, up, down
            this.speed = speed;
            width = WIDTH;
            height = HEIGHT;
            red = 1.0;
            green = 0.0;
            blue = 0.0;
            opacity = 1.0;

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
            pixPos =  topLeftPos = pos;
        }

        public void updateDir(bool keyRelease, EventKey evnt)
        {
            for (int i = 0; i < 4; i++)
            {
                bool b = keyRelease ? dirs[i] : !dirs[i];
                if (evnt.Key == View.DIRS[i] && b)
                    dirs[i] = !keyRelease;
            }
        }

        public void makePlayerDisappear()
        {
            opacity -= 0.03;
            if (opacity <= 0.0)
                opacityChanged?.Invoke();
        }

    }
}
