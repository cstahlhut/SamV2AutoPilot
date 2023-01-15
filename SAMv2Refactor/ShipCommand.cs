using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        private class ShipCommand
        { // ShipCommand
            public int Command;
            public string ShipName;
            public List<NavCmd> navCmds = new List<NavCmd> { };
        }
    }
}
