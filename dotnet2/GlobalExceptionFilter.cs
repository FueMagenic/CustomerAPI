﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

// http://www.talkingdotnet.com/global-exception-handling-in-aspnet-core-webapi/

namespace customerAPI
{
    /// <summary>
    /// ASP.NET Core Global Exception Handler
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter, IDisposable
    {
        /// <summary>
        /// Field: ILogger
        /// </summary>
        protected readonly ILogger _logger = null;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="logger">Logger to inject</param>
        public GlobalExceptionFilter(ILoggerFactory logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this._logger = logger.CreateLogger("Global Exception Filter");
        }

        #region "Dispose"
        // Flag: Has Dispose already been called?
        bool disposed = false;

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">bool</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // dispose stuff as needed
            }

            disposed = true;
        }

        #endregion

        /// <summary>
        /// Handle Exception
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            String message = String.Empty;

            var ex = context.Exception;

            TypeSwitch.Do(ex,
                    TypeSwitch.Case<ArgumentException>(() => { statusCode = HttpStatusCode.BadRequest; }),
                    TypeSwitch.Case<ArgumentNullException>(() => { statusCode = HttpStatusCode.BadRequest; }),
                    TypeSwitch.Case<ArgumentOutOfRangeException>(() => { statusCode = HttpStatusCode.BadRequest; }),
                    TypeSwitch.Case<KeyNotFoundException>(() => { statusCode = HttpStatusCode.NotFound;  })
                );

            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = (int)statusCode;
            response.ContentType = "application/json";
            var err = new Models.ErrorPayload()
            {
                Data = data,
                StackTrace = ex.StackTrace,
                Message = ex.Message,
                StatusCode = (int)statusCode
            };
            response.WriteAsync(JsonConvert.SerializeObject(err));
        }
    }
}
