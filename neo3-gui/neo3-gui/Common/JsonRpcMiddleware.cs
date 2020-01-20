﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Neo.IO.Json;
using Neo.Models;

namespace Neo.Common
{
    public class JsonRpcMiddleware : IMiddleware
    {

        private readonly IServiceProvider _provider;

        public JsonRpcMiddleware(IServiceProvider provider)
        {
            _provider = provider;
        }


        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
            context.Response.Headers["Access-Control-Max-Age"] = "31536000";
            if (context.Request.Method != "GET" && context.Request.Method != "POST"
                || context.WebSockets.IsWebSocketRequest)
            {
                await next(context);
                return;
            }
            var request = await GetRequestParameter(context.Request);
            var message = new WsMessage();
            message.MsgType = WsMessageType.Result;
            message.Id = request.Id;
            message.Method = request.Method;
            var executor = _provider.GetService<WebSocketExecutor>();
            var result = await executor.Excute(request);
            if (result is ErrorResult error)
            {
                message.MsgType = WsMessageType.Error;
                message.Message = error.Message;
            }
            else
            {
                message.Result = result;
            }

            context.Response.ContentType = "application/json-rpc";
            await context.Response.WriteAsync(message.SerializeJson(), Encoding.UTF8);
        }


        private async Task<WsRequest> GetRequestParameter(HttpRequest httpRequest)
        {
            if (httpRequest.Method == "GET")
            {
                var request = new WsRequest();
                request.Id = httpRequest.Query["id"];
                request.Method = httpRequest.Query["method"];
                //var parameters = httpRequest.Query["params"].ToString();
                request.Params = httpRequest.Query["params"].ToString().DeserializeJson<JsonElement>();
                return request;
            }
            else
            {
                using var reader = new StreamReader(httpRequest.Body);
                var requestString = await reader.ReadToEndAsync();
                var request = requestString.DeserializeJson<WsRequest>();
                return request;
            }
        }
    }
}