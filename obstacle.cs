using System;
using Cairo;
using System.Collections.Generic;
using static State;
enum State { up, down, left, right }; //types of movements

namespace hardestgame { 
    public class obstacle
    {
        public List<PointD> pos;
        public int size = 12;
        public event Notify changed;
        List<State> movementType;
        PointD centre;
        int velocity = 10;
        List<double> angle;
        double angularSpeed;

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

            }
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
                    move_4(); break;
                case 5:
                    move_5(); break;
            }
        }

        double dist (PointD p1)
        {
            return Math.Sqrt(Math.Abs((centre.X - p1.X) * (centre.X - p1.X) + ((centre.Y - p1.Y) * (centre.Y - p1.Y))));
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
            centre = new PointD(11 * 60 + 30, 7 * 60 + 30);
            angle = new List<double>();
            angularSpeed = 1.4;
            foreach(PointD po in pos)
            {
                
                if (po.Y == centre.Y) {
                    if (po.X < centre.X) angle.Add(Math.PI + angularSpeed * Math.PI / 180);
                    else angle.Add(2 * Math.PI + angularSpeed * Math.PI / 180);
                    continue;
                }
                if (po.X == centre.X)
                {
                    if (po.Y < centre.Y) angle.Add(270 * Math.PI / 180 + angularSpeed * Math.PI / 180);
                    else angle.Add(Math.PI / 2 + angularSpeed * Math.PI / 180);
                }
                
                /*
                double _angle = Math.Atan((po.Y - centre.Y) / (po.X - centre.X)) * Math.PI / 180;
                Console.WriteLine(_angle);
                angle.Add(_angle+ angularSpeed * Math.PI / 180); */
            }
        }

        public void move_4()
        {
            for (int i = 0; i < pos.Count; i++)
            {
                double distance = dist(pos[i]);
                double currAngle = angle[i];
                double x = centre.X  + distance * Math.Cos(currAngle);
                double y = centre.Y  +  distance * Math.Sin(currAngle);
                pos[i] = new PointD(x, y);
                angle[i] += angularSpeed * Math.PI / 180;
            }

        }

        public void move_5()
        {

        }
    }
}
