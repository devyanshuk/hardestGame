using System;
using System.Collections.Generic;
using Cairo;
using static XY_DIRS;
using static CIRCLE_DIRS;
namespace hardestgame
{
    public class Movement
    {
        public class CircleMovement
        {
            public double angularSpeed;
            public PointD pos;
            public double angle;
            public PointD centre;
            public CIRCLE_DIRS dir;
            public CircleMovement(double angularSpeed, PointD pos, double angle, PointD centre, CIRCLE_DIRS dir)
            {
                this.angularSpeed = angularSpeed;
                this.pos = pos;
                this.angle = angle;
                this.centre = centre;
                this.dir = dir;
            }

            double dist() => Math.Sqrt(Math.Abs((centre.X - pos.X) * (centre.X - pos.X)
                                           + ((centre.Y - pos.Y) * (centre.Y - pos.Y))));
            public void move()
            {
                double newAngle = angularSpeed * Math.PI / 180;
                angle += (dir == clockwise) ? newAngle : -newAngle;
                double distance = dist();
                pos.X = centre.X + distance * Math.Cos(angle);
                pos.Y = centre.Y + distance * Math.Sin(angle);
            }
        }

        public class XyMovement
        {
            public double velocity;
            public PointD pos;
            public XY_DIRS dir;
            public XyMovement(double velocity, PointD pos, XY_DIRS dir)
            {
                this.velocity = velocity;
                this.pos = pos;
                this.dir = dir;
            }

            public void changeDir()
            {
                this.dir = (dir == left) ? right
                            : (dir == right) ? left
                            : (dir == up) ? down
                            : up;
            }

            public void move()
            {
                pos.X += (dir == left) ? -velocity : (dir == right) ? velocity : 0;
                pos.Y += (dir == up) ? -velocity : (dir == down) ? velocity : 0;
            }
        }

        public class SquareMovement : Rectangle
        {
            List<XY_DIRS> MOVEMENTS = new List<XY_DIRS>()
                                            {up, right, down, left};
            public double velocity;
            public PointD pos;
            public XY_DIRS dir;
            public CIRCLE_DIRS movementType;
            public SquareMovement(double velocity, PointD pos, XY_DIRS dir, CIRCLE_DIRS movementType, PointD topLeftPos, double width, double breadth)
            {
                this.velocity = velocity;
                this.pos = pos;
                this.dir = dir;
                this.movementType = movementType;
                this.topLeftPos = topLeftPos;
                this.width = width;
                this.height = breadth;
                adjustPos();
            }

            bool timeToChangeDir() => ((pos.X <= (topLeftPos.X + View.CELL_WIDTH / 2 + 1) &&
                                        pos.Y >= topLeftPos.Y + (height - View.CELL_HEIGHT / 2)) ||
                                        (pos.X <= (topLeftPos.X + View.CELL_WIDTH / 2 + 1) &&
                                        pos.Y <= (topLeftPos.Y + View.CELL_HEIGHT / 2 + 1)) ||
                                        (pos.X >= topLeftPos.X + (width - View.CELL_WIDTH / 2) &&
                                        pos.Y <= (topLeftPos.Y + View.CELL_HEIGHT / 2 + 1)) ||
                                        (pos.X >= topLeftPos.X + (width - View.CELL_WIDTH / 2) &&
                                        pos.Y >= topLeftPos.Y + (height - View.CELL_HEIGHT / 2)));

            public void move()
            {
                pos.X += (dir == left) ? -velocity : (dir == right) ? velocity : 0;
                pos.Y += (dir == up) ? -velocity : (dir == down) ? velocity : 0;

                if (timeToChangeDir())
                    changeDir();
            }

            void changeDir()
            {
                int index = (movementType == anticlockwise) ? 3 : 1;
                dir = MOVEMENTS[(MOVEMENTS.IndexOf(dir) + index) % 4];
            }
        }
    }
}
