using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Apos.Batch {
    public class Batch {
        public Batch(GraphicsDevice graphicsDevice, Effect effect) {
            _graphicsDevice = graphicsDevice;
            _defaultEffect = effect;
            _defaultPass = effect.CurrentTechnique.Passes[0];

            _vertices = new VertexPositionColorTexture[_initialVertices];
            _indices = new ushort[_initialIndices];

            GenerateIndexArray(ref _indices, 0);

            _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);

            _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(ushort), _indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indices);
        }

        // TODO: Shapes (filled, borders)
        //       Textures
        //       Shaders

        public void Begin(Matrix? view = null, Matrix? projection = null, Effect? effect = null) {
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

            if (projection != null) {
                _projection = projection.Value;
            } else {
                Viewport viewport = _graphicsDevice.Viewport;
                _projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            }
        }
        public void Draw(Texture2D texture, Num.Matrix3x2? world = null) {
            // TODO: A Texture swap means a batch Flush.
            if (_texture != texture) {
                _texture = texture;
                Flush();
            }

            // if (_vertexCount + 4 > _vertices.Length) {
            //     Flush();
            // }

            EnsureSizeOrDouble(ref _vertices, _vertexCount + 4);
            if (EnsureSizeOrDouble(ref _indices, _indexCount + 6)) {
                _fromIndex = _indexCount;
                _indicesChanged = true;
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

            _vertices[_vertexCount + 0] = new VertexPositionColorTexture(new Vector3(wTopLeft.X, wTopLeft.Y, 0f), Color.White, _topLeft);
            _vertices[_vertexCount + 1] = new VertexPositionColorTexture(new Vector3(wTopRight.X, wTopRight.Y, 0f), Color.White, _topRight);
            _vertices[_vertexCount + 2] = new VertexPositionColorTexture(new Vector3(wBottomRight.X, wBottomRight.Y, 0f), Color.White, _bottomRight);
            _vertices[_vertexCount + 3] = new VertexPositionColorTexture(new Vector3(wBottomLeft.X, wBottomLeft.Y, 0f), Color.White, _bottomLeft);

            _triangleCount += 2;
            _vertexCount += 4;
            _indexCount += 6;
        }
        public void End() {
            Flush();

            // TODO: Restore old states like rasterizer, depth stencil, blend state?
        }

        private void Flush() {
            if (_triangleCount == 0) return;

            _defaultEffect.Parameters["view_projection"]?.SetValue(_view * _projection);
            // Apply the default pass in case a custom shader doesn't provide a vertex shader.
            _defaultPass.Apply();

            if (_indicesChanged) {
                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();

                _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);

                GenerateIndexArray(ref _indices, _fromIndex);

                _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(ushort), _indices.Length, BufferUsage.WriteOnly);
                _indexBuffer.SetData(_indices);

                _indicesChanged = false;
            }

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

        private bool EnsureSizeOrDouble<T>(ref T[] array, int neededCapacity) {
            if (array.Length < neededCapacity) {
                Array.Resize(ref array, array.Length * 2);
                return true;
            }
            return false;
        }

        private void GenerateIndexArray(ref ushort[] array, int index = 0) {
            int i = Floor(index, 6, 6);
            int j = Floor(index, 6, 4);
            for (; i < array.Length; i += 6, j += 4) {
                array[i + 0] = (ushort) (j + 0);
                array[i + 1] = (ushort) (j + 1);
                array[i + 2] = (ushort) (j + 3);
                array[i + 3] = (ushort) (j + 1);
                array[i + 4] = (ushort) (j + 2);
                array[i + 5] = (ushort) (j + 3);
            }
        }
        private static int Floor(int value, int div, int mul) {
            return (int)MathF.Floor((float)value / div) * mul;
        }

        private int _initialSprites = 2048;
        private int _initialTriangles = 2048 * 2;
        private int _initialVertices = 2048 * 4;
        private int _initialIndices = 2048 * 6;

        private GraphicsDevice _graphicsDevice;
        private RasterizerState _rasterizerState = new RasterizerState {
            CullMode = CullMode.None
        };

        private VertexPositionColorTexture[] _vertices;
        private ushort[] _indices;
        private int _triangleCount = 0;
        private int _vertexCount = 0;
        private int _indexCount = 0;
        private Texture2D _texture;

        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        private Matrix _view;
        private Matrix _projection;
        private Effect _defaultEffect;
        private EffectPass _defaultPass;
        private Effect _effect;
        private bool _customEffect = false;

        private Vector2 _topLeft = new Vector2(0, 0);
        private Vector2 _topRight = new Vector2(1, 0);
        private Vector2 _bottomRight = new Vector2(1, 1);
        private Vector2 _bottomLeft = new Vector2(0, 1);

        private bool _indicesChanged = false;
        private int _fromIndex = 0;
    }
}
