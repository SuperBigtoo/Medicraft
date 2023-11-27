using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System;
using TiledSharp;

namespace Medicraft.Systems
{
    public class TilemapOrthogonalRender
    {
        private TmxMap _tileMap;
        private Texture2D _tileSet;
        private int _tilesetTileWide;
        private int _tileWidth;
        private int _tileHeight;

        // Maintain tile types by number representation
        private const int BLOCK = 0;
        private const int ROAD = 1;
        private const int START = 2;
        private const int END = 3;
        private const int BLANK = 4;

        public TilemapOrthogonalRender(TmxMap _tileMap, Texture2D _tileSet)
        {
            this._tileMap = _tileMap;
            this._tileSet = _tileSet;
            _tileWidth = _tileMap.Tilesets[0].TileWidth;
            _tileHeight = _tileMap.Tilesets[0].TileHeight;
            _tilesetTileWide = _tileSet.Width / _tileWidth;

            Initialize();
        }

        private void Initialize()
        {
            GameGlobals.Instance.CollistionObject.Clear();
            GameGlobals.Instance.ObjectOnLayer1.Clear();
            GameGlobals.Instance.ObjectOnLayer2.Clear();
            GameGlobals.Instance.TableCraft.Clear();

            foreach (var o in _tileMap.ObjectGroups["Collision"].Objects)
            {
                GameGlobals.Instance.CollistionObject.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["ObjectOnLayer1"].Objects)
            {
                GameGlobals.Instance.ObjectOnLayer1.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["ObjectOnLayer2"].Objects)
            {
                GameGlobals.Instance.ObjectOnLayer2.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["TableCraft"].Objects)
            {
                GameGlobals.Instance.TableCraft.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            MapArray();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Tile Layer
            float layerDepthBack = 0.9f;
            float layerDepthFront_1 = 0.3f;
            float layerDepthFront_2 = 0.5f;

            for (int i = 0; i < _tileMap.TileLayers.Count; i++)
            {
                for (int j = 0; j < _tileMap.TileLayers[i].Tiles.Count; j++)
                {
                    int gid = _tileMap.TileLayers[i].Tiles[j].Gid;
                    if (gid != 0)
                    {
                        int tileFrame = gid - 1;
                        int column = tileFrame % _tilesetTileWide;
                        int row = (int)Math.Floor((double)tileFrame / (double)_tilesetTileWide);

                        Rectangle tilesetRec = new Rectangle(_tileWidth * column, _tileHeight * row
                            , _tileWidth, _tileHeight);

                        int x = (j % _tileMap.Width) * _tileMap.TileWidth;
                        int y = (int)Math.Floor((double)(j / _tileMap.Width)) * _tileMap.TileHeight;

                        Rectangle tileRec = new Rectangle(x, y, _tileWidth, _tileHeight);

                        // Check layer depth here
                        if (_tileMap.TileLayers[i].Name.Equals("OnjectOnLayer1"))
                        {
                            spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                                , Vector2.Zero, SpriteEffects.None, layerDepthFront_1);
                            layerDepthFront_1 -= 0.00001f;
                        }
                        else if (_tileMap.TileLayers[i].Name.Equals("ObjectOnLayer2"))
                        {
                            spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                                , Vector2.Zero, SpriteEffects.None, layerDepthFront_2);
                            layerDepthFront_2 -= 0.00001f;
                        }
                        else
                        {
                            spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                                , Vector2.Zero, SpriteEffects.None, layerDepthBack);
                            layerDepthBack -= 0.00001f;
                        }
                    }
                }
            }
        }

        public void MapArray()
        {
            GameGlobals.Instance.TILE_SIZE = _tileWidth;
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
                        if (gid - 1 == 1765)
                        {
                            //System.Diagnostics.Debug.WriteLine($"Block {num}: {x} {y}");
                            GameGlobals.Instance.Map[y, x] = BLOCK;
                        }
                    }
                    else
                    {
                        GameGlobals.Instance.Map[y, x] = BLANK;
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

            if (_tileMap.TileLayers[1].Name.Equals("Tile Layer 1"))
            {
                for (int j = 0; j < _tileMap.TileLayers[1].Tiles.Count; j++)
                {
                    int gid = _tileMap.TileLayers[1].Tiles[j].Gid;
                    if (gid != 0)
                    {
                        if (GameGlobals.Instance.Map[y, x] != BLOCK)
                        {
                            GameGlobals.Instance.Map[y, x] = ROAD;
                        }
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
    }
}
