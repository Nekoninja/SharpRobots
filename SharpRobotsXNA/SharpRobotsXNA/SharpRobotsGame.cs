using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpRobotsEngine;

namespace SharpRobotsXNA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SharpRobotsGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly BattleEngine _battleEngine;
        private int _currentWidth = 1024;
        private int _currentHeight = 768;
        private double _scaleX;
        private double _scaleY;
        readonly List<Texture2D> _tileTextures = new List<Texture2D>();

        // Keyboard states used to determine key presses
        KeyboardState _currentKeyboardState;
        KeyboardState _previousKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState _currentGamePadState;
        GamePadState _previousGamePadState;

#if WINDOWS
        // Mouse states
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
#endif

        /// <summary>
        /// 
        /// </summary>
        public SharpRobotsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _battleEngine = new BattleEngine();
            Content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = _currentWidth;
            _graphics.PreferredBackBufferHeight = _currentHeight;
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

            _graphics.ToggleFullScreen();
            _battleEngine.Load(new[] { "ExampleScripts\\LRBot.cs", "ExampleScripts\\ExampleBot.cs", "ExampleScripts\\LRBot.cs" });

            // Determine proper scaling ratio from the robots world coordinates to the screen coordinates
            _scaleX = (double)_graphics.GraphicsDevice.Viewport.Width / Arena.ArenaWidth;
            _scaleY = (double)_graphics.GraphicsDevice.Viewport.Height / Arena.ArenaHeight;

            // TODO Log settings

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D texture = Content.Load<Texture2D>("robot"); 
            _tileTextures.Add(texture);
            _tileTextures.Add(texture);
            _tileTextures.Add(texture);
            _tileTextures.Add(texture);

            texture = Content.Load<Texture2D>("missile");
            _tileTextures.Add(texture);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            // Save the previous state of the keyboard and game pad so we can determine single key/button presses
            _previousGamePadState = _currentGamePadState;
            _previousKeyboardState = _currentKeyboardState;

            // Read the current state of the keyboard, gamepad, mouse and store it
            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);

#if WINDOWS
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
#endif

            // Exit
            if (_currentKeyboardState.IsKeyDown(Keys.Q) ||
                _currentKeyboardState.IsKeyDown(Keys.Escape) || 
                _currentGamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            if (!_battleEngine.Execute())
            {
                // Display winner
                //return;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            int botNum = 0;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            {
                foreach (var missile in _battleEngine.Missiles)
                {
                    int x = (int)(missile.Location.X * _scaleX);
                    int y = (int)(missile.Location.Y * _scaleY);

                    if (missile.Dead) continue;
                    _spriteBatch.Draw(_tileTextures[4], new Rectangle(x, y, 4, 4), Color.White);
                }

                foreach (var bot in _battleEngine.Bots)
                {
                    int x = (int) (bot.Location.X * _scaleX);
                    int y = (int) (bot.Location.Y * _scaleY);
                    _spriteBatch.Draw(_tileTextures[botNum], new Rectangle(x, y, 32, 32), Color.White);
                    botNum++;
                }
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
