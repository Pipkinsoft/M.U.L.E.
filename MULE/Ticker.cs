using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MULE
{
    public delegate void TimeUpHandler(object sender, EventArgs e);

    public class Ticker
    {
        public const double MAXVALUE = 100d;

        private const double INITX = 0d;
        private const double INCREMENT = 0.01d;
        private const int FREQUENCY = 50;
        private const double SLOWCHANGE = 0.05;

        private double value;
        private double x;
        private bool slow;
        private int lastYInt;
        private int tickInc;
        private bool tickSound;
        private Timer tickTimer;
        private Game game;

        public event TimeUpHandler TimeUp;

        public Ticker(Game game, double value)
        {
            this.game = game;
            this.value = value;
            slow = false;

            getXFromValue();

            lastYInt = (int)value;
            tickInc = 0;
            tickSound = true;

            tickTimer =
                new Timer(tickEvent, this, Timeout.Infinite, Timeout.Infinite);
        }

        private void getXFromValue()
        {
            // Retrieve x from y value: x = sqrt(max - y)
            x = Math.Sqrt(MAXVALUE - value);
        }

        private void tickEvent(object stateInfo)
        {
            if (slow)
                value = value - SLOWCHANGE;
            else
            {
                // Function for timer value: y = max - x^2
                value = MAXVALUE - Math.Pow(x, 2d);
                x += INCREMENT;
            }

            if (value <= 0)
            {
                tickTimer.Change(Timeout.Infinite, Timeout.Infinite);
                value = 0;
                TimeUp(this, new EventArgs());
            }
            
            if (value < MAXVALUE / 4 && (int)value < lastYInt)
            {
                tickInc++;

                if (tickInc == 2)
                {
                    tickInc = 0;

                    if (tickSound)
                        game.Sound.PlaySound(Sounds.Tick);
                    else
                        game.Sound.PlaySound(Sounds.Tock);
                    
                    tickSound = !tickSound;
                }
            }

            lastYInt = (int)value;
        }

        public void Start()
        {
            tickTimer.Change(FREQUENCY, FREQUENCY);
        }

        public void Stop()
        {
            tickTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Slow()
        {
            slow = true;
        }

        public void Normal()
        {
            getXFromValue();
            slow = false;
        }

        public double Value
        {
            get { return value; }
        }
    }
}
