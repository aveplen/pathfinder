using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using DStar;

namespace Pathfinder
{
    public partial class Form1 : Form
    {
        bool PickStartMode = false;
        bool PickDestinationMode = false;
        bool PickRoadColorMode = false;
        int? CellSize = null;
        Point? StartPoint = null;
        Point? EndPoint = null;
        Color? RoadColor = null;
        LinkedList<RoadNode> roadNodes = null;
        Bitmap MapBitmap = null;

        char[][] mapForDStar = null;
        List<Point> Path = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadMapImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var loadMapDialog = new OpenFileDialog();
            // loadMapDialog.Filter = "*.png|*.jpeg";
            DialogResult result = loadMapDialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            Image mapImage = Image.FromFile(loadMapDialog.FileName);
            var mapImageSize = new Size(mapImage.Width, mapImage.Height);

            this.Size = mapImageSize;

            pictureBox1.Image = mapImage;

            MapBitmap = new Bitmap(mapImage);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (!CellSize.HasValue)
            {
                return;
            }


            for (int i = 0; i < pictureBox1.Size.Width; i += CellSize.Value)
            {
                e.Graphics.DrawLine(
                    new Pen(Color.Red, 3f),
                    new Point(i, 0),
                    new Point(i, pictureBox1.Size.Height)
                );
            }

            for (int i = 0; i < pictureBox1.Size.Height; i += CellSize.Value)
            {
                e.Graphics.DrawLine(
                    new Pen(Color.Red, 3f),
                    new Point(0, i),
                    new Point(pictureBox1.Size.Width, i)
                );
            }

            if (!StartPoint.HasValue)
            {
                return;
            }

            e.Graphics.FillRectangle(
                Brushes.Red,
                new Rectangle()
                {
                    Width = CellSize.Value,
                    Height = CellSize.Value,
                    X = (StartPoint.Value.X / CellSize.Value) * CellSize.Value,
                    Y = (StartPoint.Value.Y / CellSize.Value) * CellSize.Value,
                }
            );

            if (!EndPoint.HasValue)
            {
                return;
            }

            e.Graphics.FillRectangle(
                Brushes.Red,
                new Rectangle()
                {
                    Width = CellSize.Value - 2,
                    Height = CellSize.Value - 2,
                    X = (EndPoint.Value.X / CellSize.Value) * CellSize.Value + 1,
                    Y = (EndPoint.Value.Y / CellSize.Value) * CellSize.Value + 1,
                }
            );

            if (roadNodes == null || roadNodes.Count == 0)
            {
                return;
            }

            int markerSize = Math.Max(CellSize.Value/6, 4);
            foreach(RoadNode roadNode in roadNodes)
            {
                e.Graphics.FillEllipse(
                    roadNode.IsRoad ? Brushes.Beige : Brushes.Black,
                    new Rectangle()
                    {
                         X = roadNode.X - markerSize/2,
                         Y = roadNode.Y - markerSize/2,
                         Width = markerSize,
                         Height = markerSize,
                    }
                );
            }

            if (Path == null)
            {
                return;
            }

            foreach (Point node in Path)
            {
                e.Graphics.FillRectangle(
                    Brushes.Blue,
                    new Rectangle()
                    {
                        Width = CellSize.Value,
                        Height = CellSize.Value,
                        X = node.X * CellSize.Value,
                        Y = node.Y * CellSize.Value,
                    }
                );
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CellSize = int.Parse(textBox1.Text);
            }
            catch (FormatException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PickStartMode = true;
            button1.Enabled = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            var test2 = this.PointToClient(MousePosition);

            if (PickStartMode)
            {
                StartPoint = new Point()
                {
                    X = test2.X - pictureBox1.Left,
                    Y = test2.Y - pictureBox1.Top,
                };
                PickStartMode = false;
                button1.Enabled = true;
                pictureBox1.Invalidate();
            }

            if (PickDestinationMode)
            {
                EndPoint = new Point()
                {
                    X = test2.X - pictureBox1.Left,
                    Y = test2.Y - pictureBox1.Top,
                };
                PickDestinationMode = false;
                button2.Enabled = true;
                pictureBox1.Invalidate();
            }

            if (PickRoadColorMode)
            {
                RoadColor = MapBitmap.GetPixel(
                    test2.X - pictureBox1.Left, 
                    test2.Y - pictureBox1.Top
                );

                PickRoadColorMode = false;
                button3.Enabled = true;
                pictureBox1.Invalidate();
            }

            TryToFindRoadNodes();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PickDestinationMode = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PickRoadColorMode = true;
            button3.Enabled = false;
        }

