using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Data
{
    public class Statistics
    {
        private double Accumulator;
        private double Accumulator2;
        private int Count;

        public Statistics()
        {
            this.Clean();
        }

        public double Max { get; private set; }
        public double Min { get; private set; }

        public double Avg
        {
            get
            {
                return this.Count > 0 ? this.Accumulator / this.Count : 0;
            }
        }

        public double Std
        {
            get
            {
                double ret = 0;

                if (this.Count > 0)
                {
                    var avg = this.Avg;

                    var delta = this.Accumulator2 / this.Count - avg * avg;
                    if (delta < 0) delta = 0;

                    ret = Math.Sqrt(delta);
                }

                return ret;
            }
        }
        public void Clean()
        {
            this.Count = 0;
            this.Max = 0;
            this.Min = 0;
            this.Accumulator = 0;
            this.Accumulator2 = 0;
        }

        public void Push(double value)
        {
            if (Count == 0)
            {
                this.Min = value;
                this.Max = value;
            }
            else
            {
                if (value < this.Min) this.Min = value;
                if (value > this.Max) this.Max = value;
            }

            this.Accumulator += value;
            this.Accumulator2 += (value * value);

            Count++;
        }
    }
}
