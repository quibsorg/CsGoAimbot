using System;
using CsGoApplicationAimbot.MathObjects;

namespace CsGoApplicationAimbot
{
    /// <summary>
    ///     A utility-class that offers several mathematical algorithms.
    /// </summary>
    public static class MathUtils
    {
        #region VARIABLES

        private static readonly float DEG_2_RAD = (float) (Math.PI/180f);
        private static readonly float RAD_2_DEG = (float) (180f/Math.PI);

        #endregion

        #region METHODS
        /// <summary>
        ///     Applies (adds) an offset to an array of 3d-coordinates
        /// </summary>
        /// <param name="offset">Offset to apply</param>
        /// <param name="points">Array if 3d-coordinates</param>
        /// <returns>Array of manipulated 3d-coordinates</returns>
        public static Vector3[] OffsetVectors(this Vector3 offset, params Vector3[] points)
        {
            for (var i = 0; i < points.Length; i++)
                points[i] += offset;
            return points;
        }

        /// <summary>
        ///     Copies an array of vectors to a new array containing identical, new Vector3s (deep-copy)
        /// </summary>
        /// <param name="source">Source-array to copy from</param>
        /// <returns>New array containing identical yet new Vector3s</returns>
        public static Vector3[] CopyVectors(this Vector3[] source)
        {
            var ret = new Vector3[source.Length];
            for (var i = 0; i < ret.Length; i++)
                ret[i] = new Vector3(source[i]);
            return ret;
        }

        /// <summary>
        ///     Rotates a given point around another point
        /// </summary>
        /// <param name="pointToRotate">Point to rotate</param>
        /// <param name="centerPoint">Point to rotate around</param>
        /// <param name="angleInDegrees">Angle of rotation in degrees</param>
        /// <returns>Rotated point</returns>
        public static Vector2 RotatePoint(this Vector2 pointToRotate, Vector2 centerPoint, float angleInDegrees)
        {
            var angleInRadians = (float) (angleInDegrees*(Math.PI/180f));
            var cosTheta = (float) Math.Cos(angleInRadians);
            var sinTheta = (float) Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                        (cosTheta*(pointToRotate.X - centerPoint.X) -
                         sinTheta*(pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                        (sinTheta*(pointToRotate.X - centerPoint.X) +
                         cosTheta*(pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        /// <summary>
        ///     Clamps a given angle
        /// </summary>
        /// <param name="clampAngles">Angle to clamp</param>
        /// <returns>Clamped angle</returns>
        public static Vector3 ClampAngle(this Vector3 res)
        {
            while (res.Y > 180.0)
                res.Y -= 360.0F;

            while (res.Y < -180.0)
                res.Y += 360.0f;

            if (res.X > 90.0)
                res.X = 90.0F;

            if (res.X < -90.0)
                res.X = -90.0F;

            res.Z = 0;
            return res;
        }

        //Todo fix calcAngle with RCS
        public static Vector3 CalcAngle(this Vector3 src, Vector3 dst)
        {
            var output = new Vector3();
            var delta = src - dst;
            var hypotenuse = (float) Math.Sqrt(delta.X*delta.X + delta.Y*delta.Y);
            output.X = (float) Math.Atan(delta.Z/hypotenuse)*57.295779513082f;
            output.Y = (float) Math.Atan(delta.Y/delta.X)*57.295779513082f;
            output.Z = 0f;
            if (delta.X >= 0f)
            {
                output.Y += 180f;
            }
            return ClampAngle(output);
        }

        /// <summary>
        ///     Smooths an angle from src to dest
        /// </summary>
        /// <param name="src">Original angle</param>
        /// <param name="dest">Destination angle</param>
        /// <param name="smoothAmount">Value between 0 and 1 to apply as smooting where 0 is no modification and 1 is no smoothing</param>
        /// <returns></returns>
        public static Vector3 SmoothAngle(this Vector3 src, Vector3 dest, float smoothAmount)
        {
            Vector3 SmoothedAngle;
            SmoothedAngle = dest - src;
            SmoothedAngle = ClampAngle(SmoothedAngle);
            //SmoothedAngle = src + SmoothedAngle * (1f / 100f) * (100f - smoothAmount);
            SmoothedAngle = src + SmoothedAngle/100f*(100f - smoothAmount);
            //SmoothedAngle = src + SmoothedAngle * (smoothAmount);
            Console.WriteLine(SmoothedAngle);
            return ClampAngle(SmoothedAngle);

            //Vector3 smoothedAngle = src + (dest - src) * smoothAmount;
            //Console.WriteLine(smoothedAngle);
            //return ClampAngle(smoothedAngle);
        }


        /// <summary>
        ///     Converts the given angle in degrees to radians
        /// </summary>
        /// <param name="deg">Angle in degrees</param>
        /// <returns>Angle in radians</returns>
        public static float DegreesToRadians(float deg)
        {
            return deg*DEG_2_RAD;
        }

        /// <summary>
        ///     Converts the given angle in radians to degrees
        /// </summary>
        /// <param name="rad">Angle in radians</param>
        /// <returns>Angle in degrees</returns>
        public static float RadiansToDegrees(float rad)
        {
            return rad*RAD_2_DEG;
        }

        /// <summary>
        ///     Returns whether the given point is within a circle of the given radius around the given center
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <param name="circleCenter">Center of circle</param>
        /// <param name="radius">Radius of circle</param>
        /// <returns></returns>
        public static bool PointInCircle(this Vector2 point, Vector2 circleCenter, float radius)
        {
            return (point - circleCenter).Length() < radius;
        }

        #endregion
    }
}