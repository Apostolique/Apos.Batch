using System.Numerics;

namespace Apos.Batch {
    public static class Matrix32 {
        /// <summary>
        /// Creates a rotation matrix using the given rotation in radians.
        /// </summary>
        /// <param name="radians">The amount of rotation, in radians.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix3x2 CreateRotation(float radians) => Matrix3x2.CreateRotation(radians);

        /// <summary>
        /// Creates a rotation matrix using the specified rotation in radians and a center point.
        /// </summary>
        /// <param name="radians">The amount of rotation, in radians.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix3x2 CreateRotation(float radians, Microsoft.Xna.Framework.Vector2 centerPoint) => Matrix3x2.CreateRotation(radians, new Vector2(centerPoint.X, centerPoint.Y));

        /// <summary>
        /// Creates a scaling matrix from the specified vector scale.
        /// </summary>
        /// <param name="scales">The scale to use.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x2 CreateScale(Microsoft.Xna.Framework.Vector2 scales) => Matrix3x2.CreateScale(new Vector2(scales.X, scales.Y));

        /// <summary>
        /// Creates a scaling matrix from the specified vector scale with an offset from the specified center point.
        /// </summary>
        /// <param name="scales">The scale to use.</param>
        /// <param name="centerPoint">The center offset.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x2 CreateScale(Microsoft.Xna.Framework.Vector2 scales, Microsoft.Xna.Framework.Vector2 centerPoint) => Matrix3x2.CreateScale(new Vector2(scales.X, scales.Y), new Vector2(centerPoint.X, centerPoint.Y));

        /// <summary>
        /// Creates a scaling matrix that scales uniformly with the given scale.
        /// </summary>
        /// <param name="scale">The uniform scale to use.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x2 CreateScale(float scale) => Matrix3x2.CreateScale(scale);

        /// <summary>
        /// Creates a scaling matrix that scales uniformly with the specified scale with an offset from the specified center.
        /// </summary>
        /// <param name="scale">The uniform scale to use.</param>
        /// <param name="centerPoint">The center offset.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x2 CreateScale(float scale, Microsoft.Xna.Framework.Vector2 centerPoint) => Matrix3x2.CreateScale(scale, new Vector2(centerPoint.X, centerPoint.Y));

        /// <summary>
        /// Creates a scaling matrix from the specified X and Y components.
        /// </summary>
        /// <param name="xScale">The value to scale by on the X axis.</param>
        /// <param name="yScale">The value to scale by on the Y axis.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x2 CreateScale(float xScale, float yScale) => Matrix3x2.CreateScale(xScale, yScale);

        /// <summary>
        /// Creates a scaling matrix that is offset by a given center point.
        /// </summary>
        /// <param name="xScale">The value to scale by on the X axis.</param>
        /// <param name="yScale">The value to scale by on the Y axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x2 CreateScale(float xScale, float yScale, Microsoft.Xna.Framework.Vector2 centerPoint) => Matrix3x2.CreateScale(xScale, yScale, new Vector2(centerPoint.X, centerPoint.Y));

        /// <summary>
        /// Creates a skew matrix from the specified angles in radians.
        /// </summary>
        /// <param name="radiansX">The X angle, in radians.</param>
        /// <param name="radiansY">The Y angle, in radians.</param>
        /// <returns>The skew matrix.</returns>
        public static Matrix3x2 CreateSkew(float radiansX, float radiansY) => Matrix3x2.CreateSkew(radiansX, radiansY);

        /// <summary>
        /// Creates a skew matrix from the specified angles in radians and a center point.
        /// </summary>
        /// <param name="radiansX">The X angle, in radians.</param>
        /// <param name="radiansY">The Y angle, in radians.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The skew matrix.</returns>
        public static Matrix3x2 CreateSkew(float radiansX, float radiansY, Microsoft.Xna.Framework.Vector2 centerPoint) => Matrix3x2.CreateSkew(radiansX, radiansY, new Vector2(centerPoint.X, centerPoint.Y));

        /// <summary>
        /// Creates a translation matrix from the specified 2-dimensional vector.
        /// </summary>
        /// <param name="position">The translation position.</param>
        /// <returns>The translation matrix.</returns>
        public static Matrix3x2 CreateTranslation(Microsoft.Xna.Framework.Vector2 position) => Matrix3x2.CreateTranslation(new Vector2(position.X, position.Y));

        /// <summary>
        /// Creates a translation matrix from the specified X and Y components.
        /// </summary>
        /// <param name="xPosition">The X position.</param>
        /// <param name="yPosition">The Y position.</param>
        /// <returns>The translation matrix.</returns>
        public static Matrix3x2 CreateTranslation(float xPosition, float yPosition) => Matrix3x2.CreateTranslation(xPosition, yPosition);
    }
}
