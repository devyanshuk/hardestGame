using System;
using System.Collections.Generic;
using Cairo;

namespace hardestgame
{
    public class Square
    {
        public double length, height;
        public PointD topLeftPos;

        public void adjustPos()
        {
            this.length *= View.CELL_WIDTH;
            this.height *= View.CELL_HEIGHT;
            this.topLeftPos = new PointD(topLeftPos.X * View.CELL_WIDTH,
                                    topLeftPos.Y * View.CELL_HEIGHT);
        }
    }

    public class CheckPoints : Square
    {
        public double red = 0.0;
        public double green = 0.7;
        public double blue = 0.0;
        public double opacity = 0.5;
        public bool beingAnimated, increase, decrease;
        public List<double> l;
        public CheckPoints(PointD topLeftPos, double length, double height)
        {
            this.topLeftPos = topLeftPos;
            this.length = length;
            this.height = height;
            adjustPos();
            beingAnimated = increase = decrease = false;
            l = new List<double> {red, green, blue, opacity };
        }
    }
}
