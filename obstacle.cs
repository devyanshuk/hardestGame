using System;
using Cairo;
using System.Collections.Generic;
using static State;
public enum State { up, down, left, right }; //types of movements
public enum CircleDir { clockwise, anticlockwise};

namespace hardestgame {

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
    }

    public class obstacle
    {
        public List<PointD> pos;
        public int size = 12;
        public event Notify changed;
        List<circleMovement> circleMov;
        List<xyMovement> xyMov;
        List<PointD> wallHitPoints;
        List<State> movementType;

        public obstacle(List<PointD> pos, int lev, List<PointD> hitPt)
        {
            this.pos = pos;
            movementType = new List<State>();
            wallHitPoints = hitPt;
            switch (lev)
            {
                case 1:
                    init_1(); break;
                case 2:
                    init_2(); break;
                case 3:
                    init_3(); break;
                case 4:
                    init_4(); break;
                case 5:
                    init_5(); break;

            }
        }

        double findAngle(PointD centre, circleMovement po)
        {
            if (po.pos.Y == centre.Y)
            {
                if (po.pos.X < centre.X) return (Math.PI + po.angularSpeed * Math.PI / 180);
                else return (2 * Math.PI + po.angularSpeed * Math.PI / 180);
            }
            if (po.pos.X == centre.X)
            {
                if (po.pos.Y < centre.Y) return (270 * Math.PI / 180 + po.angularSpeed * Math.PI / 180);
                else return (Math.PI / 2 + po.angularSpeed * Math.PI / 180);
            }
            return -1;
        }
        bool withinBounds(PointD po, PointD p1)
        {
            return (po.X >= p1.X && po.X <= p1.X + 30 && po.Y >= p1.Y && po.Y <= p1.Y + 30);
        }

        bool collision(PointD po, PointD wall)
        {
            var l = new PointD(po.X - 4 * size , po.Y);
            var r = new PointD(po.X + size , po.Y);
            var d = new PointD(po.X, po.Y + size);
            var u = new PointD(po.X, po.Y - 4 * size);
            if (withinBounds(l, wall) || withinBounds(r, wall) || withinBounds(d, wall) || withinBounds(u, wall))
            {
                return true;
            }
            return false;
        }

        public void move(int lev)
        {
            switch (lev)
            {
                case 1:
                    move_1(); break;
                case 2:
                    move_2(); break;
                case 3:
                    move_3(); break;
                case 4:
                    move_4_5(); break;
                case 5:
                    move_4_5(); break;
            }
        }

        double dist (circleMovement p1)
        {
            return Math.Sqrt(Math.Abs((p1.centre.X - p1.pos.X) * (p1.centre.X - p1.pos.X) + ((p1.centre.Y - p1.pos.Y) * (p1.centre.Y - p1.pos.Y))));
        }

        void init_1()
        {
            xyMov = new List<xyMovement>();
            double velocity = 10;
            for (int i = 0; i < pos.Count; i++)
            {
                xyMov.Add(new xyMovement(velocity, pos[i], left));
            }

        }

        public void move_1()
        {
            for(int i = 0; i < xyMov.Count; i++)
            {
                foreach (PointD wall in wallHitPoints)
                {
                    if (collision(xyMov[i].pos, wall)){
                        xyMov[i].dir = (xyMov[i].dir == left) ? right : left;
                    }
                }
                xyMov[i].pos.X += (xyMov[i].dir == left) ? -xyMov[i].velocity : xyMov[i].velocity;
                pos[i] = xyMov[i].pos;
            }
        }

        void init_2()
        {
            xyMov = new List<xyMovement>();
            double velocity = 10;
            for (int i = 0; i < pos.Count; i++)
            {
                State dir = (i <= 6) ? down : up;
                xyMov.Add(new xyMovement(velocity, pos[i], dir));
            }
        }

        public void move_2()
        {
            for (int i = 0; i < xyMov.Count; i++)
            {
                foreach (PointD wall in wallHitPoints)
                {
                    if (collision(xyMov[i].pos, wall))
                    {
                        xyMov[i].dir = (xyMov[i].dir == up) ? down : up;
                    }
                }
                xyMov[i].pos.Y += (xyMov[i].dir == up) ? -xyMov[i].velocity : xyMov[i].velocity;
                pos[i] = xyMov[i].pos;
            }
        }

        void init_3()
        {

        }

        public void move_3()
        {

        }

        void init_4()
        {
            PointD centre = new PointD(11 * 60 + 30, 7 * 60 + 30);
            circleMov = new List<circleMovement>();
            foreach (PointD po in pos)
            {
                circleMovement cp = new circleMovement(1.4, po, 0, centre, CircleDir.clockwise);
                double angle = findAngle(centre, cp);
                cp.angle = angle;
                circleMov.Add(cp);
            }

        }

        void init_5()
        {
            PointD centre = new PointD(11 * 60 + 60, 6 * 60 + 30);
            circleMov = new List<circleMovement>();
            int count = 0;
            foreach (PointD po in pos)
            {
                CircleDir dir = (count!=4 && count !=5)? CircleDir.clockwise : CircleDir.anticlockwise;
                double sp = (count!=4 && count!=5)? 0.8 : 0.7;
                circleMovement cp = new circleMovement(sp, po, 0, centre, dir);
                double angle = findAngle(centre, cp);
                cp.angle = angle;
                circleMov.Add(cp);
                count++;
            }

        }

        public void move_4_5()
        {
            for (int i = 0; i < circleMov.Count; i++)
            {
                double distance = dist(circleMov[i]);
                double currAngle = circleMov[i].angle;
                double x = circleMov[i].centre.X + distance * Math.Cos(currAngle);
                double y = circleMov[i].centre.Y + distance * Math.Sin(currAngle);
                pos[i] = new PointD(x, y);
                double newAngle = circleMov[i].angularSpeed * Math.PI / 180;
                circleMov[i].angle += (circleMov[i].dir == CircleDir.clockwise) ? newAngle : -newAngle;
            }
        }
    }
}
