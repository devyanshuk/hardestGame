using Gtk;
using Gdk;
using Cairo;
using System.Collections.Generic;
using window = Gtk.Window;

namespace hardestgame
{
    public class View : window
    {
        public const int CELL_WIDTH = 60, CELL_HEIGHT = 60;
        public const int SCREEN_WIDTH = 1380, SCREEN_HEIGHT = 850;
        Gdk.Color BACKGROUND_COLOR = new Gdk.Color(0, 100, 128);
        public const int Y_MARGIN = 0, X_MARGIN = 0;
        Gdk.Key[] DIRS = new Gdk.Key[4] { Gdk.Key.Left,
                                          Gdk.Key.Right,
                                          Gdk.Key.Up,
                                          Gdk.Key.Down
                                        };

        List<double> l = new List<double>() { 0.0, 0.7, 0.0, 0.9 }; //checkPoint colour
        Pixbuf dollar, obstacle;
        public const uint UPDATE_TIME = 20;
        bool mainTimer = false;
        double playerOpacity;
        Game game = new Game();

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
            game.init();
            dollar = new Pixbuf("./sprites/dollar.png");
            obstacle = new Pixbuf("./sprites/obs.png");
            playerOpacity = 1;
            startTimer();
        }

        void startTimer()
        {
            if (!mainTimer)
            {
                GLib.Timeout.Add(UPDATE_TIME, delegate
                {
                    if (!game.enemy_collision)
                    {
                        game.obs.move(game.level);
                        if (!game.safeZone) game.enemyCollision();
                        game.wallCollision();
                        game.checkForCoins();
                        game.player.changePixPos();
                    }
                    game.insideSafeZone();
                    QueueDraw();
                    if (game.enemy_collision || game.roundWon)
                        makePlayerDisappear();
                    return true;
            });
                mainTimer = true;
            }
        }

        void makePlayerDisappear()
        {
            if (!game.roundWon) game.player.dirs = new bool[4];
                playerOpacity -= 0.05;
                if (playerOpacity <= 0.0)
                    init();
          }

        protected override bool OnKeyPressEvent(EventKey evnt)
        {
            for(int i = 0; i < 4; i++)
                if (evnt.Key == DIRS[i] && !game.player.dirs[i])
                    game.player.dirs[i] = true;
            return true;

        }

        protected override bool OnKeyReleaseEvent(EventKey evnt)
        {
            for (int i = 0; i < 4; i++)
                if (evnt.Key == DIRS[i] && game.player.dirs[i])
                    game.player.dirs[i] = false;
            if (!game.pauseGame && !game.roundWon) QueueDraw();
            return true;
        }

        void drawPlayer(Context c)
        {
            c.SetSourceRGBA(0.0, 0.0, 0.0, playerOpacity);
            c.Rectangle(game.player.pixPos.X, game.player.pixPos.Y,
                        game.player.size.X, game.player.size.Y);
            c.Fill();
            c.SetSourceRGBA(1.0, 0.0, 0.0, playerOpacity);
            c.Rectangle(game.player.pixPos.X + 3, game.player.pixPos.Y + 3,
                       game.player.size.X - 6, game.player.size.Y - 6 );
            c.Fill();
        }

        void drawMap(Context c)
        {
            for (int i = 0; i < Game.MAP_HEIGHT; i++)
            {
                for (int j = 0; j < Game.MAP_WIDTH; j++)
                {
                    if (game.bg[i, j] == '1')
                    {
                        PointD currPos = new PointD(X_MARGIN + CELL_WIDTH * j,
                                                    Y_MARGIN + CELL_HEIGHT * i);
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
            PointD mp = new PointD(10 + p.X - (te.Width / 2 + te.XBearing),
                                   10 + p.Y - (te.Height / 2 + te.YBearing));
            c.MoveTo(mp);
            c.ShowText(st);
            c.Stroke();
        }

        void updateScoreAndLives(Context c)
        {
            c.SetSourceRGB(0.1,0.4,0.6);
            c.Rectangle(0, 0, SCREEN_WIDTH, 50);
            c.Fill();
            updateLevels(c, $"LEVEL : {game.level}", new PointD(120, 15));
            updateLevels(c, $"FAILS : {game.fails}", new PointD(1180, 15));
            updateLevels(c, $"COINS : {game.coinsCollected} / {game.totalCoins}",
                        new PointD(700, 15));
            c.SetSourceRGB(0.1, 0.4, 0.6);
            c.Rectangle(0, 800, SCREEN_WIDTH, 50);
            c.Fill();
        }

        void drawCheckPoints(Context c)
        {
            c.SetSourceRGBA(l[0], l[1], l[2], l[3]);
            foreach (PointD pos in game.checkPoint)
            {
                c.Rectangle(pos.X, pos.Y, CELL_WIDTH, CELL_HEIGHT);
                c.Fill();
             }
        }

        void drawCoins(Context c)
        {
            c.SetSourceRGB(0.8, 0.6, 0.3);
            foreach (PointD pos in game.coinPos)
            {
                CairoHelper.SetSourcePixbuf(c, dollar, pos.X - CELL_WIDTH / 4,
                                            pos.Y - CELL_HEIGHT / 4);
                c.Paint();
            }
        }

        void drawObstacles(Context c)
        {
            foreach (PointD pos in game.obs.pos)
            {
                CairoHelper.SetSourcePixbuf(c, obstacle, pos.X - CELL_WIDTH / 4,
                                            pos.Y - CELL_HEIGHT / 4);
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
            game.player.canNotMove = new bool[4];
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