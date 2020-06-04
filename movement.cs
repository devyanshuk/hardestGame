using System;
using System.Collections.Generic;
using Cairo;
using static State;
namespace hardestgame
{
    public class movement
    {
        public class circleMovement
        {
            public double angularSpeed;
            public PointD pos;
            public double angle;
            public PointD centre;
            public CircleDir dir;
            public circleMovement(double angularSpeed, PointD pos, double angle, PointD centre, CircleDir dir)
            {
                this.angularSpeed = angularSpeed;
                this.pos = pos;
                this.angle = angle;
                this.centre = centre;
                this.dir = dir;
            }

            double dist()
            {
                return Math.Sqrt(Math.Abs((centre.X - pos.X) * (centre.X - pos.X)
                                           + ((centre.Y - pos.Y) * (centre.Y - pos.Y))));
            }

            public void move()
            {
                double newAngle = angularSpeed * Math.PI / 180;
                angle += (dir == CircleDir.clockwise) ? newAngle : -newAngle;
            }

            public PointD getNewPos()
            {
                double distance = dist();
                double currAngle = angle;
                double x = centre.X + distance * Math.Cos(currAngle);
                double y = centre.Y + distance * Math.Sin(currAngle);
                return new PointD(x, y);
            }
        }

        public class xyMovement
        {
            public double velocity;
            public PointD pos;
            public State dir;
            public xyMovement(double velocity, PointD pos, State dir)
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

        public class squareMovement
        {
            public double velocity;
            public PointD pos;
            public State dir;
            public CircleDir movementType;
            public PointD topLeftPos;
            public double length;
            public double breadth;
            public squareMovement(double velocity, PointD pos, State dir, CircleDir movementType, PointD topLeftPos, double length, double breadth)
            {
                this.velocity = velocity;
                this.pos = pos;
                this.dir = dir;
                this.movementType = movementType;
                this.topLeftPos = topLeftPos;
                this.length = View.CELL_WIDTH * length;
                this.breadth = View.CELL_HEIGHT * breadth;
            }

            public void move()
            {
                pos.X += (dir == left) ? -velocity : (dir == right) ? velocity : 0;
                pos.Y += (dir == up) ? -velocity : (dir == down) ? velocity : 0;

                if ((pos.X <= (topLeftPos.X + View.CELL_WIDTH / 2 + 1) && pos.Y >= topLeftPos.Y + (breadth - View.CELL_HEIGHT / 2)) ||
                    (pos.X <= (topLeftPos.X + View.CELL_WIDTH / 2 + 1) && pos.Y <= (topLeftPos.Y + View.CELL_HEIGHT / 2 + 1)) ||
                    (pos.X >= topLeftPos.X + (length - View.CELL_WIDTH / 2 ) && pos.Y <= (topLeftPos.Y + View.CELL_HEIGHT / 2 + 1)) ||
                    (pos.X >= topLeftPos.X + (length - View.CELL_WIDTH / 2 ) && pos.Y >= topLeftPos.Y + (breadth - View.CELL_HEIGHT / 2)))
                    changeDir();
            }

            void changeDir()
            {
                List<State> clockWiseMovements = new List<State>() {up, right, down, left};
                List<State> antiClockWiseMovements = new List<State>() {down, right, up, left};
                if (movementType == CircleDir.anticlockwise)
                    dir = antiClockWiseMovements[(antiClockWiseMovements.IndexOf(dir) + 1) % 4];
                else if (movementType == CircleDir.clockwise)
                    dir = clockWiseMovements[(clockWiseMovements.IndexOf(dir) + 1) % 4];
            }
        }
    }
}
