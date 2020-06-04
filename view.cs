using System;
using static System.Console;
using Gtk;
using Gdk;
using Cairo;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using window = Gtk.Window;
using static XY_DIRS;
using static CIRCLE_DIRS;
using static hardestgame.Movement;

namespace hardestgame
{

    public class View : window
    {
        public const int CELL_WIDTH = 60, CELL_HEIGHT = 60;
        const uint UPDATE_TIME = 20;
        int SCREEN_WIDTH = 1380, SCREEN_HEIGHT = 850;
        const int MAP_WIDTH = 23, MAP_HEIGHT = 15;
        const int Y_MARGIN = 0, X_MARGIN = 0;
        Gdk.Color BACKGROUND_COLOR = new Gdk.Color(0, 100, 128);

        Pixbuf dollar, obstacle;
        char[,] bg;
        List<PointD> walls, checkPoint, coinPos;
        Player p;
        Obstacle obs;
        bool mainTimer = false;
        double playerOpacity;
        PointD checkPointPos = new PointD(0,0);
        int coinsCollected, totalCoins, level = 9, fails = 0;
        bool pauseGame, safeZone, enemy_collision, roundWon;
        Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double>> cChar;
        Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double, double, double>> sqChar;
        List<char> xChar, yChar;
        List<double> xVel, yVel;
        List<PointD> obsList, hitPt;
        List<double> l = new List<double>() { 0.0, 0.7, 0.0, 0.9 }; //checkPoint colour
        Gdk.Key[] dirs = new Gdk.Key[4] {Gdk.Key.Left, Gdk.Key.Right, Gdk.Key.Up, Gdk.Key.Down };

        public View() : base("World's Hardest game")
        {
            AddEvents((int)(EventMask.ButtonPressMask |
                     EventMask.ButtonReleaseMask |
                     EventMask.KeyPressMask |
                     EventMask.PointerMotionMask));
            Resize(SCREEN_WIDTH, SCREEN_HEIGHT);
            init();
        }

        void init()
        {
            walls = new List<PointD>();
            coinPos = new List<PointD>();
            checkPoint = new List<PointD>();
            hitPt = new List<PointD>();
            obsList = new List<PointD>();
            playerOpacity = 1;
            if (roundWon && (coinsCollected == totalCoins && totalCoins != 0))
            {
                level++;
                checkPointPos = new PointD(0, 0);
            }
            coinsCollected = 0;
            enemy_collision = false;
            roundWon = false;
            xChar = new List<char>();
            yChar = new List<char>();
            xVel = new List<double>();
            yVel = new List<double>();
            cChar = new Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double>>();
            sqChar = new Dictionary<char, Tuple<PointD, CIRCLE_DIRS, double, double, double>>();
            totalCoins = 0;
            pauseGame = false;
            safeZone = false;
            p = new Player();
            dollar = new Pixbuf("./sprites/dollar.png");
            obstacle = new Pixbuf("./sprites/obs.png");
            p.dirs = new bool[4];
            bg = new char[MAP_HEIGHT, MAP_WIDTH];
            updateEnv($"./levels/{level}.txt", out List<CircleMovement> c, out List<XyMovement> xy, out List<SquareMovement> sq);
            obs = new Obstacle(obsList, this.level, hitPt, c, xy, sq);
            startTimer();
            QueueDraw();
        }

        void startTimer()
        {
            if (!mainTimer)
            {
                GLib.Timeout.Add(UPDATE_TIME, delegate
                {
                    if (!enemy_collision)
                    {
                        obs.move(this.level);
                        if (!safeZone) enemyCollision();
                        wallCollision();
                        checkForCoins();
                        p.changePixPos();
                    }
                    insideSafeZone();
                    QueueDraw();
                    if (enemy_collision || roundWon) makePlayerDisappear();
                    return true;

            });
                mainTimer = true;
            }

        }

