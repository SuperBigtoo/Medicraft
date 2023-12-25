using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TiledSharp;

namespace Medicraft.Systems
{
    public class TilemapOrthogonalRender
    {
        private readonly TmxMap _tileMap;
        private readonly Texture2D[] _tileSets;
        private readonly int[] _firstGid;
        private readonly int[] _tileWidth;
        private readonly int[] _tileHeight;
        private readonly int[] _tilesetTileWide;
        private readonly List<int> _tileAnimationsId;

        private float _totalMilliseconds = 0f;

        // Maintain tile types by number representation
        private const int BLOCK = 0;
        private const int ROAD = 1;
        private const int START = 2;
        private const int END = 3;
        private const int BLANK = 4;

        public TilemapOrthogonalRender(TmxMap tileMap, Texture2D[] tileSets)
        {
            _tileMap = tileMap;
            _tileSets = tileSets;

            _firstGid = new int[tileSets.Length];
            _tileWidth = new int[tileSets.Length];
            _tileHeight = new int[tileSets.Length];
            _tilesetTileWide = new int[tileSets.Length];
            _tileAnimationsId = new List<int>();

            Initialize();
        }

        private void Initialize()
        {
            SetTileSetsInfo();
            SetObjectGroups();
            MappingArray();
        }

        private void SetTileSetsInfo()
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
            
            //System.Diagnostics.Debug.WriteLine($"_tileAnimationsId Count: {_tileAnimationsId.Count}");
            //System.Diagnostics.Debug.WriteLine($"{_tileAnimationsId[0]}, {_tileAnimationsId[1]}");
        }

        private void SetObjectGroups()
        {
            GameGlobals.Instance.CollistionObject.Clear();
            GameGlobals.Instance.ObjectOnLayer1.Clear();
            GameGlobals.Instance.ObjectOnLayer2.Clear();
            GameGlobals.Instance.TableCraft.Clear();

            foreach (var o in _tileMap.ObjectGroups["Collision"].Objects)
            {
                GameGlobals.Instance.CollistionObject.Add(
                    new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["ObjectOnLayer1"].Objects)
            {
                GameGlobals.Instance.ObjectOnLayer1.Add(
                    new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["ObjectOnLayer2"].Objects)
            {
                GameGlobals.Instance.ObjectOnLayer2.Add(
                    new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["TableCraft"].Objects)
            {
                GameGlobals.Instance.TableCraft.Add(
                    new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }
        }

        private void MappingArray()
        {
            GameGlobals.Instance.TILE_SIZE = 32;    // In this game we using tile size 32*32
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
                        if (gid - 1 == 7050) GameGlobals.Instance.Map[y, x] = BLOCK;
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
            var layerDepthBack = 0.9f;
            var layerDepthFront_1 = 0.3f;
            var layerDepthFront_2 = 0.5f;

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
                                DrawTile(spriteBatch, gid, 0, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                break;

                            case 2:
                                if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                break;

                            case 3:
                                if (gid >= _firstGid[2])
                                {
                                    DrawTile(spriteBatch, gid, 2, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                break;

                            case 4:
                                if (gid >= _firstGid[3])
                                {
                                    DrawTile(spriteBatch, gid, 3, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else if (gid >= _firstGid[2])
                                {
                                    DrawTile(spriteBatch, gid, 2, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                break;

                            case 5:
                                if (gid >= _firstGid[4])
                                {
                                    DrawTile(spriteBatch, gid, 4, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else if (gid >= _firstGid[3])
                                {
                                    DrawTile(spriteBatch, gid, 3, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else if (gid >= _firstGid[2])
                                {
                                    DrawTile(spriteBatch, gid, 2, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else if (gid >= _firstGid[1])
                                {
                                    DrawTile(spriteBatch, gid, 1, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                else
                                {
                                    DrawTile(spriteBatch, gid, 0, i, j, layerDepthFront_1, layerDepthFront_2, layerDepthBack);
                                }
                                break;
                        }
                        layerDepthFront_1 -= 0.000001f;
                        layerDepthFront_2 -= 0.000001f;
                        layerDepthBack -= 0.000001f;
                    }
                }
            }
        }

        private void DrawTile(SpriteBatch spriteBatch, int gid, int index, int i, int j
            , float layerDepthFront_1, float layerDepthFront_2, float layerDepthBack)
        {;
            var tilesetRec = Rectangle.Empty;

            // Check if the tile is animated
            if (_tileAnimationsId.Contains(gid))
            {
                int frameIndex = GetCurrentAnimationFrame(gid - 1);
                int column = frameIndex % _tilesetTileWide[index];
                int row = (int)Math.Floor((double)frameIndex / (double)_tilesetTileWide[index]);

                tilesetRec = new Rectangle(_tileWidth[index] * column, _tileHeight[index] * row
                    , _tileWidth[index], _tileHeight[index]);
            }
            else
            {
                // If not animated den do dis
                int tileFrame = gid - _firstGid[index];
                int column = tileFrame % _tilesetTileWide[index];
                int row = (int)Math.Floor((double)tileFrame / (double)_tilesetTileWide[index]);

                tilesetRec = new Rectangle(_tileWidth[index] * column, _tileHeight[index] * row
                    , _tileWidth[index], _tileHeight[index]);
            }

            int x = (j % _tileMap.Width) * _tileMap.TileWidth;
            int y = (int)Math.Floor((double)(j / _tileMap.Width)) * _tileMap.TileHeight;

            var tileRec = new Rectangle(x, y, _tileWidth[index], _tileHeight[index]);

            if (_tileMap.TileLayers[i].Name.Equals("OnjectOnLayer1"))
            {
                spriteBatch.Draw(_tileSets[index], tileRec, tilesetRec, Color.White, 0f
                    , Vector2.Zero, SpriteEffects.None, layerDepthFront_1);
            }
            else if (_tileMap.TileLayers[i].Name.Equals("ObjectOnLayer2"))
            {
                spriteBatch.Draw(_tileSets[index], tileRec, tilesetRec, Color.White, 0f
                    , Vector2.Zero, SpriteEffects.None, layerDepthFront_2);
            }
            else
            {
                spriteBatch.Draw(_tileSets[index], tileRec, tilesetRec, Color.White, 0f
                    , Vector2.Zero, SpriteEffects.None, layerDepthBack);
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
