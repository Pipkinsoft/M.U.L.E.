using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;

namespace MULE.Sprites
{
    public class PlayerSprite : Sprite
    {        
        private bool small;
        private bool sound;
        private Game game;

        public PlayerSprite(Game game) : base()
        {
            small = false;
            sound = false;
            this.game = game;
            frequency = 250;
        }

        public bool Small
        {
            get { return small; }
            set { small = value; }
        }

        public bool Sound
        {
            get { return sound; }
            set { sound = value; }
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
                    firstFrame = (moving ? 4 : 3);
                    lastFrame = 5;
                    frame = firstFrame;
                    break;

                case SpriteDirection.NW:
                case SpriteDirection.W:
                case SpriteDirection.SW:
                    firstFrame = 6;
                    lastFrame = 7;
                    frame = firstFrame;
                    break;

                case SpriteDirection.NE:
                case SpriteDirection.E:
                case SpriteDirection.SE:
                    firstFrame = 8;
                    lastFrame = 9;
                    frame = firstFrame;
                    break;
            }
        }

        protected override void tickEvent(object stateInfo)
        {
            base.tickEvent(stateInfo);

            if (sound)
            {
                if (frame == firstFrame)
                    game.Sound.PlaySound(Sounds.Walk1);
                else
                    game.Sound.PlaySound(Sounds.Walk2);
            }
        }
    }
}
