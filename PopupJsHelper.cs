using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Carrefour.Clearance.UI
{
    public static class PopupJsHelper
    {
        private static PopupJs popup;

        public static HtmlString PopupJs<TModel>(this HtmlHelper<TModel> htmlHelper, string title, string JsVariableNamecontent, string actionJs)
        {
            popup = new PopupJs(title, "", actionJs);
            popup.JsVariableForContentName = JsVariableNamecontent;
            return new HtmlString(popup.GetPopupJs());
        }
    }
}
