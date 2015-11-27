using System;

namespace CsGoApplicationAimbot.MathObjects
{
    public struct Vector3
    {
        #region VARIABLES

        public float X;
        public float Y;
        public float Z;

        #endregion

        #region PROPERTIES

        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 UnitX => new Vector3(1, 0, 0);
        public static Vector3 UnitY => new Vector3(0, 1, 0);
        public static Vector3 UnitZ => new Vector3(0, 0, 1);

        #endregion

        #region CONSTRUCTOR

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(Vector3 vec) : this(vec.X, vec.Y, vec.Z)
        {
        }

        public Vector3(float[] values) : this(values[0], values[1], values[2])
        {
        }

        #endregion

        #region METHODS

        public float Length()
        {
            return (float) Math.Abs(Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2)));
        }

        public float DistanceTo(Vector3 other)
        {
            return (this - other).Length();
        }

        public override bool Equals(object obj)
        {
            var vec = (Vector3) obj;
            return GetHashCode() == vec.GetHashCode();
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override string ToString()
        {
            return $"[X={X}, Y={Y}, Z={Z}]";
        }

        #endregion

        #region OPERATORS

        public static Vector3 operator /(Vector3 v1, float scalar)
        {
            return new Vector3(v1.X/scalar, v1.Y/scalar, v1.Z/scalar);
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector3 operator *(Vector3 v1, float scalar)
        {
            return new Vector3(v1.X*scalar, v1.Y*scalar, v1.Z*scalar);
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !(v1 == v2);
        }

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion
    }
}