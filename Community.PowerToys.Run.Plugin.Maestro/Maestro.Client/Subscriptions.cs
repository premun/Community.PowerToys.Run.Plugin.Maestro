// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;

#nullable disable

namespace Community.PowerToys.Run.Plugin.Maestro.Maestro.Client;

public interface ISubscriptions
{
    Task<Models.Subscription> GetSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Models.Subscription>> ListSubscriptionsAsync(CancellationToken cancellationToken = default);
}

internal class Subscriptions(MaestroApi client) : IServiceOperations<MaestroApi>, ISubscriptions
{
    public MaestroApi Client { get; } = client;

    public async Task<List<Models.Subscription>> ListSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        const string apiVersion = "2020-02-20";

        var _baseUri = Client.Options.BaseUri;
        var _url = new RequestUriBuilder();
        _url.Reset(_baseUri);
        _url.AppendPath(
            "/api/subscriptions",
            false);
        _url.AppendQuery("api-version", Client.Serialize(apiVersion));


        using (var _req = Client.Pipeline.CreateRequest())
        {
            _req.Uri = _url;
            _req.Method = RequestMethod.Get;

            using (var _res = await Client.SendAsync(_req, cancellationToken).ConfigureAwait(false))
            {
                if (_res.Status < 200 || _res.Status >= 300)
                {
                    await OnListSubscriptionsFailed(_req, _res).ConfigureAwait(false);
                }

                if (_res.ContentStream == null)
                {
                    await OnListSubscriptionsFailed(_req, _res).ConfigureAwait(false);
                }

                using (var _reader = new StreamReader(_res.ContentStream))
                {
                    var _content = await _reader.ReadToEndAsync().ConfigureAwait(false);
                    var _body = Client.Deserialize<List<Models.Subscription>>(_content);
                    return _body;
                }
            }
        }
    }

    internal async Task OnListSubscriptionsFailed(Request req, Response res)
    {
        string content = null;
        if (res.ContentStream != null)
        {
            using (var reader = new StreamReader(res.ContentStream))
            {
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        var ex = new RestApiException<Models.ApiError>(
            req,
            res,
            content,
            Client.Deserialize<Models.ApiError>(content)
            );
        MaestroApi.OnFailedRequest(ex);
        throw ex;
    }

    internal async Task OnCreateFailed(Request req, Response res)
    {
        string content = null;
        if (res.ContentStream != null)
        {
            using (var reader = new StreamReader(res.ContentStream))
            {
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        var ex = new RestApiException<Models.ApiError>(
            req,
            res,
            content,
            Client.Deserialize<Models.ApiError>(content)
            );
        MaestroApi.OnFailedRequest(ex);
        throw ex;
    }

    public async Task<Models.Subscription> GetSubscriptionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {

        const string apiVersion = "2020-02-20";

        var _baseUri = Client.Options.BaseUri;
        var _url = new RequestUriBuilder();
        _url.Reset(_baseUri);
        _url.AppendPath(
            "/api/subscriptions/{id}".Replace("{id}", Uri.EscapeDataString(Client.Serialize(id))),
            false);

        _url.AppendQuery("api-version", Client.Serialize(apiVersion));


        using (var _req = Client.Pipeline.CreateRequest())
        {
            _req.Uri = _url;
            _req.Method = RequestMethod.Get;

            using (var _res = await Client.SendAsync(_req, cancellationToken).ConfigureAwait(false))
            {
                if (_res.Status < 200 || _res.Status >= 300)
                {
                    await OnGetSubscriptionFailed(_req, _res).ConfigureAwait(false);
                }

                if (_res.ContentStream == null)
                {
                    await OnGetSubscriptionFailed(_req, _res).ConfigureAwait(false);
                }

                using (var _reader = new StreamReader(_res.ContentStream))
                {
                    var _content = await _reader.ReadToEndAsync().ConfigureAwait(false);
                    var _body = Client.Deserialize<Models.Subscription>(_content);
                    return _body;
                }
            }
        }
    }

    internal async Task OnGetSubscriptionFailed(Request req, Response res)
    {
        string content = null;
        if (res.ContentStream != null)
        {
            using (var reader = new StreamReader(res.ContentStream))
            {
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        var ex = new RestApiException<Models.ApiError>(
            req,
            res,
            content,
            Client.Deserialize<Models.ApiError>(content)
            );
        MaestroApi.OnFailedRequest(ex);
        throw ex;
    }
}
