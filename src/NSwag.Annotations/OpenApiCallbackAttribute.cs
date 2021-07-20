//-----------------------------------------------------------------------
// <copyright file="OpenApiCallbackAttribute.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;

namespace NSwag.Annotations
{
    /// <summary>Indicates a callback to be added to the Swagger definition.</summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OpenApiCallbackAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="OpenApiCallbackAttribute"/> class.</summary>
        /// <param name="callbackUrl"></param>
        public OpenApiCallbackAttribute(string callbackUrl) : this(callbackUrl, name: null, type: null)
        { }
        
        /// <summary>Initializes a new instance of the <see cref="OpenApiCallbackAttribute"/> class.</summary>
        /// <param name="name"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="method"></param>
        public OpenApiCallbackAttribute(string callbackUrl,
                                        string name = null,
                                        string method = "post") : this(callbackUrl, name, method, mimeType : null, type: null)
        { }
                

        /// <summary>Initializes a new instance of the <see cref="OpenApiCallbackAttribute"/> class.</summary>
        /// <param name="callbackUrl"></param>
        /// <param name="types"></param>
        public OpenApiCallbackAttribute(string callbackUrl,
                                        params Type[] types) : this(callbackUrl, name: null, types: types)
        { }

        /// <summary>Initializes a new instance of the <see cref="OpenApiCallbackAttribute"/> class.</summary>
        /// <param name="name"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="method"></param>
        /// <param name="mimeType"></param>
        /// <param name="type"></param>
        public OpenApiCallbackAttribute(string callbackUrl,
                                        string name = null,
                                        string method = "post",
                                        string mimeType = null,
                                        Type type = null) : this(callbackUrl, name, method, mimeType, type != null ? new[] { type } : new Type[] { })
        { }


        /// <summary>Initializes a new instance of the <see cref="OpenApiCallbackAttribute"/> class.</summary>
        /// <param name="name"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="method"></param>
        /// <param name="mimeType"></param>
        /// <param name="types"></param>
        public OpenApiCallbackAttribute(string callbackUrl,
                                        string name = null, 
                                        string method = "post",
                                        string mimeType = null,
                                        params Type[] types) : base()
        {            
            this.Name = name;
            this.Types = types;
            this.CallbackUrl = callbackUrl;
            this.Method = method;
            this.MimeType = mimeType;
        }
        /// <summary>
        /// The name of the callback.
        /// <br />If unspecified defaults to "{operationId}_callback".
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// The URL of the endpoint that will receive the callback
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// The HTTP method that will be used to call the endpoint.
        /// <br />If unspecified defaults to POST.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The content type of the callback bodies. 
        /// <br />If unspecified defaults to "octet-stream" for binary content, "application/json" otherwise.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// The possible object types returned by the callback.
        /// <br />If none are specified, the definition will indicate a callback with an empty body.
        /// </summary>
        public Type[] Types { get; set; }
    }
}
