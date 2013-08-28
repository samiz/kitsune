using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    public enum DropType
    {
        Above, Below, Between, AsArgument
    }
    public struct DropRegion
    {
        public DropType DropType;
        public Rectangle Rectangle;
        public IBlockView Destination;
        public DataType ArgType;
        public object ExtraInfo;


        public DropRegion(DropType DropType, Rectangle Rectangle, IBlockView destination)
            : this(DropType, Rectangle, destination, null)
        {

        }

        public DropRegion(DropType DropType, Rectangle Rectangle, IBlockView destination, object extraInfo)
            :this(DropType, Rectangle, destination, extraInfo, DataType.Invalid)
        {
        }
        public DropRegion(DropType DropType, Rectangle Rectangle, IBlockView destination, object extraInfo, DataType argType)
        {
            this.DropType = DropType;
            this.Rectangle = Rectangle;
            this.Destination = destination;
            this.ExtraInfo = extraInfo;
            this.ArgType = argType;

            if (argType == DataType.Invalid && DropType == DropType.AsArgument)
            {
                throw new ArgumentException("DropRegion constructor: if DropType is argument, you must supply a valid argument type", "argType");
            }
        }

        internal bool IsCompatible(DataType bType, BlockAttributes droppedAttr)
        {
            switch (DropType)
            {
                case DropType.Above:
                    return droppedAttr == BlockAttributes.Hat || droppedAttr == BlockAttributes.Stack;
                case DropType.Below:
                    return droppedAttr == BlockAttributes.Cap || droppedAttr == BlockAttributes.Stack;
                case DropType.Between:
                    return droppedAttr == BlockAttributes.Stack;
                case DropType.AsArgument:
                    return IsAssignableFrom(this.ArgType, bType);
                default:
                    return false;
            }
        }

        private bool IsAssignableFrom(DataType target, DataType source)
        {
            if (target == DataType.Object)
                return source != DataType.Script;
            return source == target;
        }
    }
}
