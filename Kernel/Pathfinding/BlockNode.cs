using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HabboEnvironment_R3.Kernel.Pathfinding
{
    public class BlockNode
    {
        /// <summary>
        /// Coord of the block.
        /// </summary>
        public Point Point { get; private set; }

        /// <summary>
        /// Height of the tile.
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// State of the block.
        /// </summary>
        public BlockState State { get; private set; }

        public BlockNode(Point Point, double Height, BlockState State)
        {
            this.Point = Point;
            this.Height = Height;
            this.State = State;
        }
    }

    public enum BlockState
    {
        OPEN, OPEN_LAST_STEP, BLOCKED,
    }
}
