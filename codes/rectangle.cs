using System;
using Gdk;
using Cairo;

namespace hardestgame
{
    public class Rectangle
    {
        public double width { get; set; }
        public double height { get; set; }
        public PointD topLeftPos { get; set; }
        public double red { get; set; }
        public double green { get; set; }
        public double blue { get; set; }
        public double opacity { get; set; }
        public ImageSurface image { get; set; }
        public bool increase { get; set; }
        public bool decrease { get; set; }

        public void adjustPos()
        {
            this.width *= View.CELL_WIDTH;
            this.height *= View.CELL_HEIGHT;
            this.topLeftPos = new PointD(topLeftPos.X * View.CELL_WIDTH,
                                    topLeftPos.Y * View.CELL_HEIGHT);
        }

        //check if a point is within the rectangle
        public bool collision(PointD p) =>(p.X >= topLeftPos.X &&
                                           p.X <= topLeftPos.X + width &&
                                           p.Y >= topLeftPos.Y &&
                                           p.Y <= topLeftPos.Y + height);
    }
}
