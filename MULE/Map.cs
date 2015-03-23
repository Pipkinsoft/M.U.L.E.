using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MULE
{
    public class Map
    {
        private Game game;
        private Plot[,] plots;
        private int width;
        private int height;

        public Map(Game game, int width, int height)
        {
            this.game = game;
            this.width = width;
            this.height = height;

            plots = new Plot[width, height];

            generate();
        }

        private void generate()
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    if (x == width / 2 && y == height / 2)
                        plots[x, y] =
                            new Plot(game, PlotType.Store, CrystiteLevel.None);
                    else if (x == width / 2)
                        plots[x, y] =
                            new Plot(game, PlotType.River, CrystiteLevel.None);
                    else
                    {
                        if (game.Random.Next(4) == 0)
                            plots[x, y] =
                                new Plot(game, PlotType.Mountains, CrystiteLevel.None);
                        else
                            plots[x, y] =
                                new Plot(game, PlotType.Plain, CrystiteLevel.None);
                    }
                }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public Plot[,] Plots
        {
            get { return plots; }
            set { plots = value; }
        }

        public List<Plot> GetPlots(int player)
        {
            List<Plot> ret = new List<Plot>();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (plots[x, y].Player == player)
                        ret.Add(plots[x, y]);

            return ret;
        }

        public bool CheckTerrain(Rectangle rect)
        {
            if (rect.X < game.MapWidth / 2 + Graphics.PLOTSIZE / 4 &&
                rect.X > game.MapWidth / 2 - Graphics.PLOTSIZE / 4 - Graphics.SMALLPLAYERWIDTH)
                return true;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    for (int m = 0; m < plots[x, y].Mountains.Length; m++)
                    {
                        Mountain mnt = plots[x, y].Mountains[m];

                        if (
                            rect.X < x * Graphics.PLOTSIZE + mnt.X + Graphics.MOUNTAINWIDTH &&
                            rect.X > x * Graphics.PLOTSIZE + mnt.X - rect.Width &&
                            rect.Y < y * Graphics.PLOTSIZE + mnt.Y + Graphics.MOUNTAINHEIGHT &&
                            rect.Y > y * Graphics.PLOTSIZE + mnt.Y - rect.Height
                            )
                            return true;
                    }

            return false;
        }

        public Plot CheckHouse(int playerNum, Rectangle rect)
        {
            int xPlot = rect.X / Graphics.PLOTSIZE;
            int yPlot = rect.Y / Graphics.PLOTSIZE;
            int xOffest = rect.X % Graphics.PLOTSIZE;
            int yOffest = rect.Y % Graphics.PLOTSIZE;

            if (plots[xPlot, yPlot].Player != playerNum) return null;

            if (xOffest < Graphics.HOUSEXOFFSET + Graphics.HOUSEWIDTH &&
                xOffest > Graphics.HOUSEXOFFSET - Graphics.SMALLPLAYERWIDTH &&
                yOffest < Graphics.HOUSEYOFFSET + Graphics.HOUSEHEIGHT &&
                yOffest > Graphics.HOUSEYOFFSET - Graphics.SMALLPLAYERHEIGHT)
                return plots[xPlot, yPlot];

            return null;
        }
    }
}
