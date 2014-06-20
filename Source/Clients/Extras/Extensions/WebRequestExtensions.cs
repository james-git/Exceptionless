﻿#region Copyright 2014 Exceptionless

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
//     http://www.apache.org/licenses/LICENSE-2.0

#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Exceptionless.Submission.Net;
using Exceptionless.Threading.Tasks;

namespace Exceptionless.Extras.Extensions {
    public static class WebRequestExtensions {
        public const string JSON_CONTENT_TYPE = "application/json";

        public static Task<Stream> GetRequestStreamAsync(this WebRequest request) {
            return Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null);
        }

        public static Task<HttpWebResponse> GetResponseAsync(this WebRequest request) {
            return Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null).Then(t => t.Result as HttpWebResponse);
        }

        public static void AddAuthorizationHeader(this WebRequest request, ExceptionlessConfiguration configuration) {
            var authorizationHeader = new AuthorizationHeader {
                Scheme = ExceptionlessHeaders.Bearer,
                ParameterText = configuration.ApiKey
            };

            request.Headers[HttpRequestHeader.Authorization] = authorizationHeader.ToString();
        }

        private static readonly Lazy<PropertyInfo> _userAgentProperty = new Lazy<PropertyInfo>(() => typeof(HttpWebRequest).GetProperty("UserAgent"));

        public static void SetUserAgent(this HttpWebRequest request, string userAgent) {
            if (_userAgentProperty.Value != null)
                _userAgentProperty.Value.SetValue(request, userAgent, null);
            else
                request.Headers[ExceptionlessHeaders.Client] = userAgent;
        }

        public static Task<HttpWebResponse> PostJsonAsyncWithCompression(this HttpWebRequest request, string data) {
            request.Accept = request.ContentType = JSON_CONTENT_TYPE;
            request.Method = "POST";
            request.Headers["Content-Encoding"] = "gzip";

            byte[] buffer = Encoding.UTF8.GetBytes(data);
            return request.GetRequestStreamAsync().Then(t => {
                using (var zipStream = new GZipStream(t.Result, CompressionMode.Compress)) {
                    zipStream.Write(buffer, 0, buffer.Length);
                    zipStream.Close();
                }

                return request.GetResponseAsync();
            });
        }

        public static Task<HttpWebResponse> GetJsonAsync(this HttpWebRequest request) {
            request.Accept = JSON_CONTENT_TYPE;
            request.Method = "GET";

            return request.GetResponseAsync();
        }
    }
}