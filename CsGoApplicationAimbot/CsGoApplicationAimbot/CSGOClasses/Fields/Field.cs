﻿using System;

namespace CsGoApplicationAimbot.CSGOClasses.Fields
{
    public class Field<T> where T : struct
    {
        public bool ValueRead { get; protected set; }
        public int Offset { get; protected set; }
        public T Value { get; protected set; }

        public Field() : this(0)
        { }
        public Field(int offset) : this(offset, default(T))
        { }
        public Field(int offset, T value)
        {
            this.Offset = offset;
            this.Value = value;
            this.ValueRead = false;
        }

        public virtual void ReadValue(int baseAddress)
        {
            this.Value = Program.MemUtils.Read<T>((IntPtr)(baseAddress + this.Offset));
            this.ValueRead = true;
        }
    }
}
