using Carrefour.Clearance.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Carrefour.Clearance.UI
{
    public static class DropDownHelper
    {
        private static MvcHtmlString DropDownListCheckable<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, bool multiselect, bool canBeEmpty)
        {
            string path = HttpRuntime.AppDomainAppVirtualPath;
            if (path!=null && !path.EndsWith("/"))
                path += "/";
            htmlHelper.Resource("css", "bootstrap-multiselect.css", "<link rel='stylesheet' href='" + path + "Content/bootstrap-multiselect.css' type='text/css'/>");
            htmlHelper.Resource("js", "bootstrap-multiselect.js", "<script type='text/javascript' src='" + path + "Scripts/bootstrap-multiselect.js'></script>");

            //Model field name
            string name = ExpressionHelper.GetExpressionText(expression);

            //Get name with prefixes to link properly control in partial views
            string fullHtmlFieldName;
            if (htmlHelper.ViewContext.ViewData != null)
                fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            else
                fullHtmlFieldName = name;
           
            object value = null;
            var model = htmlHelper.ViewData.Model;
            if (model != null)
                value = ((LambdaExpression) expression).Compile().DynamicInvoke(model);

            

            //<select>
            var select = new TagBuilder("select");
            select.MergeAttribute("id", name);
            select.MergeAttribute("name", fullHtmlFieldName + "[]");
            if(multiselect)
                select.MergeAttribute("multiple", "multiple");
            if (selectList == null)
                selectList = new List<SelectListItem>();

            ModelState modelState = null;
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullHtmlFieldName, out modelState))
            {
                if(modelState.Errors.Count > 0)
                    select.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                if(modelState.Value != null)
                    value = modelState.Value.ConvertTo(typeof(string[]), (CultureInfo)null);
            }
            
            //Model field value (used to know the current item)
            string[] fieldsValue = { "" };
            if (value != null)
            {
                if (value.GetType().IsArray)
                {
                    object[] values = (object[]) value;
                    fieldsValue = values.Select(x => x.ToString()).ToArray();
                }
                else
                {
                    fieldsValue = new[] { value.ToString() };
                }
            }

            //Default empty value
            if (canBeEmpty && !multiselect)
            {
                var option = new TagBuilder("option");
                option.MergeAttribute("value", "");
                option.InnerHtml = Localization.LocResources.NoElementsSelected;
                select.InnerHtml += option.ToString();
            }

            //<option>
            foreach (var selectListItem in selectList)
            {
                var option = new TagBuilder("option");
                option.MergeAttribute("value", selectListItem.Value);
                option.InnerHtml = selectListItem.Text;
                if (fieldsValue.Contains(selectListItem.Value) || fieldsValue.Contains(selectListItem.Text))
                    option.MergeAttribute("selected", "selected");
                select.InnerHtml += option.ToString();
            }


            //multiSelect attributes and class
            String attributes = "";
            IDictionary<string, object> htmlAttributesDictionnary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            List<string> reservedAttributes = new List<string>() { "class", "onChange", "disableIfEmpty", "disabled", "tabindex" };
            foreach (var htmlAttribute in htmlAttributesDictionnary)
            {
                if (!reservedAttributes.Contains( htmlAttribute.Key))
                    attributes += htmlAttribute.Key + "=\"" + htmlAttribute.Value + "\" ";
            }
            string tabIndex = htmlAttributesDictionnary["tabindex"] != null ? htmlAttributesDictionnary["tabindex"].ToString() : "";
            if (!string.IsNullOrEmpty(tabIndex))
                select.MergeAttribute("tabindex", tabIndex);
            
            string htmlclass = htmlAttributesDictionnary["class"] != null ? htmlAttributesDictionnary["class"].ToString() : "";
            bool isDisabled = htmlAttributesDictionnary["disabled"] != null && (htmlAttributesDictionnary["disabled"].ToString().ToLower() == "true" || htmlAttributesDictionnary["disabled"].ToString().ToLower() == "disabled");
            string disable = isDisabled ? "$('#" + name + "').multiselect('disable');" : "";

            //TODO: refactor to be params of this method (not html classes) 
            string onChangeHandler = htmlAttributesDictionnary["onChange"] != null ? htmlAttributesDictionnary["onChange"].ToString() + "(option, checked);" : "";
            string disableIfEmptyHandler =  htmlAttributesDictionnary["disableIfEmpty"] != null ? htmlAttributesDictionnary["disableIfEmpty"].ToString() : "true";

            //Plugin that load bootstrap-multiselect with options
            string plugin = "<script type='text/javascript'>";
            plugin +=           "$(document).ready(function() {";
            plugin +=               "$('#" + name + "').multiselect({";
            plugin +=                   "templates: {button: '<button type=\"button\" class=\"multiselect dropdown-toggle\" style=\"overflow: hidden; text-overflow: ellipsis;white-space: nowrap;\" data-toggle=\"dropdown\"></button>'},";
            plugin +=                   "maxHeight: 200,";
            plugin +=                   "dropRight: true,";
            plugin +=                   "enableCaseInsensitiveFiltering: true,";
            plugin +=                   "includeSelectAllOption: true,";
            plugin +=                   "selectAllText: '" + Localization.LocResources.SelectAll + "',";
            plugin +=                   "nonSelectedText: '" + Localization.LocResources.NoElementsSelected+ "',";
            plugin +=                   "nSelectedText: '" + Localization.LocResources.NumberElementsSelected + "',";
            plugin +=                   "allSelectedText: '" + Localization.LocResources.AllElementsSelected + "',";
            plugin +=                   "onChange: function(option, checked) { ";
            plugin +=                       onChangeHandler;
            plugin +=                   "},";
            //              Corrected in bootstrap multiselect (pull request)
            //plugin +=                   "rebuild: function(){"; 
            //plugin +=                       "var that = $('#" + name + "'); ";
            //plugin +=                       "if (that.length > 0) ";
            //plugin +=                           "that.multiselect('enable');";
            //plugin +=                       "else ";
            //plugin +=                           "that.multiselect('disable');";
            //plugin +=                   "},";
            plugin +=                   "disableIfEmpty: "+ disableIfEmptyHandler +",";
            plugin +=                   "buttonClass: '" + htmlclass + " " + attributes + "'";
            plugin +=               "});";
            plugin +=               disable;
            plugin +=           "});";
            plugin +=       "</script>";
            htmlHelper.Resource("js", name, plugin);

            return MvcHtmlString.Create(select.ToString());
        }

        public static MvcHtmlString DropDownListCheckable<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, bool multiselect)
        {
            return DropDownListCheckable(htmlHelper, expression, selectList, htmlAttributes, multiselect, false);
        }

        public static MvcHtmlString DropDownListCheckable<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return DropDownListCheckable(htmlHelper, expression, selectList, htmlAttributes, true);
        }

        public static MvcHtmlString DropDownListCheckable<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList)
        {
            return DropDownListCheckable(htmlHelper, expression, selectList, new Dictionary<string, object>());
        }



        /// <summary>
        /// Dropdown list with single selection
        /// </summary>
        public static MvcHtmlString DropDownList<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, bool canBeEmpty, object htmlAttributes)
        {
            return DropDownListCheckable(htmlHelper, expression, selectList, htmlAttributes, false, canBeEmpty);
        }

        /// <summary>
        /// Dropdown list with single selection
        /// </summary>
        public static MvcHtmlString DropDownList<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return DropDownList(htmlHelper, expression, selectList, false, htmlAttributes);
        }
    }
}
