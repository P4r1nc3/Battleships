using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships
{
    internal class Ship
    {
        public int Size { get; }
        public int Hits { get; set; }
        public List<(int x, int y)> Positions { get; }

        public Ship(int size)
        {
            Size = size;
            Hits = 0;
            Positions = new List<(int x, int y)>();
        }
    }
}
