using System;
using Cairo;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static XY_DIRS;
using static CIRCLE_DIRS;
using static hardestgame.Movement;

namespace hardestgame
{
    public class Parser
    {
        List<char> xChar, yChar;
        List<double> xVel, yVel;
        Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double>> cChar;
        Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double, double, double>> sqChar;

        public void init()
        {
            xChar = new List<char>();
            yChar = new List<char>();
            xVel = new List<double>();
            yVel = new List<double>();
            cChar = new Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double>>();
            sqChar = new Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double, double, double>>();
        }

        public void updateEnv(string fileName, ref char[,]bg)
        {
            List<char> wallChars = new List<char> {'#', 'W', ']', '[' };
            using (StreamReader r = new StreamReader(fileName))
            {
                while (r.ReadLine() is string s)
                {
                    if (s == "")
                        break;
                }
                for (int i = 0; i < Game.MAP_HEIGHT; i++)
                {
                    string s = r.ReadLine();
                    for (int j = 0; j < Game.MAP_WIDTH; j++)
                    {
                        if (!wallChars.Contains(s[j]))
                            bg[i, j] = '1';
                    }
                }
            }
        }

        public void updateEnv(string fileName, Player player, out List<CircleMovement> circleMov,
                     out List<XyMovement> xyMov, out List<SquareMovement> sqMov,
                     ref List<Wall> walls, ref List<CheckPoints> checkPoint, ref List<PointD> obsList,
                     ref PointD checkPointPos, ref int totalCoins, ref List<PointD> coinPos, ref char[,]bg,
                     ref List<PointD> hitPt)
        {
            circleMov = new List<CircleMovement>();
            xyMov = new List<XyMovement>();
            sqMov = new List<SquareMovement>();
            using (StreamReader r = new StreamReader(fileName))
            {
                double.TryParse(r.ReadLine().Split()[3], out player.speed);
                parseMovementsAndCheckPoints(r, ref checkPoint);
                for (int i = 0; i < Game.MAP_HEIGHT; i++)
                {
                    string s = r.ReadLine();
                    for (int j = 0; j < Game.MAP_WIDTH; j++)
                    {
                        char ch = s[j];

                        PointD tlp = new PointD(j, i);

                        PointD pos = new PointD(View.X_MARGIN + View.CELL_WIDTH * j,
                                        View.Y_MARGIN + View.CELL_HEIGHT * i);
                        PointD newPos = new PointD(pos.X + View.CELL_WIDTH / 2,
                                        pos.Y + View.CELL_HEIGHT / 2);

                        if (cChar.ContainsKey(ch))
                            circleMov.AddRange(addCircularMovement(ch, pos, newPos));
                        if (sqChar.ContainsKey(ch))
                            sqMov.AddRange(addSquareMovement(ch, newPos));
                        if (xChar.Contains(ch))
                        {
                            int index = xChar.IndexOf(ch);
                            xyMov.Add(new XyMovement(xVel[index], newPos, left));
                        }
                        if (yChar.Contains(ch))
                        {
                            int index = yChar.IndexOf(ch);
                            xyMov.Add(new XyMovement(yVel[index], newPos, up));
                        }
                        _updateEnv(ch, pos, newPos, i, j, player, ref walls,
                                   ref checkPoint, ref obsList, ref checkPointPos,
                                   ref totalCoins, ref coinPos, ref bg, ref hitPt, tlp );
                    }
                }
                var k = circleMov;
                circleMov.AddRange(copyCircleMovements(r, k));
            }
        }

