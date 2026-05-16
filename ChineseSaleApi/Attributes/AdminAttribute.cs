using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ChineseSaleApi.Attributes
{
   

public class AdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

            if (user == null || !user.Claims.Any(c => c.Type == "isAdmin" && c.Value == "true"))
            {
                context.Result = new ForbidResult();
            }
        }
}

}