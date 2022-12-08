using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStar
{
    public class DStarPathfinder
    {
        public static float NormalCost = 1.0f;
        public static float AdjacentCost = 1.4f;
        public static float InfinityCost = float.MaxValue;

        public CostTable CostTable { get; private set; }

        public DStarMap DStarMap { get; private set; }

        public PriorityQueue<DStarNode> OpenList { get; private set; }

        public DStarNode Start { get; private set; }
        public DStarNode End { get; private set; }
        public DStarNode CurrentPosition { get; private set; }

        public DStarPathfinder(DStarMap map)
        {
            CostTable = new CostTable();
            OpenList = new PriorityQueue<DStarNode>();
            DStarMap = map;

            Start = map.Start;
            End = map.End;
            CurrentPosition = map.CurrentLocation;

            BuildCostTable();
        }

        public List<DStarNode> TraverseMap()
        {
            double minK;

            Insert(End, 0.0f);

            do
            {
                minK = ProcessState();
            } 
            while (minK != -1.0f && Start.Tag != "CLOSED");

            // if minK != -1, then end is unreachable
            if (minK == -1.0f)
            {
                throw new Exception("unreachable");
            }

            // trace path through back references
            do
            {
                DStarNode next;
                DStarNode here;
                bool unknownFound = false;

                do
                {
                    if (DetectCycle(DStarMap.CurrentLocation))
                    {
                        throw new UnreachableException("cycle");
                    }

                    here = DStarMap.CurrentLocation;
                    next = here.BackReference;

                    unknownFound = next.State == "U";
                    
                    if (!unknownFound)
                    {
                        DStarMap.CurrentLocation = next;
                        CurrentPosition = DStarMap.CurrentLocation;
                    }
                }
                while (CurrentPosition != End && !unknownFound);

                // if we reach the goal, exit
                if (CurrentPosition == End)
                {
                    var ret = new List<DStarNode>();
                    DStarNode iter = Start;
                    while (iter != null)
                    {
                        ret.Add(iter);
                        iter = iter.BackReference;
                    }
                    return ret;
                }
                else
                {
                    ModifyCost(here, next, InfinityCost);

                    // loop process state again until
                    // mimimum_K < h(next) or mimimum_K == -1.0
                    do
                    {
                        minK = ProcessState();
                    } 
                    while (minK < next.H && minK !=-1.0 &&  CurrentPosition.Tag != "CLOSED");

                    if (minK == -1.0f)
                    {
                        throw new UnreachableException("negative cost");
                    }
                }
            }
            while (true);
        }

        private double ModifyCost(DStarNode current, DStarNode neighbor, double newVal)
        {
            List<DStarNode> neighbors = DStarMap.Neighbors(neighbor.Y, neighbor.X);

            // update the arc path costs of neighbor to its neighbors
            foreach (DStarNode each in neighbors)
            {
                CostTable[neighbor, each] = InfinityCost;
            }

            // if neight is closed, then add it back to the open list with new h value
            if (neighbor.Tag == "CLOSED")
            {
                Insert(neighbor, newVal);
            }

            // get the new minimum k value on the open list
            DStarNode currentMin = OpenList.Peek();

            return currentMin.K;
        }

        private double ProcessState()
        {
            double oldK;

            // step 1 - if open list is empty, exit and return -1
            if (OpenList.Count == 0)
            {
                return -1.0f;
            }

            if (CurrentPosition.K == InfinityCost && CurrentPosition.H == InfinityCost)
            {
                return -1.0f;
            }

            DStarNode currentNode = OpenList.Poll();
            oldK = currentNode.K;
            currentNode.Tag = "CLOSED";

            // step 2 - reroute if necessary
            if (oldK < currentNode.H)
            {
                List<DStarNode> neighbors = DStarMap.Neighbors(currentNode.Y, currentNode.X);
                foreach(DStarNode neighbor in neighbors)
                {
                    double costXThroughY = neighbor.H + CostTable[neighbor, currentNode];
                    if (neighbor.H <= oldK && currentNode.H > costXThroughY)
                    {
                        currentNode.BackReference = neighbor;
                        currentNode.H = costXThroughY;
                    }
                }
            }

            // step 3 - usually done in initial map state processing
            if (oldK == currentNode.H)
            {
                List<DStarNode> neighbors = DStarMap.Neighbors(currentNode.Y, currentNode.X);
                foreach (DStarNode neighbor in neighbors)
                {
                    double costThroughX = currentNode.H + CostTable[currentNode, neighbor];
                    double roundedCost = Math.Round(costThroughX * 10)/10;

                    if (
                        neighbor.Tag == "NEW" || 
                        (neighbor.BackReference == currentNode && neighbor.H != costThroughX) ||
                        (neighbor.BackReference != currentNode && neighbor.H > costThroughX)
                    )
                    {
                        neighbor.BackReference = currentNode;
                        Insert(neighbor, roundedCost);
                    }
                }
            }

            // step 4
            else
            {
                List<DStarNode> neighbors = DStarMap.Neighbors(currentNode.Y, currentNode.X);
                foreach (DStarNode neighbor in neighbors)
                {
                    double costThroughX = currentNode.H + CostTable[currentNode, neighbor];
                    double costXThroughY = neighbor.H + CostTable[neighbor, currentNode];

                    double roundedCostThroughX = Math.Round(costThroughX * 10) / 10;
                    double roundedCostXThroughY = Math.Round(costXThroughY * 10) / 10;

                    // step 5
                    if (neighbor.Tag == "NEW" || (neighbor.BackReference == currentNode && neighbor.H != costThroughX))
                    {
                        neighbor.BackReference = currentNode;
                        Insert(neighbor, roundedCostThroughX);
                    }
                    else if (neighbor.BackReference != currentNode && neighbor.H > roundedCostThroughX)
                    {
                        double roundedH = Math.Round(currentNode.H * 10) / 10;
                        Insert(currentNode, roundedH);
                    }
                    else if (
                        neighbor.BackReference != currentNode && 
                        currentNode.H > roundedCostXThroughY && 
                        neighbor.Tag == "CLOSED" && 
                        neighbor.H > oldK
                    )
                    {
                        double roundedH = Math.Round(neighbor.H * 10) / 10;
                        Insert(neighbor, roundedH);
                    }
                }
            }

            // set minK, if openList is not empty, then getK from openList
            double minK = -1.0f;
            if (OpenList.Count != 0)
            {
                minK = OpenList.Peek().K;
            }

            return minK;
        }

        private void BuildCostTable()
        {
            for (int i = 0; i < DStarMap.RowCount; i++)
            {
                for (int j = 0; j < DStarMap.ColumnCount; j++)
                {
                    var diagonals = new List<CoordDelta>()
                    {
                        new CoordDelta(){ X = -1, Y = -1 },
                        new CoordDelta(){ X = -1, Y = 1 },
                        new CoordDelta(){ X = 1, Y = -1 },
                        new CoordDelta(){ X = 1, Y = 1 },
                    };

                    var streights = new List<CoordDelta>()
                    {
                        new CoordDelta(){ X = -1, Y = 0 },
                        new CoordDelta(){ X = 0,  Y = -1 },
                        new CoordDelta(){ X = 0,  Y = 1 },
                        new CoordDelta(){ X = 1,  Y = 0 },
                    };

                    DStarNode currentNode = DStarMap[i, j];

                    foreach(CoordDelta diagDelta in diagonals)
                    {
                        int neighborX = currentNode.X + diagDelta.X;
                        int neighborY = currentNode.Y + diagDelta.Y;

                        if (!(0 <= neighborX && neighborX <= DStarMap.ColumnCount-1))
                        {
                            continue;
                        }

                        if (!(0 <= neighborY && neighborY <= DStarMap.RowCount-1))
                        {
                            continue;
                        }

                        DStarNode neighbor = DStarMap[neighborY, neighborX];

                        CostTable[currentNode, neighbor] = (currentNode.State == "B" || neighbor.State == "B")
                            ? InfinityCost
                            : AdjacentCost;
                    }

                    foreach (CoordDelta diagDelta in streights)
                    {
                        int neighborX = currentNode.X + diagDelta.X;
                        int neighborY = currentNode.Y + diagDelta.Y;

                        if (!(0 <= neighborX && neighborX <= DStarMap.ColumnCount-1))
                        {
                            continue;
                        }

                        if (!(0 <= neighborY && neighborY <= DStarMap.RowCount-1))
                        {
                            continue;
                        }

                        DStarNode neighbor = DStarMap[neighborY, neighborX];

                        CostTable[currentNode, neighbor] = (currentNode.State == "B" || neighbor.State == "B")
                            ? InfinityCost
                            : NormalCost;
                    }
                }
            }
        }

        private void Insert(DStarNode node, double newH)
        {
            double roundedH;
            double currentK;

            if (node.Tag == "NEW")
            {
                node.K = newH;
                node.H = newH;
                node.Tag = "OPEN";
                OpenList.Add(node);
                return;
            }

            if (node.Tag == "OPEN")
            {
                currentK = node.K;
                OpenList.Remove(node);
                node.K = Math.Min(currentK, newH);
                OpenList.Add(node);
                return;
            }

            if (node.Tag == "CLOSED")
            {
                roundedH = Math.Round(node.H * 10) / 10;
                node.K = Math.Min(roundedH, newH);
                node.H = newH;
                node.Tag = "OPEN";
                OpenList.Add(node);
            }
        }

        private bool DetectCycle(DStarNode start)
        {
            if (start == null)
            {
                return false;
            }

            if (start.BackReference == null)
            {
                return false;
            }

            if (start.BackReference.BackReference == null)
            {
                return false;
            }

            DStarNode slow = start.BackReference;
            DStarNode fast = start.BackReference.BackReference;

            while (
                slow.BackReference != null && 
                fast.BackReference != null &&
                fast.BackReference.BackReference != null &&
                fast != slow
            )
            {
                slow = slow.BackReference;
                fast = fast.BackReference.BackReference;
            }

            return slow == fast;
        }
    }
}
