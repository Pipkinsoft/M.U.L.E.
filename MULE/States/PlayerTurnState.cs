using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using SdlDotNet.Input;
using MULE.Sprites;

namespace MULE.States
{
    public class PlayerTurnState : State
    {
        public enum LocalState
        {
            WaitForButton,
            ZoomInStore,
            ZoomOutStore,
            InStore,
            Outside,
            Pub,
            TimeUp
        }

        private enum StoreTransLoc
        {
            Middle,
            Left,
            Right
        }

        private LocalState state;
        private int playerNum;
        private PlayerSprite playerSprite;
        private MuleSprite muleSprite;
        private Ticker ticker;
        private double storeScale;
        private double smallScale;
        private bool deptBottomActed;
        private bool deptTopActed;
        
        private bool hasMule;
        private SpriteDirection lastMuleDir;

        private int playerIncX;
        private int playerIncY;

        private StoreTransLoc storeTransLoc;

        public PlayerTurnState(Game game, int playerNum) : base(game)
        {
            this.playerNum = playerNum;

            state = LocalState.WaitForButton;
            
            playerSprite = new PlayerSprite(game);

            playerSprite.Sound = true;
            playerSprite.Direction = SpriteDirection.S;
            PlayerSprite.Small = true;
            playerSprite.X =
                game.MapWidth / 2 - 
                Graphics.SMALLPLAYERWIDTH / 2;
            playerSprite.Y = 
                game.MapHeight / 2 - 
                Graphics.SMALLPLAYERHEIGHT / 2 - 
                Graphics.MAPSTOREOFFSET;

            muleSprite = new MuleSprite(game);
            lastMuleDir = SpriteDirection.S;
            
            game.Players[playerNum].Highlight = false;

            smallScale = 100 / Graphics.STOREWIDTH * game.Graphics.MapScale;
            storeScale = smallScale;
            deptBottomActed = false;
            deptTopActed = false;

            storeTransLoc = StoreTransLoc.Middle;

            ticker = new Ticker(game, 100d);
            ticker.TimeUp += new TimeUpHandler(ticker_TimeUp);

            stateTimer.Change(750, 750);
        }

        private void ticker_TimeUp(object sender, EventArgs e)
        {
            playerSprite.StopAnimation();
            state = LocalState.TimeUp;
            ticker.Stop();
            stopTimer();
            game.Sound.PlaySound(Sounds.Auction);
        }

        public LocalState State
        {
            get { return state; }
        }

        public int PlayerNum
        {
            get { return playerNum; }
        }

        public PlayerSprite PlayerSprite
        {
            get { return playerSprite; }
        }

        public MuleSprite MuleSprite
        {
            get { return muleSprite; }
        }

        public bool HasMule
        {
            get { return hasMule; }
        }

        public Ticker Ticker
        {
            get { return ticker; }
        }

        public double StoreScale
        {
            get { return storeScale; }
        }

