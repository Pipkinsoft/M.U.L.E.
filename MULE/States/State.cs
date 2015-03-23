using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SdlDotNet.Input;

namespace MULE.States
{
    public delegate void StateCompleteHandler(object sender, EventArgs e);

    public abstract class State : IDisposable
    {
        protected Game game;
        protected Timer stateTimer;

        public event StateCompleteHandler StateComplete;

        public State(Game game)
        {
            this.game = game;
            stateTimer = 
                new Timer(tickEvent, this, Timeout.Infinite, Timeout.Infinite);
        }

        protected abstract void tickEvent(object stateInfo);

        public abstract void DoAction(int player, PlayerAction action);

        protected void stateComplete()
        {
            StateComplete(this, new EventArgs());
        }

        protected void stopTimer()
        {
            stateTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            stateTimer.Dispose();
        }
    }
}
