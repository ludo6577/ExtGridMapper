using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Carrefour.Clearance.UI.Grid.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GridColumnWidthAttribute : Attribute
    {
        internal int Width;

        public GridColumnWidthAttribute(int width)
        {
            this.Width = width;
        }

    }
}