        void _updateEnv(char ch, PointD pos, PointD newPos, int i, int j)
        {
            if (ch != '1' && ch != '#' && ch != 'W' && ch != ']' && ch != '[' && ch != 'H')
            {
                if (ch == 'P')
                {
                    p.pixPos = (checkPointPos.X != 0 && checkPointPos.Y != 0) ? checkPointPos : pos;
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
                    obsList.Add(new PointD(newPos.X + ((ch == ';') ? CELL_WIDTH / 2 : 0),
                        newPos.Y + ((ch == 'V') ? CELL_HEIGHT / 2 : (ch == '^') ? -CELL_HEIGHT / 2 : 0)));
                }
                else
                {
                    newPos = new PointD(pos.X + 30 + ((ch == '>' || ch == ')') ? CELL_WIDTH / 2
                                        : (ch == '<' || ch == '(') ? -CELL_WIDTH / 2 : 0), pos.Y + 30);
                    obsList.Add(newPos);
                }
                ch = (ch != '!' && ch != ':') ? '1' : 'W';
            }
            bg[i, j] = ch;
            if (ch == 'W') walls.Add(pos);
            else if (ch == '[' || ch == ']')
            {
                walls.Add(pos);
                newPos = new PointD(newPos.X + ((ch == ']') ? CELL_WIDTH / 2 :
                                    (ch == '[') ? -CELL_WIDTH / 2 : 0), newPos.Y);
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
                    sqChar.Add(char.Parse(sp[j]), Tuple.Create(new PointD(x * CELL_WIDTH, y * CELL_HEIGHT), d, vel, l, b));
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

            PointD centre = new PointD(x * CELL_WIDTH + CELL_WIDTH / 2, y * CELL_HEIGHT + CELL_HEIGHT / 2);
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
                    x = x * CELL_WIDTH + CELL_WIDTH / 2;
                    y = y * CELL_HEIGHT + CELL_HEIGHT / 2;

                    for (int i = 0; i < k.Count; i++)
                    {
                        l.Add(new CircleMovement(vel, new PointD(k[i].pos.X + x - k[i].centre.X, k[i].pos.Y + y - k[i].centre.Y), 0, new PointD(x, y), dir));
                    }
                }
            }
            return l;
        }

