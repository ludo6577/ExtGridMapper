using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carrefour.Clearance.UI.Grid.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GridColumnImageAttribute : Attribute
    {
        internal string ImageUrl = null;

        /// <summary>
        /// The value returned by the Propertie will be the link to the image
        /// </summary>
        public GridColumnImageAttribute()
        {}

        /// <summary>
        /// All row will show the same image
        /// </summary>
        /// <param name="imageUrl">The image url</param>
        public GridColumnImageAttribute(string imageUrl)
        {
            this.ImageUrl = imageUrl;
        }

    }
}
