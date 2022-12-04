using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStar
{
	public class DStarNode : IComparable<DStarNode>
	{
		public string Tag { get; set; } = "NEW";
		public string State { get; set; } = "O";
		public string Label { get; set; } = "";

		public int Y { get; private set; }
		public int X { get; private set; }

		public double H { get; set; } = 0.0f;
		public double K { get; set; } = 0.0f;

		public DStarNode BackReference { get; set; } = null;

		public int CompareTo(DStarNode other)
        {
			return this.K.CompareTo(other.K);
		}

		public DStarNode(int y, int x)
        {
			Y = y;
			X = x;

			Label = $"({x}, {y})";
		}
	}
}
