﻿using System;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Fields
{
    public class BonesField : Field<Vector3>
    {
        public BonesField(int index) : base(index) 
        { }

        public override void ReadValue(int baseAddress)
        {
            float x, y, z;
            x = Program.MemUtils.Read<float>((IntPtr)(baseAddress + this.Offset * 0x30 + 0x0C));
            y = Program.MemUtils.Read<float>((IntPtr)(baseAddress + this.Offset * 0x30 + 0x1C));
            z = Program.MemUtils.Read<float>((IntPtr)(baseAddress + this.Offset * 0x30 + 0x2C));
            this.Value = new Vector3(x, y, z);
            this.ValueRead = true;
        }
    }
}
