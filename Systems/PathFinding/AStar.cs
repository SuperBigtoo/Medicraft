using Medicraft.Entities;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using static Medicraft.Entities.Entity;

namespace Medicraft.Systems.PathFinding
{
    public class AStar
    {    
        // Maintain tile types by number representation
        public const int BLOCK = 0;
        public const int ROAD = 1;
        public const int START = 2;
        public const int END = 3;
        public const int BLANK = 4;

        // Grid scaling variables
        public int NUM_ROWS;
        public int NUM_COLUMNS;

        // Maintain size of tiles
        public int TILE_SIZE;

        // Define Colors based on Numbered tile types
        readonly Color[] tileColors =
        [
            Color.Red,
            Color.Green,
            Color.Aqua,
            Color.Yellow,
            Color.Tan
        ];

        // Create a Map 5*5
        //int[,] map = new int[,]
        //{
        //    { ROAD, ROAD, ROAD, ROAD, ROAD },
        //    { BLOCK, BLOCK, ROAD, BLOCK, BLOCK },
        //    { ROAD, ROAD, ROAD, ROAD, ROAD },
        //    { ROAD, BLOCK, BLOCK, BLOCK, ROAD },
        //    { ROAD, ROAD, ROAD, ROAD, ROAD }
        //};

        // Tile Area of Path Finding
        public int[,] map;
        public int startRow;
        public int startCol;
        public int endRow;
        public int endCol;
        const int NOT_FOUND = -1;

        // Define movement cost multiplier based on Numbered tile type
        readonly float[] tileCosts =
        [
            10000f,     // Wall
            0.5f,       // Road
            1f,         // Start
            1f,         // End
            10000f      // Blank
        ];

        readonly float hvCost = 10f;     // Cost to move horizontally, vertically on standard terrain
        readonly float diagCost = 14f;   // Cost to move diagonally on standard terrain

        // Maintain a map that track all of the tile (Node) information
        readonly Node[,] tileMap;
        Vector2 mapSize;

        // track the Beginning and End of the path
        Node start;
        Node end;

        // Track all of the path Nodes
        private readonly List<Node> path = [];

        // Maintain two lists, one of Nodes to check and one of potential Nodes
        readonly List<Node> open = [];
        readonly List<Node> closed = [];

        public AStar() { }

        public AStar(int startPosX, int startPosY, int endPosX, int endPosY)
        {
            TILE_SIZE = GameGlobals.Instance.TILE_SIZE;

            var distance = (int)(new Vector2(startPosX, startPosY) - new Vector2(endPosX, endPosY)).Length();

            map = CloneMapAroundCenter(
                (int[,])GameGlobals.Instance.TILEMAP.Clone(),
                startPosY / TILE_SIZE,
                startPosX / TILE_SIZE,
                (distance / TILE_SIZE) + 1);

            NUM_ROWS = map.GetLength(0);
            NUM_COLUMNS = map.GetLength(1);

            //System.Diagnostics.Debug.WriteLine($"NUM_ROWS & NUM_COLUMNS : {NUM_ROWS} {NUM_COLUMNS}");

            mapSize = new Vector2(NUM_ROWS, NUM_COLUMNS);
            tileMap = new Node[NUM_ROWS, NUM_COLUMNS];

            SetStart(startPosX, startPosY);
            SetEnd(endPosX, endPosY);
            Initialize();
            path = FindPath(start, end);
        }

        private void Initialize()
        {
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLUMNS; col++)
                {
                    // Based on the int map array create a Node of that type in the same grid coordinates
                    tileMap[row, col] ??= new Node(
                        startRow + row,
                        startCol + col,
                        map[row, col],
                        tileColors[map[row, col]],
                        mapSize,
                        NUM_ROWS,
                        NUM_COLUMNS,
                        startRow, startCol, endRow, endCol);

                    // Find and track the start and end of the path
                    if (map[row, col] == START)
                    {
                        start = tileMap[row, col];
                    }
                    else if (map[row, col] == END)
                    {
                        end = tileMap[row, col];
                    }
                }
            }

            start ??= end;

            //if (entityTypes == EntityTypes.Companion)
            //{
            //    System.Diagnostics.Debug.WriteLine($"NUM_ROWS: {NUM_ROWS} NUM_COLUMNS: {NUM_COLUMNS} mapSize: {mapSize}");
            //    System.Diagnostics.Debug.WriteLine($"start & end : {start.Col} {start.Row} | {end.Col} {end.Row}");
            //}

