using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HabboEnvironment_R3.Kernel.Pathfinding
{
    public class BlockCalculator
    {
        /// <summary>
        /// Directions to work on.
        /// </summary>
        public short[,] Directions
        {
            get;
            private set;
        }

        public BlockCalculator(bool EnableDiagonal)
        {
            if (EnableDiagonal)
            {
                Directions = new short[8, 2] { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 }, { -1, -1 }, { 1, -1 }, { -1, 1 }, { 1, 1 } };
            }
            else
            {
                Directions = new short[4, 2] { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 } };
            }
        }

        /// <summary>
        /// Generates an path from start to end.
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <param name="Nodes"></param>
        /// <returns></returns>
        public Stack<BlockNode> Generate(Point Start, Point End, ICollection<BlockNode> Nodes)
        {
            var Output = new List<BlockNode>();
            var Flag = false;
            var WorkTile = Start;

            while (!Flag)
            {
                var Set = new HashSet<BlockNode>();

                for (short i = 0; i < Directions.GetLength(0); i++)
                {
                    var X = Directions[i, 0] + WorkTile.X;
                    var Y = Directions[i, 1] + WorkTile.Y;

                    BlockNode WorkingNode;

                    if (TryPopNode(new Point(X, Y), Nodes, out WorkingNode))
                    {
                        switch (WorkingNode.State)
                        {
                            case BlockState.BLOCKED:
                                continue;
                            case BlockState.OPEN:
                                Set.Add(WorkingNode);
                                continue;
                            case BlockState.OPEN_LAST_STEP:
                                if (WorkingNode.Point == End)
                                {
                                    Set.Add(WorkingNode);
                                }
                                continue;

                        }
                    }
                }

                var MySortedPoints = (from p in Set orderby GetVector(p.Point, End) ascending select p);

                if (MySortedPoints.Count() > 0)
                {
                    var Node = MySortedPoints.First();

                    if (!Output.Contains(Node))
                    {
                        WorkTile = Node.Point;
                        Output.Add(Node);
                    }
                    else
                    {
                        Flag = true;
                    }

                    if (WorkTile.Equals(End))
                    {
                        Flag = true;
                    }
                }
                else
                {
                    Flag = true;
                }
            }

            Stack<BlockNode> StackReverse = new Stack<BlockNode>();

            foreach (var Node in (from item in Output orderby Array.IndexOf(Output.ToArray(), item) descending select item))
            {
                StackReverse.Push(Node);
            }

            return StackReverse;
        }

        /// <summary>
        /// Tries to get an node.
        /// </summary>
        /// <param name="Block"></param>
        /// <param name="Nodes"></param>
        /// <param name="Node"></param>
        /// <returns></returns>
        public bool TryPopNode(Point Block, ICollection<BlockNode> Nodes, out BlockNode Node)
        {
            Node = null;

            foreach (var Item in Nodes)
            {
                if (Item.Point == Block)
                {
                    Node = Item;
                }
            }

            return Node != null;
        }

        /// <summary>
        /// Returns the distance between two tiles.
        /// </summary>
        /// <param name="PointA"></param>
        /// <param name="PointB"></param>
        /// <returns></returns>
        public double GetVector(Point PointA, Point PointB)
        {
            double a = (double)(PointB.X - PointA.X);
            double b = (double)(PointB.Y - PointA.Y);

            return Math.Sqrt(a * a + b * b);
        }

        /// <summary>
        /// Calculates the rotation from two points.
        /// </summary>
        /// <param name="PointA"></param>
        /// <param name="PointB"></param>
        /// <returns></returns>
        public int GetRotation(Point PointA, Point PointB)
        {
            return GetRotation(PointA.X, PointA.Y, PointB.X, PointB.Y);
        }

        /// <summary>
        /// Calculates the rotation from two points.
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="Y1"></param>
        /// <param name="X2"></param>
        /// <param name="Y2"></param>
        /// <returns></returns>
        public int GetRotation(int X1, int Y1, int X2, int Y2)
        {
            int Rotation = 0;

            if (X1 > X2 && Y1 > Y2)
            {
                Rotation = 7;
            }
            else if (X1 < X2 && Y1 < Y2)
            {
                Rotation = 3;
            }
            else if (X1 > X2 && Y1 < Y2)
            {
                Rotation = 5;
            }
            else if (X1 < X2 && Y1 > Y2)
            {
                Rotation = 1;
            }
            else if (X1 > X2)
            {
                Rotation = 6;
            }
            else if (X1 < X2)
            {
                Rotation = 2;
            }
            else if (Y1 < Y2)
            {
                Rotation = 4;
            }
            else if (Y1 > Y2)
            {
                Rotation = 0;
            }

            return Rotation;
        }
    }
}
