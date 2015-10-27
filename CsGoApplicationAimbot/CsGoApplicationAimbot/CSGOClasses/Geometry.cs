using System;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Geometry
    {
        public static float GetDistanceToPoint(Vector3 pointA, Vector3 pointB)
        {
            return (float)Math.Abs((pointA - pointB).Length());
        }
    }
}