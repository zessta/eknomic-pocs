using Microsoft.AspNetCore.Mvc.Filters;

namespace InventoryManagement.Filters
{
    public class UriParserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var parameter in context.ActionArguments)
            {
                if (parameter.Value is string stringValue)
                {
                    // Decode the URI-encoded string
                    context.ActionArguments[parameter.Key] = Uri.UnescapeDataString(stringValue);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