            // Setup necessary pathing data, adjacent Nodes(Tiles) and the H cost from each Node to the Target
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLUMNS; col++)
                {
                    // For each tile, find all valid tiles surraounding it that are not walls, off the map or obstacles
                    tileMap[row, col].SetAdjacencies(tileMap);
                }
            }

            // Calcurlate the H cost to get from Every tile to the End space
            SetHCosts(tileMap, end.Row, end.Col);
        }

        public void SetStart(float x, float y)
        {
            var startPos = new Vector2(x, y);

            // Original Col and Row in base map
            int col = (int)(startPos.X / TILE_SIZE);
            int row = (int)(startPos.Y / TILE_SIZE);

            // Set new Col and Row for cropping Map
            var newCol = col - startCol;
            var newRow = row - startRow;

            map[newRow, newCol] = START;
            tileMap[newRow, newCol] = new Node(
                    row,
                    col,
                    map[newRow, newCol],
                    tileColors[map[newRow, newCol]],
                    mapSize,
                    NUM_ROWS,
                    NUM_COLUMNS,
                    startRow, startCol, endRow, endCol);

            //System.Diagnostics.Debug.WriteLine($"Vo: {col}");
            //System.Diagnostics.Debug.WriteLine($"Mo: {GameGlobals.Instance.TILEMAP.GetLength(1)}");
            //System.Diagnostics.Debug.WriteLine($"Mn: {NUM_COLUMNS}");
            //System.Diagnostics.Debug.WriteLine($"(Vo/Mo) * Mn: {(float)col / (float)GameGlobals.Instance.TILEMAP.GetLength(1) * NUM_COLUMNS}");
            //System.Diagnostics.Debug.WriteLine($"newCol & newRow: {newCol} {newRow} | col & row {col} {row}");

            //System.Diagnostics.Debug.WriteLine($"\nstartCol: {startCol}");
            //System.Diagnostics.Debug.WriteLine($"TILE_SIZE: {TILE_SIZE}");
            //System.Diagnostics.Debug.WriteLine($"startCol * TILE_SIZE: {startCol * TILE_SIZE}");

            //System.Diagnostics.Debug.WriteLine($"\nstartCol: {startCol}");
            //System.Diagnostics.Debug.WriteLine($"startRow: {startRow}");
            //System.Diagnostics.Debug.WriteLine($"START: Row = {(int)newRow} Col = {(int)newCol}");
        }

        public void SetEnd(float x, float y)
        {
            var endPos = new Vector2(x, y);

            // Original Col and Row in base map
            int col = (int)(endPos.X / TILE_SIZE);
            int row = (int)(endPos.Y / TILE_SIZE);

            // Set new Col and Row for cropping Map
            var newCol = col - startCol;
            var newRow = row - startRow;

            map[newRow, newCol] = END;
            tileMap[newRow, newCol] = new Node(
                    row,
                    col,
                    map[newRow, newCol],
                    tileColors[map[newRow, newCol]],
                    mapSize,
                    NUM_ROWS,
                    NUM_COLUMNS,
                    startRow, startCol, endRow, endCol);

            //System.Diagnostics.Debug.WriteLine($"END: Row = {(int)newRow} Col = {(int)newCol}");
        }

        public static int[,] CloneTileType()
        {
            int[,] tileType = (int[,])GameGlobals.Instance.TILEMAP.Clone();
            int rows = tileType.GetLength(0);
            int cols = tileType.GetLength(1);

            int[,] deepCopy = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    deepCopy[i, j] = tileType[i, j];
                }
            }

            return deepCopy;
        }

        public int[,] CloneMapAroundCenter(int[,] originalMap, int centerRow, int centerCol, int radius)
        {
            int minX = Math.Max(centerCol - radius, 0);
            int maxX = Math.Min(centerCol + radius, originalMap.GetLength(1) - 1);
            int minY = Math.Max(centerRow - radius, 0);
            int maxY = Math.Min(centerRow + radius, originalMap.GetLength(0) - 1);

            startCol = minX;
            startRow = minY;
            endCol = maxX;
            endRow = maxY;

            int newWidth = maxX - minX + 1;
            int newHeight = maxY - minY + 1;

            int[,] clonedMap = new int[newHeight, newWidth];

            for (int row = minY, newRow = 0; row <= maxY; row++, newRow++)
            {
                for (int col = minX, newCol = 0; col <= maxX; col++, newCol++)
                {
                    clonedMap[newRow, newCol] = originalMap[row, col];
                }
            }

            return clonedMap;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.End();

            var graphicsDevice = ScreenManager.Instance.GraphicsDevice;

            spriteBatch.Begin(
                transformMatrix: ScreenManager.Camera.GetTransform(
                    graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height)
            );

            // Draw the world elements including the terrain, the frid and the path in that order for visibility
            DrawTerrain(spriteBatch, tileMap);
            DrawGrid(spriteBatch, tileMap);
            DrawPath(spriteBatch);
        }

        private void DrawTerrain(SpriteBatch spriteBatch, Node[,] map)
        {
            Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });      

            // Draw tiles by Color based on type if are not blank tiles
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    // Only draw non blank tiles
                    if (map[row, col].tileType != BLANK)
                    {
                        spriteBatch.Draw(pixelTexture, map[row, col].rec, map[row, col].color);
                    }
                }
            }
        }

        private void DrawPath(SpriteBatch spriteBatch)
        {
            Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });

            // Only draw the path if neccessary
            if (path.Count > 0)
            {
                // Draw each Node of the path in the chosen color
                for (int i = 0; i < path.Count; i++)
                {
                    spriteBatch.Draw(pixelTexture, path[i].rec, Color.Orange);
                }
            }
        }

        private void DrawHLine(SpriteBatch spriteBatch, int x, int y, int length, int lineWidth, Color color)
        {
            Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });

            spriteBatch.Draw(pixelTexture, new Rectangle(x, y, length, lineWidth), color);
        }

        private void DrawVLine(SpriteBatch spriteBatch, int x, int y, int length, int lineWidth, Color color)
        {
            Texture2D pixelTexture = new Texture2D(ScreenManager.Instance.GraphicsDevice, 1, 1);
            pixelTexture.SetData(new Color[] { Color.White });

            spriteBatch.Draw(pixelTexture, new Rectangle(x, y, lineWidth, length), color);
        }

        private void DrawGrid(SpriteBatch spriteBatch, Node[,] map)
        {
            // Horizontal lines = number of rows + 1
            for (int i = 0; i < map.GetLength(0); i++)
            {
                DrawHLine(spriteBatch, startCol * TILE_SIZE, (i + startRow) * TILE_SIZE
                    , TILE_SIZE * map.GetLength(0), 1, Color.Black);
            }

            // Vertical lines = number of columns + 1
            for (int i = 0; i < map.GetLength(1); i++)
            {
                DrawVLine(spriteBatch, (i + startCol) * TILE_SIZE, startRow * TILE_SIZE
                    , TILE_SIZE * map.GetLength(1), 1, Color.Black);
            }
        }

        private List<Node> FindPath(Node start, Node end)
        {
            // Maintain a resulting path to return
            List<Node> result = new List<Node>();

            // Variables to be recalculated in each iteration of finding potential path nodes
            float minF = 10000f;    // Used to find thne cheapest F cost in the open list of Nodes
            int minIndex = 0;       // Tracks the open list index of the smallest F cost Node
            Node curNode;           // Tracks the Node with the minimum F cost to be further tested

            // 1. Set start G cost to of start Node to 0, since it cost nothing to go from start to start
            start.g = 0;
            start.f = start.g + start.h;

            // 2. Add start point to the open list
            open.Add(start);

            // Repeat the following steps until
            // A) target is added to close (path found), or B) open list is empty (no path)
            // Loop until a path is found or it is impossible to find a path
            while (true)
            {
                // 3. Find smallest F cost in open list
                minF = 10000f;
                for (int i = 0; i < open.Count; i++)
                {
                    if (open[i].f < minF)
                    {
                        // Set the current minimum F and index it is at
                        minF = open[i].f;
                        minIndex = i;
                    }
                }

                // Minimum F cost has been found at minIndex, setup the current Node by
                // Tracking it, removing it from the open list and adding it to the closed list
                curNode = open[minIndex];
                open.RemoveAt(minIndex);
                closed.Add(curNode);

                // 4. if the added node is the target, STOP!! (path found)
                if (curNode.id == end.id)
                {
                    break;
                }

                // 5. Go through each of the current node's adjacent Nodes
                // A) Ignore it if it is already in the closed list or is not walkable
                // B) If it is not in the open list, Set its parent to current node, recalculate its G, H and F and add it to the open list
                // C) If it is already in the open list, compare its current G with its potential new G cost
                Node compNode;
                for (int i = 0; i < curNode.adjacent.Count; i++)
                {
                    // Retrieve the next adjacent Node of curNode, this will be our comparison Node
                    compNode = curNode.adjacent[i];

                    // A) Check it is not in the closed list and its a walkable type
                    if (compNode.tileType != BLOCK
                        && ContainsNode(closed, compNode) == NOT_FOUND)
                    {
                        // At this point we  know that compNode will be added or is in the open list
                        // In Both cases we will need to recalculate its G cost
                        float newG = GetGCost(compNode, curNode);
                        //float newH = GetHCost();

                        // B) Check the Open list
                        if (ContainsNode(open, compNode) == NOT_FOUND)
                        {
                            // Set parent
                            compNode.parent = curNode;

                            // Recalculate G, H and F
                            compNode.g = newG;
                            compNode.f = compNode.g + compNode.h;

                            // Add it to the open list
                            open.Add(compNode);
                        }
                        else
                        {
                            // C) It is in the open list, compre its current G again its potential new G to see which is better(lower)
                            if (newG < compNode.g)
                            {
                                // the bew parent is a better parent, reset compNode's parent and G, F Costs to reflect this
                                compNode.parent = curNode;
                                compNode.g = newG;
                                compNode.f = compNode.g + compNode.h;
                            }
                        }
                    }
                }

                // 6. If the open list is empty, STOP!! (No path possible)
                if (open.Count == 0)
                {
                    break;
                }
            }

            // 7. If a path is found, retrace the steps starting at the end going through each parent until the start is reached
            if (ContainsNode(closed, end) != NOT_FOUND)
            {
                // If a path was found, trace it back from the end Node
                Node pathNode = end;

                // Keep tracking back until the start Node, which has no parent is reached
                while (pathNode != null)
                {
                    // Always add the next path Node to the front of the list to maintain order
                    result.Insert(0, pathNode);

                    // Get the next parent
                    pathNode = pathNode.parent;
                }
            }

            // 8. Return the resulting path
            return result;
        }

        /// <summary>
        /// Determine the H cost from the given grid coordinate to the target using Manhattan heuristic
        /// </summary>
        /// <param name="tileRow">The row of the current tile</param>
        /// <param name="tileCol">The column of the current tile</param>
        /// <param name="targetRow">The row of the target tile</param>
        /// <param name="targetCol">The column of the target tile</param>
        /// <returns>The cost to get from the current tile to the Target</returns>
        private float GetHCost(int tileRow, int tileCol, int targetRow, int targetCol)
        {
            // Using the Manhattan heuristic (cost to move from the current location to the target)
            // making only horizontal and vertical movement)
            return (float)Math.Abs(targetRow - tileRow) * hvCost + (float)Math.Abs(targetCol - tileCol) * hvCost;
        }

        /// <summary>
        /// Calculate the H cost of all tile to the target
        /// </summary>
        /// <param name="map">The collection of all the tiles in the game world</param>
        /// <param name="targetRow">The row of the target tile</param>
        /// <param name="targetCol">The column of the target tile</param>
        private void SetHCosts(Node[,] map, int targetRow, int targetCol)
        {
            // NOTE:
            // If the target moves, this method needs to be recalled at the beginning of each path finding check
            // If the target does not move, it only needs to be called once in the beginning after adjacenies are determined
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLUMNS; col++)
                {
                    // Calculate and set the cost for EACH tile to the end space
                    map[row, col].h = GetHCost(row + startRow, col + startCol, targetRow, targetCol);
                    map[row, col].f = map[row, col].g + map[row, col].h;
                }
            }
        }

        /// <summary>
        /// Calculate the cost from the starting location to the given tile
        /// </summary>
        /// <param name="compNode">The tile being tested</param>
        /// <param name="parentNode">The parent of the tile being tested</param>
        /// <returns>The cumulative cost to get from the starting tile to begin tested</returns>
        private float GetGCost(Node compNode, Node parentNode)
        {
            if (compNode.Row == parentNode.Row || compNode.Col == parentNode.Col)
            {
                // compNode is either directly horizontal or vertical to curNode
                return parentNode.g + hvCost * tileCosts[compNode.tileType];
            }
            else
            {
                // compNode is diagonal to curNode
                return parentNode.g + diagCost * tileCosts[compNode.tileType];
            }
        }

        /// <summary>
        /// Determine if a givan tile exists within a given collection of tiles
        /// </summary>
        /// <param name="nodeList">The collection to check within</param>
        /// <param name="checkNode">The tile to be searched for</param>
        /// <returns>True id checkNode is inside nodeList, False otherwise</returns>
        private int ContainsNode(List<Node> nodeList, Node checkNode)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                // If both Nodes have the same unique ID (Node number), they are a match
                if (nodeList[i].id == checkNode.id)
                {
                    // Node found in list, return the index
                    return i;
                }
            }

            // Node was not found in entire list, return invalid index
            return NOT_FOUND;
        }

        public List<Node> GetPath()
        {
            return path;
        }
    }
}
