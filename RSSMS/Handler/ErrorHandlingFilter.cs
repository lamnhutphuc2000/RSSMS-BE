using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RSSMS.DataService.Responses;

namespace RSSMS.API.Handler
{
    public class ErrorHandlingFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is System.Linq.Dynamic.Core.Exceptions.ParseException || context.Exception is ErrorResponse)
            {
                string message = context.Exception.ToString();
                if (context.Exception.GetType() == typeof(ErrorResponse)) message = ((ErrorResponse)context.Exception).Error.Message;
                context.Result = new ObjectResult(new ErrorResponse(((ErrorResponse)context.Exception).Error.Code, message))
                {
                    StatusCode = ((ErrorResponse)context.Exception).Error.Code
                };
                context.ExceptionHandled = true;
                return;
            }
        }
    }
}
