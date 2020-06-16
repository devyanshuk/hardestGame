using System;
using Cairo;

namespace hardestgame
{
    public class Wall : Rectangle
    {
        public Wall(PointD topLeftPos, double width, double height)
        {
            this.topLeftPos = topLeftPos;
            this.width = width;
            this.height = height;
            adjustPos();
        }
    }
}
