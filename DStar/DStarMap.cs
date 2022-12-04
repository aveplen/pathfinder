using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStar
{
    public class DStarMap
    {
		public int RowCount { get; private set; } = 0;
		public int ColumnCount { get; private set; } = 0;

		public DStarNode Start { get; private set; }
		public DStarNode End { get; private set; }

		public DStarNode CurrentLocation { get; set; }

		public DStarNode[][] Map { get; private set; }

		public DStarMap(int rowCount, int columnCount)
		{
			RowCount = rowCount;
			ColumnCount = columnCount;

			Map = new DStarNode[rowCount][];
			for (int i = 0; i < rowCount; i++)
            {
				Map[i] = new DStarNode[columnCount];
            }
		}

		public DStarNode this[int i, int j]
		{
			get { return Map[i][j]; }
			private set { Map[i][j] = value; }
		}

		public void SetStart(int i, int j)
        {
			Start = Map[i][j];
        }

		public void SetEnd(int i, int j)
        {
			End = Map[i][j];
			End.K = 0;
			End.H = 0;
        }

		public List<DStarNode> Neighbors(int y, int x)
        {
			var deltas = new List<CoordDelta>()
			{
				new CoordDelta(){ X = -1, Y = -1 },
				new CoordDelta(){ X = -1, Y = 0 },
				new CoordDelta(){ X = -1, Y = 1 },

				new CoordDelta(){ X = 0, Y = -1 },
				new CoordDelta(){ X = 0, Y = 1 },

				new CoordDelta(){ X = 1, Y = -1 },
				new CoordDelta(){ X = 1, Y = 0 },
				new CoordDelta(){ X = 1, Y = 1 },
			};

			var neighbors = new List<DStarNode>();
			foreach(CoordDelta delta in deltas)
            {
				int neighborX = x + delta.X;
				if (!(0 <= neighborX && neighborX <= ColumnCount-1))
                {
					continue;
                }

				int neighborY = y + delta.Y;
				if (!(0 <= neighborY && neighborY <= RowCount-1))
                {
					continue;
                }

				neighbors.Add(Map[neighborY][neighborX]);
            }

			return neighbors;
		}

		private static HashSet<char> alphabet = new HashSet<char>() { 'O', 'B', 'U', 'S', 'G'};

		public void LoadMap(char[][] cells)
        {
			for (int i = 0; i < cells.Length; i++)
            {
				for (int j = 0; j < cells[i].Length; j++)
                {
					if (!alphabet.Contains(cells[i][j]))
					{
						throw new Exception($"char on pos [{i}, {j}] not found in alphabet: {cells[i][j]}");
                    }

					var node = new DStarNode(i, j);
					node.State = $"{cells[i][j]}";
					node.Tag = "NEW";

					if (cells[i][j] == 'S')
                    {
						Start = node;
						node.State = "O";
						CurrentLocation = node;
                    }

					if (cells[i][j] == 'G')
                    {
						End = node;
						node.State = "O";
					}

					Map[i][j] = node;
				}
			}
        }
	}
}
