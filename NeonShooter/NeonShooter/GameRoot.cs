using System;
using BloomPostprocess;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace NeonShooter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameRoot : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static GameRoot Instance { get; private set; }
        public static Viewport Viewport => Instance.GraphicsDevice.Viewport;
        public static Vector2 ScreenSize => new Vector2(Viewport.Width, Viewport.Height);
        public static GameTime GameTime => new GameTime();
        public static ParticleManager<ParticleState> ParticleManager { get; private set; }
        public static WarpingGrid WarpingGrid { get; private set; }
        private RenderTarget2D _renderTarget1, _renderTarget2;
        private Bloom bloom;
        public GameRoot()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1350;
            graphics.PreferredBackBufferHeight = 730;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            ParticleManager = new ParticleManager<ParticleState>(1024 * 20, ParticleState.UpdateParticle);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Sound.Music);
            EntityManager.Add(PlayerShip.Instance);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Sound.Music);
            const int maxGridPoints = 1600;
            Vector2 gridSpacing = new Vector2((float)Math.Sqrt(Viewport.Width * Viewport.Height / maxGridPoints));
            WarpingGrid = new WarpingGrid(Viewport.Bounds, gridSpacing);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Art.Load(Content);
            Sound.Load(Content);
            _renderTarget1 = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);
            _renderTarget2 = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat, GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);
            bloom = new Bloom(GraphicsDevice, spriteBatch) {Settings = BloomSettings.PresetSettings[1]};
            bloom.LoadContent(Content, GraphicsDevice.PresentationParameters);
            bloom.Settings.BloomSaturation = 0.5f;
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            bloom.UnloadContent();
            _renderTarget1.Dispose();
            _renderTarget2.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            EntityManager.Update();
            Input.Update();
            ParticleManager.Update();
            WarpingGrid.Update();
            GameManager.Update();
            EnemySpawner.Update();
            PlayerStatus.Update();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget1);
            GraphicsDevice.Clear(Color.Transparent);
            //Draw all game object to render target here
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.Additive);
            WarpingGrid.Draw(spriteBatch);
            ParticleManager.Draw(spriteBatch);
            EntityManager.Draw(spriteBatch);
            spriteBatch.End();
            bloom.Draw(_renderTarget1, _renderTarget2);
            GraphicsDevice.SetRenderTarget(null);
            //Draw post bloom here
            spriteBatch.Begin(0, BlendState.AlphaBlend);
            spriteBatch.Draw(_renderTarget2, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White); // draw all glowing components            
            spriteBatch.End();
            //Draw all UI Component here
            spriteBatch.Begin();
            spriteBatch.DrawString(Art.Font, "Lives: " + PlayerStatus.Lives, new Vector2(5), Color.White);
            DrawRightAlignedString("Score: " + PlayerStatus.Score, 5);
            DrawRightAlignedString("Multiplier: " + PlayerStatus.Multiplier, 35);
            spriteBatch.Draw(Art.Pointer, Input.MousePosition, Color.White);
            if (GameManager.IsPausedWhenGameOver)
            {
                var text = "Game Over\n" +
                           "Your Score: " + PlayerStatus.Score + "\n" +
                           "High Score: " + PlayerStatus.HighScore + "\n" +
                           $"Restart in {GameManager.PauseFrame / 60 + 1:D1}";

                var textSize = Art.Font.MeasureString(text);
                spriteBatch.DrawString(Art.Font, text, ScreenSize / 2 - textSize / 2, Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);

        }
        private void DrawRightAlignedString(string text, float y)
        {
            var textWidth = Art.Font.MeasureString(text).X;
            spriteBatch.DrawString(Art.Font, text, new Vector2(ScreenSize.X - textWidth - 5, y), Color.White);
        }
    }
}