        private void TryToFindRoadNodes()
        {
            var conditions = new bool[]{
                StartPoint.HasValue,
                EndPoint.HasValue,
                RoadColor.HasValue,
            };

            bool allSatisfied = true;
            for (int i = 0; i < conditions.Length; i++)
            {
                allSatisfied = allSatisfied && conditions[i];
            }

            if (!allSatisfied)
            {
                return;
            }

            FindRoadNodes();
        }

        private struct RoadNode
        {
            public int X { get; set; }
            public int Y { get; set; }
            public bool IsRoad { get; set; }
        }

        private void FindRoadNodes()
        {
            mapForDStar = new char[MapBitmap.Height/CellSize.Value][];
            for (int i = 0; i < mapForDStar.Length; i++)
            {
                mapForDStar[i] = new char[MapBitmap.Width/CellSize.Value];
            }

            roadNodes = new LinkedList<RoadNode>();
            for (int i = 0; i < MapBitmap.Height/CellSize.Value; i++)
            {
                for (int j = 0; j < MapBitmap.Width/CellSize.Value; j++)
                {
                    var cellCenter = new Point()
                    {
                        Y = i * CellSize.Value + CellSize.Value / 2,
                        X = j * CellSize.Value + CellSize.Value / 2,
                    };

                    roadNodes.AddLast(new RoadNode()
                    {
                        X = cellCenter.X,
                        Y = cellCenter.Y,
                        IsRoad = IsRoadNode(cellCenter),
                    });

                    // this should not be here
                    mapForDStar[i][j] = IsRoadNode(cellCenter) ? 'O' : 'B';
                }
            }
        }

        private bool IsRoadNode(Point center)
        {
            var cornerToCenterRelatives = new List<Point>() { 
                new Point(){ X = -CellSize.Value/2 + 1, Y = -CellSize.Value/2 + 1 },
                new Point(){ X = CellSize.Value/2 - 1,  Y = -CellSize.Value/2 + 1 },
                new Point(){ X = -CellSize.Value/2 + 1, Y = CellSize.Value/2 - 1 },
                new Point(){ X = CellSize.Value/2 - 1,  Y = CellSize.Value/2 - 1 },
            };

            var cornerColors = new LinkedList<Color>();
            foreach(Point transition in cornerToCenterRelatives)
            {
                cornerColors.AddLast(MapBitmap.GetPixel(center.X + transition.X, center.Y + transition.Y));
            }

            bool isRoadNode = true;
            foreach(Color cornerColor in cornerColors)
            {
                isRoadNode = isRoadNode && cornerColor.Equals(RoadColor);
            }

            return isRoadNode;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            mapForDStar[StartPoint.Value.Y/CellSize.Value][StartPoint.Value.X/CellSize.Value] = 'S';
            mapForDStar[EndPoint.Value.Y/CellSize.Value][EndPoint.Value.X/CellSize.Value] = 'G';

            var dstarMap = new DStarMap(MapBitmap.Height/CellSize.Value, MapBitmap.Width/CellSize.Value);
            dstarMap.LoadMap(mapForDStar);

            var dstar = new DStarPathfinder(dstarMap);

            List<DStarNode> path = dstar.TraverseMap();

            Path = new List<Point>();
            foreach(DStarNode node in path)
            {
                Path.Add(new Point() { X = node.X, Y = node.Y });
            }
            pictureBox1.Invalidate();
        }
    }
}
