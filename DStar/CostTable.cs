using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStar
{
    public class CostTable
    {
        public Dictionary<string, float> table { get; private set; }

        public CostTable()
        {
            table = new Dictionary<string, float>();
        }

        public float this[DStarNode first, DStarNode second]
        {
            get {
                return table[first.Label + second.Label];
            }
            set {
                table[first.Label + second.Label] = value;
                table[second.Label + first.Label] = value;
            }
        }
    }
}
