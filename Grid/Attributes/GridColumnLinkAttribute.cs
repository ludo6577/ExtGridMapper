using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carrefour.Clearance.UI.Grid.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GridColumnLinkAttribute : Attribute
    {
        /// <summary>
        /// The value returned by the Propertie will be the link
        /// </summary>
        public GridColumnLinkAttribute()
        {}
    }
}
