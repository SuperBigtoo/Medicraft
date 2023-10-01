using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Diagnostics;
using TiledSharp;

namespace Medicraft.Systems
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
            Singleton.Instance.CollistionObject.Clear();
            for (int i = 0; i < _tileMap.TileLayers.Count; i++)
            {
                for (int j = 0; j < _tileMap.TileLayers[i].Tiles.Count; j++)
                {
                    int gid = _tileMap.TileLayers[i].Tiles[j].Gid;
                    if (gid != 0)
                    {
                        int tileFrame = gid - 1;

                        if (tileFrame == 1 || tileFrame == 36)
                        {
                            int x = ((j % _tileMap.Width) * (_tileMap.TileWidth / 2));
                            int y = (int)((int)Math.Floor((double)(j / _tileMap.Width)) * _tileMap.TileHeight);

                            int isoX = (x - y);
                            int isoY = (int)((x + y) / 2);

                            Debug.WriteLine($"isoX: {isoX}");
                            Debug.WriteLine($"isoY: {isoY}");

                            var point = new Point2(isoX + (_tileWidth / 2), isoY + (_tileHeight / 2));
                            var tileBound = new CircleF(point, _tileHeight);

                            //Singleton.Instance.ObjectOnTile.Add(tileBound, isoY + (_tileHeight / 2));
                            Singleton.Instance.tileHeight = _tileHeight;
                        }
                    }
                }
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

                        int x = ((j % _tileMap.Width) * (_tileMap.TileWidth / 2));
                        int y = (int)((int)Math.Floor((double)(j / _tileMap.Width)) * _tileMap.TileHeight / 1.25);

                        int isoX = (x - y);
                        int isoY = (int)((x + y) / 1.75);

                        Rectangle tileRec = new Rectangle(isoX, isoY, _tileWidth, _tileHeight);

                        // define layerDepth
                        //if (i == 1)
                        //{
                        //    int playerPosY = (int)((int)PlayerManager.Instance.player.Position.Y + (float)_tileHeight / 2);
                        //    int tilePosY = (isoY + (_tileHeight / 2));

                        //    if (tilePosY >= playerPosY)
                        //    {
                        //        // In front Player
                        //        spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                        //            , Vector2.Zero, SpriteEffects.None, layerDepth);
                        //    }
                        //    else
                        //    {   
                        //        // Behide Player
                        //        spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                        //            , Vector2.Zero, SpriteEffects.None, layerDepth);
                        //    }
                        //}
                        //else
                        //{
                        //    spriteBatch.Draw(_tileSet, tileRec, tilesetRec, Color.White, 0f
                        //        , Vector2.Zero, SpriteEffects.None, layerDepth);
                        //}

                        if (tileFrame == 1 || tileFrame == 36)
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
