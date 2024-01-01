using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Diagnostics;
using TiledSharp;

namespace Medicraft.Systems.TilemapRenderer
{
    public class TilemapIsometricRender
    {
        private TmxMap _tileMap;
        private Texture2D _tileSet;
        private int _tilesetTileWide;
        private int _tileWidth;
        private int _tileHeight;

        public TilemapIsometricRender(TmxMap _tileMap, Texture2D _tileSet)
        {
            this._tileMap = _tileMap;
            this._tileSet = _tileSet;
            _tileWidth = _tileMap.Tilesets[0].TileWidth;
            _tileHeight = _tileMap.Tilesets[0].TileHeight;
            _tilesetTileWide = _tileSet.Width / _tileWidth;

            SetObjectOnTile();
        }

        private void SetObjectOnTile()
        {
            GameGlobals.Instance.CollistionObject.Clear();
            GameGlobals.Instance.TopLayerObject.Clear();

            foreach (var o in _tileMap.ObjectGroups["Collision"].Objects)
            {
                GameGlobals.Instance.CollistionObject.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["ObjectLayer3"].Objects)
            {
                GameGlobals.Instance.TopLayerObject.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Tile Layer
            float layerDepthBack = 0.9f;
            float layerDepthFront = 0.5f;

            for (int i = 0; i < _tileMap.TileLayers.Count; i++)
            {
                for (int j = 0; j < _tileMap.TileLayers[i].Tiles.Count; j++)
                {
                    int gid = _tileMap.TileLayers[i].Tiles[j].Gid;
                    if (gid != 0)
                    {
                        int tileFrame = gid - 1;
                        int column = tileFrame % _tilesetTileWide;
                        int row = (int)Math.Floor(tileFrame / (double)_tilesetTileWide);

                        Rectangle tilesetRec = new Rectangle(_tileWidth * column, _tileHeight * row
                            , _tileWidth, _tileHeight);

                        int x = j % _tileMap.Width * (_tileMap.TileWidth / 2);
                        int y = (int)((int)Math.Floor((double)(j / _tileMap.Width)) * _tileMap.TileHeight / 1.25);

                        int isoX = x - y;
                        int isoY = (int)((x + y) / 1.75);

                        Rectangle tileRec = new Rectangle(isoX, isoY, _tileWidth, _tileHeight);

                        if (_tileMap.TileLayers[i].Name.Equals("Tile Layer 3"))
                        {
                            spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                                , Vector2.Zero, SpriteEffects.None, layerDepthFront);
                        }
                        else
                        {
                            spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                                , Vector2.Zero, SpriteEffects.None, layerDepthBack);
                        }

                        layerDepthBack -= 0.00001f;
                        layerDepthFront -= 0.00001f;
                    }
                }
            }
        }
    }
}
