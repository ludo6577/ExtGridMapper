using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Carrefour.Clearance.Localization;
using Carrefour.Clearance.UI.Grid;
using Carrefour.Clearance.UI.Grid.Attributes;
using Carrefour.Clearance.Utils.Helpers;

namespace Carrefour.Clearance.UI
{
    public static class GridHelper
    {
        private static string Session_GridData = "GridData";

        public static MvcHtmlString AwesomeGrid<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool renderImmediately)
        {
            GridModel gridModel = InitGridModel(htmlHelper, expression);

            htmlHelper.Resource("js", gridModel.ModelName, HttpUtility.HtmlDecode(htmlHelper.Raw(htmlHelper.Partial("_GridPartial", gridModel)).ToString()), renderImmediately);

            // The grid div
            TagBuilder gridDiv = new TagBuilder("div");
            gridDiv.MergeAttribute("id", gridModel.DivName);
            return MvcHtmlString.Create(gridDiv.ToString());
        }

        public static MvcHtmlString AwesomeGrid<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return AwesomeGrid(htmlHelper, expression, false);
        }


        /// <summary>
        /// Get the Javascript code that will update the grid
        /// </summary>
        /// <param name="datas">The new DataSet</param>
        /// <returns>Javascript code to execute in the view</returns>
        public static string UpdateGridCode(IEnumerable<object> datas)
        {
            return "updateGrid([" + GetData(datas) + "])";
        }

        /// <summary>
        /// Get the previously saved dataset
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object> GetGridData()
        {
            return (IEnumerable<object>)HttpContext.Current.Session[Session_GridData];
        }




        public static GridModel InitGridModel<TModel, TProperty>(HtmlHelper htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            //Model name (will be given to the div id)
            string modelName = ExpressionHelper.GetExpressionText(expression);
            if (string.IsNullOrEmpty(modelName))
                modelName = "UnknownModelName";

            object model = null;
            if (htmlHelper.ViewData.Model != null)
                model = ((LambdaExpression)expression).Compile().DynamicInvoke(htmlHelper.ViewData.Model);

            var modelState = htmlHelper.ViewData.ModelState[modelName];
            if (modelState != null)
                model = modelState.Value.ConvertTo(typeof(object));


            GridModel gridModel = new GridModel()
            {
                ModelName = modelName,
                Id = modelName + "_id",
                DivName = modelName + "_grid",
                StoreName = modelName + "_store"
            };

            if (model == null)
                return gridModel;


            // Get the type T from IEnumerable<T> then read his attributes
            IEnumerable<object> models = (IEnumerable<object>)model;
            StringBuilder gridColumns = new StringBuilder();
            StringBuilder gridFields = new StringBuilder().Append("[");
            int columnNumber = 0;

            /*
             * Class attributs (apply to all the grid)
             */
            // Grid display title
            object[] htmlAttributes = models.GetType().GetGenericArguments()[0].GetCustomAttributes(typeof(GridDisplayTitleAttribute), false);
             if(htmlAttributes.Count()!=0)
                gridModel.GridTitle = GetLocalizedText(((GridDisplayTitleAttribute)htmlAttributes[0]).Text);

             // Grid height
             htmlAttributes = models.GetType().GetGenericArguments()[0].GetCustomAttributes(typeof(GridHeightAttribute), false);
             if (htmlAttributes.Count() != 0)
                 gridModel.Height = ((GridHeightAttribute)htmlAttributes[0]).Height;
             else
                 gridModel.Height = 300;

            // Grid Export To Csv button
             htmlAttributes = models.GetType().GetGenericArguments()[0].GetCustomAttributes(typeof(GridExportToCsvAction), false);
             if (htmlAttributes.Count() != 0)
                 gridModel.ExportCsvButton = GetExportCsvButton(htmlHelper, gridModel.GridTitle+ ".zip");

            //TODO Other class attributes
            
            /*
             *  Properties attributes (apply to each columns of the grid)
             */
            //Properties attributes
            PropertyInfo[] propertyInfos = models.GetType().GetGenericArguments()[0].GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string title = "Undefined_" + columnNumber++;
                string fieldName = propertyInfo.Name;
                string isHidden = "";
                string width = "";
                string key = "";
                string image = "";
                string imageUrl = null;
                string link = "";
                string handler = "";

                // Column display title
                htmlAttributes = propertyInfo.GetCustomAttributes(typeof(GridDisplayTitleAttribute), false);
                if (htmlAttributes.Count() != 0)
                    title = GetLocalizedText(((GridDisplayTitleAttribute)htmlAttributes[0]).Text);

                // Column visibility
                htmlAttributes = propertyInfo.GetCustomAttributes(typeof(GridColumnHiddenAttribute), false);
                if (htmlAttributes.Count() != 0)
                    isHidden = ",hidden:true";

                // Column width
                htmlAttributes = propertyInfo.GetCustomAttributes(typeof(GridColumnWidthAttribute), false);
                if (htmlAttributes.Count() != 0)
                    width = ",width:" + ((GridColumnWidthAttribute)htmlAttributes[0]).Width;

                // Column key
                htmlAttributes = propertyInfo.GetCustomAttributes(typeof(GridColumnKey), false);
                if (htmlAttributes.Count() != 0)
                    key = ",isKey:true";

                // Column image
                htmlAttributes = propertyInfo.GetCustomAttributes(typeof(GridColumnImageAttribute), false);
                if (htmlAttributes.Count() != 0)
                {
                    imageUrl = ((GridColumnImageAttribute)htmlAttributes[0]).ImageUrl;
                    if (imageUrl!=null && imageUrl.Contains("~"))
                    {
                        UrlHelper url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
                        imageUrl = url.Content(imageUrl);
                    }

                    if (imageUrl != null)
                        image = ",renderer:function(value){return '<img src=\"" + imageUrl + "\"/>';}";
                    else
                        image = ",renderer:function(value){return '<img src=\"' + value + '\"/>';}";
                }

                // Column link
                htmlAttributes = propertyInfo.GetCustomAttributes(typeof(GridColumnLinkAttribute), false);
                if (htmlAttributes.Count() != 0)
                {
                    if(string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(image))
                        throw new Exception("Can't add attribute GridColumnLink and GridColumnImage without specifing an url for the image");

                    if (imageUrl != null)
                    {
                        image = ""; // We can't have an image with the url given by value and a link.
                        link = ",renderer:function(value){return '<a href=\"' + value + '\"><img src=\"" + imageUrl + "\"/></a>';}";
                    }
                    else
                        link = ",renderer:function(value){return '<a href=\"' + value + '\">' + value + '</a>';}";
                }

                // Action column
                htmlAttributes = propertyInfo.GetCustomAttributes(typeof(GridColumnClickHandlerAttribute), false);
                if (htmlAttributes.Count() != 0)
                {
                    if (string.IsNullOrEmpty(imageUrl))
                        throw new Exception("Attribute GridColumnClickHandler need the GridColumnImage attribute with an image url");
                    image = "";

                    if (!string.IsNullOrEmpty(link))
                        throw new Exception("Can't use attribute GridColumnClickHandler with GridColumnLink");
                    link = "";

                    string action = ((GridColumnClickHandlerAttribute) htmlAttributes[0]).Action;
                    string method = ((GridColumnClickHandlerAttribute)htmlAttributes[0]).Method == SendMethod.Post ? "POST" : "GET";
                    bool isAjax = ((GridColumnClickHandlerAttribute)htmlAttributes[0]).IsAjaxSubmit;
                    string onSuccessGridAction = ((GridColumnClickHandlerAttribute)htmlAttributes[0]).GetOnSuccessGridActionString();
                    string confirmationMessage = GetLocalizedText(((GridColumnClickHandlerAttribute)htmlAttributes[0]).ConfirmationMessage);
                    handler = ",xtype:'actioncolumn',items: [{icon: \"" + imageUrl + "\",handler: function(grid, rowIndex, colIndex) {postRowAsModel(\"" + action + "\", \"" + method + "\", \"" + isAjax + "\", \"" + onSuccessGridAction + "\", \"" + confirmationMessage + "\", grid, rowIndex);}}]";
                }

                //TODO other columns attributes

                gridFields.Append("\"" + fieldName + "\",");
                gridColumns.Append("{text:\"" + title + "\",dataIndex:\"" + fieldName + "\"" + isHidden + width + key + link + image + handler + "},");
            }
            if (gridColumns.Length > 0)
                gridColumns.Remove(gridColumns.Length - 1, 1);
            
            if(gridFields.Length>0)
                gridFields.Remove(gridFields.Length-1, 1);

            gridFields.Append("]");

            gridModel.Columns = gridColumns.ToString();
            gridModel.Fields = gridFields.ToString();
            gridModel.Data = GetData(models);
            return gridModel;
        }

        private static string GetData(IEnumerable<object> models)
        {
            //Save data for export
            HttpContext.Current.Session[Session_GridData] = models;

            StringBuilder gridData = new StringBuilder();
            /*
             *  Properties attributes for each rows (apply to each cells of the grid)
             */
            //Enumerate the IEnumerator (rows)
            foreach (object model in models)
            {
                string data = "";

                //For each field
                PropertyInfo[] propertyInfos = model.GetType().GetProperties();
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    string value = "";

                    // Cell value
                    if (propertyInfo.GetValue(model) != null)
                        value = propertyInfo.GetValue(model).ToString();

                    data += "\"" + HttpUtility.HtmlEncode(value) + "\",";
                }
                // Line of Data
                gridData.Append("[" + data.Remove(data.LastIndexOf(',')) + "],");
            }

            if(gridData.Length>0)
                gridData.Remove(gridData.Length - 1, 1);
            return gridData.ToString();
        }

        private static string GetLocalizedText(string text)
        {
            if(text==null)
                return null;

            string title = LocResources.ResourceManager.GetString(text);
            if (string.IsNullOrEmpty(title))
                title = text;
            return title;
        }

        private static string GetExportCsvButton(HtmlHelper htmlHelper, string fileName)
        {
            UrlHelper url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            string imageUrl = url.Content("~/Content/images/icons/Excel.png");
            string href = url.Action("DownloadCsv", "Grid") + "?fileName=" + fileName;
            return  "<div class='row gridButtonExport'>" +
                        "<div class='right'>" +
                            "<a class='link-icon' href='" + href + "'>" +
                                "<img src='" + imageUrl + "' alt='@LocResources.ExportExcel' />" +
                            "</a>" +
                        "</div>" +
                    "</div>";
        }
    }
}
