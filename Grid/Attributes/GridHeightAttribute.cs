using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Carrefour.Clearance.UI.Grid.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GridHeightAttribute : Attribute
    {
        internal int Height;

        public GridHeightAttribute(int height)
        {
            this.Height = height;
        }

    }
}
