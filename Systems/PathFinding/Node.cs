using Microsoft.Xna.Framework;
using System.Collections.Generic;

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
        public int Row { get; set; }
        public int Col { get; set; }
        public int id;
        public int tileType;

        // Map Row and Col number
        public int NUM_ROWS;
        public int NUM_COLUMNS;
        public int startRow;
        public int startCol;
        public int endRow;
        public int endCol;

        // Maintain a list of all valid tile touching this that can be walked to
        public List<Node> adjacent = [];

        // Tile graphical data
        public Rectangle rec;
        public Color color;

        // This code is called whenever a new Tile is to be created
        public Node(int row, int col, int tileType, Color color, Vector2 mapSize, int NUM_ROWS, int NUM_COLUMNS
            , int startRow, int startCol, int endRow, int endCol)
        {
            // map row & col number
            this.NUM_ROWS = NUM_ROWS;
            this.NUM_COLUMNS = NUM_COLUMNS;
            this.startRow = startRow;
            this.startCol = startCol;
            this.endRow = endRow;
            this.endCol = endCol;

            // Store the grid location of the tile by row and column
            Row = row;
            Col = col;

            // Calculate the tiles numerical order (left to right, top to bottom) in the grid
            id = row * (int)mapSize.Y + col;

            // Store the tile's graphicak data
            this.tileType = tileType;
            this.color = color;

            rec = new Rectangle(
                col * GameGlobals.Instance.TILE_SIZE,
                row * GameGlobals.Instance.TILE_SIZE,
                GameGlobals.Instance.TILE_SIZE,
                GameGlobals.Instance.TILE_SIZE);
        }

        public void SetAdjacencies(Node[,] map)
        {
            // Only add walkable terrain, e.g. ignore walls and other obstacles
            for (int curRow = (Row - startRow) - 1; curRow <= (Row - startRow) + 1; curRow++)
            {
                for (int curCol = (Col - startCol) - 1; curCol <= (Col - startCol) + 1; curCol++)
                {
                    // Do not add itself
                    if (Row != curRow || Col != curCol)
                    {
                        // Add only Nodes at valid row and column that it walkable terrain
                        if (curRow >= 0 && curRow < NUM_ROWS          // Within bounds vertically
                            && curCol >= 0 && curCol < NUM_COLUMNS    // Within bounds horizontally
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
