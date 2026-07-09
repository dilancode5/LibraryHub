using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryHub.Filters
{
    public class AdminSessionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var username = context.HttpContext.Session.GetString("AdminUser");

            if (string.IsNullOrEmpty(username))
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null);
            }
        }
    }
}