using System;
using Cairo;
using System.Collections.Generic;
using System.Linq;
using static XY_DIRS;
using static CIRCLE_DIRS;
using static hardestgame.Movement;
using System.IO;

namespace hardestgame
{
    public class Game
    {
        public const int MAP_WIDTH = 23, MAP_HEIGHT = 15;
        public Player player;
        public Obstacle obs;
        public PointD checkPointPos = new PointD(0, 0);
        public int coinsCollected, totalCoins, level = 7, fails = 0;
        public List<PointD> walls, checkPoint, coinPos;
        public bool pauseGame, safeZone, enemy_collision, roundWon;
        public char[,] bg;
        Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double>> cChar;
        Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double, double, double>> sqChar;
        List<char> xChar, yChar;
        List<double> xVel, yVel;
        public List<PointD> obsList, hitPt;

        public void init()
        {
            walls = new List<PointD>();
            coinPos = new List<PointD>();
            checkPoint = new List<PointD>();
            hitPt = new List<PointD>();
            obsList = new List<PointD>();

            xChar = new List<char>();
            yChar = new List<char>();
            xVel = new List<double>();
            yVel = new List<double>();
            cChar = new Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double>>();
            sqChar = new Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double, double, double>>();

            if (roundWon && (coinsCollected == totalCoins && totalCoins != 0))
            {
                level++;
                checkPointPos = new PointD(0, 0);
            }
            coinsCollected = 0;
            enemy_collision = false;
            roundWon = false;
            totalCoins = 0;
            pauseGame = false;
            safeZone = false;
            player = new Player();
            player.dirs = new bool[4];
            bg = new char[MAP_HEIGHT, MAP_WIDTH];
            updateEnv($"./levels/{this.level}.txt", out List<CircleMovement> c,
                      out List<XyMovement> xy, out List<SquareMovement> sq);
            obs = new Obstacle(obsList, this.level, hitPt, c, xy, sq);
        }

        void _updateEnv(char ch, PointD pos, PointD newPos, int i, int j)
        {
            if (ch != '1' && ch != '#' && ch != 'W' && ch != ']' && ch != '[' && ch != 'H')
            {
                if (ch == 'P')
                {
                    player.pixPos = (checkPointPos.X != 0 && checkPointPos.Y != 0) ? checkPointPos : pos;
                    checkPoint.Add(pos);
                }
                else if (ch == 'C') checkPoint.Add(pos);
                else if (ch == 'X')
                {
                    totalCoins++;
                    coinPos.Add(newPos);
                }
                else if (ch == '2')
                {
                    totalCoins++;
                    coinPos.Add(newPos);
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
                    newPos = new PointD(pos.X + 30 + ((ch == '>' || ch == ')')?
                                        View.CELL_WIDTH / 2 : (ch == '<' || ch == '(') ?
                                        -View.CELL_WIDTH / 2 : 0), pos.Y + 30);
                    obsList.Add(newPos);
                }
                ch = (ch != '!' && ch != ':') ? '1' : 'W';
            }
            bg[i, j] = ch;
            if (ch == 'W') walls.Add(pos);
            else if (ch == '[' || ch == ']')
            {
                walls.Add(pos);
                newPos = new PointD(newPos.X + ((ch == ']') ? View.CELL_WIDTH / 2 :
                                    (ch == '[') ? -View.CELL_WIDTH / 2 : 0), newPos.Y);
                obsList.Add(newPos);
            }

            else if (ch == 'H')
            {
                walls.Add(pos);
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
                                Tuple.Create(new PointD(x * View.CELL_WIDTH,
                                y * View.CELL_HEIGHT), d, vel, l, b));
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


        void parseMovements(StreamReader r)
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

            if (ch == ';' || ch == 'V' || ch == '^' || ch == '.')
            {
                PointD n = new PointD(newPos.X + ((ch == ';' || ch == ':' || ch == '.') ?
                    View.CELL_WIDTH / 2 : 0), newPos.Y + ((ch == 'V') ? View.CELL_HEIGHT / 2 :
                    (ch == '^') ? -View.CELL_HEIGHT / 2 : 0));
                l.Add(new CircleMovement(t.Item3, n, 0, t.Item1, t.Item2));
            }
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
            if (ch == ';' || ch == 'V' || ch == '^' || ch == '.')
            {
                PointD n = new PointD(newPos.X + ((ch == ';' || ch == ':' || ch == '.') ?
                    View.CELL_WIDTH / 2 : 0), newPos.Y + ((ch == 'V') ?
                    View.CELL_HEIGHT / 2 : (ch == '^') ? -View.CELL_HEIGHT / 2 : 0));

                l.Add(new SquareMovement(t.Item3, n, down, t.Item2, t.Item1, t.Item4, t.Item5));
            }
            return l;
        }

