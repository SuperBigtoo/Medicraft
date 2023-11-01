using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace Medicraft.Systems
{
    public class TiledmapBGExtended
    {
        private TiledMap _tiledMap;
        private TiledMapRenderer _tiledMapRenderer;
        private TiledMapLayer _tiledMapLayer;

        private OrthographicCamera _orthographicCamera;
        private GraphicsDevice GraphicsDevice;

        public TiledmapBGExtended(ContentManager Content, GraphicsDevice GraphicsDevice, GameWindow Window, string PATH)
        {
            this.GraphicsDevice = GraphicsDevice;

            _tiledMap = Content.Load<TiledMap>(PATH);
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);
            _tiledMapLayer = _tiledMap.GetLayer<TiledMapLayer>("Tiles2");

            var viewportadapter  = new BoxingViewportAdapter(Window, GraphicsDevice
                , (int)GameGlobals.Instance.gameScreen.X, (int)GameGlobals.Instance.gameScreen.Y);
            _orthographicCamera = new OrthographicCamera(viewportadapter);
        }

        public void Dispose()
        {
            _tiledMapRenderer?.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            //var playerPos = PlayerManager.Instance.player.Position;
            //ushort TileX = (ushort)(playerPos.X / _tiledMap.TileWidth);
            //ushort TileY = (ushort)(playerPos.Y / _tiledMap.TileHeight);

            //for (int i = 0; i < _tiledMapTileLayer.TileWidth; i++)
            //{
            //    for (int j = 0; j < _tiledMapTileLayer.TileHeight; j++)
            //    {
            //        var tile = _tiledMapTileLayer.GetTile((ushort)i, (ushort)j);

            //        if (!tile.IsBlank)
            //        {
            //            int id = tile.GlobalIdentifier;
            //        }
            //    }
            //}

            //EntityManager.Instance.Update(gameTime);

            // Camera
            _orthographicCamera.LookAt((GameGlobals.Instance.cameraPosition + GameGlobals.Instance.addingCameraPos)
                + new Vector2(0, _tiledMap.HeightInPixels + _tiledMap.TileHeight) * 0.5f);
            _tiledMapRenderer.Update(gameTime);
        }

        public void Draw()
        {
            var viewMatrix = _orthographicCamera.GetViewMatrix();
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width
                , GraphicsDevice.Viewport.Height, 0, 0f, -1f);
            _tiledMapRenderer.Draw(ref viewMatrix, ref projectionMatrix, depth:0.1f);

            //foreach (TiledMapTileLayer layer in _tiledMap.Layers.Cast<TiledMapTileLayer>())
            //{
            //    _tiledMapRenderer.Draw(layer, viewMatrix, projectionMatrix, null, 0f);

            //    if (layer.Name == "Tiles1")
            //    {
            //        _hudSystem.DrawTest(spriteBatch);

            //        EntityManager.Instance.Draw(spriteBatch);

            //        spriteBatch.End();
            //        spriteBatch.Begin();
                    
            //    }               
            //}
        }
    }
}
