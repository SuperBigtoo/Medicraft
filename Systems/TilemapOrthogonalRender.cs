using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public TilemapOrthogonalRender(TmxMap _tileMap, Texture2D _tileSet)
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
            Singleton.Instance.CollistionObject.Clear();
            Singleton.Instance.OnGroundObject.Clear();

            foreach (var o in _tileMap.ObjectGroups["Collision"].Objects)
            {
                Singleton.Instance.CollistionObject.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }

            foreach (var o in _tileMap.ObjectGroups["ObjectLayer3"].Objects)
            {
                Singleton.Instance.OnGroundObject.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
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
                        int row = (int)Math.Floor((double)tileFrame / (double)_tilesetTileWide);

                        Rectangle tilesetRec = new Rectangle(_tileWidth * column, _tileHeight * row
                            , _tileWidth, _tileHeight);

                        int x = (j % _tileMap.Width) * _tileMap.TileWidth;
                        int y = (int)Math.Floor((double)(j / _tileMap.Width)) * _tileMap.TileHeight;

                        Rectangle tileRec = new Rectangle(x, y, _tileWidth, _tileHeight);

                        if (_tileMap.TileLayers[i].Name.Equals("Tile Layer 3"))
                        {
                            spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                                , Vector2.Zero, SpriteEffects.None, layerDepthFront);
                            layerDepthFront -= 0.00001f;
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
    }
}