        List<CircleMovement> addCircularMovement(char ch, PointD pos, PointD newPos)
            {
                var t = cChar[ch];
                var l = new List<CircleMovement>();
                l.Add(new CircleMovement(t.Item3, new PointD(pos.X + CELL_WIDTH / 2 + ((ch == '>' || ch == ')' || ch == ']'
                                        || ch == ':') ? CELL_WIDTH / 2
                                        : (ch == '<' || ch == '(' || ch == '[') ? -CELL_WIDTH / 2 : 0), pos.Y + CELL_HEIGHT / 2  +
                                        ((ch == '!') ? CELL_HEIGHT / 2 : 0)), 0, t.Item1, t.Item2));
                if (ch == ';' || ch == 'V' || ch == '^' || ch == '.')
                {
                    PointD n = new PointD(newPos.X + ((ch == ';' || ch == ':' || ch == '.') ?
                        CELL_WIDTH / 2 : 0), newPos.Y + ((ch == 'V') ? CELL_HEIGHT / 2 : (ch == '^') ? -CELL_HEIGHT / 2 : 0));
                    l.Add(new CircleMovement(t.Item3, n, 0, t.Item1, t.Item2));
                }
                if (ch == '.') l.Add(new CircleMovement(t.Item3, new PointD(newPos.X - CELL_WIDTH / 2, newPos.Y), 0, t.Item1, t.Item2));
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
                    CELL_WIDTH / 2 : 0), newPos.Y + ((ch == 'V') ? CELL_HEIGHT / 2 : (ch == '^') ? -CELL_HEIGHT / 2 : 0));
                l.Add(new SquareMovement(t.Item3, n, down, t.Item2, t.Item1, t.Item4, t.Item5));
            }
            return l;
        }

        void updateEnv(string fileName, out List<CircleMovement> circleMov, out List<XyMovement> xyMov, out List<SquareMovement> sqMov)
        {
            circleMov = new List<CircleMovement>();
            xyMov = new List<XyMovement>();
            sqMov = new List<SquareMovement>();
            using (StreamReader r = new StreamReader(fileName))
            {
                parseMovements(r);
                for(int i = 0; i < MAP_HEIGHT; i++)
                {
                    string s = r.ReadLine();
                    for (int j = 0; j < MAP_WIDTH; j++) {
                        char ch = s[j];
                        PointD pos = new PointD(X_MARGIN + CELL_WIDTH * j, Y_MARGIN + CELL_HEIGHT * i);
                        PointD newPos = new PointD(pos.X + CELL_WIDTH/2, pos.Y + CELL_HEIGHT/2);
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

        void checkForCoins()
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

        void wallCollision()
        {
            PointD pPos = new PointD(p.pixPos.X + p.size.X / 2, p.pixPos.Y + p.size.Y / 2);
            double[] hitPoints = new double[4] { pPos.X - p.SPEED - p.size.X / 2,
                                                pPos.X + p.SPEED + p.size.X / 2,
                                                pPos.Y - p.SPEED - p.size.Y / 2,
                                                pPos.Y + p.SPEED + p.size.Y / 2 };
            bool[] canNotMove = new bool[4]; //left, right, up, down
            foreach (PointD wall in walls)
            {
                PointD wPos = new PointD(wall.X + CELL_WIDTH / 2, wall.Y + CELL_HEIGHT / 2);
                for (int i = 0; i < 4; i++)
                {
                    double wpo = (i < 2) ? wPos.X : wPos.Y;
                    double wpo2 = (i < 2) ? wPos.Y : wPos.X;
                    double ppo = (i < 2) ? pPos.Y : pPos.X;
                    int cell = (i < 2) ? (CELL_WIDTH / 2) - 1 : (CELL_HEIGHT / 2) - 1;
                    int cell2 = (i < 2) ? (CELL_HEIGHT / 2) - 1 : (CELL_WIDTH / 2) - 1 ;
                    double s = (i < 2) ? p.size.Y / 2 : p.size.X / 2;
                    if (coll(hitPoints[i], wpo, cell) && (coll(ppo + s, wpo2, cell2) ||
                        coll(ppo - s, wpo2, cell2)))
                        canNotMove[i] = true;
                }
            }
            p.canNotMove = canNotMove;
        }

        bool withinBounds(PointD po)
        {
            return (po.X >= p.pixPos.X && po.X <= p.pixPos.X + p.size.X &&
                po.Y >= p.pixPos.Y && po.Y <= p.pixPos.Y + p.size.Y);
        }

        bool collision(PointD po, int rad)
        {
            var l = new PointD(po.X - rad, po.Y);
            var r = new PointD(po.X + rad, po.Y);
            var d = new PointD(po.X, po.Y + rad);
            var u = new PointD(po.X, po.Y - rad);
            if (withinBounds(l) || withinBounds(r) || withinBounds(d) || withinBounds(u))
                return true;
            return false;
        }

        void makePlayerDisappear()
        {
            if (!roundWon) p.dirs = new bool[4];
                playerOpacity -= 0.05;
                if (playerOpacity <= 0.0)
                {
                    init();
                    return;
                }
                QueueDraw();
          }

        void enemyCollision()
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

        void insideSafeZone()
        {
            if (!roundWon)
            {
                safeZone = false;
                foreach (PointD pos in checkPoint)
                {
                    PointD po = new PointD(pos.X + CELL_WIDTH / 2, pos.Y + CELL_HEIGHT / 2);
                    if (collision(po, CELL_WIDTH / 2))
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

                if (roundWon)
                {
                    makePlayerDisappear();
                }
            }
         }

        protected override bool OnKeyPressEvent(EventKey evnt)
        {
            for(int i = 0; i < 4; i++)
                if (evnt.Key == this.dirs[i] && !p.dirs[i])
                    p.dirs[i] = true;
            return true;

        }

        protected override bool OnKeyReleaseEvent(EventKey evnt)
        {
            for (int i = 0; i < 4; i++)
                if (evnt.Key == this.dirs[i] && p.dirs[i])
                    p.dirs[i] = false;
            if (!pauseGame && !roundWon) QueueDraw();
            return true;
        }

        void drawPlayer(Context c)
        {
            c.SetSourceRGBA(0.0, 0.0, 0.0, playerOpacity);
            c.Rectangle(p.pixPos.X, p.pixPos.Y, p.size.X, p.size.Y);
            c.Fill();
            c.SetSourceRGBA(1.0, 0.0, 0.0, playerOpacity);
            c.Rectangle(p.pixPos.X + 3, p.pixPos.Y + 3, p.size.X - 6, p.size.Y - 6 );
            c.Fill();
        }

        void drawMap(Context c)
        {
            for (int i = 0; i < MAP_HEIGHT; i++)
            {
                for (int j = 0; j < MAP_WIDTH; j++)
                {
                    if (bg[i, j] == '1')
                    {
                        PointD currPos = new PointD(X_MARGIN + CELL_WIDTH * j, Y_MARGIN + CELL_HEIGHT * i);
                        c.MoveTo(currPos);
                        if (j % 2 == i % 2) c.SetSourceRGB(1.0, 1.0, 1.0);
                        else c.SetSourceRGB(0.6, 0.9, 0.9);
                        c.Rectangle(currPos.X, currPos.Y, CELL_WIDTH, CELL_HEIGHT);
                        c.Fill();
                    }
                }
            }
        }

        void updateLevels(Context c, string st, PointD p)
        {
            c.SetFontSize(30);
            c.SetSourceRGB(1,1,1);
            TextExtents te = c.TextExtents(st);
            PointD mp = new PointD(10 + p.X - (te.Width / 2 + te.XBearing), 10 + p.Y - (te.Height / 2 + te.YBearing));
            c.MoveTo(mp);
            c.ShowText(st);
            c.Stroke();
        }

        void updateScoreAndLives(Context c)
        {
            c.SetSourceRGB(0.1,0.4,0.6);
            c.Rectangle(0, 0, SCREEN_WIDTH, 50);
            c.Fill();
            updateLevels(c, $"LEVEL : {this.level}", new PointD(120, 15));
            updateLevels(c, $"FAILS : {this.fails}", new PointD(1180, 15));
            updateLevels(c, $"COINS : {this.coinsCollected} / {this.totalCoins}", new PointD(700, 15));
            c.SetSourceRGB(0.1, 0.4, 0.6);
            c.Rectangle(0, 800, SCREEN_WIDTH, 50);
            c.Fill();
        }

        void drawCheckPoints(Context c)
        {
            c.SetSourceRGBA(l[0], l[1], l[2], l[3]);
            foreach (PointD pos in checkPoint)
            {
                c.Rectangle(pos.X, pos.Y, CELL_WIDTH, CELL_HEIGHT);
                c.Fill();
             }
        }

        void drawCoins(Context c)
        {
            c.SetSourceRGB(0.8, 0.6, 0.3);
            foreach (PointD pos in coinPos)
            {
                CairoHelper.SetSourcePixbuf(c, dollar, pos.X - CELL_WIDTH / 4, pos.Y - CELL_HEIGHT / 4);
                c.Paint();
            }
        }

        void drawObstacles(Context c)
        {
            foreach (PointD pos in obs.pos)
            {
                CairoHelper.SetSourcePixbuf(c, obstacle, pos.X - CELL_WIDTH / 4, pos.Y - CELL_HEIGHT / 4);
                c.Paint();
            }
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            using (Context c = CairoHelper.Create(GdkWindow))
            {
                ModifyBg(StateType.Normal, BACKGROUND_COLOR);
                drawMap(c);
                drawCheckPoints(c);
                drawCoins(c);
                updateScoreAndLives(c);
                drawObstacles(c);
                drawPlayer(c);
            }
            p.canNotMove = new bool[4];
            return true;
        }

        protected bool OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
            return true;
        }


        static void Main()
        {
            Application.Init();
            View v = new View();
            v.ShowAll();
            Application.Run();
        }
    }
}