        void _updateEnv(char ch, PointD pos, PointD newPos, int i, int j, Player player,
                        ref List<Wall> walls, ref List<CheckPoints> checkPoint,
                        ref List<PointD> obsList, ref PointD checkPointPos, ref int totalCoins,
                        ref List<PointD> coinPos, ref char[,]bg, ref List<PointD> hitPt, PointD tlp )
        {
            if (ch != '1' && ch != '#' && ch != 'W' && ch != ']' && ch != '[' && ch != 'H')
            {
                if (ch == 'P')
                    player.pixPos = (checkPointPos.X != 0 && checkPointPos.Y != 0) ? checkPointPos : pos;
                else if (checkPointPos.X == 0 && checkPointPos.Y == 0 && ch == 'X')
                {
                    totalCoins++;
                    coinPos.Add(newPos);
                }
                else if (ch == '2')
                {
                    if (checkPointPos.X == 0 && checkPointPos.Y == 0)
                    {
                        totalCoins++;
                        coinPos.Add(newPos);
                    }
                    obsList.Add(newPos);
                }
                else if (ch == ';' || ch == 'V' || ch == '^')
                {
                    obsList.Add(newPos);
                    obsList.Add(new PointD(newPos.X + ((ch == ';') ?
                                View.CELL_WIDTH / 2 : 0),
                                newPos.Y + ((ch == 'V') ? View.CELL_HEIGHT / 2 :
                                (ch == '^') ? -View.CELL_HEIGHT / 2 : 0)));
                }
                else
                {
                    newPos = new PointD(pos.X + 30 + ((ch == '>' || ch == ')') ?
                                        View.CELL_WIDTH / 2 : (ch == '<' || ch == '(') ?
                                        -View.CELL_WIDTH / 2 : 0), pos.Y + 30);
                    obsList.Add(newPos);
                }
                ch = (ch != '!' && ch != ':') ? '1' : 'W';
            }
            bg[i, j] = ch;
            if (ch == 'W') walls.Add(new Wall(tlp, 1, 1));
            else if (ch == '[' || ch == ']')
            {
                walls.Add(new Wall(tlp, 1, 1));
                newPos = new PointD(newPos.X + ((ch == ']') ? View.CELL_WIDTH / 2 :
                                    (ch == '[') ? -View.CELL_WIDTH / 2 : 0), newPos.Y);
                obsList.Add(newPos);
            }

            else if (ch == 'H')
            {
                walls.Add(new Wall(tlp, 1, 1));
                hitPt.Add(pos);
            }
        }

        void parseSquareMovement(List<string> sp)
        {
            CIRCLE_DIRS d = (sp[1] == "(") ? clockwise : anticlockwise;

            double.TryParse(sp[2], out double vel);
            double.TryParse(sp[3], out double x);
            double.TryParse(sp[4], out double y);
            double.TryParse(sp[5], out double l);
            double.TryParse(sp[6], out double b);

            for (int j = 7; j < sp.Count; j++)
                if (!sqChar.ContainsKey(char.Parse(sp[j])))
                    sqChar.Add(char.Parse(sp[j]),
                                Tuple.Create(new PointD(x, y), d, vel, l, b));
        }

        void parseYMovement(List<string> sp)
        {
            for (int j = 1; j < sp.Count; j++)
            {
                if (j % 2 == 1)
                    yChar.Add(char.Parse(sp[j]));
                else
                {
                    double.TryParse(sp[j], out double v);
                    yVel.Add(v);
                }
            }
        }

        void parseCircleMovement(List<string> sp)
        {
            CIRCLE_DIRS dir = (sp[1] == "(") ? clockwise : anticlockwise;
            double.TryParse(sp[2], out double vel);
            double.TryParse(sp[3], out double x);
            double.TryParse(sp[4], out double y);

            PointD centre = new PointD(x * View.CELL_WIDTH + View.CELL_WIDTH / 2,
                                    y * View.CELL_HEIGHT + View.CELL_HEIGHT / 2);
            for (int j = 5; j < sp.Count; j++)
                if (!cChar.ContainsKey(char.Parse(sp[j])))
                    cChar.Add(char.Parse(sp[j]), Tuple.Create(centre, dir, vel));
        }

        void parseXMovement(List<string> sp)
        {
            for (int j = 1; j < sp.Count; j++)
            {
                if (j % 2 == 1)
                    xChar.Add(char.Parse(sp[j]));
                else
                {
                    double.TryParse(sp[j], out double v);
                    xVel.Add(v);
                }
            }
        }

