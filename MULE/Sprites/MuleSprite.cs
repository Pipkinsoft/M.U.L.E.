using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;

namespace MULE.Sprites
{
    public class MuleSprite : Sprite
    {
        private bool small;
        private PlotResource resource;
        private Game game;

        public MuleSprite(Game game) : base()
        {
            small = false;
            resource = PlotResource.None;
            this.game = game;
            frequency = 250;
        }

        public bool Small
        {
            get { return small; }
            set { small = value; }
        }

        public PlotResource Resource
        {
            get { return resource; }
            set { resource = value; }
        }

        public bool Outfitted
        {
            get { return resource != PlotResource.None; }
        }

        public override void ChangeDirection(SpriteDirection direction)
        {
            base.ChangeDirection(direction);

            switch (direction)
            {
                case SpriteDirection.S:
                    firstFrame = (moving ? 1 : 0);
                    lastFrame = 2;
                    frame = firstFrame;
                    break;

                case SpriteDirection.N:
                    firstFrame = 3;
                    lastFrame = 4;
                    frame = firstFrame;
                    break;

                case SpriteDirection.NW:
                case SpriteDirection.W:
                case SpriteDirection.SW:
                    firstFrame = 5;
                    lastFrame = 6;
                    frame = firstFrame;
                    break;

                case SpriteDirection.NE:
                case SpriteDirection.E:
                case SpriteDirection.SE:
                    firstFrame = 7;
                    lastFrame = 8;
                    frame = firstFrame;
                    break;
            }
        }
    }
}
