using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public interface IBlock
    {
        ParentRelationship ParentRelationship { get; set; }
        IBlock DeepClone();
        void PostSerializationPatchUp();

        string ToJson();
    }
}