        protected override void tickEvent(object stateInfo)
        {
            switch (state)
            {
                case LocalState.WaitForButton:
                    game.Players[playerNum].Highlight = 
                        !game.Players[playerNum].Highlight;
                    break;

                case LocalState.ZoomInStore:
                    storeScale += 0.1d;

                    if (storeScale >= 1d)
                    {
                        storeScale = 1d;
                        
                        playerSprite.Small = false;
                        playerSprite.Speed = 3.5;
                        playerSprite.Frequency = 250;
                        playerIncX = 0;
                        playerIncY = 0;

                        muleSprite.Small = false;
                        muleSprite.Speed = 3.5;
                        muleSprite.Frequency = 250;

                        if (storeTransLoc == StoreTransLoc.Middle)
                            playerSprite.X = 
                                Graphics.STOREWIDTH / 2 -
                                Graphics.PLAYERWIDTH / 2;
                        else if (storeTransLoc == StoreTransLoc.Left)
                            playerSprite.X = Graphics.STOREBUFFER;
                        else if (storeTransLoc == StoreTransLoc.Right)
                            playerSprite.X =
                                Graphics.STOREWIDTH -
                                Graphics.STOREBUFFER -
                                Graphics.PLAYERWIDTH;

                        playerSprite.Y =
                            Graphics.STOREBUFFER +
                            Graphics.DEPTTOPHEIGHT +
                            15;

                        muleSprite.X =
                            playerSprite.X + Graphics.PLAYERWIDTH / 2 -
                            Graphics.MULEWIDTH / 2;
                        muleSprite.Y =
                            playerSprite.Y + Graphics.PLAYERHEIGHT / 2 -
                            Graphics.MULEHEIGHT / 2;

                        state = LocalState.InStore;
                        stateTimer.Change(1, 1);
                        ticker.Start();
                    }
                    
                    break;

                case LocalState.ZoomOutStore:
                    storeScale -= 0.1d;

                    if (storeScale <= smallScale)
                    {
                        storeScale = smallScale;

                        playerSprite.Small = true;
                        playerSprite.Speed = 1.5;
                        playerSprite.Frequency = 250;
                        playerIncX = 0;
                        playerIncY = 0;

                        muleSprite.Small = true;
                        muleSprite.Speed = 1.5;
                        muleSprite.Frequency = 250;

                        if (storeTransLoc == StoreTransLoc.Right)
                            playerSprite.X =
                                game.MapWidth / 2 + 
                                Graphics.PLOTSIZE / 2 + 5;
                        else if (storeTransLoc == StoreTransLoc.Left)
                            playerSprite.X =
                                game.MapWidth / 2 - 
                                Graphics.PLOTSIZE / 2 - 
                                Graphics.SMALLPLAYERWIDTH - 5;

                        playerSprite.Y =
                            game.MapHeight / 2 - Graphics.SMALLPLAYERHEIGHT / 2;

                        muleSprite.X =
                            playerSprite.X + Graphics.SMALLPLAYERWIDTH / 2 -
                            Graphics.SMALLMULEWIDTH / 2;
                        muleSprite.Y =
                            playerSprite.Y + Graphics.SMALLPLAYERHEIGHT / 2 -
                            Graphics.SMALLMULEHEIGHT / 2;
                        
                        state = LocalState.Outside;
                        stateTimer.Change(1, 1);
                        ticker.Start();
                    }

                    break;

                case LocalState.InStore:
                    if (playerSprite.Y >= Graphics.STOREBUFFER + Graphics.DEPTTOPHEIGHT &&
                        playerSprite.Y < Graphics.STOREHEIGHT - Graphics.STOREBUFFER -
                        Graphics.DEPTBOTTOMHEIGHT - Graphics.PLAYERHEIGHT)
                    {
                        if (playerIncX > 0)
                        {
                            if (playerSprite.X + playerSprite.Speed + Graphics.PLAYERWIDTH >
                                Graphics.STOREWIDTH)
                            {
                                playerSprite.StopAnimation();
                                playerSprite.ChangeDirection(SpriteDirection.S);
                                muleSprite.StopAnimation();
                                muleSprite.ChangeDirection(SpriteDirection.S);
                                storeTransLoc = StoreTransLoc.Right;
                                state = LocalState.ZoomOutStore;
                                stateTimer.Change(10, 10);
                                ticker.Stop();
                                return;
                            }
                            else
                                playerSprite.MoveRight();
                        }
                        else if (playerIncX < 0)
                        {
                            if (playerSprite.X - playerSprite.Speed < 0)
                            {
                                playerSprite.StopAnimation();
                                playerSprite.ChangeDirection(SpriteDirection.S);
                                muleSprite.StopAnimation();
                                muleSprite.ChangeDirection(SpriteDirection.S);
                                storeTransLoc = StoreTransLoc.Left;
                                state = LocalState.ZoomOutStore;
                                stateTimer.Change(10, 10);
                                ticker.Stop();
                                return;
                            }
                            else
                                playerSprite.MoveLeft();
                        }
                    }

                    if (playerIncY < 0)
                    {
                        deptBottomActed = false;

                        if (playerSprite.Y <
                            Graphics.STOREBUFFER + Graphics.DEPTTOPHEIGHT + playerSprite.Speed)
                        {
                            int xOffset = 
                                (playerSprite.X - Graphics.STOREBUFFER) % 
                                (Graphics.DEPTWIDTH + Graphics.DEPTSPACING);

                            if (xOffset >
                                Graphics.DEPTWIDTH / 5 &&
                                xOffset <
                                Graphics.DEPTWIDTH -
                                Graphics.DEPTWIDTH / 5 -
                                Graphics.PLAYERWIDTH)
                            {
                                if (playerSprite.Y <
                                    Graphics.STOREBUFFER +
                                    Graphics.DEPTTOPHEIGHT / 4)
                                {
                                    if (!deptTopActed)
                                    {
                                        int store =
                                            (playerSprite.X - Graphics.STOREBUFFER) /
                                            (Graphics.DEPTWIDTH + Graphics.DEPTSPACING);

                                        // do store

                                        deptTopActed = true;
                                    }
                                }
                                else
                                    playerSprite.MoveUp();
                            }
                            else if (xOffset <= Graphics.DEPTWIDTH / 5 &&
                                xOffset > 0)
                                playerSprite.MoveRight();
                            else if (xOffset >=  
                                Graphics.DEPTWIDTH -
                                Graphics.DEPTWIDTH / 5 -
                                Graphics.PLAYERWIDTH &&
                                xOffset <
                                Graphics.DEPTWIDTH -
                                Graphics.PLAYERWIDTH)
                                playerSprite.MoveLeft();
                        }
                        else
                            playerSprite.MoveUp();
                    }
                    else if (playerIncY > 0)
                    {
                        deptTopActed = false;

                        if (playerSprite.Y >
                            Graphics.STOREHEIGHT -
                            Graphics.STOREBUFFER -
                            Graphics.DEPTBOTTOMHEIGHT -
                            Graphics.PLAYERHEIGHT -
                            playerSprite.Speed
                            )
                        {
                            if (playerSprite.X <
                                Graphics.STOREBUFFER +
                                3 * (Graphics.DEPTWIDTH + Graphics.DEPTTIGHTSPACING))
                            {
                                int xOffset =
                                    (playerSprite.X - Graphics.STOREBUFFER) %
                                    (Graphics.DEPTWIDTH + Graphics.DEPTTIGHTSPACING);

                                if (xOffset >
                                    Graphics.DEPTWIDTH / 5 &&
                                    xOffset <
                                    Graphics.DEPTWIDTH -
                                    Graphics.DEPTWIDTH / 5 -
                                    Graphics.PLAYERWIDTH)
                                {
                                    if (playerSprite.Y >
                                        Graphics.STOREHEIGHT -
                                        Graphics.STOREBUFFER -
                                        Graphics.DEPTBOTTOMHEIGHT +
                                        10)
                                    {
                                        if (!deptBottomActed)
                                        {
                                            int store =
                                                (playerSprite.X - Graphics.STOREBUFFER) /
                                                (Graphics.DEPTWIDTH + Graphics.DEPTTIGHTSPACING);

                                            switch (store)
                                            {
                                                case 0:
                                                    break;

                                                case 1:
                                                    break;

                                                case 2:
                                                    playerSprite.StopAnimation();
                                                    playerSprite.ChangeDirection(SpriteDirection.S);
                                                    muleSprite.StopAnimation();
                                                    muleSprite.ChangeDirection(SpriteDirection.S);
                                                    state = LocalState.Pub;
                                                    ticker.Stop();
                                                    game.Sound.PlaySound(Sounds.Pub);
                                                    break;
                                            }

                                            deptBottomActed = true;
                                        }
                                    }
                                    else
                                        playerSprite.MoveDown();
                                }
                                else if (xOffset <= Graphics.DEPTWIDTH / 5 &&
                                    xOffset > 0)
                                    playerSprite.MoveRight();
                                else if (xOffset >=
                                    Graphics.DEPTWIDTH -
                                    Graphics.DEPTWIDTH / 5 -
                                    Graphics.PLAYERWIDTH &&
                                    xOffset <
                                    Graphics.DEPTWIDTH -
                                    Graphics.PLAYERWIDTH)
                                    playerSprite.MoveLeft();
                            }
                            else
                            {
                                if (playerSprite.X >
                                    Graphics.STOREWIDTH -
                                    Graphics.STOREBUFFER -
                                    Graphics.DEPTMULEWIDTH &&
                                    playerSprite.X <
                                    Graphics.STOREWIDTH -
                                    Graphics.STOREBUFFER -
                                    Graphics.PLAYERWIDTH)
                                {
                                    if (playerSprite.Y >
                                        Graphics.STOREHEIGHT -
                                        Graphics.STOREBUFFER -
                                        Graphics.DEPTBOTTOMHEIGHT +
                                        10)
                                    {
                                        if (!deptBottomActed)
                                        {
                                            if (!hasMule)
                                            {
                                                muleSprite.X =
                                                    playerSprite.X + Graphics.PLAYERWIDTH / 2 -
                                                    Graphics.MULEWIDTH / 2;
                                                muleSprite.Y =
                                                    playerSprite.Y + Graphics.PLAYERHEIGHT / 2 -
                                                    Graphics.MULEHEIGHT / 2;
                                                muleSprite.Direction = SpriteDirection.S;
                                                hasMule = true;
                                            }

                                            deptBottomActed = true;
                                        }
                                    }
                                    else
                                        playerSprite.MoveDown();
                                }
                            }
                        }
                        else
                            playerSprite.MoveDown();
                    }

                    checkMuleMovement();

                    break;

                case LocalState.Outside:
                    if (playerIncX != 0 || playerIncY != 0)
                        if (playerSprite.X <
                            game.MapWidth / 2 + Graphics.PLOTSIZE / 2 &&
                            playerSprite.X >
                            game.MapWidth / 2 - Graphics.PLOTSIZE / 2 -
                            Graphics.SMALLPLAYERWIDTH &&
                            playerSprite.Y >
                            game.MapHeight / 2 - Graphics.PLOTSIZE / 2 -
                            Graphics.SMALLPLAYERHEIGHT &&
                            playerSprite.Y <
                            game.MapHeight / 2 + Graphics.PLOTSIZE / 2)
                        {
                            if (playerSprite.X < game.MapWidth / 2 - Graphics.PLOTSIZE / 4 - 
                                Graphics.SMALLPLAYERWIDTH)
                                storeTransLoc = StoreTransLoc.Left;
                            else if (playerSprite.X > game.MapWidth / 2 + Graphics.PLOTSIZE / 4)
                                storeTransLoc = StoreTransLoc.Right;
                            else
                                storeTransLoc = StoreTransLoc.Middle;

                            playerSprite.StopAnimation();
                            playerSprite.ChangeDirection(SpriteDirection.S);
                            muleSprite.StopAnimation();
                            muleSprite.ChangeDirection(SpriteDirection.S);
                            state = LocalState.ZoomInStore;
                            stateTimer.Change(10, 10);
                            ticker.Stop();
                            return;
                        }

                    bool terrain =
                        game.Map.CheckTerrain(
                            new Rectangle(
                                playerSprite.X,
                                playerSprite.Y,
                                Graphics.SMALLPLAYERWIDTH,
                                Graphics.SMALLPLAYERHEIGHT
                                ));

                    if (terrain && playerSprite.Speed == 1.5d)
                    {
                        playerSprite.Speed = 0.75d;
                        playerSprite.Frequency = 500;
                        muleSprite.Speed = 0.75d;
                        muleSprite.Frequency = 500;
                    }
                    else if (!terrain && playerSprite.Speed == 0.75d)
                    {
                        playerSprite.Speed = 1.5d;
                        playerSprite.Frequency = 250;
                        muleSprite.Speed = 1.5d;
                        muleSprite.Frequency = 250;
                    }
                    
                    if (playerIncX > 0)
                        playerSprite.MoveRight();
                    else if (playerIncX < 0)
                        playerSprite.MoveLeft();

                    if (playerIncY > 0)
                        playerSprite.MoveDown();
                    else if (playerIncY < 0)
                        playerSprite.MoveUp();

                    checkMuleMovement();

                    break;
            }

        }

