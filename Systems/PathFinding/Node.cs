using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medicraft.Systems.PathFinding
{
    public class Node
    {
        // Path costs to get from the start to this tile (g) and from this tile to the target (h)
        public float f = 0f;
        public float g = 0f;
        public float h = 0f;

        // Store the Tile that led to this tile so it can be backtracked later to create the path
        public Node parent = null;

        // Tile specific data
        public int row;
        public int col;
        public int id;
        public int tileType;

        // Maintain a list of all valid tile touching this that can be walked to
        public List<Node> adjacent = new List<Node>();

        // Tile graphical data
        public Rectangle rec;
        public Color color;

        // This code is called whenever a new Tile is to be created
        public Node(int row, int col, int tileType, Color color, Vector2 mapSize)
        {
            // Store the grid location of the tile by row and column
            this.row = row;
            this.col = col;

            // Calculate the tiles numerical order (left to right, top to bottom) in the grid
            this.id = row * (int)mapSize.Y + col;

            // Store the tile's graphicak data
            this.tileType = tileType;
            this.color = color;

            rec = new Rectangle(col * AStar.TILE_SIZE, row * AStar.TILE_SIZE, AStar.TILE_SIZE, AStar.TILE_SIZE);
        }

        public void SetAdjacencies(Node[,] map)
        {
            // Only add walkable terrain, e.g. ignore walls and other obstacles
            for (int curRow = row - 1; curRow <= row + 1; curRow++)
            {
                for (int curCol = col - 1; curCol <= col + 1; curCol++)
                {
                    // Do not add itself
                    if (row != curRow || col != curCol)
                    {
                        // Add only Nodes at valid row and column that it walkable terrain
                        if (curRow >= 0 && curRow < AStar.NUM_ROWS          // Within bounds vertically
                            && curCol >= 0 && curCol < AStar.NUM_COLUMNS    // Within bounds horizontally
                            && map[curRow, curCol].tileType != AStar.BLOCK)  //  Valid terrain type
                        {
                            adjacent.Add(map[curRow, curCol]);
                        }
                    }
                }
            }
        }
    }
}
