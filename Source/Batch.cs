using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Apos.Batch {
    public class Batch {
        public Batch(GraphicsDevice graphicsDevice, Effect effect) {
            _graphicsDevice = graphicsDevice;
            _effect = effect;
            _vertices = new VertexPositionColorTexture[MAX_VERTICES];
            _indices = GenerateIndexArray();

            _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertices);

            _indexBuffer = new DynamicIndexBuffer(_graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indices);
        }

        // TODO: Shapes (filled, borders)
        //       Textures
        //       Shaders

        public void Begin(Matrix? view = null) {
            if (view != null) {
                _view = view.Value;
            } else {
                _view = Matrix.Identity;
            }

            ResetProjection();
        }
        public void Draw(Texture2D texture, Num.Matrix3x2? world = null) {
            // TODO: A Texture swap means a batch Flush.
            _texture = texture;

            // TODO: world shouldn't be null.
            if (world != null) {
                world = Num.Matrix3x2.CreateScale(texture.Width, texture.Height) * world.Value;
            } else {
                world = Num.Matrix3x2.CreateScale(texture.Width, texture.Height);
            }

            Num.Vector2 topLeft = Num.Vector2.Transform(new Num.Vector2(0, 0), world.Value);
            Num.Vector2 topRight = Num.Vector2.Transform(new Num.Vector2(1, 0), world.Value);
            Num.Vector2 bottomRight = Num.Vector2.Transform(new Num.Vector2(1, 1), world.Value);
            Num.Vector2 bottomLeft = Num.Vector2.Transform(new Num.Vector2(0, 1), world.Value);

            _vertices[_vertexCount + 0] = new VertexPositionColorTexture(new Vector3(topLeft.X, topLeft.Y, 0f), Color.White, new Vector2(0, 0));
            _vertices[_vertexCount + 1] = new VertexPositionColorTexture(new Vector3(topRight.X, topRight.Y, 0f), Color.White, new Vector2(1, 0));
            _vertices[_vertexCount + 2] = new VertexPositionColorTexture(new Vector3(bottomRight.X, bottomRight.Y, 0f), Color.White, new Vector2(1, 1));
            _vertices[_vertexCount + 3] = new VertexPositionColorTexture(new Vector3(bottomLeft.X, bottomLeft.Y, 0f), Color.White, new Vector2(0, 1));

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

            _vertexBuffer.SetData(_vertices);
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            _graphicsDevice.RasterizerState = _rasterizerState;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;

            _effect.Parameters["view_projection"].SetValue(_view * _projection);
            _graphicsDevice.Textures[0] = _texture;

            foreach (EffectPass pass in _effect.CurrentTechnique.Passes) {
                pass.Apply();
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
            }

            _triangleCount = 0;
            _vertexCount = 0;
            _indexCount = 0;
        }
        private Vector2 GetUV(Texture2D texture, float x, float y) {
            return new Vector2(x / texture.Width, y / texture.Height);
        }
        private void ResetProjection() {
            int width = _graphicsDevice.Viewport.Width;
            int height = _graphicsDevice.Viewport.Height;
            _projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
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
        DynamicIndexBuffer _indexBuffer;

        Matrix _view;
        Matrix _projection;
        Effect _effect;
    }
}
