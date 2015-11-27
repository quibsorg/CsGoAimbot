using System;

namespace CsGoApplicationAimbot.MathObjects
{
    /// <summary>
    ///     A matrix.
    /// </summary>
    public class Matrix
    {
        #region CONSTRUCTOR

        public Matrix(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            data = new float[rows*columns];
        }

        #endregion

        #region VARIABLES

        private readonly float[] data;
        private readonly int rows;
        private readonly int columns;

        #endregion

        #region METHODS

        public void Read(byte[] data)
        {
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < columns; x++)
                    this[y, x] = BitConverter.ToSingle(data, sizeof (float)*(y*columns + x));
        }

        public byte[] ToByteArray()
        {
            var sof = sizeof (float);
            var data = new byte[this.data.Length*sof];
            for (var i = 0; i < this.data.Length; i++)
                Array.Copy(BitConverter.GetBytes(this.data[i]), 0, data, i*sof, sof);
            return data;
        }

        #endregion

        #region OPERANDS

        public float this[int i]
        {
            get { return data[i]; }
            set { data[i] = value; }
        }

        public float this[int row, int column]
        {
            get { return data[row*columns + column]; }
            set { data[row*columns + column] = value; }
        }

        #endregion
    }
}