using System;
using System.Collections.Generic;

namespace UE4TextConverter.Model
{
    public class NaturalStringComparer : IComparer<string>
    {
        public static readonly NaturalStringComparer Instance = new NaturalStringComparer();

        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int ix = 0, iy = 0;

            while (ix < x.Length && iy < y.Length)
            {
                bool xDigit = char.IsDigit(x[ix]);
                bool yDigit = char.IsDigit(y[iy]);

                if (xDigit && yDigit)
                {
                    // Compare numeric segments by value
                    long numX = 0;
                    int startX = ix;
                    while (ix < x.Length && char.IsDigit(x[ix]))
                    {
                        numX = numX * 10 + (x[ix] - '0');
                        ix++;
                    }

                    long numY = 0;
                    int startY = iy;
                    while (iy < y.Length && char.IsDigit(y[iy]))
                    {
                        numY = numY * 10 + (y[iy] - '0');
                        iy++;
                    }

                    if (numX != numY)
                        return numX.CompareTo(numY);

                    // Same value but different lengths (e.g. "02" vs "2") — shorter first
                    int lenDiff = (ix - startX) - (iy - startY);
                    if (lenDiff != 0)
                        return lenDiff;
                }
                else if (!xDigit && !yDigit)
                {
                    // Compare text segments
                    int cmp = char.ToUpperInvariant(x[ix]).CompareTo(char.ToUpperInvariant(y[iy]));
                    if (cmp != 0)
                        return cmp;
                    ix++;
                    iy++;
                }
                else
                {
                    // One is digit, one is text — digits sort before letters
                    return xDigit ? -1 : 1;
                }
            }

            return x.Length.CompareTo(y.Length);
        }
    }
}
