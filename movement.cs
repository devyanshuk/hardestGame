using System;
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

    }
}
