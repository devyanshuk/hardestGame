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
        public const int SCREEN_WIDTH = 1380, SCREEN_HEIGHT = 800;
        Gdk.Color BACKGROUND_COLOR = new Gdk.Color(0, 100, 128);
        public const int Y_MARGIN = 0, X_MARGIN = 0;

        public static List<Gdk.Key> DIRS = new List<Gdk.Key>
                                          { Gdk.Key.Left,
                                            Gdk.Key.Right,
                                            Gdk.Key.Up,
                                            Gdk.Key.Down
                                          };

        Rectangle musicIcon = new Rectangle();

        Rectangle menuIcon = new Rectangle();

        Rectangle levelSelect;

        Pixbuf dollar, obstacle, musicOn, musicOff, menu, greenMenu;
        //SoundPlayer music;
        bool playMusic = true;
        public bool mouseOnMenuIcon;
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

            musicIcon.topLeftPos = new PointD(1300, 750);
            musicIcon.width = 48;
            musicIcon.height = 48;

            menuIcon.topLeftPos = new PointD(20, 750);
            menuIcon.width = 50;
            menuIcon.height = 50;

            initializeMenuOptions();

            dollar = new Pixbuf("./sprites/dollar.png");
            greenMenu = new Pixbuf("./sprites/menu_green.png").ScaleSimple((int)menuIcon.width,
                             (int)menuIcon.height, InterpType.Bilinear);
            obstacle = new Pixbuf("./sprites/obs.png");
            musicOn = new Pixbuf("./music/music_on.png").ScaleSimple((int)musicIcon.width,
                                   (int)musicIcon.height, InterpType.Bilinear);
            musicOff = new Pixbuf("./music/music_off.png");
            menu = new Pixbuf("./sprites/menu.png").ScaleSimple((int)menuIcon.width,
                             (int)menuIcon.height, InterpType.Bilinear);

            mouseOnMenuIcon = false;


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

        void initializeMenuOptions()
        {
            levelSelect = new Rectangle();
            levelSelect.text = $"Select level";
            levelSelect.red = 0;
            levelSelect.green = 0.5;
            levelSelect.blue = 0;
            levelSelect.opacity = 0.5;

        }

        void addMenuOptions()
        {
             using (Context c = new Context(menuOptions))
             {
                 c.SetSourceRGBA(0.3, 0.1, 0.2, 0.5);
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
            menuLogoSelected = !DIRS.Contains(evnt.Key);
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
            mouseOnMenuIcon = menuIcon.collision(mouseLocation);
            return true;
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            double a = mouseLocation.X, b = mouseLocation.Y;
            PointD p = new PointD(a, b);

            if (musicIcon.collision(p))
                playMusic = !playMusic;

            if (menuIcon.collision(p))
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

        void displayText(Context c, string st, PointD p, int fontSize)
        {
            c.SetFontSize(fontSize);
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
            displayText(c, $"LEVEL : {game.level}", new PointD(120, 15), 30);
            displayText(c, $"FAILS : {game.fails}", new PointD(1180, 15), 30);
            displayText(c, $"COINS : {game.coinsCollected} / {game.totalCoins}",
                         new PointD(700, 15), 30);
            c.SetSourceRGB(0.1, 0.4, 0.6);
            c.Rectangle(0, 750, SCREEN_WIDTH, 50);
            c.Fill();
        }

        void drawCheckPoints(Context c)
        {
            foreach (var k in game.checkPoint)
            {
                if (k.beingAnimated)
                    k.animateCheckPoint();
                c.SetSourceRGBA(k.red, k.green, k.blue, k.opacity);
                c.Rectangle(k.topLeftPos.X, k.topLeftPos.Y, k.width, k.height);
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
            CairoHelper.SetSourcePixbuf(c, p, musicIcon.topLeftPos.X, musicIcon.topLeftPos.Y);
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

        void drawMenuIcon(Context c)
        {
            Pixbuf p = mouseOnMenuIcon ? greenMenu : menu;
            CairoHelper.SetSourcePixbuf(c, p, menuIcon.topLeftPos.X, menuIcon.topLeftPos.Y);
            c.Paint();
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
                drawMenuIcon(c);
                displayMenu(c);
                displayLevelMenu(c);
            }
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
