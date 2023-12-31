using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Content;

namespace Apos.Batch {
    public class Batch {
        public Batch(GraphicsDevice graphicsDevice, ContentManager content) {
            _graphicsDevice = graphicsDevice;

            _effect = content.Load<Effect>("apos-batch");

            _defaultEffect = _effect;
            _defaultPass = _effect.CurrentTechnique.Passes[0];

            _vertices = new VertexPositionColorTexture[_initialVertices];
            _indices = new uint[_initialIndices];

            GenerateIndexArray();

            _vertexBuffer = new DynamicVertexBuffer(_graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Length, BufferUsage.WriteOnly);

            _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(uint), _indices.Length, BufferUsage.WriteOnly);
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
                _projection = Matrix.CreateOrthographicOffCenter(viewport.X, viewport.Width, viewport.Height, viewport.Y, 0, 1);
            }
        }
        public void Draw(Texture2D texture, Num.Matrix3x2? world = null, Num.Matrix3x2? source = null, Color? color = null) {
            if (_texture != texture) {
                _texture = texture;
                Flush();
            }

            EnsureSizeOrDouble(ref _vertices, _vertexCount + 4);
            _indicesChanged = EnsureSizeOrDouble(ref _indices, _indexCount + 6) || _indicesChanged;

            // TODO: world shouldn't be null.
            if (world == null) {
                world = Num.Matrix3x2.Identity;
            }

            Num.Vector2 topLeft;
            Num.Vector2 topRight;
            Num.Vector2 bottomRight;
            Num.Vector2 bottomLeft;
            if (source == null) {
                topLeft = new Num.Vector2(0, 0);
                topRight = new Num.Vector2(texture.Width, 0);
                bottomRight = new Num.Vector2(texture.Width, texture.Height);
                bottomLeft = new Num.Vector2(0, texture.Height);
            } else {
                topLeft = Num.Vector2.Transform(new Num.Vector2(0f, 0f), source.Value);
                topRight = Num.Vector2.Transform(new Num.Vector2(1f, 0f), source.Value);
                bottomRight = Num.Vector2.Transform(new Num.Vector2(1f, 1f), source.Value);
                bottomLeft = Num.Vector2.Transform(new Num.Vector2(0, 1f), source.Value);
            }

            Num.Vector2 wTopLeft = Num.Vector2.Transform(topLeft, world.Value);
            Num.Vector2 wTopRight = Num.Vector2.Transform(topRight, world.Value);
            Num.Vector2 wBottomRight = Num.Vector2.Transform(bottomRight, world.Value);
            Num.Vector2 wBottomLeft = Num.Vector2.Transform(bottomLeft, world.Value);

            _vertices[_vertexCount + 0] = new VertexPositionColorTexture(
                new Vector3(wTopLeft.X, wTopLeft.Y, 0f),
                color ?? Color.White,
                GetUV(texture, topLeft)
            );
            _vertices[_vertexCount + 1] = new VertexPositionColorTexture(
                new Vector3(wTopRight.X, wTopRight.Y, 0f),
                color ?? Color.White,
                GetUV(texture, topRight)
            );
            _vertices[_vertexCount + 2] = new VertexPositionColorTexture(
                new Vector3(wBottomRight.X, wBottomRight.Y, 0f),
                color ?? Color.White,
                GetUV(texture, bottomRight)
            );
            _vertices[_vertexCount + 3] = new VertexPositionColorTexture(
                new Vector3(wBottomLeft.X, wBottomLeft.Y, 0f),
                color ?? Color.White,
                GetUV(texture, bottomLeft)
            );

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

                GenerateIndexArray();

                _indexBuffer = new IndexBuffer(_graphicsDevice, typeof(uint), _indices.Length, BufferUsage.WriteOnly);
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

        private Vector2 GetUV(Texture2D texture, Num.Vector2 xy) {
            return new Vector2(xy.X / texture.Width, xy.Y / texture.Height);
        }

        private void GenerateIndexArray() {
            uint i = Floor(_fromIndex, 6, 6);
            uint j = Floor(_fromIndex, 6, 4);
            for (; i < _indices.Length; i += 6, j += 4) {
                _indices[i + 0] = j + 0;
                _indices[i + 1] = j + 1;
                _indices[i + 2] = j + 3;
                _indices[i + 3] = j + 1;
                _indices[i + 4] = j + 2;
                _indices[i + 5] = j + 3;
            }
            _fromIndex = _indices.Length;
        }
        private uint Floor(int value, int div, uint mul) {
            return (uint)MathF.Floor((float)value / div) * mul;
        }

        private const int _initialSprites = 2048;
        private const int _initialTriangles = _initialSprites * 2;
        private const int _initialVertices = _initialSprites * 4;
        private const int _initialIndices = _initialSprites * 6;

        private GraphicsDevice _graphicsDevice;
        private RasterizerState _rasterizerState = new RasterizerState {
            CullMode = CullMode.None
        };

        private VertexPositionColorTexture[] _vertices;
        private uint[] _indices;
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

        private bool _indicesChanged = false;
        private int _fromIndex = 0;
    }
}
