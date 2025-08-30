// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;

#nullable disable
namespace Community.PowerToys.Run.Plugin.Maestro.Maestro.Client;

public partial interface IMaestroApi
{
    MaestroApiOptions Options { get; set; }
    ISubscriptions Subscriptions { get; }
}

public partial interface IServiceOperations<T>
{
    T Client { get; }
}

public partial class MaestroApiOptions : ClientOptions
{
    public MaestroApiOptions()
        : this(new Uri(""))
    {
    }

    public MaestroApiOptions(Uri baseUri)
        : this(baseUri, null!)
    {
    }

    public MaestroApiOptions(TokenCredential credentials)
        : this(new Uri(""), credentials)
    {
    }

    public MaestroApiOptions(Uri baseUri, TokenCredential credentials)
    {
        BaseUri = baseUri;
        Credentials = credentials;
        InitializeOptions();
    }

    partial void InitializeOptions();

    /// <summary>
    ///   The base URI of the service.
    /// </summary>
    public Uri BaseUri { get; }

    /// <summary>
    ///   Credentials to authenticate requests.
    /// </summary>
    public TokenCredential Credentials { get; }
}

internal partial class MaestroApiResponseClassifier : ResponseClassifier
{
}

public partial class MaestroApi : IMaestroApi
{
    private readonly JsonSerializerOptions _serializerSettings;

    public MaestroApiOptions Options
    {
        get;
        set
        {
            field = value;
            Pipeline = CreatePipeline(value);
        }
    }

    private static HttpPipeline CreatePipeline(MaestroApiOptions options)
    {
        return HttpPipelineBuilder.Build(options, [], [], new MaestroApiResponseClassifier());
    }

    public HttpPipeline Pipeline
    {
        get;
        private set;
    }

    public ISubscriptions Subscriptions { get; }


    public MaestroApi()
        : this(new MaestroApiOptions())
    {
    }

    public MaestroApi(MaestroApiOptions options)
    {
        Options = options;
        Subscriptions = new Subscriptions(this);
        _serializerSettings = new JsonSerializerOptions();

        Init();
    }

    /// <summary>
    ///    Optional initialization defined outside of auto-gen code
    /// </summary>
    partial void Init();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void OnFailedRequest(RestApiException ex)
    {
        HandleFailedRequest(ex);
    }

    private static void HandleFailedRequest(RestApiException ex)
    {
        if (ex.Response.Status == (int)HttpStatusCode.BadRequest)
        {
            JsonNode content;
            try
            {
                content = JsonNode.Parse(ex.Response.Content)!;
                if (content["Message"] is JsonValue value && value.GetValueKind() == System.Text.Json.JsonValueKind.String)
                {
                    string message = value.GetValue<string>();
                    throw new ArgumentException(message, ex);
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        else if (ex.Response.Status == (int)HttpStatusCode.Unauthorized)
        {
            throw new AuthenticationException("Unauthorized access while trying to use Maestro API. " +
                "Make sure your darc client is authenticated. More details: https://github.com/dotnet/arcade/blob/main/Documentation/Darc.md");
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize(string value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize(bool value)
    {
        return value ? "true" : "false";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize(long value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize(float value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize(double value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize(Guid value)
    {
        return value.ToString("D", CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Serialize<T>(T value)
    {
        string result = JsonSerializer.Serialize(value, _serializerSettings);

        if (value is Enum)
        {
            return result.Substring(1, result.Length - 2);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Deserialize<T>(string value)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)value;
        }

        return JsonSerializer.Deserialize<T>(value, _serializerSettings);
    }

    public virtual ValueTask<Response> SendAsync(Request request, CancellationToken cancellationToken)
    {
        return Pipeline.SendRequestAsync(request, cancellationToken);
    }
}

public partial class RequestWrapper
{
    public RequestWrapper(Request request)
    {
        Uri = request.Uri.ToUri();
        Method = request.Method;
        Headers = request.Headers.ToDictionary(h => h.Name, h => h.Value);
    }

    public Uri Uri { get; }
    public RequestMethod Method { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
}

public partial class ResponseWrapper(Response response, string responseContent)
{
    public string Content { get; } = responseContent;

    public ResponseHeaders Headers { get; } = response.Headers;

    public string ReasonPhrase { get; } = response.ReasonPhrase;

    public int Status { get; } = response.Status;
}

[Serializable]
public partial class RestApiException : Exception
{
    private static string FormatMessage(Response response, string responseContent)
    {
        var result = $"The response contained an invalid status code {response.Status} {response.ReasonPhrase}";
        if (responseContent != null)
        {
            result += "\n\nBody: ";
            result += responseContent.Length < 300 ? responseContent : responseContent.Substring(0, 300);
        }
        return result;
    }

    public RequestWrapper Request { get; }

    public ResponseWrapper Response { get; }

    public RestApiException(Request request, Response response, string responseContent)
        : base(FormatMessage(response, responseContent))
    {
        Request = new RequestWrapper(request);
        Response = new ResponseWrapper(response, responseContent);
    }
}

[Serializable]
public partial class RestApiException<T>(Request request, Response response, string responseContent, T body)
    : RestApiException(request, response, responseContent)
{
    public T Body { get; } = body;
}

public partial class QueryBuilder : List<KeyValuePair<string, string>>
{
    public QueryBuilder()
    {
    }

    public QueryBuilder(IEnumerable<KeyValuePair<string, string>> parameters)
        : base(parameters)
    {
    }

    public void Add(string key, IEnumerable<string> values)
    {
        foreach (string str in values)
            Add(new KeyValuePair<string, string>(key, str));
    }

    public void Add(string key, string value)
    {
        Add(new KeyValuePair<string, string>(key, value));
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        for (int index = 0; index < Count; ++index)
        {
            KeyValuePair<string, string> keyValuePair = this[index];
            if (index != 0)
            {
                builder.Append('&');
            }
            builder.Append(UrlEncoder.Default.Encode(keyValuePair.Key));
            builder.Append('=');
            builder.Append(UrlEncoder.Default.Encode(keyValuePair.Value));
        }
        return builder.ToString();
    }
}
