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
        public List<(int y, int x)> Positions { get; } = new();

        public Ship(int size) => Size = size;
    }
}
