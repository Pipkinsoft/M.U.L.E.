using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.IO;
using SdlDotNet;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Audio;
using SdlDotNet.Input;
using MULE.States;

namespace MULE
{
    public class Game : IDisposable
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }

        private Graphics graphics;
        private Sound sound;
        private Input input;
        private Network network;
        private State state;

        private Random random;

        private Map map;
        private Player[] players;
        
        private int myPlayerNum;
        private int round;
        private int numRounds;
        private int playerTurn;
        
        private int plotValue;
        private int foodValue;
        private int energyValue;
        private int smithoreValue;
        private int crystiteValue;

        private Store store;

        public Game() { }

        public void Start()
        {
            random = new Random();

            graphics = new Graphics(this);
            sound = new Sound(this);
            input = new Input(this);

            input.KeyPress += new KeyPressHandler(input_KeyPress);
            Events.Quit += new EventHandler<QuitEventArgs>(Events_Quit);

            initializeTestGame();

            Events.Run();
        }

        private void input_KeyPress(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Key.Escape)
                close();
        }

        public string AppPath
        {
            get
            {
                return 
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + 
                    Path.DirectorySeparatorChar;
            }
        }

        public State State
        {
            get { return state; }
        }

        public Sound Sound
        {
            get { return sound; }
        }

        public Graphics Graphics
        {
            get { return graphics; }
        }

        public Random Random
        {
            get { return random; }
        }

        public Map Map
        {
            get { return map; }
        }

        public int Width
        {
            get { return graphics.Width; }
        }

        public int Height
        {
            get { return graphics.Height; }
        }

        public int MapWidth
        {
            get { return map.Width * Graphics.PLOTSIZE; }
        }

        public int MapHeight
        {
            get { return map.Height * Graphics.PLOTSIZE; }
        }

        public Player[] Players
        {
            get { return players; }
        }

        public Player[] OrderedPlayers
        {
            get
            {
                Player[] sortedPlayers = new Player[players.Length];

                SortedList<double, int> playerRanks =
                    new SortedList<double, int>();

                for (int i = 0; i < players.Length; i++)
                    playerRanks.Add(
                        double.Parse(
                            GetTotalValue(i).ToString() + "." + 
                            (players[i].AI ? "5" : "0") +
                            (8 - i).ToString()
                            ), 
                        i
                        );

                for (int i = playerRanks.Keys.Count - 1; i >= 0; i--)
                    sortedPlayers[playerRanks.Keys.Count - 1 - i] = 
                        players[playerRanks[playerRanks.Keys[i]]];

                return sortedPlayers;
            }
        }

        public int MyPlayerNum
        {
            get { return myPlayerNum; }
        }

        public int NumRounds
        {
            get { return numRounds; }
        }

        public int Round
        {
            get { return round; }
        }

        public Store Store
        {
            get { return store; }
        }

        public int PlotValue
        {
            get { return plotValue; }
        }

        public int GetLandValue(int player)
        {
            return map.GetPlots(player).Count * plotValue;
        }

        public int GetGoodsValue(int player)
        {
            return
                players[player].Food * foodValue +
                players[player].Energy * energyValue +
                players[player].Smithore * smithoreValue +
                players[player].Crystite * crystiteValue;
        }

        public int GetTotalValue(int player)
        {
            return
                players[player].Money +
                GetLandValue(player) +
                GetGoodsValue(player);
        }

        public int GetColonyTotal()
        {
            int total = 0;

            for (int i = 0; i < players.Length; i++)
                total += GetTotalValue(i);

            return total;
        }

        private void initializeTestGame()
        {
            newMap(9, 5);

            numRounds = 12;
            round = 1;
            playerTurn = 0;

            plotValue = 500;
            foodValue = 25;
            energyValue = 25;
            smithoreValue = 100;
            crystiteValue = 100;

            players = new Player[8];

            for (int i = 0; i < players.Length; i++)
            {
                players[i] = 
                    new Player(
                        i,
                        (i > 0 ? "Computer #" + i.ToString() : "Player #1"),
                        (PlayerColor)(i + 1), 
                        PlayerType.Mechtron,
                        i > 0
                        );
                players[i].Money = (i == 0 ? 1400 : 1200);
                players[i].Food = 4;
                players[i].Energy = 2;
            }

            myPlayerNum = 0;

            store = new Store();

            map.Plots[2, 2].Player = myPlayerNum;

            //beginPlayerTurn(myPlayerNum);
            beginTransport(TransportState.LocalState.MoveLeft);
        }

        private void newMap(int width, int height)
        {
            map = new Map(this, width, height);
            graphics.SetTiles(width, height);
        }

        private void beginTransport(
            TransportState.LocalState initialState = TransportState.LocalState.MoveLeft
            )
        {
            if (state != null) state.Dispose();
            state = new TransportState(this, initialState);
            state.StateComplete += new StateCompleteHandler(state_StateComplete);
        }

        private void beginSummary()
        {
            if (state != null) state.Dispose();
            state = new SummaryState(this);
            state.StateComplete += new StateCompleteHandler(state_StateComplete);
        }

        private void beginLandGrant()
        {
            if (state != null) state.Dispose();
            state = new LandGrantState(this);
            state.StateComplete += new StateCompleteHandler(state_StateComplete);
        }

        private void beginPlayerTurn(int playerNum)
        {

            if (state != null) state.Dispose();
            state = new PlayerTurnState(this, playerNum);
            state.StateComplete += new StateCompleteHandler(state_StateComplete);
        }

        private void state_StateComplete(object sender, EventArgs e)
        {
            graphics.Drawing = false;
            Thread.Sleep(100);

            if (state is TransportState)
            {
                TransportState s = (TransportState)state;

                if (s.ShipRectangle.X < 0)
                    beginLandGrant();
                else
                    beginSummary();
            }
            else if (state is SummaryState)
            {
                round++;

                if (round == 1)
                    beginTransport(TransportState.LocalState.Lifting);
                else
                    beginLandGrant();
            }
            else if (state is LandGrantState)
            {
                playerTurn = 0;
                beginPlayerTurn(OrderedPlayers[playerTurn].PlayerNum);
            }
            else if (state is PlayerTurnState)
            {
                playerTurn++;
                if (playerTurn == players.Length)
                {
                }
                else
                    beginPlayerTurn(OrderedPlayers[playerTurn].PlayerNum);
            }

            graphics.Drawing = true;
        }

        private void Events_Quit(object sender, QuitEventArgs e)
        {
            close();
        }

        private void close()
        {
            Events.Close();
            Events.QuitApplication();
            Dispose();
        }

        #region IDisposable Members

        private bool disposed;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose();
        }

        ~Game()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
            }
        }

        #endregion
    }
}
