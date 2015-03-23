using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SdlDotNet.Core;
using SdlDotNet.Input;

namespace MULE
{
    public enum PlayerAction
    {
        JoystickButton = 1,
        JoystickN = 2,
        JoystickNE = 3,
        JoystickE = 4,
        JoystickSE = 5,
        JoystickS = 6,
        JoystickSW = 7,
        JoystickW = 8,
        JoystickNW = 9,
        JoystickCenter = 10
    }

    public delegate void KeyPressHandler(object sender, KeyboardEventArgs e);

    public class Input
    {
        private struct KeyStatus
        {
            public bool Up;
            public bool Right;
            public bool Down;
            public bool Left;
            public bool Button;
            public bool Center;
        }

        private Game game;
        private Joystick[] joys;
        private KeyStatus keyStatus;

        public event KeyPressHandler KeyPress;

        public Input(Game game)
        {
            this.game = game;

            keyStatus = new KeyStatus();
            keyStatus.Up = false;
            keyStatus.Right = false;
            keyStatus.Down = false;
            keyStatus.Left = false;
            keyStatus.Button = false;
            keyStatus.Center = true;

            Joysticks.Initialize();

            Events.KeyboardDown += new EventHandler<KeyboardEventArgs>(KeyboardEvent);
            Events.KeyboardUp += new EventHandler<KeyboardEventArgs>(KeyboardEvent);
            Events.JoystickAxisMotion += new EventHandler<JoystickAxisEventArgs>(JoystickAxisMotion);
            Events.JoystickButtonDown += new EventHandler<JoystickButtonEventArgs>(JoystickButtonEvent);
            Events.JoystickButtonUp += new EventHandler<JoystickButtonEventArgs>(JoystickButtonEvent);
        }

        private void KeyboardEvent(object sender, KeyboardEventArgs e)
        {
            // temp //
            switch (e.Key)
            {
                case Key.LeftControl:
                    keyStatus.Button = e.Down;

                    if (e.Down)
                        game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickButton);

                    break;

                case Key.LeftArrow:
                    keyStatus.Left = e.Down;

                    if (e.Down)
                    {
                        if (keyStatus.Up)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickNW);
                        else if (keyStatus.Down)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickSW);
                        else
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickW);
                    }
                    else
                    {
                        if (keyStatus.Up)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickN);
                        else if (keyStatus.Down)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickS);
                    }

                    break;

                case Key.UpArrow:
                    keyStatus.Up = e.Down;

                    if (e.Down)
                    {
                        if (keyStatus.Left)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickNW);
                        else if (keyStatus.Right)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickNE);
                        else
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickN);
                    }
                    else
                    {
                        if (keyStatus.Left)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickW);
                        else if (keyStatus.Right)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickE);
                    }

                    break;

                case Key.RightArrow:
                    keyStatus.Right = e.Down;

                    if (e.Down)
                    {
                        if (keyStatus.Up)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickNE);
                        else if (keyStatus.Down)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickSE);
                        else
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickE);
                    }
                    else
                    {
                        if (keyStatus.Up)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickN);
                        else if (keyStatus.Down)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickS);
                    }

                    break;

                case Key.DownArrow:
                    keyStatus.Down = e.Down;

                    if (e.Down)
                    {
                        if (keyStatus.Left)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickSW);
                        else if (keyStatus.Right)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickSE);
                        else
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickS);
                    }
                    else
                    {
                        if (keyStatus.Left)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickW);
                        else if (keyStatus.Right)
                            game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickE);
                    }

                    break;
            }

            if (!keyStatus.Center &&
                !keyStatus.Up &&
                !keyStatus.Right &&
                !keyStatus.Down &&
                !keyStatus.Left)
            {
                keyStatus.Center = true;
                game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickCenter);
            }
            else if (
                keyStatus.Up ||
                keyStatus.Right ||
                keyStatus.Down ||
                keyStatus.Left)
                keyStatus.Center = false;

            // temp //

            if (!e.Down) KeyPress(this, e);
        }

        private void JoystickButtonEvent(object sender, JoystickButtonEventArgs e)
        {
            if (!e.ButtonPressed && e.Button == 0)
                game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickButton);
        }

        private void JoystickAxisMotion(object sender, JoystickAxisEventArgs e)
        {
            if (e.AxisValue == 0)
                game.State.DoAction(game.MyPlayerNum, PlayerAction.JoystickCenter);

        }
    }
}
