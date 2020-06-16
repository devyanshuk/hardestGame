using System;
using System.Collections.Generic;
using Cairo;

namespace hardestgame
{
    public class CheckPoints : Rectangle
    {
        public bool beingAnimated, increase, decrease;

        public CheckPoints(PointD topLeftPos, double length, double height)
        {
            this.topLeftPos = topLeftPos;
            this.width = length;
            this.height = height;
            adjustPos();
            beingAnimated = increase = decrease = false;
            red = 0.0;
            green = 0.7;
            blue = 0.0;
            opacity = 0.5;
    }

        public void animateCheckPoint()
        {
            if (decrease)
            {
                green -= 0.02;
                if (green <= 0.2)
                {
                    decrease = false;
                    increase = true;
                }
            }
            else if (increase)
            {
                green += 0.02;
                if (green >= 0.7)
                    increase = false;
            }
        }
    }
}
