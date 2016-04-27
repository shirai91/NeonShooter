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
        BloomComponent bloom;
        RenderTarget2D renderTarget2D;
        public GameRoot()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            bloom = new BloomComponent(this);
            Components.Add(bloom);
            graphics.PreferredBackBufferWidth = 1350;
            graphics.PreferredBackBufferHeight = 730;
            bloom.Settings = BloomSettings.PresetSettings[2];
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Art.Load(Content);
            Sound.Load(Content);
            ParticleManager = new ParticleManager<ParticleState>(1024*20,ParticleState.UpdateParticle);
            EntityManager.Add(PlayerShip.Instance);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Sound.Music);
            renderTarget2D = new RenderTarget2D(graphics.GraphicsDevice, Viewport.Width, Viewport.Height, false, graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Sound.Music);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            bloom.LoadContent(graphics.GraphicsDevice,Content);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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
            bloom.BeginDraw(renderTarget2D);
            bloom.ShowBuffer = BloomComponent.IntermediateBuffer.FinalResult;
            bloom.Draw(gameTime, renderTarget2D);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.Additive);
            ParticleManager.Draw(spriteBatch);
            EntityManager.Draw(spriteBatch);
            spriteBatch.DrawString(Art.Font, "Lives: " + PlayerStatus.Lives, new Vector2(5), Color.White);
            DrawRightAlignedString("Score: " + PlayerStatus.Score, 5);
            DrawRightAlignedString("Multiplier: " + PlayerStatus.Multiplier, 35);
            spriteBatch.Draw(Art.Pointer,Input.MousePosition,Color.White);
            if (GameManager.IsPausedWhenGameOver)
            {
                var text = "Game Over\n" +
                           "Your Score: " + PlayerStatus.Score + "\n" +
                           "High Score: " + PlayerStatus.HighScore + "\n" +
                           $"Restart in {GameManager.PauseFrame/60+1:D1}";

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
