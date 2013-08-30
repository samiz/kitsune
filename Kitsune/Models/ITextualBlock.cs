using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    public interface ITextualBlock : IBlock
    {
        string Text { get; }
        void SetText(string text);
    }
}
