using System;
using System.Web.Mvc;

namespace Carrefour.Clearance.UI.Grid.Attributes
{
    public enum SendMethod
    {
        Get,
        Post
    }

    public enum OnSuccessGridAction
    {
        DoNoting,
        DeleteRow
    }

    /// <summary>
    /// Add a javascript click action on the column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class GridColumnClickHandlerAttribute : Attribute
    {
        internal string Action;
        internal SendMethod Method;
        internal bool IsAjaxSubmit;
        internal OnSuccessGridAction GridAction = OnSuccessGridAction.DoNoting;
        internal string ConfirmationMessage = null;

        /// <summary>
        /// Add a javascript click action on the column
        /// All Properties marked with GridColumnKey will be send to the controller
        /// </summary>
        /// <param name="action">The action called on the click</param>
        /// <param name="method">Post or Get</param>
        /// <param name="isAjaxSubmit">Call by ajax (Asynchrone)</param>
        public GridColumnClickHandlerAttribute(string action, SendMethod method, bool isAjaxSubmit)
        {
            this.Action = action;
            this.Method = method;
            this.IsAjaxSubmit = isAjaxSubmit;
        }

        /// <summary>
        /// Add a javascript click action on the column
        /// All Properties marked with GridColumnKey will be send to the controller
        /// </summary>
        /// <param name="action">The action called on the click</param>
        /// <param name="method">Post or Get</param>
        /// <param name="isAjaxSubmit">Call by ajax (Asynchrone)</param>
        /// <param name="gridAction">Call an action on the grid if the server response is OK</param>
        public GridColumnClickHandlerAttribute(string action, SendMethod method, bool isAjaxSubmit, OnSuccessGridAction gridAction)
            : this(action, method, isAjaxSubmit)
        {
            this.GridAction = gridAction;
        }

        /// <summary>
        /// Add a javascript click action on the column
        /// All Properties marked with GridColumnKey will be send to the controller
        /// </summary>
        /// <param name="action">The action called on the click</param>
        /// <param name="method">Post or Get</param>
        /// <param name="isAjaxSubmit">Call by ajax (Asynchrone)</param>
        /// <param name="gridAction">Call an action on the grid if the server response is OK</param>
        /// <param name="confirmationMessage">Validation message to show</param>
        public GridColumnClickHandlerAttribute(string action, SendMethod method, bool isAjaxSubmit, OnSuccessGridAction gridAction, string confirmationMessage)
            : this(action, method, isAjaxSubmit)
        {
            this.GridAction = gridAction;
            this.ConfirmationMessage = confirmationMessage;
        }

        /// <summary>
        /// Add a javascript click action on the column
        /// All Properties marked with GridColumnKey will be send to the controller
        /// This call is not asynchrone (don't use Ajax)
        /// </summary>
        /// <param name="action">The action called on the click</param>
        /// <param name="method">Post or Get</param>
        public GridColumnClickHandlerAttribute(string action, SendMethod method) 
            : this(action, method, false)
        {}



        internal string GetOnSuccessGridActionString()
        {
            switch (this.GridAction)
            {
                case OnSuccessGridAction.DeleteRow:
                    return "store.removeAt(rowIndex);";
                case OnSuccessGridAction.DoNoting:
                default:
                    return "";
            }
        }
    }
}
