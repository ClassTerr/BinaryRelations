using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binary_relations
{
    class Pair
    {
        public Pair()
        {

        }

        public Pair(string v1, string v2)
        {
            X = v1;
            Y = v2;
        }

        string x, y;

        public string Y
        {
            get { return y; }
            set { y = value; }
        }

        public string X
        {
            get { return x; }
            set { x = value; }
        }
    }
}
