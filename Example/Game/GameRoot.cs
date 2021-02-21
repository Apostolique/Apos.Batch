using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using Apos.Batch;
using Num = System.Numerics;
using System;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            Window.AllowUserResizing = true;

            base.Initialize();
        }

        protected override void LoadContent() {

            InputHelper.Setup(this);

            _apos = Content.Load<Texture2D>("apos");
            _effect = Content.Load<Effect>("batch");
            _b = new Batch(GraphicsDevice, _effect);

            _random = new Random();
        }

        protected override void Update(GameTime gameTime) {
            InputHelper.UpdateSetup();

            if (_quit.Pressed())
                Exit();

            // TODO: Add your update logic here

            InputHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            _b.Begin();
            for (int i = 0; i < 1000; i++) {
                _b.Draw(_apos, Num.Matrix3x2.CreateTranslation(_random.Next(0, Window.ClientBounds.Width), _random.Next(0, Window.ClientBounds.Height)));
            }
            _b.End();

            base.Draw(gameTime);
        }

        GraphicsDeviceManager _graphics;
        Batch _b;
        Random _random;

        ICondition _quit =
            new AnyCondition(
                new KeyboardCondition(Keys.Escape),
                new GamePadCondition(GamePadButton.Back, 0)
            );

        Texture2D _apos;
        Effect _effect;
    }
}