        private void checkMuleMovement()
        {
            if (hasMule)
            {
                int incX = 0, incY = 0;

                if (playerSprite.Moving)
                {
                    if (playerSprite.X +
                        (playerSprite.Small ? Graphics.SMALLPLAYERWIDTH : Graphics.PLAYERWIDTH) <
                        muleSprite.X - muleSprite.Speed)
                        incX = -1;
                    else if (playerSprite.X >
                        muleSprite.X +
                        (muleSprite.Small ? Graphics.SMALLMULEWIDTH : Graphics.MULEWIDTH) +
                        muleSprite.Speed)
                        incX = 1;

                    if (playerSprite.Y +
                        (playerSprite.Small ? Graphics.SMALLPLAYERHEIGHT : Graphics.PLAYERHEIGHT) <
                        muleSprite.Y - muleSprite.Speed)
                        incY = -1;
                    else if (playerSprite.Y >
                        muleSprite.Y +
                        (muleSprite.Small ? Graphics.SMALLMULEHEIGHT : Graphics.MULEHEIGHT) +
                        muleSprite.Speed)
                        incY = 1;

                    int playerXOffset =
                        (playerSprite.X +
                        (playerSprite.Small ? Graphics.SMALLPLAYERWIDTH : Graphics.PLAYERWIDTH) / 2) -
                        (muleSprite.X +
                        (muleSprite.Small ? Graphics.SMALLMULEWIDTH : Graphics.MULEWIDTH) / 2);

                    int playerYOffset =
                        (playerSprite.Y +
                        (playerSprite.Small ? Graphics.SMALLPLAYERHEIGHT : Graphics.PLAYERHEIGHT) / 2) -
                        (muleSprite.Y +
                        (muleSprite.Small ? Graphics.SMALLMULEHEIGHT : Graphics.MULEHEIGHT) / 2);

                    if ((
                            playerSprite.Direction == SpriteDirection.E &&
                            playerXOffset > muleSprite.Speed
                        ) ||
                        (
                            playerSprite.Direction == SpriteDirection.W &&
                            playerXOffset < -muleSprite.Speed
                        ))
                    {
                        if (playerYOffset < -muleSprite.Speed)
                            incY = -1;
                        else if (playerYOffset > muleSprite.Speed)
                            incY = 1;
                    }

                    if ((
                            playerSprite.Direction == SpriteDirection.N &&
                            playerYOffset < -muleSprite.Speed
                        ) ||
                        (
                            playerSprite.Direction == SpriteDirection.S &&
                            playerYOffset > muleSprite.Speed
                        ))
                    {
                        if (playerXOffset < -muleSprite.Speed)
                            incX = -1;
                        else if (playerXOffset > muleSprite.Speed)
                            incX = 1;
                    }
                }

                if (incX == 0 && incY == 0)
                {
                    if (muleSprite.Moving)
                    {
                        muleSprite.StopAnimation();
                        muleSprite.ChangeDirection(SpriteDirection.S);
                    }
                }
                else
                {
                    muleSprite.StartAnimation();

                    SpriteDirection dir = Sprite.GetDirection(incX, incY);

                    if (muleSprite.Direction != dir)
                        muleSprite.ChangeDirection(dir);

                    lastMuleDir = dir;

                    if (incX == 1)
                        muleSprite.MoveRight();
                    else if (incX == -1)
                        muleSprite.MoveLeft();

                    if (incY == 1)
                        muleSprite.MoveDown();
                    else if (incY == -1)
                        muleSprite.MoveUp();
                }
            }
        }

