using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULE
{
    public class Store
    {
        private int mules;
        private int mulePrice;
        private int food;
        private int foodPrice;
        private int energy;
        private int energyPrice;
        private int smithore;
        private int smithorePrice;
        private int crystite;
        private int crystitePrice;

        public Store()
        {
            mules = 14;
            mulePrice = 100;
            food = 100;
            foodPrice = 10;
            energy = 100;
            energyPrice = 10;
            smithore = 100;
            smithorePrice = 10;
            crystite = 100;
            crystitePrice = 10;
        }

        public int Mules
        {
            get { return mules; }
            set { mules = value; }
        }

        public int MulePrice
        {
            get { return mulePrice; }
            set { mulePrice = value; }
        }

        public int Food
        {
            get { return food; }
            set { food = value; }
        }

        public int FoodPrice
        {
            get { return foodPrice; }
            set { foodPrice = value; }
        }

        public int Energy
        {
            get { return energy; }
            set { energy = value; }
        }

        public int EnergyPrice
        {
            get { return energyPrice; }
            set { energyPrice = value; }
        }

        public int Smithore
        {
            get { return smithore; }
            set { smithore = value; }
        }

        public int SmithorePrice
        {
            get { return smithorePrice; }
            set { smithorePrice = value; }
        }

        public int Crystite
        {
            get { return crystite; }
            set { crystite = value; }
        }

        public int CrystitePrice
        {
            get { return crystitePrice; }
            set { crystitePrice = value; }
        }
    }
}
