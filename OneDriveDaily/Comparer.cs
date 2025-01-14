using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneDriveDaily
{
    public class MyComparer:Comparer<string>
    {
        public override int Compare(string? x, string? y)
        {
            Regex r = new Regex(@"p[0-9]{1,}");

            if (r.IsMatch(x) && r.IsMatch(y))
            {
                string[] newX = x.Split('p');
                string[] newY = y.Split('p');

                if (newX[0].CompareTo(newY[0]) == 0)
                {
                    Int32.TryParse(newX[1], out int newXInt);
                    Int32.TryParse(newY[1], out int newYInt);

                    return newXInt.CompareTo(newYInt);
                }
                else
                    return x.CompareTo(y);
            }
            else
                return x.CompareTo(y);
        }
    }
}
