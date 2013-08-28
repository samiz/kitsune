using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    [Serializable]
    public class BlockInfo
    {
        public string Text;
        public DataType[] ArgTypes;
        public DataType ReturnType;
        public BlockAttributes Attribute;
        
        public BlockInfo(string methodName, BlockAttributes attribute, DataType returnType, DataType[] argTypes)
        {
            this.Text = methodName;
            this.Attribute = attribute;
            this.ReturnType = returnType;
            this.ArgTypes = argTypes;
        }
    }
}
