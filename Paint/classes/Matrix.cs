using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint.classes
{
    // |a b e p|
    // |d c f q|
    // |g h j r|
    // |m n k S|

    class Matrix3D_
    {
        public double a;
        public double b;
        public double c;
        public double d;
        public double e;
        public double f;
        public double g;
        public double h;
        public double j;
        public double p;
        public double q;
        public double r;
        public double m;
        public double n;
        public double k;
        public double S;

        public static int Size = 4;

        public Matrix3D_(double[][] arr)
        {
            if (arr.GetLength(0) != Size)
            {
                throw new ArgumentOutOfRangeException("Size not equal 4",(Exception)null);
            }

            foreach (var row in arr)
            {
                if (row.GetLength(0) != Size)
                {
                    throw new ArgumentOutOfRangeException("Size not equal 4", (Exception)null);
                }
            }

            this.a = arr[0][0];
            this.b = arr[0][1];
            this.e = arr[0][2];
            this.p = arr[0][3];

            this.d = arr[1][0];
            this.c = arr[1][1];
            this.f = arr[1][2];
            this.q = arr[1][3];

            this.g = arr[2][0];
            this.h = arr[2][1];
            this.j = arr[2][2];
            this.r = arr[2][3];

            this.m = arr[3][0];
            this.n = arr[3][1];
            this.k = arr[3][2];
            this.S = arr[3][3];
        }
    }
}
