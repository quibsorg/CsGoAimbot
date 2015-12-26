using System;
using System.Numerics;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Geometry
    {
        public static float GetDistanceToPoint(Vector3 pointA, Vector3 pointB)
        {
            return Math.Abs((pointA - pointB).Length());
        }
    }
}