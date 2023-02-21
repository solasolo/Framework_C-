using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenBao.MES.Business
{
    internal class StableFilter
    {
        int Header;
        int Size;
        double Threshold;

        private double[] Buffer;

        public StableFilter(int size)
        {
            this.Header = 0;
            this.Threshold = 10;
            this.Size = size;
            this.Buffer = new double[size];

            for (int i = 0; i < size; i++)
            { 
                this.Buffer[i] = 0;
            }
        }

        public bool Check(double value, ref double result)
        {
            bool ret = false;

            this.Buffer[this.Header++] = value;
            if(this.Header >= this.Size) this.Header = 0;

            double max = this.Buffer.Max();
            double min = this.Buffer.Min();

            if (Math.Abs(max - min) < this.Threshold)
            { 
                result = Buffer.Average();

                ret = true;
            }

            return ret;
        }
    }
}