        public override void DoAction(int player, PlayerAction action)
        {
            if (player != playerNum) return;

            if (state == LocalState.WaitForButton)
            {
                if (action == PlayerAction.JoystickButton)
                {
                    game.Sound.PlayBeep(playerNum);
                    state = LocalState.ZoomInStore;
                    stateTimer.Change(10, 10);
                    game.Players[playerNum].Highlight = false;
                }
            }
            else if (
                state == LocalState.InStore ||
                state == LocalState.Outside)
            {
                switch (action)
                {
                    case PlayerAction.JoystickButton:
                        if (state == LocalState.Outside)
                        {
                            Plot plot =
                                game.Map.CheckHouse(
                                    playerNum,
                                    new Rectangle(
                                        playerSprite.X,
                                        playerSprite.Y,
                                        Graphics.SMALLPLAYERWIDTH,
                                        Graphics.SMALLPLAYERHEIGHT
                                        ));

                            if (plot != null)
                                game.Sound.PlayBeep(playerNum);
                        }
                        break;

                    case PlayerAction.JoystickCenter:
                        playerSprite.StopAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.S);
                        playerIncX = 0;
                        playerIncY = 0;
                        break;

                    case PlayerAction.JoystickN:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.N);
                        playerIncX = 0;
                        playerIncY = -1;
                        break;

                    case PlayerAction.JoystickNE:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.NE);
                        playerIncX = 1;
                        playerIncY = -1;
                        break;

                    case PlayerAction.JoystickE:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.E);
                        playerIncX = 1;
                        playerIncY = 0;
                        break;

                    case PlayerAction.JoystickSE:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.SE);
                        playerIncX = 1;
                        playerIncY = 1;
                        break;

                    case PlayerAction.JoystickS:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.S);
                        playerIncX = 0;
                        playerIncY = 1;
                        break;

                    case PlayerAction.JoystickSW:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.SW);
                        playerIncX = -1;
                        playerIncY = 1;
                        break;

                    case PlayerAction.JoystickW:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.W);
                        playerIncX = -1;
                        playerIncY = 0;
                        break;

                    case PlayerAction.JoystickNW:
                        playerSprite.StartAnimation();
                        playerSprite.ChangeDirection(SpriteDirection.NW);
                        playerIncX = -1;
                        playerIncY = -1;
                        break;
                }
            }
        }
    }
}
