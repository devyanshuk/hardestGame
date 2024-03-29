using System;
using Cairo;
using System.Linq;
using System.Collections.Generic;
using static hardestgame.Movement;
public delegate void Notify();

namespace hardestgame
{
    public class Game
    {
        public event Notify gameStateChanged;

        public const int MAP_WIDTH = 23, MAP_HEIGHT = 15;
        const uint UPDATE_TIME = 20;

        public Player player;
        public Obstacle obs;
        public PointD checkPointPos = new PointD(0, 0);
        public int coinsCollected = 0, totalCoins = 0, level = 10, fails = 0;
        public List<Wall> walls;
        public List<CheckPoints> checkPoint;
        public List<PointD> coinPos = new List<PointD>();
        public bool pauseGame, safeZone, enemy_collision, roundWon;
        public char[,] bg;
        List<PointD> coinCollected = new List<PointD>();
        public List<PointD> obsList, hitPt;
        Parser parser = new Parser();

        public Game()
        {
            startTimer();
        }

        public void clear()
        {
            totalCoins = 0;
            coinCollected.Clear();
            checkPoint.Clear();
            coinPos.Clear();
            coinsCollected = 0;
            roundWon = false;
            checkPointPos = new PointD(0, 0);
        }

        public void init()
        {
            parser.init();
            walls = new List<Wall>();
            hitPt = new List<PointD>();
            obsList = new List<PointD>();
            checkPoint = new List<CheckPoints>();
            coinPos.AddRange(coinCollected);
            coinsCollected = totalCoins - coinPos.Count;
            coinCollected = new List<PointD>();

            if (roundWon && (coinsCollected == totalCoins && totalCoins != 0))
            {
                level++;
                checkPointPos = new PointD(0,0);
                coinPos = new List<PointD>();
                totalCoins = 0;
                coinsCollected = 0;
            }
            enemy_collision = false;
            roundWon = false;
            pauseGame = false;
            safeZone = false;
            bg = new char[MAP_HEIGHT, MAP_WIDTH];
            player = new Player(2.5);
            player.dirs = new bool[4];
            if (level > View.maxLevel)
                level = 1;
            parser.updateEnv($"./levels/{this.level}.txt", player, out List<CircleMovement> c,
                            out List<XyMovement> xy, out List<SquareMovement> sq, ref walls,
                            ref checkPoint, ref obsList, ref checkPointPos, ref totalCoins,
                            ref coinPos, ref bg, ref hitPt);
            obs = new Obstacle(obsList, this.level, hitPt, c, xy, sq);
        }

        void startTimer()
        {
            GLib.Timeout.Add(UPDATE_TIME, delegate
            {
                if (!enemy_collision)
                {
                    if (!View.displayAllLevels)
                    {
                        if (!safeZone)
                            enemyCollision();
                        obs.move(level);
                        wallCollision();
                        checkForCoins();
                        player.changePixPos();
                    }
                }
                insideSafeZone();
                if (enemy_collision || roundWon)
                    player.makePlayerDisappear();
                gameStateChanged?.Invoke();
                player.canNotMove = new bool[4];
                return true;

            });
        }


        public void checkForCoins()
        {
            foreach (PointD pos in coinPos)
            {
                if (collision(pos, (double) Obstacle.RADIUS, (double) Obstacle.RADIUS, 0))
                {
                    coinsCollected++;
                    coinPos.Remove(pos);
                    coinCollected.Add(pos);
                    break;
                }
            }
        }
        bool coll(double a, double pos, int r) => (a >= (pos - r) && a <= (pos + r));

        public void wallCollision()
        {
            PointD pPos = new PointD(player.pixPos.X + player.size.X / 2,
                                     player.pixPos.Y + player.size.Y / 2);
            double[] hitPoints = new double[4] { pPos.X - player.speed - player.size.X / 2,
                                                pPos.X + player.speed + player.size.X / 2,
                                                pPos.Y - player.speed - player.size.Y / 2,
                                                pPos.Y + player.speed + player.size.Y / 2 };

            bool[] canNotMove = new bool[4]; //left, right, up, down

            foreach (Wall wall in walls)
            {
                PointD wPos = new PointD(wall.topLeftPos.X + View.CELL_WIDTH / 2,
                                         wall.topLeftPos.Y + View.CELL_HEIGHT / 2);
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

        List<PointD> getHitPoints(PointD po, double radX, double radY, double a)
        {
            var l = new PointD(po.X - radX - a, po.Y);
            var r = new PointD(po.X + radX + a, po.Y);
            var d = new PointD(po.X, po.Y + radY + a);
            var u = new PointD(po.X, po.Y - radY - a);
            return new List<PointD> { l, r, u, d };
        }

        public bool collision(PointD po, double radX, double radY, double a)
            => getHitPoints(po, radX, radY, a).Any(i => player.collision(i));


        public void enemyCollision()
        {
            if (!roundWon)
            {
                foreach (PointD po in obs.pos)
                {
                    if (collision(po, (double)Obstacle.RADIUS - (double)Obstacle.RADIUS / 4
                                    , (double)Obstacle.RADIUS - (double)Obstacle.RADIUS / 4, 0))
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
            bool changed = false;
            if (!roundWon)
            {
                foreach (var c in checkPoint)
                {
                     if (c.collision(player.pixPos) &&
                        c.collision(new PointD(player.pixPos.X + player.size.X,
                                               player.pixPos.Y + player.size.Y)))
                     {
                        checkPointPos = c.topLeftPos;
                        safeZone = true;
                        changed = true;
                        if (coinCollected.Count >= 1)
                            c.beingAnimated = c.decrease = true;
                        coinCollected.Clear();
                        if (!enemy_collision && coinsCollected == totalCoins &&
                            totalCoins != 0)
                        {
                            roundWon = true;
                            break;
                        }
                    }
                }
                if (changed)
                    return;
                safeZone = false;
            }
        }
    }
}
