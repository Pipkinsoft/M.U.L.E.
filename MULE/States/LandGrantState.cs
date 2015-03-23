using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using SdlDotNet.Input;

namespace MULE.States
{
    public class LandGrantState : State
    {
        public enum LocalState
        {
            Starting,
            Normal,
            GainedPlot,
            Finished
        }

        private LocalState state;
        private Point cursorPos;
        private bool[] playerActed;

        public LandGrantState(Game game) : base(game)
        {
            cursorPos = new Point(-1, 0);
            state = LocalState.Starting;

            playerActed = new bool[8];
            for (int i = 0; i < 8; i++) playerActed[i] = false;

            stateTimer.Change(1000, 500);
        }

        public LocalState State
        {
            get { return state; }
        }

        public Point CursorPos
        {
            get { return cursorPos; }
            set { cursorPos = value; }
        }

        protected override void tickEvent(object stateInfo)
        {
            if (state == LocalState.Finished)
            {
                stateComplete();
                return;
            }
            else if (state != LocalState.Normal)
                state = LocalState.Normal;

            bool allPlayersActed = true;

            for (int i = 0; i < game.Players.Length; i++)
                if (!playerActed[i])
                {
                    allPlayersActed = false;
                    break;
                }

            if (allPlayersActed)
            {
                state = LocalState.Finished;
                game.Sound.PlaySound(Sounds.Auction);

                stateTimer.Change(2000, Timeout.Infinite);
                return;
            }

            do
            {
                cursorPos.X++;

                if (cursorPos.X == game.Map.Width)
                {
                    cursorPos.X = 0;
                    cursorPos.Y++;
                }
                else if (
                    cursorPos.X == game.Map.Width / 2 &&
                    cursorPos.Y == game.Map.Height / 2)
                {
                    cursorPos.X++;
                }

                if (cursorPos.Y == game.Map.Height)
                {
                    state = LocalState.Finished;
                    game.Sound.PlaySound(Sounds.Auction);

                    stateTimer.Change(2000, Timeout.Infinite);
                    break;
                }
            }
            while (game.Map.Plots[cursorPos.X, cursorPos.Y].Player > -1);
        }

        public override void DoAction(int player, PlayerAction action)
        {
            if (state == LocalState.Normal && 
                action == PlayerAction.JoystickButton &&
                !playerActed[player])
            {
                game.Map.Plots[cursorPos.X, cursorPos.Y].Player = player;
                playerActed[player] = true;

                game.Sound.PlayBeep(player);

                state = LocalState.GainedPlot;
                stateTimer.Change(1000, 500);
            }
        }
    }
}
