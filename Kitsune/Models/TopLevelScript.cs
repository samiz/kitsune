using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kitsune
{
    [Serializable]
    public class TopLevelScript
    {
        public Point _Location;
        public IBlock Block;
        BlockSpace owner;

        public TopLevelScript(Point Location, IBlock Block, BlockSpace owner)
        {
            this._Location = Location;
            this.Block = Block;
            this.owner = owner;
        }

        public Point Location
        {
            get
            {
                return _Location;
            }
            set
            {
                _Location = value;
                owner.NotifyTopLevelMoved(this);
            }
        }

        internal string ToJson()
        {
            return string.Format("{{'kind':'script',\n'location':[{0},{1}],\n'code':{2}\n}}", 
                Location.X,Location.Y, Block.ToJson());
        }
    }
}
