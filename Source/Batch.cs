using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended;

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
                _projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            }
        }
        public void Draw(Texture2D texture, Matrix3x2? world = null, Matrix3x2? source = null, Color? color = null) {
            if (_texture != texture) {
                Flush();
                _texture = texture;
            }

            EnsureSizeOrDouble(ref _vertices, _vertexCount + 4);
            _indicesChanged = EnsureSizeOrDouble(ref _indices, _indexCount + 6) || _indicesChanged;

            // TODO: world shouldn't be null.
            if (world == null) {
                world = Matrix3x2.Identity;
            }

            Vector2 topLeft;
            Vector2 topRight;
            Vector2 bottomRight;
            Vector2 bottomLeft;
            if (source == null) {
                topLeft = new Vector2(0, 0);
                topRight = new Vector2(texture.Width, 0);
                bottomRight = new Vector2(texture.Width, texture.Height);
                bottomLeft = new Vector2(0, texture.Height);
            } else {
                topLeft = Vector2.Transform(new Vector2(0f, 0f), source.Value);
                topRight = Vector2.Transform(new Vector2(1f, 0f), source.Value);
                bottomRight = Vector2.Transform(new Vector2(1f, 1f), source.Value);
                bottomLeft = Vector2.Transform(new Vector2(0, 1f), source.Value);
            }

            Vector2 wTopLeft = Vector2.Transform(topLeft, world.Value);
            Vector2 wTopRight = Vector2.Transform(topRight, world.Value);
            Vector2 wBottomRight = Vector2.Transform(bottomRight, world.Value);
            Vector2 wBottomLeft = Vector2.Transform(bottomLeft, world.Value);

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

        private Vector2 GetUV(Texture2D texture, Vector2 xy) {
            return new Vector2(xy.X / texture.Width, xy.Y / texture.Height);
        }

        private void GenerateIndexArray() {
            for (uint i = _fromIndex, j = _fromVertex; i < _indices.Length; i += 6, j += 4) {
                _indices[i + 0] = j + 0;
                _indices[i + 1] = j + 1;
                _indices[i + 2] = j + 3;
                _indices[i + 3] = j + 1;
                _indices[i + 4] = j + 2;
                _indices[i + 5] = j + 3;
            }
            _fromIndex = (uint)_indices.Length;
            _fromVertex = (uint)_vertices.Length;
        }

        private const int _initialSprites = 2048;
        private const int _initialTriangles = _initialSprites * 2;
        private const int _initialVertices = _initialSprites * 4;
        private const int _initialIndices = _initialSprites * 6;

        private readonly GraphicsDevice _graphicsDevice;
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
        private readonly Effect _defaultEffect;
        private readonly EffectPass _defaultPass;
        private Effect _effect;
        private bool _customEffect = false;

        private bool _indicesChanged = false;
        private uint _fromIndex = 0;
        private uint _fromVertex = 0;
    }
}
