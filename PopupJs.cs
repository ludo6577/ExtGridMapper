using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Carrefour.Clearance.UI
{
    public enum PopupButtons
    {
        None,
        /// <summary>
        /// Ok button close the popup
        /// </summary>
        Ok,
        /// <summary>
        /// You need to set the popup property: 'RedirectTo' 
        /// </summary>
        OkRedirect,
        /// <summary>
        /// You need to set the popup property: 'RedirectTo' for the Ok button to work
        /// </summary>
        OkCancelRedirect,
        /// <summary>
        /// You need to set the popup property: 'RedirectTo' for the Ok button to work
        /// </summary>
        OkCancelAjaxRedirect,
        /// <summary>
        /// You need to set the popup property: 'ActionJs' for the Ok button to work
        /// </summary>
        OkCancelAction
    }

    public class PopupJs
    {
        /// <summary>
        /// Buttons to show on the popup
        /// </summary>
        public PopupButtons PopupButtons { get; set; }
        /// <summary>
        /// Redirect on confirmation (Button: OkCancelRedirect)
        /// </summary>
        public string RedirectTo { get; set; }
        /// <summary>
        /// Execute javascript on confirmation (Button: OkCancelAction)
        /// </summary>
        public string ActionJs { get; set; }
        /// <summary>
        /// Used to set the text content from a JS variable
        /// </summary>
        public string JsVariableForContentName { get; set; }


        public string Title { get; set; }
        public string Content { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public bool AutoOpen { get; set; }
        public bool Resizable { get; set; }
        public bool Modal { get; set; }

        private static int _count = 1;
        internal readonly string Name;


        public PopupJs(string title, string content, PopupButtons buttons, int? width, int? height, int maxWidth, int maxHeight, bool autoOpen, bool resizable, bool modal)
        {
            Name = "popup_" + _count++;

            this.Title = title;
            this.Content = content;
            this.PopupButtons = buttons;
            this.Width = width;
            this.Height = height;
            this.MaxWidth = maxWidth;
            this.MaxHeight = maxHeight;
            this.AutoOpen = autoOpen;
            this.Resizable = resizable;
            this.Modal = modal;
            this.JsVariableForContentName = null;
        }

        /// <summary>
        /// Create the popup
        /// </summary>
        /// <param name="title">Title header</param>
        /// <param name="content">Html content</param>
        public PopupJs(string title, string content)
            : this(title, content, PopupButtons.Ok, null, null, 1500, 1000, true, true, false)
        {}

        /// <summary>
        /// Create the popup
        /// </summary>
        /// <param name="title">Title header</param>
        /// <param name="content">Html content</param>
        /// <param name="buttons">Buttons to show</param>
        public PopupJs(string title, string content, PopupButtons buttons)
            : this(title, content, buttons, null, null, 1500, 1000, true, true, false)
        {}

        /// <summary>
        /// Create the popup and an action to execute on click to Ok
        /// </summary>
        /// <param name="title">Title header</param>
        /// <param name="content">Html content</param>
        /// <param name="actionJs">Code javascript to execute</param>
        public PopupJs(string title, string content, string actionJs)
            : this(title, content, PopupButtons.OkCancelAction, null, null, 1500, 1000, true, true, false)
        {
            this.ActionJs = actionJs;
        }

        /// <summary>
        /// Get the popop in JS (Controller eg: return JavaScript(popup.GetPopupJs()); )
        /// (Use HtmlHelper instead to use from View)
        /// </summary>
        public string GetPopupJs()
        {
            string result = 
                //The Jquery Popup
                "$(\"" + getPopupHtml() + "\").dialog({" +
                    "title: '" + Title + "'," +
                    "width: " + (Width == null ? "'auto'" : Width.ToString()) + "," +
                    "height: " + (Height == null ? "'auto'" : Height.ToString()) + "," +
                    "maxWidth: " + MaxWidth + "," +
                    "maxHeight: " + MaxHeight + "," +
                    "autoOpen: " + AutoOpen.ToString().ToLower() + "," +
                    "resizable: " + Resizable.ToString().ToLower() + "," +
                    "modal: " + Modal.ToString().ToLower() + "," +
                    "buttons: {" +
                        getPopupButtons() +
                   "}," +
                   "create: function(event,ui){" +
                        (Width==null ? "$(this).css('max-width','"+ MaxWidth +"px');" : "") + //Fix maxWidth problem when width = auto
                    "}" +
                "});";
            return result;
        }

        private string getPopupHtml()
        {
            if(JsVariableForContentName==null)
                return "<div id='" + Name + "'>" + HttpUtility.JavaScriptStringEncode(Regex.Replace(Content, @"(?:\r\n|\n|\r)", "<br/>")) + "</div>";
            else
                return "<div id='" + Name + "'>\" + " + JsVariableForContentName + " + \"</div>";
        }

        private string getPopupButtons()
        {
            string popUpButton = "";
            /*
             *  Add the "Ok" button
             */
            switch (PopupButtons)
            {
                case PopupButtons.Ok:
                    popUpButton +=  "Ok: function () {" +
                                        "$(this).dialog('close');" +
                                    "}";
                    break;
                case PopupButtons.OkRedirect:
                    popUpButton +=  "Ok: function () {" +
                                        "$(this).dialog('close');" +
                                        "window.location.href = \"" + RedirectTo + "\"" +
                                    "}";
                    break;
                case PopupButtons.OkCancelRedirect:
                    popUpButton +=  "Ok: function () {" +
                                        "$(this).dialog('close');" +
                                        "window.location.href = \"" + RedirectTo + "\"" +
                                    "},";
                    break;
                case PopupButtons.OkCancelAjaxRedirect:
                    popUpButton += "Ok: function () {" +
                                        "$(this).dialog('close');" +
                                        "$.ajax({" +
                                            "url: \"" + RedirectTo + "\"," +
                                        "})},";
                    break;
                case PopupButtons.OkCancelAction:
                    popUpButton += "Ok: function () {" +
                                        "$(this).dialog('close');" +
                                        ActionJs +
                                    "},";
                    break;

                //Add other button here
            }

            /*
             *  Add the "Cancel" button if needed
             */
            switch (PopupButtons)
            {
                    case PopupButtons.OkCancelAction:
                    case PopupButtons.OkCancelAjaxRedirect:
                    case PopupButtons.OkCancelRedirect:
                        popUpButton += "Cancel: function () {" +
                                           "$(this).dialog('close');" +
                                       "}";
                        break;
            }

            return popUpButton;
        }
    }
}
