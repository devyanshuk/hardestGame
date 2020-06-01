using System;
using Cairo;
using System.Collections.Generic;
using static State;
enum State { up, down, left, right }; //types of movements
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

    public class obstacle
    {
        public List<PointD> pos;
        public int size = 12;
        public event Notify changed;
        List<circleMovement> circleMov;
        List<State> movementType;
        int velocity = 10;

        public obstacle(List<PointD> pos, int lev)
        {
            this.pos = pos;
            movementType = new List<State>();
            switch (lev)
            {
                case 1:
                    init_1(); break;
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
            for (int i = 0; i <=pos.Count; i++)
                movementType.Add((i % 2 == 1)? left : right);
        }

        public void move_1()
        {
            for(int i = 0; i <= pos.Count; i++)
            {

            }
        }

        public void move_2()
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
                double sp = (count!=4 && count!=5)? 1 : 0.7;
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
