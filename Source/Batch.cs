using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Apos.Batch {
    // TODO: Have the VertexBuffer and IndexBuffer auto resize.
    public class Batch {
        public Batch(GraphicsDevice graphicsDevice, Effect effect) {
            _graphicsDevice = graphicsDevice;
            _defaultEffect = effect;
            _defaultPass = effect.CurrentTechnique.Passes[0];
            _vertices = new VertexPositionColorTexture[MAX_VERTICES];
            _indices = GenerateIndexArray();

            _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertices);

            _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indices);
        }

        // TODO: Shapes (filled, borders)
        //       Textures
        //       Shaders

        public void Begin(Matrix? view = null, Effect? effect = null) {
            if (view != null) {
                _view = view.Value;
            } else {
                _view = Matrix.Identity;
            }
            if (effect != null) {
                _effect = effect;
                _customEffect = true;
            } else {
                _effect = _defaultEffect;
                _customEffect = false;
            }

            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;
            _projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
        }
        public void Draw(Texture2D texture, Num.Matrix3x2? world = null) {
            // TODO: A Texture swap means a batch Flush.
            if (_texture != texture) {
                _texture = texture;
                Flush();
            }

            // TODO: world shouldn't be null.
            if (world == null) {
                world = Num.Matrix3x2.Identity;
            }

            // TODO: Use a source rectangle to get the values.
            Num.Vector2 topLeft = new Num.Vector2(0, 0);
            Num.Vector2 topRight = new Num.Vector2(texture.Width, 0);
            Num.Vector2 bottomRight = new Num.Vector2(texture.Width, texture.Height);
            Num.Vector2 bottomLeft = new Num.Vector2(0, texture.Height);

            Num.Vector2 wTopLeft = Num.Vector2.Transform(topLeft, world.Value);
            Num.Vector2 wTopRight = Num.Vector2.Transform(topRight, world.Value);
            Num.Vector2 wBottomRight = Num.Vector2.Transform(bottomRight, world.Value);
            Num.Vector2 wBottomLeft = Num.Vector2.Transform(bottomLeft, world.Value);

            _vertices[_vertexCount + 0] = new VertexPositionColorTexture(new Vector3(wTopLeft.X, wTopLeft.Y, 0f), Color.White, GetUV(texture, topLeft));
            _vertices[_vertexCount + 1] = new VertexPositionColorTexture(new Vector3(wTopRight.X, wTopRight.Y, 0f), Color.White, GetUV(texture, topRight));
            _vertices[_vertexCount + 2] = new VertexPositionColorTexture(new Vector3(wBottomRight.X, wBottomRight.Y, 0f), Color.White, GetUV(texture, bottomRight));
            _vertices[_vertexCount + 3] = new VertexPositionColorTexture(new Vector3(wBottomLeft.X, wBottomLeft.Y, 0f), Color.White, GetUV(texture, bottomLeft));

            _triangleCount += 2;
            _vertexCount += 4;
            _indexCount += 6;

            if (_triangleCount >= MAX_TRIANGLES) {
                Flush();
            }
        }
        public void End() {
            Flush();
        }

        private void Flush() {
            if (_triangleCount == 0) return;

            _defaultEffect.Parameters["view_projection"]?.SetValue(_view * _projection);
            // Apply the default pass in case a custom shader doesn't provide a vertex shader.
            _defaultPass.Apply();

            _vertexBuffer.SetData(_vertices);
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            _graphicsDevice.RasterizerState = _rasterizerState;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;

            if (_customEffect) {
                foreach (EffectPass pass in _effect.CurrentTechnique.Passes) {
                    pass.Apply();
                    _graphicsDevice.Textures[0] = _texture;

                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
                }
            } else {
                _graphicsDevice.Textures[0] = _texture;
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
            }

            _triangleCount = 0;
            _vertexCount = 0;
            _indexCount = 0;
        }
        private Vector2 GetUV(Texture2D texture, Num.Vector2 xy) {
            return new Vector2(xy.X / texture.Width, xy.Y / texture.Height);
        }
        private static short[] GenerateIndexArray() {
            short[] result = new short[MAX_INDICES];
            for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4) {
                result[i + 0] = (short) (j + 0);
                result[i + 1] = (short) (j + 1);
                result[i + 2] = (short) (j + 3);
                result[i + 3] = (short) (j + 1);
                result[i + 4] = (short) (j + 2);
                result[i + 5] = (short) (j + 3);
            }
            return result;
        }

        const int MAX_SPRITES = 2048;
        const int MAX_TRIANGLES = 2048 * 2;
        const int MAX_VERTICES = 2048 * 4;
        const int MAX_INDICES = 2048 * 6;

        GraphicsDevice _graphicsDevice;
        RasterizerState _rasterizerState = new RasterizerState {
            CullMode = CullMode.None
        };

        VertexPositionColorTexture[] _vertices;
        short[] _indices;
        int _triangleCount = 0;
        int _vertexCount = 0;
        int _indexCount = 0;
        Texture2D _texture;

        DynamicVertexBuffer _vertexBuffer;
        IndexBuffer _indexBuffer;

        Matrix _view;
        Matrix _projection;
        Effect _defaultEffect;
        EffectPass _defaultPass;
        Effect _effect;
        bool _customEffect = false;
    }
}
