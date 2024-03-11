using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using TiledSharp;

namespace Medicraft.Systems.TilemapRenderer
{
    public class TilemapOrthogonalRender
    {
        private readonly TmxMap _tileMap;
        private readonly Texture2D[] _tileSets;
        private readonly int _tileSize;

        private readonly int[] _firstGid;
        private readonly int[] _tileWidth;
        private readonly int[] _tileHeight;
        private readonly int[] _tilesetTileWide;
        private readonly List<int> _tileAnimationsId;

        // For calcurlate the frame rate of tile animation
        private float _totalMilliseconds = 0f;

        // Maintain tile types by number representation
        private const int BLOCK = 0;
        private const int ROAD = 1;
        private const int START = 2;
        private const int END = 3;
        private const int BLANK = 4;

        private const int BLOCK_ID = 7050;

        public TilemapOrthogonalRender(TmxMap tileMap, Texture2D[] tileSets, int tileSize)
        {
            _tileMap = tileMap;
            _tileSets = tileSets;
            _tileSize = tileSize;

            _firstGid = new int[tileSets.Length];
            _tileWidth = new int[tileSets.Length];
            _tileHeight = new int[tileSets.Length];
            _tilesetTileWide = new int[tileSets.Length];
            _tileAnimationsId = [];

            // Initialize
            InitializeTileSetsInfo();
            InitializeObjectGroup();
            MappingArray();
        }

        private void InitializeTileSetsInfo()
        {
            for (int i = 0; i < _tileSets.Length; i++)
            {
                _firstGid[i] = _tileMap.Tilesets[i].FirstGid;
                _tileWidth[i] = _tileMap.Tilesets[i].TileWidth;
                _tileHeight[i] = _tileMap.Tilesets[i].TileHeight;
                _tilesetTileWide[i] = _tileSets[i].Width / _tileWidth[i];

                // Find AnimationFrames if it has
                foreach (var tile in _tileMap.Tilesets[i].Tiles.Values)
                {
                    if (tile.AnimationFrames != null)
                    {
                        _tileAnimationsId.Add(tile.Id + 1);
                    }
                }
            }
        }