        void parseCheckPoints(List<string> sp, ref List<CheckPoints> checkPoint)
        {
            double.TryParse(sp[1], out double tlpX);
            double.TryParse(sp[2], out double tlpY);
            double.TryParse(sp[3], out double l);
            double.TryParse(sp[4], out double h);
            checkPoint.Add(new CheckPoints(new PointD(tlpX, tlpY), l, h));
        }


        void parseMovementsAndCheckPoints(StreamReader r, ref List<CheckPoints> checkPoint)
        {
            while (r.ReadLine() is string s)
            {
                List<string> sp = s.Split().ToList();
                if (sp.Count <= 0 || s == "") break;
                if (sp[0] == "()")
                    parseCircleMovement(sp);
                else if (sp[0] == "-")
                    parseXMovement(sp);
                else if (sp[0] == "|")
                    parseYMovement(sp);
                else if (sp[0] == "[]")
                    parseSquareMovement(sp);
                else if (sp[0] == "ch")
                    parseCheckPoints(sp, ref checkPoint);
            }
        }

        List<CircleMovement> copyCircleMovements(StreamReader r, List<CircleMovement> k)
        {
            var l = new List<CircleMovement>();
            while (r.ReadLine() is string s)
            {
                List<string> st = s.Split().ToList();
                if (st[0] == "cp")
                {
                    CIRCLE_DIRS dir = (st[1] == "(") ? clockwise : anticlockwise;
                    double x, y, vel;
                    double.TryParse(st[3], out x);
                    double.TryParse(st[4], out y);
                    double.TryParse(st[2], out vel);
                    x = x * View.CELL_WIDTH + View.CELL_WIDTH / 2;
                    y = y * View.CELL_HEIGHT + View.CELL_HEIGHT / 2;

                    for (int i = 0; i < k.Count; i++)
                    {
                        l.Add(new CircleMovement(vel, new PointD(k[i].pos.X + x - k[i].centre.X,
                            k[i].pos.Y + y - k[i].centre.Y), 0, new PointD(x, y), dir));
                    }
                }
            }
            return l;
        }

        bool checkForSpecialCharacters(char ch, PointD newPos, out PointD n)
        {
            n = new PointD(0, 0);
            if (ch == ';' || ch == 'V' || ch == '^' || ch == '.')
            {
                n = new PointD(newPos.X + ((ch == ';' || ch == ':' || ch == '.') ?
                View.CELL_WIDTH / 2 : 0), newPos.Y + ((ch == 'V') ?
                View.CELL_HEIGHT / 2 : (ch == '^') ? -View.CELL_HEIGHT / 2 : 0));
                return true;
            }
            return false;
        }

        List<CircleMovement> addCircularMovement(char ch, PointD pos, PointD newPos)
        {
            var t = cChar[ch];
            var l = new List<CircleMovement>();
            l.Add(new CircleMovement(t.Item3, new PointD(pos.X + View.CELL_WIDTH / 2
                                    + ((ch == '>' || ch == ')' || ch == ']'
                                    || ch == ':') ? View.CELL_WIDTH / 2
                                    : (ch == '<' || ch == '(' || ch == '[') ?
                                    -View.CELL_WIDTH / 2 : 0), pos.Y +
                                    View.CELL_HEIGHT / 2 + ((ch == '!') ?
                                    View.CELL_HEIGHT / 2 : 0)), 0, t.Item1, t.Item2));

            if (checkForSpecialCharacters(ch, newPos, out PointD n))
                l.Add(new CircleMovement(t.Item3, n, 0, t.Item1, t.Item2));
            if (ch == '.') l.Add(new CircleMovement(t.Item3,
                                new PointD(newPos.X - View.CELL_WIDTH / 2,
                                newPos.Y), 0, t.Item1, t.Item2));
            return l;
        }

        List<SquareMovement> addSquareMovement(char ch, PointD newPos)
        {
            var t = sqChar[ch];
            var l = new List<SquareMovement>();
            l.Add(new SquareMovement(t.Item3, newPos, down, t.Item2, t.Item1, t.Item4, t.Item5));
            if (checkForSpecialCharacters(ch, newPos, out PointD n))
                l.Add(new SquareMovement(t.Item3, n, down, t.Item2, t.Item1, t.Item4, t.Item5));
            return l;
        }
    }

}
