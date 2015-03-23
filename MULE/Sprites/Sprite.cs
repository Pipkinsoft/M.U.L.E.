using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MULE.Sprites
{
    public enum SpriteDirection
    {
        N = 1,
        NE = 2,
        E = 3,
        SE = 4,
        S = 5,
        SW = 6,
        W = 7,
        NW = 8
    }

    public abstract class Sprite : IDisposable
    {
        protected double x;
        protected double y;
        protected double scale;
        protected SpriteDirection direction;
        protected int firstFrame;
        protected int lastFrame;
        protected int frame;
        protected bool moving;
        protected double speed;
        protected int frequency;

        private Timer animationTimer;

        public Sprite()
        {
            x = 0d;
            y = 0d;
            speed = 3d;
            scale = 1d;
            direction = SpriteDirection.S;
            frame = 0;
            frequency = 10;
            lastFrame = 0;
            moving = false;

            animationTimer =
                new Timer(tickEvent, this, Timeout.Infinite, Timeout.Infinite);
        }

        public SpriteDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public double Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public int X
        {
            get { return (int)x; }
            set { x = value; }
        }

        public int Y
        {
            get { return (int)y; }
            set { y = value; }
        }

        public virtual void MoveUp()
        {
            y -= speed;
        }

        public virtual void MoveRight()
        {
            x += speed;
        }

        public virtual void MoveDown()
        {
            y += speed;
        }

        public virtual void MoveLeft()
        {
            x -= speed;
        }

        public double Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public int Frequency
        {
            get { return frequency; }
            set
            {
                frequency = value;
                if (moving)
                    animationTimer.Change(0, frequency);
            }
        }

        public int FirstFrame
        {
            get { return firstFrame; }
            set { firstFrame = value; }
        }

        public int LastFrame
        {
            get { return lastFrame; }
            set { lastFrame = value; }
        }

        public int Frame
        {
            get { return frame; }
            set { frame = value; }
        }

        public bool Moving
        {
            get { return moving; }
            set { moving = value; }
        }

        public void StartAnimation()
        {
            if (moving) return;

            animationTimer.Change(0, frequency);
            moving = true;
        }

        public void StopAnimation()
        {
            if (!moving) return;

            animationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            moving = false;
            frame = firstFrame;
        }

        public virtual void ChangeDirection(SpriteDirection direction)
        {
            this.direction = direction;
        }

        protected virtual void tickEvent(object stateInfo)
        {
            frame++;
            if (frame > lastFrame) frame = firstFrame;
        }

        public static SpriteDirection GetDirection(int x, int y)
        {
            if (x == 1 && y == 1)
                return SpriteDirection.NE;
            else if (x == 1 && y == -1)
                return SpriteDirection.SE;
            else if (x == -1 && y == -1)
                return SpriteDirection.SW;
            else if (x == -1 && y == 1)
                return SpriteDirection.NW;
            else if (x == 1)
                return SpriteDirection.E;
            else if (x == -1)
                return SpriteDirection.W;
            else if (y == 1)
                return SpriteDirection.S;
            else if (y == -1)
                return SpriteDirection.N;
            else
                return SpriteDirection.S;
        }

        public void Dispose()
        {
            animationTimer.Dispose();
        }
    }
}