        private void InitializeObjectGroup()
        {
            GameGlobals.Instance.CollistionObject.Clear();
            GameGlobals.Instance.TopLayerObject.Clear();
            GameGlobals.Instance.MiddleLayerObject.Clear();
            GameGlobals.Instance.BottomLayerObject.Clear();
            GameGlobals.Instance.CraftingTableArea.Clear();
            GameGlobals.Instance.SavingTableArea.Clear();
            GameGlobals.Instance.EnteringZoneArea.Clear();
            GameGlobals.Instance.MobPartrolArea.Clear();

            foreach (var o in _tileMap.ObjectGroups["Collision"]?.Objects)
            {
                GameGlobals.Instance.CollistionObject.Add(
                    new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["TopLayerObject"]?.Objects)
            {
                GameGlobals.Instance.TopLayerObject.Add(
                    new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["MiddleLayerObject"]?.Objects)
            {
                GameGlobals.Instance.MiddleLayerObject.Add(
                    new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["BottomLayerObject"]?.Objects)
            {
                GameGlobals.Instance.BottomLayerObject.Add(
                    new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["CraftingTableArea"]?.Objects)
            {
                GameGlobals.Instance.CraftingTableArea.Add(
                    new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["SavingTableArea"]?.Objects)
            {
                GameGlobals.Instance.SavingTableArea.Add(
                    new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["EnteringZoneArea"]?.Objects)
            {
                GameGlobals.Instance.EnteringZoneArea.Add(new AreaData()
                {
                    Name = o.Name,
                    Bounds = new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height)
                });
            }

            foreach (var o in _tileMap.ObjectGroups["MobPartrolArea"]?.Objects)
            {
                GameGlobals.Instance.MobPartrolArea.Add(new AreaData() 
                {
                    Name = o.Name,
                    Bounds = new RectangleF((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height)
                });
            }
        }

        private void MappingArray()
        {
            GameGlobals.Instance.TILE_SIZE = _tileSize;             // In this game we using tile size 32*32
            GameGlobals.Instance.NUM_ROWS = _tileMap.Height;
            GameGlobals.Instance.NUM_COLUMNS = _tileMap.Width;
            GameGlobals.Instance.Map = new int[_tileMap.Height, _tileMap.Width];

            int x = 0;
            int y = 0;

            if (_tileMap.TileLayers[0].Name.Equals("Block"))
            {
                for (int j = 0; j < _tileMap.TileLayers[0].Tiles.Count; j++)
                {
                    int gid = _tileMap.TileLayers[0].Tiles[j].Gid;
                    if (gid != 0)
                    {
                        if (gid - 1 == BLOCK_ID) GameGlobals.Instance.Map[y, x] = BLOCK;
                    }
                    else GameGlobals.Instance.Map[y, x] = BLANK;

                    if (x == _tileMap.Width - 1)
                    {
                        x = 0;
                        y++;
                        if (y == _tileMap.Height) y = 0;
                    }
                    else x++;
                }
            }

            if (_tileMap.TileLayers[1].Name.Equals("Tile Layer 1"))
            {
                for (int j = 0; j < _tileMap.TileLayers[1].Tiles.Count; j++)
                {
                    int gid = _tileMap.TileLayers[1].Tiles[j].Gid;
                    if (gid != 0)
                    {
                        if (GameGlobals.Instance.Map[y, x] != BLOCK) GameGlobals.Instance.Map[y, x] = ROAD;
                    }

                    if (x == _tileMap.Width - 1)
                    {
                        x = 0;
                        y++;
                        if (y == _tileMap.Height) y = 0;
                    }
                    else x++;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            _totalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Tile Layer
            var layerDepthBackground = GameGlobals.Instance.BackgroundDepth;
            var topLayerDepth = GameGlobals.Instance.TopObjectDepth;
            var middleLayerDepth = GameGlobals.Instance.MiddleObjectDepth;
            var bottomLayerDepth = GameGlobals.Instance.BottomObjectDepth;

            for (int i = 0; i < _tileMap.TileLayers.Count; i++)
            {
                for (int j = 0; j < _tileMap.TileLayers[i].Tiles.Count; j++)
                {
                    int gid = _tileMap.TileLayers[i].Tiles[j].Gid;
                    if (gid != 0)
                    {
                        switch (_tileSets.Length)
                        {
                            case 1:
                                DrawTile(spriteBatch, gid, 0, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                break;

                            case 2:
                                if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                break;

                            case 3:
                                if (gid >= _firstGid[2])
                                {
                                    DrawTile(spriteBatch, gid, 2, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                break;

                            case 4:
                                if (gid >= _firstGid[3])
                                {
                                    DrawTile(spriteBatch, gid, 3, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else if (gid >= _firstGid[2])
                                {
                                    DrawTile(spriteBatch, gid, 2, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                break;

                            case 5:
                                if (gid >= _firstGid[4])
                                {
                                    DrawTile(spriteBatch, gid, 4, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else if (gid >= _firstGid[3])
                                {
                                    DrawTile(spriteBatch, gid, 3, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else if (gid >= _firstGid[2])
                                {
                                    DrawTile(spriteBatch, gid, 2, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, topLayerDepth, middleLayerDepth, bottomLayerDepth, layerDepthBackground);
                                }
                                break;
                        }
                        topLayerDepth -= 0.000001f;
                        middleLayerDepth -= 0.000001f;
                        bottomLayerDepth -= 0.000001f;
                        layerDepthBackground -= 0.000001f;
                    }
                }
            }
        }

        private void DrawTile(SpriteBatch spriteBatch, int gid, int index, int i, int j
            , float topLayerDepth, float middleLayerDepth, float bottomLayerDepth, float layerDepthBackground)
        {
            Rectangle tilesetRec = Rectangle.Empty;

            // Check if the tile is animated
            if (_tileAnimationsId.Contains(gid))
            {
                int frameIndex = GetCurrentAnimationFrame(gid - 1);
                int column = frameIndex % _tilesetTileWide[index];
                int row = (int)Math.Floor(frameIndex / (double)_tilesetTileWide[index]);

                tilesetRec = new Rectangle(_tileWidth[index] * column, _tileHeight[index] * row
                    , _tileWidth[index], _tileHeight[index]);
            }
            else
            {
                // If not animated den do dis
                int tileFrame = gid - _firstGid[index];
                int column = tileFrame % _tilesetTileWide[index];
                int row = (int)Math.Floor(tileFrame / (double)_tilesetTileWide[index]);

                tilesetRec = new Rectangle(_tileWidth[index] * column, _tileHeight[index] * row
                    , _tileWidth[index], _tileHeight[index]);
            }

            int x = j % _tileMap.Width * _tileMap.TileWidth;
            int y = (int)Math.Floor((double)(j / _tileMap.Width)) * _tileMap.TileHeight;

            var tileRec = new Rectangle(x, y, _tileWidth[index], _tileHeight[index]);

            // Check Rendering if x y is far from Player position less den Screen width*1.25 and height*1.25
            var playerPos = PlayerManager.Instance.Player.Position;
            var lengthX = MathF.Abs(x - playerPos.X);
            var lengthY = MathF.Abs(y - playerPos.Y);
            var gameScreenX = ScreenManager.Instance.GraphicsDevice.Viewport.Width * 1.25;
            var gameScreenY = ScreenManager.Instance.GraphicsDevice.Viewport.Height * 1.25;

            if (lengthX <= gameScreenX && lengthY <= gameScreenY)
            {
                if (_tileMap.TileLayers[i].Name.Equals("TopLayerObject"))
                {
                    spriteBatch.Draw(_tileSets[index], tileRec, tilesetRec, Color.White, 0f
                        , Vector2.Zero, SpriteEffects.None, topLayerDepth);
                }
                else if (_tileMap.TileLayers[i].Name.Equals("MiddleLayerObject"))
                {
                    spriteBatch.Draw(_tileSets[index], tileRec, tilesetRec, Color.White, 0f
                        , Vector2.Zero, SpriteEffects.None, middleLayerDepth);
                }
                else if (_tileMap.TileLayers[i].Name.Equals("BottomLayerObject"))
                {
                    spriteBatch.Draw(_tileSets[index], tileRec, tilesetRec, Color.White, 0f
                        , Vector2.Zero, SpriteEffects.None, bottomLayerDepth);
                }
                else
                {
                    spriteBatch.Draw(_tileSets[index], tileRec, tilesetRec, Color.White, 0f
                        , Vector2.Zero, SpriteEffects.None, layerDepthBackground);
                }
            }
        }

        private int GetCurrentAnimationFrame(int gid)
        {
            // Retrieve the animation frames for the tile
            var frames = _tileMap.Tilesets
                .SelectMany(ts => ts.Tiles.Values)
                .First(tile => tile.Id == gid).AnimationFrames;

            // Calculate the current frame based on time or any other criteria
            // Fixed frame rate and loop through frames
            int totalFrames = frames.Count;

            // Calculate total duration in milliseconds
            int totalDuration = frames.Sum(frame => frame.Duration) / totalFrames;

            int frameIndex = (int)(_totalMilliseconds / totalDuration) % totalFrames;

            //System.Diagnostics.Debug.WriteLine($"totalFrames : {(int)(_totalMilliseconds / totalDuration) % totalFrames}");

            return frames[frameIndex].Id;
        }
    }
}
