using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace MULE.States
{
    public class TransportState : State
    {
        public enum LocalState
        {
            MoveLeft,
            MoveRight,
            Landing,
            Lifting,
            Leaving,
            Finished
        }

        private const int LANDWIDTH = 62;
        private const int LANDHEIGHT = 19;

        private LocalState state;
        private Rectangle shipRect;
        private bool playSound;

        public TransportState(
            Game game, 
            LocalState initialState = LocalState.MoveLeft
            ) : base(game)
        {
            state = initialState;

            if (state == LocalState.Lifting)
                shipRect =
                    new Rectangle(
                        game.MapWidth / 2 - LANDWIDTH / 2,
                        game.MapHeight / 2 - LANDHEIGHT / 2 - Graphics.MAPSTOREOFFSET,
                        LANDWIDTH,
                        LANDHEIGHT
                        );
            else
                shipRect = 
                    new Rectangle(
                        game.MapWidth,
                        game.MapHeight / 2 - Graphics.SHIPHEIGHT / 2 - Graphics.MAPSTOREOFFSET, 
                        Graphics.SHIPWIDTH, 
                        Graphics.SHIPHEIGHT
                        );

            playSound = true;

            stateTimer.Change(750, 1);
        }

        public LocalState State
        {
            get { return state; }
        }

        public Rectangle ShipRectangle
        {
            get { return shipRect; }
        }

        protected override void tickEvent(object stateInfo)
        {
            if (playSound)
            {
                playSound = false;
                game.Sound.PlaySound(Sounds.Ship);
            }

            switch (state)
            {
                case LocalState.MoveLeft:
                    shipRect.X -= 4;

                    if (shipRect.X <= game.MapWidth / 2 - Graphics.PLOTSIZE * 4 - 10)
                        state = LocalState.MoveRight;
                    break;

                case LocalState.MoveRight:
                    shipRect.X++;

                    if (shipRect.X >= game.MapWidth / 2 - Graphics.PLOTSIZE * 3)
                        state = LocalState.Landing;
                    break;

                case LocalState.Landing:
                    shipRect.X += 8;

                    if (shipRect.Height > LANDHEIGHT)
                    {
                        shipRect.Height -= 2;
                        shipRect.Width -= 4;
                        shipRect.Y++;
                    }

                    if (shipRect.X >= game.MapWidth / 2 - shipRect.Width / 2)
                    {
                        game.Sound.StopSound(Sounds.Ship);
                        stateTimer.Change(1000, Timeout.Infinite);
                        state = LocalState.Finished;
                    }
                    break;

                case LocalState.Lifting:
                    shipRect.X -= 8;

                    if (shipRect.Height < Graphics.SHIPHEIGHT)
                    {
                        shipRect.Height += 2;
                        shipRect.Width += 4;
                        shipRect.Y--;
                    }

                    if (shipRect.X <= game.MapWidth / 2 - Graphics.PLOTSIZE * 3)
                        state = LocalState.Leaving;
                    break;

                case LocalState.Leaving:
                    shipRect.X -= 2;

                    if (shipRect.X <= -Graphics.SHIPWIDTH)
                    {
                        game.Sound.StopSound(Sounds.Ship);
                        stateTimer.Change(1000, Timeout.Infinite);
                        state = LocalState.Finished;
                    }
                    break;

                case LocalState.Finished:
                    stateComplete();
                    break;
            }
        }

        public override void DoAction(int player, PlayerAction action)
        {
        }
    }
}
