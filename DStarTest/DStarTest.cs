using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using DStar;
using System.Threading.Tasks;

namespace DStarTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMap1()
        {
            var dstarMap = new DStarMap(7, 7);
            dstarMap.LoadMap(new char[][]
            {
                new char[] { 'O', 'O', 'O', 'U', 'O', 'O', 'G' },
                new char[] { 'O', 'O', 'O', 'O', 'B', 'O', 'B' },
                new char[] { 'O', 'O', 'O', 'B', 'U', 'O', 'O' },
                new char[] { 'O', 'O', 'O', 'O', 'B', 'O', 'O' },
                new char[] { 'O', 'O', 'O', 'O', 'B', 'O', 'O' },
                new char[] { 'O', 'S', 'O', 'O', 'U', 'O', 'O' },
                new char[] { 'O', 'O', 'O', 'O', 'O', 'O', 'O' },
            });

            var dstar = new DStarPathfinder(dstarMap);
            List<DStarNode> res = dstar.TraverseMap();
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public void TestMap2()
        {
            var dstarMap = new DStarMap(7, 7);
            dstarMap.LoadMap(new char[][]
            {
                new char[] { 'O', 'O', 'O', 'B', 'O', 'O', 'O' },
                new char[] { 'O', 'O', 'O', 'B', 'O', 'O', 'G' },
                new char[] { 'O', 'O', 'O', 'B', 'U', 'O', 'O' },
                new char[] { 'O', 'O', 'O', 'B', 'U', 'O', 'O' },
                new char[] { 'O', 'S', 'O', 'B', 'B', 'O', 'O' },
                new char[] { 'O', 'O', 'O', 'O', 'U', 'O', 'O' },
                new char[] { 'O', 'O', 'O', 'O', 'O', 'O', 'O' },
            });

            var dstar = new DStarPathfinder(dstarMap);
            List<DStarNode> res = dstar.TraverseMap();
            Assert.IsNotNull(res);
        }

        [TestMethod]
        public async Task TestPathfinderDoesntGetStuck()
        {
            var dstarMap = new DStarMap(7, 7);
            dstarMap.LoadMap(new char[][]
            {
                new char[] { 'B', 'B', 'B', 'B', 'B', 'B', 'S' },
                new char[] { 'B', 'B', 'B', 'B', 'B', 'B', 'B' },
                new char[] { 'B', 'B', 'B', 'B', 'B', 'B', 'B' },
                new char[] { 'B', 'B', 'B', 'B', 'B', 'B', 'B' },
                new char[] { 'B', 'B', 'B', 'B', 'B', 'B', 'B' },
                new char[] { 'B', 'B', 'B', 'B', 'B', 'B', 'B' },
                new char[] { 'G', 'B', 'B', 'B', 'B', 'B', 'B' },
            });

            var dstar = new DStarPathfinder(dstarMap);

            var task = Task.Run(() => dstar.TraverseMap());
            if (task.Wait(TimeSpan.FromSeconds(3)))
            {
                throw new Exception("pathfinder is stuck");
            }

            Assert.IsNotNull(await task);
        }
    }
}
