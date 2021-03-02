using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using Apos.Batch;
using Num = System.Numerics;
using System;
using FontStashSharp;
using MonoGame.Extended;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
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
            _s = new SpriteBatch(GraphicsDevice);
            _effect = Content.Load<Effect>("batch");
            _b = new Batch(GraphicsDevice, _effect);

            _random = new Random();

            _fps = new FPSCounter();

            _fontSystem = FontSystemFactory.Create(GraphicsDevice, 2048, 2048);
            _fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/source-code-pro-medium.ttf"));

            _font = _fontSystem.GetFont(30);
        }

        protected override void Update(GameTime gameTime) {
            InputHelper.UpdateSetup();
            _fps.Update(gameTime.ElapsedGameTime.Ticks);

            if (_quit.Pressed())
                Exit();

            // TODO: Add your update logic here

            InputHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            _fps.Draw();
            GraphicsDevice.Clear(Color.Black);

            _b.Begin();
            for (int i = 0; i < 10000; i++) {
                _b.Draw(_apos, Num.Matrix3x2.CreateTranslation(_random.Next(0, Window.ClientBounds.Width), _random.Next(0, Window.ClientBounds.Height)));
            }
            _b.End();

            string fps = $"fps: {_fps.FramesPerSecond} - Dropped Frames: {_fps.DroppedFrames} - Draw ms: {_fps.TimePerFrame} - Update ms: {_fps.TimePerUpdate}";
            Vector2 size = _font.MeasureString(fps, new Vector2(1));
            _s.Begin();
            _s.FillRectangle(new RectangleF(new Vector2(0), size + new Vector2(20)), Color.Black * 0.7f);
            _s.DrawString(_font, fps, new Vector2(10), Color.White);
            _s.End();

            base.Draw(gameTime);
        }

        GraphicsDeviceManager _graphics;
        Batch _b;
        SpriteBatch _s;
        Random _random;

        ICondition _quit =
            new AnyCondition(
                new KeyboardCondition(Keys.Escape),
                new GamePadCondition(GamePadButton.Back, 0)
            );

        Texture2D _apos;
        Effect _effect;

        FPSCounter _fps;

        FontSystem _fontSystem;
        DynamicSpriteFont _font;
    }
}
