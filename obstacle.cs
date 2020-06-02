using System;
using Cairo;
using System.Collections.Generic;
using static State;
using static hardestgame.movement;

public enum State { up, down, left, right }; //types of movements
public enum CircleDir { clockwise, anticlockwise};

namespace hardestgame
{
    public class obstacle
    {
        public List<PointD> pos;
        public int size = 12;
        public event Notify changed;
        List<circleMovement> circleMov;
        List<xyMovement> xyMov;
        List<PointD> wallHitPoints;

        public obstacle(List<PointD> pos, int lev, List<PointD> hitPt, List<circleMovement> c, List<xyMovement> xy)
        {
            this.pos = pos;
            wallHitPoints = hitPt;
            circleMov = c;
            xyMov = xy;
            for (int i = 0; i < circleMov.Count; i++)
            {
                circleMov[i].angle = findAngle(circleMov[i].centre, circleMov[i]);
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

        bool withinBounds(PointD po, PointD p1, double s) => (po.X >= p1.X && po.X <= p1.X + s && po.Y >= p1.Y && po.Y <= p1.Y + s);

        bool collision(PointD po, PointD wall, double s)
        {
            var l = new PointD(po.X - 3.5 * size, po.Y);
            var r = new PointD(po.X + size, po.Y);
            var d = new PointD(po.X, po.Y + size);
            var u = new PointD(po.X, po.Y - 3.5 * size);
            if (withinBounds(l, wall, s) || withinBounds(r, wall, s) || withinBounds(d, wall, s) || withinBounds(u, wall, s))
                return true;
            return false;
        }

        public void move(int lev)
        {
            List<PointD> p = new List<PointD>();

            for (int i = 0; i < xyMov.Count; i++)
            {
                foreach (PointD wall in wallHitPoints)
                    if (collision(xyMov[i].pos, wall, 30))
                        xyMov[i].changeDir();
                xyMov[i].move();
                p.Add(xyMov[i].pos);
            }
            for (int i = 0; i < circleMov.Count; i++)
            {
                p.Add(circleMov[i].getNewPos());
                circleMov[i].move();
            }
            pos = p;
        }

        public void move_3()
        {
            for (int i = 0; i < xyMov.Count; i++)
            {
                foreach (PointD wall in wallHitPoints)
                {
                    if (((xyMov[i].pos.X <= wall.X) || (xyMov[i].pos.Y >= wall.Y)) && collision(xyMov[i].pos, wall, 30))
                    {
                        xyMov[i].dir = (xyMov[i].dir == right) ? down
                                       : (xyMov[i].dir == left) ? up
                                       : (xyMov[i].dir == up) ? right
                                       : (xyMov[i].dir == down) ? left
                                       : xyMov[i].dir;
                        break;
                    }
                }
                xyMov[i].pos.Y += (xyMov[i].dir == up) ? -xyMov[i].velocity
                                : (xyMov[i].dir == down) ? xyMov[i].velocity : 0;
                xyMov[i].pos.X += (xyMov[i].dir == left) ? -xyMov[i].velocity
                                : (xyMov[i].dir == right) ? xyMov[i].velocity : 0;
                pos[i] = xyMov[i].pos;
            }
        }
    }
}