        public void updateEnv(string fileName, out List<CircleMovement> circleMov, out List<XyMovement> xyMov, out List<SquareMovement> sqMov)
        {
            circleMov = new List<CircleMovement>();
            xyMov = new List<XyMovement>();
            sqMov = new List<SquareMovement>();
            using (StreamReader r = new StreamReader(fileName))
            {
                parseMovements(r);
                for (int i = 0; i < MAP_HEIGHT; i++)
                {
                    string s = r.ReadLine();
                    for (int j = 0; j < MAP_WIDTH; j++)
                    {
                        char ch = s[j];

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
                        _updateEnv(ch, pos, newPos, i, j);
                    }
                }
                var k = circleMov;
                circleMov.AddRange(copyCircleMovements(r, k));
            }
        }

        public void checkForCoins()
        {
            foreach (PointD pos in coinPos)
            {
                if (collision(pos, Obstacle.RADIUS))
                {
                    coinsCollected++;
                    coinPos.Remove(pos);
                    break;
                }
            }
        }
        bool coll(double a, double pos, int r) => (a >= (pos - r) && a <= (pos + r));

        public void wallCollision()
        {
            PointD pPos = new PointD(player.pixPos.X + player.size.X / 2,
                                     player.pixPos.Y + player.size.Y / 2);
            double[] hitPoints = new double[4] { pPos.X - player.SPEED - player.size.X / 2,
                                                pPos.X + player.SPEED + player.size.X / 2,
                                                pPos.Y - player.SPEED - player.size.Y / 2,
                                                pPos.Y + player.SPEED + player.size.Y / 2 };
            bool[] canNotMove = new bool[4]; //left, right, up, down
            foreach (PointD wall in walls)
            {
                PointD wPos = new PointD(wall.X + View.CELL_WIDTH / 2, wall.Y + View.CELL_HEIGHT / 2);
                for (int i = 0; i < 4; i++)
                {
                    double wpo = (i < 2) ? wPos.X : wPos.Y;
                    double wpo2 = (i < 2) ? wPos.Y : wPos.X;
                    double ppo = (i < 2) ? pPos.Y : pPos.X;
                    int cell = (i < 2) ? (View.CELL_WIDTH / 2) - 1
                                : (View.CELL_HEIGHT / 2) - 1;
                    int cell2 = (i < 2) ? (View.CELL_HEIGHT / 2) - 1
                                : (View.CELL_WIDTH / 2) - 1;
                    double s = (i < 2) ? player.size.Y / 2 : player.size.X / 2;
                    if (coll(hitPoints[i], wpo, cell) && (coll(ppo + s, wpo2, cell2) ||
                        coll(ppo - s, wpo2, cell2)))
                        canNotMove[i] = true;
                }
            }
            player.canNotMove = canNotMove;
        }

        bool withinBounds(PointD po)
        {
            return (po.X >= player.pixPos.X && po.X <= player.pixPos.X + player.size.X &&
                po.Y >= player.pixPos.Y && po.Y <= player.pixPos.Y + player.size.Y);
        }

        public bool collision(PointD po, int rad)
        {
            var l = new PointD(po.X - rad, po.Y);
            var r = new PointD(po.X + rad, po.Y);
            var d = new PointD(po.X, po.Y + rad);
            var u = new PointD(po.X, po.Y - rad);
            if (withinBounds(l) || withinBounds(r) || withinBounds(d) || withinBounds(u))
                return true;
            return false;
        }

        public void enemyCollision()
        {
            if (!roundWon)
            {
                foreach (PointD po in obs.pos)
                {
                    if (collision(po, Obstacle.RADIUS / 2))
                    {
                        fails++;
                        enemy_collision = true;
                        pauseGame = true;
                        break;
                    }
                }
            }
        }

        public void insideSafeZone()
        {
            if (!roundWon)
            {
                safeZone = false;
                foreach (PointD pos in checkPoint)
                {
                    PointD po = new PointD(pos.X + View.CELL_WIDTH / 2, pos.Y + View.CELL_HEIGHT / 2);
                    if (collision(po, View.CELL_WIDTH / 2))
                    {
                        checkPointPos = pos;
                        safeZone = true;
                        if (!enemy_collision && coinsCollected == totalCoins && totalCoins != 0)
                        {
                            roundWon = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
