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
        public const int CELL_WIDTH = 56, CELL_HEIGHT = 56;
        public const int SCREEN_WIDTH = 1380, SCREEN_HEIGHT = 800;
        Gdk.Color BACKGROUND_COLOR = new Gdk.Color(0, 100, 128);
        public const int Y_MARGIN = 0, X_MARGIN = 0;

        public static List<Gdk.Key> DIRS = new List<Gdk.Key>
                                          { Gdk.Key.Left,
                                            Gdk.Key.Right,
                                            Gdk.Key.Up,
                                            Gdk.Key.Down
                                          };

        Rectangle musicIcon, menuIcon, leftArrowIcon, rightArrowIcon;

        Pixbuf dollar, obstacle, musicOn, musicOff, menu, greenMenu;
        Pixbuf left, right, leftGreen, rightGreen;
        //SoundPlayer music;
        bool playMusic = true;
        public bool mouseOnMenuIcon, mouseOnLeftArrowIcon, mouseOnRightArrowIcon;
        PointD mouseLocation;

        int currPage = 0;
        public static bool displayAllLevels;
        bool menuLogoSelected;
        Parser parser = new Parser();
        char[,] bg;
        List<Rectangle> allLevels;

        Game game;

        public View() : base("World's Hardest game")
        {

            musicIcon = new Rectangle {
                topLeftPos = new PointD(1300, 750),
                width = 48,
                height = 48
            };

            menuIcon = new Rectangle
            {
                topLeftPos = new PointD(20, 750),
                width = 50,
                height = 50
            };

            leftArrowIcon = new Rectangle
            {
                topLeftPos = new PointD(345, 460),
                width = 50,
                height = 50
            };

            rightArrowIcon = new Rectangle()
            {
                topLeftPos = new PointD(860, 460),
                width = 50,
                height = 50
            };

            dollar = new Pixbuf("./sprites/dollar.png");
            greenMenu = new Pixbuf("./sprites/menu_green.png").ScaleSimple((int)menuIcon.width,
                             (int)menuIcon.height, InterpType.Bilinear);
            obstacle = new Pixbuf("./sprites/obs.png");
            musicOn = new Pixbuf("./music/music_on.png").ScaleSimple((int)musicIcon.width,
                                   (int)musicIcon.height, InterpType.Bilinear);
            musicOff = new Pixbuf("./music/music_off.png");
            menu = new Pixbuf("./sprites/menu.png").ScaleSimple((int)menuIcon.width,
                             (int)menuIcon.height, InterpType.Bilinear);
            left = new Pixbuf("./sprites/arrow_left_pink.png").ScaleSimple((int)leftArrowIcon.width,
                             (int)leftArrowIcon.height, InterpType.Bilinear);
            right = new Pixbuf("./sprites/arrow_right_pink.png").ScaleSimple((int)rightArrowIcon.width,
                             (int)rightArrowIcon.height, InterpType.Bilinear);
            leftGreen = new Pixbuf("./sprites/arrow_left_green.png").ScaleSimple((int)leftArrowIcon.width,
                             (int)leftArrowIcon.height, InterpType.Bilinear);
            rightGreen = new Pixbuf("./sprites/arrow_right_green.png").ScaleSimple((int)rightArrowIcon.width,
                             (int)rightArrowIcon.height, InterpType.Bilinear);

            mouseOnMenuIcon = false;
            //music.SoundLocation = "./music/ffmusic.wav";
            allLevels = new List<Rectangle>();
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
            mouseOnLeftArrowIcon = false;
            mouseOnRightArrowIcon = false;
            //music = new SoundPlayer();
            game.init();
            //music.Load();
            //music.PlayLooping();
            game.gameStateChanged += QueueDraw;
            game.player.opacityChanged += init;
        }

        void addAllLevels()
        {
            int level = 1;
            int x = 400, y = 170;
            int w = 460, h = 284;
            while (true)
            {
                try
                {
                    int k = (level % 2 == 1) ? y : y + h;
                    ImageSurface s = new ImageSurface(Format.Argb32, SCREEN_WIDTH, SCREEN_HEIGHT);
                    bg = new char[Game.MAP_HEIGHT, Game.MAP_WIDTH];
                    parser.updateEnv($"./levels/{level}.txt", ref bg);
                    using (Context c = new Context(s))
                    {
                        drawMap(c, x, k, 20, 20, bg, true);
                        displayText(c, $"{level}.", new PointD(x + 5, k + 5), 30, true);
                    }
                    level++;
                    Rectangle r = new Rectangle
                    {
                        image = s,
                        topLeftPos = new PointD(x, k),
                        width = w,
                        height = h,
                        opacity = 1,
                        increase = false,
                        decrease = false
                    };
                    allLevels.Add(r);
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
            menuLogoSelected  = displayAllLevels = !DIRS.Contains(evnt.Key);
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
            mouseOnLeftArrowIcon = leftArrowIcon.collision(mouseLocation);
            mouseOnRightArrowIcon = rightArrowIcon.collision(mouseLocation);
            int x = currPage;
            for (int i = x; i < x + 2; i++)
            {
                if (i <= allLevels.Count - 1)
                {
                    bool a = allLevels[i].collision(mouseLocation);
                    if (a && !allLevels[i].increase && !allLevels[i].decrease)
                        allLevels[i].increase = true;
                    else if (!a)
                        allLevels[i].increase = allLevels[i].decrease = false;
                }
            }
            return true;
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            double a = mouseLocation.X, b = mouseLocation.Y;
            PointD p = new PointD(a, b);

            if (menuLogoSelected) {

                if (leftArrowIcon.collision(p))
                {
                    currPage-=2;
                    if (currPage < 0)
                        currPage = 0;
                }

                else if (rightArrowIcon.collision(p))
                {
                    currPage+=2;
                    if (currPage > allLevels.Count - 1)
                        currPage = allLevels.Count - 1;
                }
            }

            if (musicIcon.collision(p))
                playMusic = !playMusic;

            if (menuIcon.collision(p))
            {
                menuLogoSelected = !menuLogoSelected;
                displayAllLevels = !displayAllLevels;
            }
            if (displayAllLevels)
            {
                int x = currPage;
                for (int i = x; i < x + 2; i++)
                {
                    if (i <= allLevels.Count-1 && allLevels[i].collision(p))
                    {
                        game.level = i + 1;
                        game.clear();
                        init();
                    }
                }
            }
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

        void drawMap(Context c, int startX, int startY, int x, int y, char[,] bg, bool allLevels)
        {
            for (int i = 0; i < Game.MAP_HEIGHT; i++)
            {
                for (int j = 0; j < Game.MAP_WIDTH; j++)
                {
                    PointD currPos = new PointD(X_MARGIN + startX + x * j,
                                                Y_MARGIN + startY + y * i);
                    if (bg[i, j] == '1')
                    {
                        c.MoveTo(currPos);
                        if (!allLevels)
                        {
                            if (j % 2 == i % 2) c.SetSourceRGB(1.0, 1.0, 1.0);
                            else c.SetSourceRGB(0.6, 0.9, 0.9);
                        }
                        else
                        {
                            if (j % 2 == i % 2) c.SetSourceRGBA(0.3, 0, .3, 0.4);
                            else c.SetSourceRGBA(0, 0.2, 0.1, 0.5);
                        }
                        c.Rectangle(currPos.X, currPos.Y, x, y);
                        c.Fill();
                    }
                }
            }
        }

        void displayText(Context c, string st, PointD p, int fontSize, bool allLev = false)
        {
            c.SetFontSize(fontSize);
            if (!allLev)
                c.SetSourceRGB(1, 1, 1);
            else
                c.SetSourceRGBA(1, 0, 0, 5);
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
            if (menuLogoSelected)
            {
                int x = currPage;
                for (int i = x; i < x + 2; i++)
                {
                    if (i <= allLevels.Count - 1)
                    {
                        c.SetSourceSurface(allLevels[i].image, 0, 0);
                        c.Paint();
                    }
                }
                if (currPage != 0)
                {
                    Pixbuf l = mouseOnLeftArrowIcon ? leftGreen : left;
                    drawIcon(c, l, leftArrowIcon.topLeftPos.X, leftArrowIcon.topLeftPos.Y);
                }
                if (currPage != allLevels.Count - 1)
                {
                    Pixbuf r = mouseOnRightArrowIcon ? rightGreen : right;
                    drawIcon(c, r, rightArrowIcon.topLeftPos.X, rightArrowIcon.topLeftPos.Y);
                }
            }
        }

        void drawIcon(Context c, Pixbuf p, double x, double y)
        {
            CairoHelper.SetSourcePixbuf(c, p, x, y);
            c.Paint();
        }

        void drawMenuIcon(Context c)
        {
            Pixbuf p = mouseOnMenuIcon ? greenMenu : menu;
            drawIcon(c, p, menuIcon.topLeftPos.X, menuIcon.topLeftPos.Y);
        }

        void animateLevelMenu(Context c)
        {
            int x = currPage;
            for (int i = x; i < x + 2; i++)
            {
                if (i <= allLevels.Count - 1)
                {
                    var k = allLevels[i];
                    if (k.decrease || k.increase)
                    {
                        c.SetSourceRGBA(1, 0.4, 1, k.opacity);
                        c.Rectangle(k.topLeftPos, k.width, k.height);
                        c.Stroke();
                        if (k.decrease)
                        {
                            k.opacity -= 0.1;
                            if (k.opacity <= 0.0)
                            {
                                k.decrease = false;
                                k.increase = true;
                            }
                        }
                        else if (k.increase)
                        {
                            k.opacity += 0.1;
                            if (k.opacity >= 1.0)
                            {
                                k.increase = false;
                                k.decrease = true;
                            }
                        }

                    }
                }
            }
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            using (Context c = CairoHelper.Create(GdkWindow))
            {
                drawMap(c, 0, 0, CELL_WIDTH, CELL_HEIGHT, game.bg, false);
                drawCheckPoints(c);
                drawSprite(c, game.coinPos, dollar);
                updateScoreAndLives(c);
                drawSprite(c, game.obs.pos, obstacle);
                drawPlayer(c);
                if (displayAllLevels)
                {
                    displayLevelMenu(c);
                    animateLevelMenu(c);
                }
                drawMusicIcon(c);
                drawMenuIcon(c);
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
