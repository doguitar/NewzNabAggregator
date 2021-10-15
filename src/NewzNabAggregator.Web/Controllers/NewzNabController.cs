using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using NewzNabAggregator.Common;
using NewzNabAggregator.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NewzNabAggregator.Web.Controllers
{
    [ApiController]
    public class NewzNabController : ControllerBase
    {
        public class HttpResponseMessageResult : IActionResult
        {
            private readonly HttpResponseMessage _responseMessage;

            public HttpResponseMessageResult(HttpResponseMessage responseMessage)
            {
                _responseMessage = responseMessage;
            }

            public async Task ExecuteResultAsync(ActionContext context)
            {
                var response = context.HttpContext.Response;
                if (_responseMessage == null)
                {
                    var message = "Response message cannot be null";
                    throw new InvalidOperationException(message);
                }
                using (_responseMessage)
                {
                    response.StatusCode = (int)_responseMessage.StatusCode;
                    var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
                    if (responseFeature != null)
                    {
                        responseFeature.ReasonPhrase = _responseMessage.ReasonPhrase;
                    }
                    var responseHeaders = _responseMessage.Headers;
                    if (responseHeaders.TransferEncodingChunked == true && responseHeaders.TransferEncoding.Count == 1)
                    {
                        responseHeaders.TransferEncoding.Clear();
                    }
                    foreach (var header2 in responseHeaders)
                    {
                        response.Headers.TryAdd(header2.Key, header2.Value.ToArray());
                    }
                    if (_responseMessage.Content == null)
                    {
                        return;
                    }
                    var contentHeaders = _responseMessage.Content.Headers;
                    _ = contentHeaders.ContentLength;
                    foreach (var header in contentHeaders)
                    {
                        response.Headers.TryAdd(header.Key, header.Value.ToArray());
                    }
                    await _responseMessage.Content.CopyToAsync(response.Body);
                }
            }
        }

        private class ClientInfo
        {
            public string Name
            {
                get;
                set;
            }

            public HttpClient Client
            {
                get;
                set;
            }

            public string Token
            {
                get;
                set;
            }
        }

        private enum Option
        {
            XML,
            JSON
        }

        private readonly Synchronizer<NewzNabAggregator.Database.Database> _db;

        private readonly ClientInfo[] _clients;
        private readonly Dictionary<string, TokenInfo> _tokens;

        private Guid RequestId
        {
            get;
        } = Guid.NewGuid();


        public NewzNabController(Synchronizer<NewzNabAggregator.Database.Database> db, NewzNabInfo[] newzNabs, Dictionary<string, TokenInfo> tokens)
        {
            _db = db;
            _clients = newzNabs.Select((NewzNabInfo i) => new ClientInfo
            {
                Client = new HttpClient
                {
                    BaseAddress = i.Uri,
                    Timeout = TimeSpan.FromSeconds(30.0)
                },
                Token = i.Token,
                Name = i.Name
            }).ToArray();
            _tokens = tokens;
        }

        ~NewzNabController()
        {
            _clients.ToList().ForEach(delegate (ClientInfo c)
            {
                c.Client.Dispose();
            });
        }

        private bool Authenticate(string token = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                if (Request.QueryString.HasValue)
                {
                    var queryString = (from p in base.Request.QueryString.Value!.Remove(0, 1).Split("&")
                                       select p.Split("=")).ToDictionary((string[] p) => p.First(), (string[] p) => p.Last());
                    queryString.TryGetValue("apikey", out token);
                }
            }
            return token != null && _tokens.ContainsKey(token.Trim());
        }

        private async Task<List<Tuple<ClientInfo, HttpResponseMessage>>> QueryAllClients(string path, Dictionary<string, string> queryString, string client = null)
        {
            var responses = new ConcurrentBag<Tuple<ClientInfo, HttpResponseMessage>>();
            await Task.WhenAll(_clients.Where((ClientInfo c) => client == null || c.Name == client).Select((Func<ClientInfo, Task>)async delegate (ClientInfo clientInfo)
            {
                var qs = new Dictionary<string, string>(queryString);
                if (qs.ContainsKey("apikey"))
                {
                    queryString["apikey"] = clientInfo.Token;
                }
                path = path + "?" + string.Join('&', queryString.Select((KeyValuePair<string, string> kvp) => kvp.Key + "=" + kvp.Value));
                try
                {
                    var start = DateTime.Now;
                    var response = await clientInfo.Client.GetAsync(path);
                    responses.Add(new(clientInfo, response));
                    Console.WriteLine($"{RequestId} Indexer {clientInfo.Client.BaseAddress} responded in {(DateTime.Now - start).TotalSeconds} seconds");
                }
                catch (Exception e)
                {
                    if (e.InnerException is TimeoutException)
                    {
                        Console.WriteLine($"{RequestId} Indexer {clientInfo.Client.BaseAddress} timed out");
                    }
                    else
                    {
                        Console.WriteLine($"{RequestId} Indexer {clientInfo.Client.BaseAddress} threw an exception at url {path}");
                        Console.Write(e.ToString());
                    }
                }
            }));
            return responses.ToList();
        }

        [HttpGet]
        [Route("api/")]
        public async Task<IActionResult> GetPassthrough(string client = null)
        {
            Console.WriteLine($"{RequestId} Request {base.Request.Path}?{base.Request.QueryString}");
            var requestUri = new Uri($"{base.Request.Scheme}://{base.Request.Host}/{base.Request.PathBase}");
            var results = new List<Tuple<ClientInfo, string>>();
            if (base.Request.QueryString.HasValue)
            {
                var path = base.Request.Path.Value!.Replace("/api", string.Empty);
                var queryString = (from p in base.Request.QueryString.Value!.Remove(0, 1).Split("&")
                                   select p.Split("=")).ToDictionary((string[] p) => p.First(), (string[] p) => p.Last());

                if (!(queryString.TryGetValue("apikey", out var token) && Authenticate(token)))
                {
                    return new UnauthorizedResult();
                }

                if (!queryString.TryGetValue("t", out var t))
                {
                    return new BadRequestResult();
                }
                if (t == "c" || t == "caps")
                {
                    return await Caps();
                }
                var option = Option.XML;
                if (queryString.TryGetValue("o", out var o) && o == "json")
                {
                    option = Option.JSON;
                }
                queryString["o"] = option.ToString().ToLowerInvariant();
                foreach (var response in await QueryAllClients(path, queryString, client))
                {
                    if (response.Item2.IsSuccessStatusCode)
                    {
                        results.Add(new(response.Item1, await response.Item2.Content.ReadAsStringAsync()));
                    }
                    base.HttpContext.Response.RegisterForDispose(response.Item2);
                }
                switch (option)
                {
                    case Option.XML:
                        {
                            var offset = 0uL;
                            if (queryString.TryGetValue("offset", out var offsetString) && ulong.TryParse(offsetString, out offset))
                            {
                            }
                            var total = 0uL;
                            var newznab = (XNamespace?)"http://www.newznab.com/DTD/2010/feeds/attributes/";
                            _ = (XNamespace?)"http://www.w3.org/2005/Atom";
                            var response2 = new XElement(newznab + "response");
                            var channel = new XElement((XName?)"channel", new XElement((XName?)"title", "NzbAggregator"), new XElement((XName?)"description", "NzbAggregator"), response2);
                            var returnXml = new XDocument(new XElement((XName?)"rss", channel));
                            foreach (var result in results)
                            {
                                try
                                {
                                    var xml = XElement.Parse(result.Item2);
                                    var subchannel = xml.Element((XName?)"channel");
                                    var subresponse = subchannel.Element(newznab + "response");
                                    var subtotal = subresponse.Attribute((XName?)"total")?.Value;
                                    if (!string.IsNullOrEmpty(subtotal) && ulong.TryParse(subtotal, out var ulongSubtotal) && ulongSubtotal > total)
                                    {
                                        total = ulongSubtotal;
                                    }
                                    foreach (var item in xml.Element((XName?)"channel")!.Elements("item"))
                                    {
                                        var enclosure = item.Element((XName?)"enclosure");
                                        var link = item.Element((XName?)"link");
                                        var lengthString = enclosure.Attribute((XName?)"length")?.Value;
                                        var url2 = enclosure.Attribute((XName?)"url")?.Value;
                                        var type = enclosure.Attribute((XName?)"type")?.Value;
                                        var title = item.Element((XName?)"title")?.Value;
                                        var pubDate = item.Element((XName?)"pubDate")?.Value;
                                        if (!string.IsNullOrWhiteSpace(url2) && !string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(pubDate) && !string.IsNullOrWhiteSpace(lengthString) && long.TryParse(lengthString, out var length))
                                        {
                                            var nzb = new Nzb
                                            {
                                                length = length,
                                                link = url2,
                                                pubDate = pubDate,
                                                type = type,
                                                title = title
                                            };
                                            nzb = _db.Synchronize((NewzNabAggregator.Database.Database db) => db.SaveNzb(nzb));
                                            url2 = (link.Value = new Uri(requestUri, $"/nzb/{nzb.id}").ToString());
                                            enclosure.Attribute((XName?)"url")!.Value = url2;
                                            channel.Add(item);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine($"Failed to parse results at '{result.Item1.Name}': {Environment.NewLine}{result.Item2}");
                                }
                            }
                            response2.Add(new XAttribute((XName?)"offset", offset), new XAttribute((XName?)"total", total));
                            return new ContentResult
                            {
                                Content = returnXml.ToString(),
                                ContentType = "text/xml",
                                StatusCode = 200
                            };
                        }
                }
            }
            return new OkResult();
        }

        [HttpGet]
        [Route("{client}/api/")]
        public Task<IActionResult> GetClientPassthrough(string client)
        {
            return GetPassthrough(client);
        }

        [HttpGet]
        [Route("nzb/{id}")]
        public async Task<IActionResult> Nzb(string id)
        {
            if (!Authenticate())
            {
                return new UnauthorizedResult();
            }

            if (Guid.TryParse(id, out var guid))
            {
                Nzb nzb = null;
                await _db.SynchronizeAsync(delegate (NewzNabAggregator.Database.Database db)
                {
                    nzb = db.GetNzb(guid);
                    return Task.CompletedTask;
                });
                if (nzb != null)
                {
                    using var client = new HttpClient();
                    var nzbResponse = await client.GetAsync(nzb.link);
                    if (nzbResponse.IsSuccessStatusCode)
                    {
                        try
                        {
                            var filename = nzbResponse.Content.Headers.ContentDisposition?.FileName?.Replace("\"", string.Empty);
                            if (filename == null)
                            {
                                return File(await nzbResponse.Content.ReadAsStreamAsync(), nzbResponse.Content.Headers.ContentType!.MediaType);
                            }
                            return File(await nzbResponse.Content.ReadAsStreamAsync(), nzbResponse.Content.Headers.ContentType!.MediaType, filename);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return NotFound();
        }

        [HttpGet]
        [Route("caps")]
        public async Task<IActionResult> Caps()
        {
            if (!Authenticate())
            {
                return new UnauthorizedResult();
            }

            var requestUri = new Uri($"{base.Request.Scheme}://{base.Request.Host}/{base.Request.PathBase}");
            var path = string.Empty;
            var queryString = new Dictionary<string, string>
            {
                {
                    "t",
                    "caps"
                }
            };
            var option = Option.XML;
            if (queryString.TryGetValue("o", out var o) && o == "json")
            {
                option = Option.JSON;
            }
            queryString["o"] = option.ToString().ToLowerInvariant();
            var responses = (await QueryAllClients(path, queryString)).Where(r => r.Item2.IsSuccessStatusCode);
            var allcaps = new List<XElement>();
            foreach (var r in responses)
            {
                allcaps.Add(XElement.Parse(await r.Item2.Content.ReadAsStringAsync()));
            }

            var server = new XElement((XName?)"server", new XAttribute((XName?)"title", "NzbAggregator"), new XAttribute((XName?)"url", requestUri.ToString()));
            var limits = new XElement((XName?)"limits", new XAttribute((XName?)"max", (from e in allcaps
                                                                                       select e.Element((XName?)"limits")!.Attribute((XName?)"max")!.Value into d
                                                                                       select Convert.ToUInt64(d)).Min()), new XAttribute((XName?)"default", 100));
            var searching = new XElement((XName?)"searching");
            var categories = new XElement((XName?)"categories");
            var caps = new XElement((XName?)"caps", server, limits, searching, categories);
            var responseXml = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), caps);
            foreach (var group in from e in allcaps.Select((XElement c) => c.Element((XName?)"searching")!.Elements()).SelectMany((IEnumerable<XElement> e) => e)
                                  group e by e.Name)
            {
                if (group.All((XElement s) => s.Attribute((XName?)"available")?.Value == "yes"))
                {
                    var commonParameters = (from p in (from s in @group
                                                       select s.Attribute((XName?)"supportedParams")?.Value into p
                                                       where !string.IsNullOrWhiteSpace(p)
                                                       select p.Split(",")).SelectMany((string[] p) => p)
                                            group p by p into g
                                            select g.Key).Distinct().ToList();
                    if (!commonParameters.Contains("q"))
                    {
                        commonParameters.Add("q");
                    }
                    searching.Add(new XElement(group.Key, new XAttribute((XName?)"available", "yes"), new XAttribute((XName?)"supportedParams", string.Join(",", commonParameters))));
                }
            }
            foreach (var cats in from e in allcaps.Select((XElement c) => c.Element((XName?)"categories")!.Elements("category")).SelectMany((IEnumerable<XElement> e) => e)
                                 group e by e.Attribute((XName?)"id")!.Value into g
                                 where g.Count() == allcaps.Count()
                                 select g)
            {
                var category = new XElement((XName?)"category", new XAttribute((XName?)"id", cats.Key), new XAttribute((XName?)"name", cats.First().Attribute((XName?)"name")!.Value));
                foreach (var subcats in from e in cats.Select((XElement c) => c.Elements("subcat")).SelectMany((IEnumerable<XElement> e) => e)
                                        group e by e.Attribute((XName?)"id")!.Value into g
                                        where g.Count() == cats.Count()
                                        select g)
                {
                    var subcat = new XElement((XName?)"category", new XAttribute((XName?)"id", subcats.Key), new XAttribute((XName?)"name", subcats.First().Attribute((XName?)"name")!.Value));
                    category.Add(subcat);
                }
                categories.Add(category);
            }
            var wr = new StringWriter();
            responseXml.Save((TextWriter)wr);
            return new ContentResult
            {
                Content = wr.ToString(),
                ContentType = "text/xml",
                StatusCode = 200
            };
        }
    }
}
