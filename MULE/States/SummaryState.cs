using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MULE.Sprites;

namespace MULE.States
{
    public class SummaryState : State
    {
        public enum LocalState
        {
            WalkIn,
            DisplayWait,
            Finished
        }

        public const int SCREENBUFFER = 15;
        public const int VERTICALBUFFER = 40;

        private LocalState state;
        private PlayerSprite[] sprites;
        private bool[] playerReady;

        public SummaryState(Game game) : base(game)
        {
            state = new LocalState();
            state = LocalState.WalkIn;

            sprites = new PlayerSprite[game.Players.Length];

            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i] = new PlayerSprite(game);
                sprites[i].X = game.Width + (Graphics.PLAYERWIDTH * 2) * i;
                sprites[i].Y = 
                    game.Height - Graphics.PLAYERHEIGHT - SCREENBUFFER - VERTICALBUFFER;
                sprites[i].Speed = 3.5;
                sprites[i].StartAnimation();
                sprites[i].ChangeDirection(SpriteDirection.W);
            }

            playerReady = new bool[sprites.Length];

            for (int i = 0; i < playerReady.Length; i++)
                playerReady[i] = game.Players[i].AI;

            game.Sound.PlaySound(Sounds.IntroMusic, 64);

            stateTimer.Change(500, 1);
        }

        public LocalState State
        {
            get { return state; }
        }

        public PlayerSprite[] Sprites
        {
            get { return sprites; }
        }

        public bool[] PlayerReady
        {
            get { return playerReady; }
        }

        protected override void tickEvent(object stateInfo)
        {
            if (state == LocalState.WalkIn)
            {
                bool allStopped = true;

                for (int i = 0; i < sprites.Length; i++)
                {
                    if ((i < 4 && sprites[i].X > SCREENBUFFER) ||
                        (i > 3 && sprites[i].X > game.Width / 2 + SCREENBUFFER))
                        sprites[i].MoveLeft();
                    else if (
                        sprites[i].Y >
                        (i % 4) *
                        ((game.Height - (SCREENBUFFER + VERTICALBUFFER) * 2) / 4) +
                        SCREENBUFFER + VERTICALBUFFER
                        )
                    {
                        if (sprites[i].Direction != SpriteDirection.N)
                            sprites[i].ChangeDirection(SpriteDirection.N);
                        sprites[i].MoveUp();
                    }
                    else
                    {
                        if (sprites[i].Moving)
                        {
                            sprites[i].StopAnimation();
                            sprites[i].ChangeDirection(SpriteDirection.S);
                        }
                    }

                    if (sprites[i].Moving) allStopped = false;
                }

                if (allStopped)
                {
                    state = LocalState.DisplayWait;
                }
            }
        }

        public override void DoAction(int player, PlayerAction action)
        {
            if (state == LocalState.DisplayWait &&
                action == PlayerAction.JoystickButton &&
                !playerReady[player])
            {
                playerReady[player] = true;

                game.Sound.PlayBeep(player);

                bool allReady = true;

                for (int i = 0; i < sprites.Length; i++)
                    if (!playerReady[i])
                    {
                        allReady = false;
                        break;
                    }

                if (allReady)
                {
                    game.Sound.StopSound(Sounds.IntroMusic);
                    state = LocalState.Finished;
                    stateComplete();
                }
            }
        }
    }
}
