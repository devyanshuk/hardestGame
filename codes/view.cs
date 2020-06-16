using Gtk;
using System;
using System.Media;
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
        const int MUSIC_ICON_X = 1300, MUSIC_ICON_Y = 800,
                  MUSIC_ICON_WIDTH = 48, MUSIC_ICON_HEIGHT = 48;

        Pixbuf dollar, obstacle, musicOn, musicOff;
        SoundPlayer music;
        bool playMusic = true;
        PointD mouseLocation;

        Game game;

        public View() : base("World's Hardest game")
        {
            AddEvents((int)(EventMask.ButtonPressMask |
                     EventMask.ButtonReleaseMask |
                     EventMask.KeyPressMask |
                     EventMask.ButtonPressMask |
                     EventMask.PointerMotionMask));
            Resize(SCREEN_WIDTH, SCREEN_HEIGHT);
            ModifyBg(StateType.Normal, BACKGROUND_COLOR);
            game = new Game();
            init();
        }

        void init()
        {
            music = new SoundPlayer();
            game.init();
            dollar = new Pixbuf("./sprites/dollar.png");
            obstacle = new Pixbuf("./sprites/obs.png");
            musicOn = new Pixbuf("./music/music_on.png");
            musicOff = new Pixbuf("./music/music_off.png");
            //music.SoundLocation = "./music/ffmusic.wav";
            //music.Load();
            //music.PlayLooping();
            game.gameStateChanged += QueueDraw;
            game.player.opacityChanged += init;
        }

        protected override bool OnKeyPressEvent(EventKey evnt)
        {
            game.player.updateDir(false, evnt);
            return true;
        }

        protected override bool OnKeyReleaseEvent(EventKey evnt)
        {
            game.player.updateDir(true, evnt);
            return true;
        }

        protected override bool OnMotionNotifyEvent(EventMotion evnt)
        {
            mouseLocation = new PointD(evnt.X, evnt.Y);
            return true;
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            double a = mouseLocation.X, b = mouseLocation.Y;
            if (a >= MUSIC_ICON_X && a <= MUSIC_ICON_X + MUSIC_ICON_WIDTH &&
                b >= MUSIC_ICON_Y && b <= MUSIC_ICON_Y + MUSIC_ICON_HEIGHT)
                playMusic = !playMusic;
            return true;
        }

        void drawPlayer(Context c)
        {
            c.SetSourceRGBA(0.0, 0.0, 0.0, game.player.opacity);
            c.Rectangle(game.player.pixPos.X, game.player.pixPos.Y,
                        game.player.size.X, game.player.size.Y);
            c.Fill();
            c.SetSourceRGBA(game.player.red, game.player.green, game.player.blue, game.player.opacity);
            c.Rectangle(game.player.pixPos.X + 3, game.player.pixPos.Y + 3,
                       game.player.size.X - 6, game.player.size.Y - 6);
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
                        if (j % 2 == i % 2)
                            c.SetSourceRGB(1.0, 1.0, 1.0);
                        else
                            c.SetSourceRGB(0.6, 0.9, 0.9);
                        c.Rectangle(currPos.X, currPos.Y, CELL_WIDTH, CELL_HEIGHT);
                        c.Fill();
                    }
                }
            }
        }

        void updateLevels(Context c, string st, PointD p)
        {
            c.SetFontSize(30);
            c.SetSourceRGB(1, 1, 1);
            TextExtents te = c.TextExtents(st);
            PointD mp = new PointD(10 + p.X - (te.Width / 2 + te.XBearing),
                                    10 + p.Y - (te.Height / 2 + te.YBearing));
            c.MoveTo(mp);
            c.ShowText(st);
            c.Stroke();
        }

        void updateScoreAndLives(Context c)
        {
            c.SetSourceRGB(0.1, 0.4, 0.6);
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
            foreach (var k in game.checkPoint)
            {
                if (k.beingAnimated)
                    k.animateCheckPoint();
                c.SetSourceRGBA(k.red, k.green, k.blue, k.opacity);
                c.Rectangle(k.topLeftPos.X, k.topLeftPos.Y, k.length, k.height);
                c.Fill();
            }
        }

        void drawSprite(Context c, List<PointD> lis, Pixbuf p)
        {
            foreach (PointD pos in lis)
            {
                CairoHelper.SetSourcePixbuf(c, p, pos.X - CELL_WIDTH / 4, pos.Y - CELL_HEIGHT / 4);
                c.Paint();
            }
        }

        void drawMusicIcon(Context c)
        {
            Pixbuf p = playMusic ? musicOn : musicOff;
            CairoHelper.SetSourcePixbuf(c, p, MUSIC_ICON_X, MUSIC_ICON_Y);
            c.Paint();
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            using (Context c = CairoHelper.Create(GdkWindow))
            {
                drawMap(c);
                drawCheckPoints(c);
                drawSprite(c, game.coinPos, dollar);
                updateScoreAndLives(c);
                drawSprite(c, game.obs.pos, obstacle);
                drawPlayer(c);
                drawMusicIcon(c);
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
