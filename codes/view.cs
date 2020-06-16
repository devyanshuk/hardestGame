using Gtk;
using System;
//using System.Media;
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

        const int MUSIC_ICON_X = 1300, MUSIC_ICON_Y = 800,
                  MUSIC_ICON_WIDTH = 48, MUSIC_ICON_HEIGHT = 48;

        const int MENU_ICON_X = 20, MENU_ICON_Y = 800,
                  MENU_ICON_WIDTH = 50, MENU_ICON_HEIGHT = 50;



        Pixbuf dollar, obstacle, musicOn, musicOff, menu;
        //SoundPlayer music;
        bool playMusic = true;
        PointD mouseLocation;

        //int currPage = 1;
        bool displayAllLevels;
        bool menuLogoSelected;

        Parser parser = new Parser();
        char[,] bg;
        List<ImageSurface> allLevels;

        ImageSurface menuOptions;

        Game game;

        public View() : base("World's Hardest game")
        {
            dollar = new Pixbuf("./sprites/dollar.png");
            obstacle = new Pixbuf("./sprites/obs.png");
            musicOn = new Pixbuf("./music/music_on.png");
            musicOff = new Pixbuf("./music/music_off.png");
            menu = new Pixbuf("./sprites/menu.png").ScaleSimple(50, 50, InterpType.Bilinear);
            menuOptions = new ImageSurface(Format.Argb32, 500, 500);
            addMenuOptions();

            //music.SoundLocation = "../../ffmusic.wav";

            allLevels = new List<ImageSurface>();
            addAllLevels();

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
            menuLogoSelected = false;
            displayAllLevels = false;
            //music = new SoundPlayer();
            game.init();
            //music.Load();
            //music.PlayLooping();
            game.gameStateChanged += QueueDraw;
            game.player.opacityChanged += init;
        }

        void addMenuOptions()
        {
            using (Context c = new Context(menuOptions))
            {
                c.SetSourceRGBA(0.3, 0, 0.6, 0.3);
                c.Rectangle(0, 0, 500, 500);
                c.Fill();
            }
        }

        void addAllLevels()
        {
            int level = 1;
            while (true)
            {
                try
                {
                    bg = new char[Game.MAP_HEIGHT, Game.MAP_WIDTH];
                    parser.updateEnv($"./levels/{level}.txt", ref bg);
                    level++;
                    ImageSurface s = new ImageSurface(Format.Argb32, 460, 284);
                    using (Context c = new Context(s))
                    {
                        c.SetSourceRGBA(1.0, 0, 0, 0.5);
                        c.Rectangle(new PointD(0,0), 460, 284);
                        c.Fill();
                        drawMap(c, 20, 20, bg, true);
                        allLevels.Add(s);
                    }
                }

                catch (System.IO.FileNotFoundException)
                {
                    break;
                }
            }
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

        bool pointerInsideObj(double a, double b, int x, int y, int l, int h)
        {
            return (a >= x && a <= x + l && b >= y && b <= y + h);
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            double a = mouseLocation.X, b = mouseLocation.Y;

            if (pointerInsideObj(a,b, MUSIC_ICON_X, MUSIC_ICON_Y, MUSIC_ICON_WIDTH, MUSIC_ICON_HEIGHT))
                playMusic = !playMusic;

            if (pointerInsideObj(a, b, MENU_ICON_X, MENU_ICON_Y, MENU_ICON_WIDTH, MENU_ICON_HEIGHT))
                menuLogoSelected = !menuLogoSelected;

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

        void drawMap(Context c, int x, int y, char[,] bg, bool allLevels)
        {
            for (int i = 0; i < Game.MAP_HEIGHT; i++)
            {
                for (int j = 0; j < Game.MAP_WIDTH; j++)
                {
                    PointD currPos = new PointD(X_MARGIN + x * j,
                                                Y_MARGIN + y * i);
                    if (bg[i, j] == '1')
                    {
                        c.MoveTo(currPos);
                        if (j % 2 == i % 2) c.SetSourceRGB(1.0, 1.0, 1.0);
                        else c.SetSourceRGB(0.6, 0.9, 0.9);
                        c.Rectangle(currPos.X, currPos.Y, x, y);
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

        void displayLevelMenu(Context c)
        {
            if (displayAllLevels)
            {
                c.SetSourceSurface(allLevels[4], 20, 560);
                c.Paint();
            }
        }

        void displayMenu(Context c)
        {
            if (menuLogoSelected)
            {
                c.SetSourceSurface(menuOptions, 400, 190);
                c.Paint();
            }
        }


        protected override bool OnExposeEvent(EventExpose evnt)
        {
            using (Context c = CairoHelper.Create(GdkWindow))
            {
                drawMap(c, CELL_WIDTH, CELL_HEIGHT, game.bg, false);
                drawCheckPoints(c);
                drawSprite(c, game.coinPos, dollar);
                updateScoreAndLives(c);
                drawSprite(c, game.obs.pos, obstacle);
                drawPlayer(c);
                drawMusicIcon(c);
                CairoHelper.SetSourcePixbuf(c, menu, MENU_ICON_X, MENU_ICON_Y);
                c.Paint();
                displayMenu(c);
                displayLevelMenu(c);
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
