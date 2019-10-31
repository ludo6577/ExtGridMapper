using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carrefour.Clearance.UI.Grid.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class GridDisplayTitleAttribute : Attribute
    {
        internal string Text = null;

        public GridDisplayTitleAttribute(string text)
        {
            this.Text = text;
        }
    }
}
