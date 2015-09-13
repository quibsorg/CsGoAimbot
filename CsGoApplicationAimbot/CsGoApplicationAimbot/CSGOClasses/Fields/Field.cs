using System;

namespace CsGoApplicationAimbot.CSGOClasses.Fields
{
    public class Field<T> where T : struct
    {
        public Field() : this(0)
        {
        }

        public Field(int offset) : this(offset, default(T))
        {
        }

        public Field(int offset, T value)
        {
            Offset = offset;
            Value = value;
            ValueRead = false;
        }

        public bool ValueRead { get; protected set; }
        public int Offset { get; protected set; }
        public T Value { get; protected set; }

        public virtual void ReadValue(int baseAddress)
        {
            Value = Program.MemUtils.Read<T>((IntPtr) (baseAddress + Offset));
            ValueRead = true;
        }
    }
}