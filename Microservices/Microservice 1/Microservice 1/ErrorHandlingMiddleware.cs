// ***********************************************************************
// <copyright file="ErrorHandlingMiddleware.cs">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ExceptionHandlingInMicroservices.Controllers;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.Serialization;

namespace AEG.Common.Middlewares
{
    /// <summary>
    /// Error handling middleware class which will be injected in request pipeline
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        /// <summary>
        /// The next
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor of error handling middleware class which is taking request delegate object as input parameter
        /// </summary>
        /// <param name="next">The next.</param>
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        /// <summary>
        /// Method executes on each request and response
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        public static bool CheckIfCustomException(Exception ex)
        {
            var exceptionsList = new List<string>();
            if (!string.IsNullOrEmpty(ex.Message))
                exceptionsList = ex.Message.Split(",").ToList();

            return (ex.InnerException != null && ex.InnerException.Data.Contains("ExceptionType") && ex.InnerException.Data["ExceptionType"].ToString().Equals("CustomException", StringComparison.OrdinalIgnoreCase)) || (ex != null && ex.Data.Contains("ExceptionType") && ex.Data["ExceptionType"].ToString().Equals("CustomException", StringComparison.OrdinalIgnoreCase)) || (exceptionsList.Any() && exceptionsList.LastOrDefault().Equals("CustomException", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Logic for handling response of exception
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="ex">The ex.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            /// Log the exception

            var response = context.Response;
            context.Response.ContentType = "application/json";

            if (ex is CustomException)
            {
                await HandleCustomException(context, ex, response);
            }
            else if (ex is DivideByZeroException)
            {
                await HandleDivideByZeroException(context, ex, response);
            }
            ///Handle all other exceptions Individually  
            else
            {
                /// All Unhandled exceptions come here and handled
                await HandleInternalServerError(context, ex, response);
            }
        }

        /// <summary>
        /// Handles the internal server error.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="response">The response.</param>
        private static async Task HandleInternalServerError(HttpContext context, Exception ex, HttpResponse response, bool isCustomException = false)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await response.WriteAsync(JsonConvert.SerializeObject(new ResponseClass() { Content = "This is the content sent by Microservice 1", StatusCode = 222, Message = "Type of original Exception is  " + ex.GetType().ToString() + "  And the Exception Message is :" + ex.Message + " With Stack Trace :" + ex.StackTrace }
            ));
            // throw ex;
        }

        /// <summary>
        /// Handles the custom exception.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="response">The response.</param>
        private static async Task HandleCustomException(HttpContext context, Exception ex, HttpResponse response, bool isCustomException = false)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await response.WriteAsync(JsonConvert.SerializeObject(new ResponseClass() { Content = "This is the content sent by Microservice 1 for HandleCustomException", StatusCode = 224, Message = "This is the message with exception stack trace " + ex.StackTrace }
            ));
        }


        /// <summary>
        /// Handle argument exception thrown in code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task HandleDivideByZeroException(HttpContext context, Exception ex, HttpResponse response)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await response.WriteAsync(JsonConvert.SerializeObject(new ResponseClass() { Content = "This is the content sent by Microservice 1 for DivideByZeroException", StatusCode = 221, Message = "This is the message with exception stack trace " + ex.StackTrace }
           ));
        }
    }

    /// <summary>
    /// Middleware extension
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Registering middleware
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder UseErrorHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }


    public class CustomException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException" /> class.
        /// </summary>
        public CustomException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CustomException(string message) : base(message)
        {
            Data.Add("ExceptionType", "CustomException");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomException"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public CustomException(Exception exception) : base(exception.Message, exception) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        public CustomException(string message, string source) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected CustomException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}