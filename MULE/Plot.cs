using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULE
{
    public enum PlotType
    {
        Plain = 1,
        River = 2,
        Mountains = 3,
        Store = 4
    }

    public enum MountainType
    {
        Type1 = 1,
        Type2 = 2,
        Type3 = 3,
        Type4 = 4
    }

    public enum PlotResource
    {
        None = 0,
        Food = 1,
        Energy = 2,
        Smithore = 3,
        Crystite = 4
    }

    public enum CrystiteLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        VeryHigh = 4
    }

    public struct Mountain
    {
        public int X;
        public int Y;
        public MountainType Type;
    }

    public class Plot
    {

        private Game game;
        private PlotType type;
        private PlotResource resource;
        private CrystiteLevel crystite;
        private int goods;
        private Mountain[] mountains;
        private int player;

        public Plot(Game game, PlotType type, CrystiteLevel crystite)
        {
            this.game = game;
            this.type = type;
            this.crystite = crystite;
            mountains = new Mountain[0];
            resource = PlotResource.None;
            player = -1;
            goods = 0;

            if (type == PlotType.Mountains)
                generateMountains();
        }

        private void generateMountains()
        {
            int numMountains = game.Random.Next(3) + 1;

            mountains = new Mountain[numMountains];

            for (int i = 0; i < numMountains; i++)
            {
                mountains[i] = new Mountain();
                mountains[i].Type =
                    (MountainType)(game.Random.Next(4) + 1);

                int y = 0;

                while (true)
                {
                    y =
                        game.Random.Next(
                            Graphics.PLOTSIZE - Graphics.MOUNTAINBUFFER * 2 - Graphics.MOUNTAINHEIGHT
                            ) +
                        Graphics.MOUNTAINBUFFER;

                    bool conflict = false;

                    for (int j = 0; j < i; j++)
                    {
                        if (y > mountains[j].Y - Graphics.MOUNTAINBUFFER - Graphics.MOUNTAINHEIGHT &&
                            y < mountains[j].Y + Graphics.MOUNTAINHEIGHT + Graphics.MOUNTAINBUFFER)
                        {
                            conflict = true;
                            break;
                        }
                    }

                    if (!conflict) break;
                }

                mountains[i].Y = y;
                mountains[i].X = game.Random.Next(Graphics.PLOTSIZE - Graphics.MOUNTAINWIDTH);
            }
        }

        public PlotType Type
        {
            get { return type; }
        }

        public PlotResource Resource
        {
            get { return resource; }
            set { resource = value; }
        }

        public CrystiteLevel Crystite
        {
            get { return crystite; }
            set { crystite = value; }
        }

        public int Goods
        {
            get { return goods; }
        }

        public Mountain[] Mountains
        {
            get { return mountains; }
        }

        public int Player
        {
            get { return player; }
            set { player = value; }
        }
    }
}
