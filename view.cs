using System;
using Gtk;
using Gdk;
using Cairo;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using window = Gtk.Window;


namespace hardestgame
{

    public class View : window
    {
        const int CELL_WIDTH = 60, CELL_HEIGHT = 60;
        int yMargin = 0, xMargin = 0;
        Pixbuf dollar;
        char[,] bg;
        List<PointD> walls;
        Player p;
        obstacle obs;
        List<PointD> checkPoint;
        int width = 1380, height = 850;
        double playerOpacity;
        int mapWidth = 23, mapHeight = 13;
        PointD checkPointPos = new PointD(0,0);
        int coinsCollected, totalCoins, level = 4, fails = 0;
        bool pauseGame;
        bool enemy_collision;
        bool roundWon;
		bool mainTimer = false;
        int circRad = 10;
        List<double> l = new List<double>() { 0.0, 0.7, 0.0, 0.9 }; //checkPoint colour
        Gdk.Key[] dirs = new Gdk.Key[4] {Gdk.Key.Left, Gdk.Key.Right, Gdk.Key.Up, Gdk.Key.Down };
        List<PointD> coinPos;

        public View() : base("World's Hardest game")
        {
            AddEvents((int)(EventMask.ButtonPressMask |
                     EventMask.ButtonReleaseMask |
                     EventMask.KeyPressMask |
                     EventMask.PointerMotionMask));
            Resize(width, height);
            init();
        }

        void init()
        {
            walls = new List<PointD>();
            coinPos = new List<PointD>();
            checkPoint = new List<PointD>();
            playerOpacity = 1;
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
            p = new Player();
            dollar = new Pixbuf("./dollar.png");
            p.dirs = new bool[4];
            bg = new char[mapHeight,mapWidth];
            var lis = updateEnv($"./levels/{level}.txt");
            obs = new obstacle(lis, this.level);
            p.changed += QueueDraw;
            obs.changed += QueueDraw;
            startTimer();
            QueueDraw();
        }

		void startTimer()
        {
            if (!mainTimer)
            {
                GLib.Timeout.Add(10, delegate
                {
                    if (!enemy_collision)
                    {
                        obs.move(this.level);
                        enemyCollision();
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

        List<PointD> updateEnv(string fileName)
        {
            List<PointD> lis = new List<PointD>();
            using (StreamReader r = new StreamReader(fileName))
            {
                for(int i = 0; i < mapHeight; i++)
                {
                    string s = r.ReadLine();
                    for (int j = 0; j < mapWidth; j++) {
                        char currChar = s[j];
                        PointD pos = new PointD(xMargin + CELL_WIDTH * j, yMargin + CELL_HEIGHT * i);
                        if (currChar != '1' && currChar != '#')
                        {
                            if (currChar == 'P')
                            {
                                p.pixPos = (checkPointPos.X != 0 && checkPointPos.Y != 0) ? checkPointPos : pos;
                                checkPoint.Add(pos);
                            }
                            else if (currChar == 'C') checkPoint.Add(pos);
                            else if (currChar == 'X')
                            {
                                totalCoins++;
                                coinPos.Add(new PointD(pos.X + CELL_WIDTH / 2, pos.Y + CELL_HEIGHT / 2));
                            }
                            else if (currChar == '2')
                            {
                                totalCoins++;
                                coinPos.Add(new PointD(pos.X + CELL_WIDTH / 2, pos.Y + CELL_HEIGHT / 2));
                                lis.Add(new PointD(pos.X + CELL_WIDTH / 2, pos.Y + CELL_HEIGHT / 2));

                            }
                            else lis.Add(new PointD(pos.X + CELL_WIDTH / 2 + ((currChar =='>')? CELL_WIDTH/2 : (currChar == '<')? -CELL_WIDTH/2 : 0), pos.Y + CELL_HEIGHT / 2)) ;
                            currChar = '1';
                        }

                        bg[i, j] = currChar;
                        if (currChar == '#') walls.Add(pos);
                    }
                }
            }
            return lis;
        }

        void checkForCoins()
        {
            foreach (PointD pos in coinPos)
            {
                if (collision(pos, circRad))
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
            double[] hitPoints = new double[4] { pPos.X - p.speed - p.size.X / 2, pPos.X + p.speed + p.size.X / 2, pPos.Y - p.speed - p.size.Y / 2, pPos.Y + p.speed + p.size.Y / 2 };
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
                    if (coll(hitPoints[i], wpo, cell) && (coll(ppo + s, wpo2, cell2) || coll(ppo - s, wpo2, cell2))) canNotMove[i] = true;
                }
            }
            p.canNotMove = canNotMove;
        }

        bool withinBounds(PointD po)
        {
            return (po.X >= p.pixPos.X && po.X <= p.pixPos.X + p.size.X && po.Y >= p.pixPos.Y && po.Y <= p.pixPos.Y + p.size.Y);
        }

        bool collision(PointD po, int rad)
        {
            var l = new PointD(po.X - rad, po.Y);
            var r = new PointD(po.X + rad, po.Y);
            var d = new PointD(po.X, po.Y + rad);
            var u = new PointD(po.X, po.Y - rad);
            if (withinBounds(l) || withinBounds(r) || withinBounds(d) || withinBounds(u))
            {
                return true;
            }
            return false;
        }

        void makePlayerDisappear()
        {
            if (!roundWon) p.dirs = new bool[4];
                playerOpacity -= 0.02;
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
                    if (collision(po, obs.size / 2))
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
                foreach (PointD pos in checkPoint)
                {
                    PointD po = new PointD(pos.X + CELL_WIDTH / 2, pos.Y + CELL_HEIGHT / 2);
                    if (collision(po, CELL_WIDTH / 2))
                    {
                        checkPointPos = pos;
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
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (bg[i, j] == '1')
                    {
                        PointD currPos = new PointD(xMargin + CELL_WIDTH * j, yMargin + CELL_HEIGHT * i);
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
            c.Rectangle(0, 0, width, 50);
            c.Fill();
            updateLevels(c, $"LEVEL : {this.level}", new PointD(120, 15));
            updateLevels(c, $"FAILS : {this.fails}", new PointD(1180, 15));
            updateLevels(c, $"COINS : {this.coinsCollected} / {this.totalCoins}", new PointD(700, 15));
            c.SetSourceRGB(0.1, 0.4, 0.6);
            c.Rectangle(0, 755, width, 50);
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
                CairoHelper.SetSourcePixbuf(c, dollar, pos.X - 15, pos.Y - 15);
                c.Paint();
            }
        }

        void drawObstacles(Context c)
        {
            foreach(PointD pos in obs.pos)
            {
                c.SetSourceRGB(0, 0, 0);
                c.Arc(pos.X, pos.Y, obs.size, 0, 2 * Math.PI);
                c.Fill();
                c.SetSourceRGB(0, 0, 1);
                c.Arc(pos.X, pos.Y, obs.size - 3, 0, 2 * Math.PI);
                c.Fill();
            }
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            using (Context c = CairoHelper.Create(GdkWindow))
            {
                c.SetSourceRGB(0.0,0.5,0.5);
                c.Arc(7 * 60 + 30, 11 * 60 + 30, 5, 0, 2 * Math.PI);
                c.Rectangle(0,0, width, height);
                c.Fill();
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
