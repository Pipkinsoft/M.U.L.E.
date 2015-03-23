using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULE
{
    public enum PlayerColor
    {
        Green = 1,
        Purple = 2,
        Orange = 3,
        Blue = 4,
        Red = 5,
        Yellow = 6,
        Brown = 7,
        Gray = 8
    }

    public enum PlayerType
    {
        Mechtron = 1,
        Gollumer = 2,
        Packer = 3,
        Bonzoid = 4,
        Spheroid = 5,
        Flapper = 6,
        Leggite = 7,
        Humanoid = 8
    }

    public class Player
    {
        public const int MAXNAMESIZE = 15;

        private PlayerColor color;
        private PlayerType type;
        private int playerNum;
        private string name;
        private bool ai;
        private int money;
        private int food;
        private int energy;
        private int smithore;
        private int crystite;
        private bool highlight;

        public Player(
            int playerNum,
            string name,
            PlayerColor color, 
            PlayerType type, 
            bool ai
            )
        {
            this.playerNum = playerNum;
            this.name = name;
            this.color = color;
            this.type = type;
            this.ai = ai;
            highlight = false;
        }

        public int PlayerNum
        {
            get { return playerNum; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public PlayerColor Color
        {
            get { return color; }
        }

        public PlayerType Type
        {
            get { return type; }
        }

        public bool AI
        {
            get { return ai; }
        }

        public int Money
        {
            get { return money; }
            set { money = value; }
        }

        public int Food
        {
            get { return food; }
            set { food = value; }
        }

        public int Energy
        {
            get { return energy; }
            set { energy = value; }
        }

        public int Smithore
        {
            get { return smithore; }
            set { smithore = value; }
        }

        public int Crystite
        {
            get { return crystite; }
            set { crystite = value; }
        }

        public bool Highlight
        {
            get { return highlight; }
            set { highlight = value; }
        }
    }
}
