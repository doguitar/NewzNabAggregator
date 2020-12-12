#define DEBUG
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NewzNabAggregator.Common
{
    public class NewzNab
    {
        public class SearchRootAttributes
        {
            public string version
            {
                get;
                set;
            }
        }

        public class Category
        {
        }

        public class Image
        {
            public string url
            {
                get;
                set;
            }

            public string title
            {
                get;
                set;
            }

            public string link
            {
                get;
                set;
            }

            public string description
            {
                get;
                set;
            }
        }

        public class ResponseAttributes
        {
            public string offset
            {
                get;
                set;
            }

            public string total
            {
                get;
                set;
            }
        }

        public class Response
        {
            [JsonProperty("@attributes")]
            public ResponseAttributes attributes
            {
                get;
                set;
            }
        }

        public class EnclosureAttributes
        {
            public string url
            {
                get;
                set;
            }

            public string length
            {
                get;
                set;
            }

            public string type
            {
                get;
                set;
            }
        }

        public class Enclosure
        {
            [JsonProperty("@attributes")]
            public EnclosureAttributes attributes
            {
                get;
                set;
            }
        }

        public class AttrAttributes
        {
            public string name
            {
                get;
                set;
            }

            public string value
            {
                get;
                set;
            }
        }

        public class Attr
        {
            [JsonProperty("@attributes")]
            public AttrAttributes attributes
            {
                get;
                set;
            }
        }

        public class Item
        {
            public string title
            {
                get;
                set;
            }

            public string guid
            {
                get;
                set;
            }

            public string link
            {
                get;
                set;
            }

            public string comments
            {
                get;
                set;
            }

            public string pubDate
            {
                get;
                set;
            }

            public string category
            {
                get;
                set;
            }

            public string description
            {
                get;
                set;
            }

            public Enclosure enclosure
            {
                get;
                set;
            }

            public List<Attr> attr
            {
                get;
                set;
            }
        }

        public class Channel
        {
            public string title
            {
                get;
                set;
            }

            public string description
            {
                get;
                set;
            }

            public string link
            {
                get;
                set;
            }

            public string language
            {
                get;
                set;
            }

            public string webMaster
            {
                get;
                set;
            }

            public Category category
            {
                get;
                set;
            }

            public Image image
            {
                get;
                set;
            }

            public Response response
            {
                get;
                set;
            }

            public List<Item> item
            {
                get;
                set;
            }
        }

        public class SearchRoot
        {
            [JsonProperty("@attributes")]
            public SearchRootAttributes attributes
            {
                get;
                set;
            }

            public Channel channel
            {
                get;
                set;
            }
        }

        public class ServerAttributes
        {
            [JsonProperty("appversion")]
            public string Appversion
            {
                get;
                set;
            }

            [JsonProperty("version")]
            public string Version
            {
                get;
                set;
            }

            [JsonProperty("title")]
            public string Title
            {
                get;
                set;
            }

            [JsonProperty("strapline")]
            public string Strapline
            {
                get;
                set;
            }

            [JsonProperty("email")]
            public string Email
            {
                get;
                set;
            }

            [JsonProperty("url")]
            public string Url
            {
                get;
                set;
            }
        }

        public class Server
        {
            [JsonProperty("@attributes")]
            public ServerAttributes Attributes
            {
                get;
                set;
            }
        }

        public class LimitsAttributes
        {
            [JsonProperty("max")]
            public string Max
            {
                get;
                set;
            }

            [JsonProperty("default")]
            public string Default
            {
                get;
                set;
            }
        }

        public class Limits
        {
            [JsonProperty("@attributes")]
            public LimitsAttributes Attributes
            {
                get;
                set;
            }
        }

        public class RetentionAttributes
        {
            [JsonProperty("days")]
            public string Days
            {
                get;
                set;
            }
        }

        public class Retention
        {
            [JsonProperty("@attributes")]
            public RetentionAttributes Attributes
            {
                get;
                set;
            }
        }

        public class SearchAttributes
        {
            [JsonProperty("available")]
            public string Available
            {
                get;
                set;
            }
        }

        public class CapsSearch
        {
            [JsonProperty("@attributes")]
            public SearchAttributes Attributes
            {
                get;
                set;
            }
        }

        public class CapsTvSearchAttributes
        {
            [JsonProperty("available")]
            public string Available
            {
                get;
                set;
            }
        }

        public class CapsTvSearch
        {
            [JsonProperty("@attributes")]
            public CapsTvSearchAttributes Attributes
            {
                get;
                set;
            }
        }

        public class CapsMovieSearchAttributes
        {
            [JsonProperty("available")]
            public string Available
            {
                get;
                set;
            }
        }

        public class CapsMovieSearch
        {
            [JsonProperty("@attributes")]
            public CapsMovieSearchAttributes Attributes
            {
                get;
                set;
            }
        }

        public class CapsAudioSearchAttributes
        {
            [JsonProperty("available")]
            public string Available
            {
                get;
                set;
            }
        }

        public class CapsAudioSearch
        {
            [JsonProperty("@attributes")]
            public CapsAudioSearchAttributes Attributes
            {
                get;
                set;
            }
        }

        public class Searching
        {
            [JsonProperty("search")]
            public CapsSearch Search
            {
                get;
                set;
            }

            [JsonProperty("tv-search")]
            public CapsTvSearch TvSearch
            {
                get;
                set;
            }

            [JsonProperty("movie-search")]
            public CapsMovieSearch MovieSearch
            {
                get;
                set;
            }

            [JsonProperty("audio-search")]
            public CapsAudioSearch AudioSearch
            {
                get;
                set;
            }
        }

        public class CapsCategoryAttributes
        {
            [JsonProperty("id")]
            public string Id
            {
                get;
                set;
            }

            [JsonProperty("name")]
            public string Name
            {
                get;
                set;
            }

            [JsonProperty("description")]
            public string Description
            {
                get;
                set;
            }
        }

        public class SubcatAttributes
        {
            [JsonProperty("id")]
            public string Id
            {
                get;
                set;
            }

            [JsonProperty("name")]
            public string Name
            {
                get;
                set;
            }

            [JsonProperty("description")]
            public string Description
            {
                get;
                set;
            }
        }

        public class Subcat
        {
            [JsonProperty("@attributes")]
            public SubcatAttributes Attributes
            {
                get;
                set;
            }
        }

        public class CapsCategory
        {
            [JsonProperty("@attributes")]
            public CapsCategoryAttributes Attributes
            {
                get;
                set;
            }

            [JsonProperty("subcat")]
            public List<Subcat> Subcat
            {
                get;
                set;
            }
        }

        public class Categories
        {
            [JsonProperty("category")]
            public List<CapsCategory> Category
            {
                get;
                set;
            }
        }

        public class GroupAttributes
        {
            [JsonProperty("id")]
            public string Id
            {
                get;
                set;
            }

            [JsonProperty("name")]
            public string Name
            {
                get;
                set;
            }

            [JsonProperty("lastupdate")]
            public string Lastupdate
            {
                get;
                set;
            }
        }

        public class Group
        {
            [JsonProperty("@attributes")]
            public GroupAttributes Attributes
            {
                get;
                set;
            }
        }

        public class Groups
        {
            [JsonProperty("group")]
            public List<Group> Group
            {
                get;
                set;
            }
        }

        public class GenreAttributes
        {
            [JsonProperty("id")]
            public string Id
            {
                get;
                set;
            }

            [JsonProperty("categoryid")]
            public string Categoryid
            {
                get;
                set;
            }

            [JsonProperty("name")]
            public string Name
            {
                get;
                set;
            }
        }

        public class Genre
        {
            [JsonProperty("@attributes")]
            public GenreAttributes Attributes
            {
                get;
                set;
            }
        }

        public class Genres
        {
            [JsonProperty("genre")]
            public List<Genre> Genre
            {
                get;
                set;
            }
        }

        public class CapsRoot
        {
            [JsonProperty("server")]
            public Server Server
            {
                get;
                set;
            }

            [JsonProperty("limits")]
            public Limits Limits
            {
                get;
                set;
            }

            [JsonProperty("retention")]
            public Retention Retention
            {
                get;
                set;
            }

            [JsonProperty("searching")]
            public Searching Searching
            {
                get;
                set;
            }

            [JsonProperty("categories")]
            public Categories Categories
            {
                get;
                set;
            }

            [JsonProperty("groups")]
            public Groups Groups
            {
                get;
                set;
            }

            [JsonProperty("genres")]
            public Genres Genres
            {
                get;
                set;
            }
        }

        private CapsRoot _caps;

        private Uri Uri
        {
            get;
        }

        public NewzNab(Uri uri, string token)
        {
            Uri = new UriBuilder(uri)
            {
                Query = "apikey=" + token + "&o=json"
            }.Uri;
        }

        private async Task<string> GetMethod(string method, Dictionary<string, string> parametersDictionary = null)
        {
            var parameters = ((parametersDictionary != null) ? ("&" + string.Join("&", parametersDictionary.Select((KeyValuePair<string, string> kv) => kv.Key + "=" + kv.Value))) : string.Empty);
            using var client = HttpClientFactory.Create(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            });
            var uriBuilder = new UriBuilder(Uri);
            uriBuilder.Query = uriBuilder.Query + "&t=" + method + parameters;
            client.Timeout = TimeSpan.FromMinutes(5.0);
            Debug.WriteLine($"NewzNab - Requesting url {uriBuilder.Uri}");
            var response = await client.GetAsync(uriBuilder.Uri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new WebException($"Error retrieving data {response.StatusCode}");
        }

        public async Task<SearchRoot> Search(string query = null, string[] groups = null, int returnLimit = 100, string[] categories = null, string[] attributes = null, bool extended = false, int maxAgeDays = 1, int offset = 0)
        {
            return await Search(query, (groups != null) ? string.Join(",", groups) : null, returnLimit, (categories != null) ? string.Join(",", categories) : null, (attributes != null) ? string.Join(",", attributes) : null, extended, maxAgeDays, offset);
        }

        public async Task<SearchRoot> Search(string query = null, string groups = null, int returnLimit = 100, string categories = null, string attributes = null, bool extended = false, int maxAgeDays = 1, int offset = 0)
        {
            var parametersDictionary = new Dictionary<string, string>
            {
                {
                    "limit",
                    returnLimit.ToString()
                },
                {
                    "maxage",
                    maxAgeDays.ToString()
                }
            };
            if (query != null)
            {
                parametersDictionary.Add("q", query);
            }
            if (groups != null && groups.Length > 0)
            {
                parametersDictionary.Add("group", groups);
            }
            if (categories != null && categories.Length > 0)
            {
                parametersDictionary.Add("cat", categories);
            }
            if (attributes != null && attributes.Length > 0)
            {
                parametersDictionary.Add("attrs", attributes);
            }
            if (extended)
            {
                parametersDictionary.Add("extended", "1");
            }
            if (offset > 0)
            {
                parametersDictionary.Add("offset", offset.ToString());
            }
            var response = await GetMethod("search", parametersDictionary);
            try
            {
                return JsonConvert.DeserializeObject<SearchRoot>(response);
            }
            catch (Exception e)
            {
                throw new Exception(response, e);
            }
        }

        public async Task<SearchRoot> TvSearch(string query = null, string groups = null, int returnLimit = 100, string categories = null, string attributes = null, bool extended = false, int maxAgeDays = 1, int offset = 0, string season = null, string episode = null, string rageId = null, string tvdbId = null)
        {
            var parametersDictionary = new Dictionary<string, string>
            {
                {
                    "limit",
                    returnLimit.ToString()
                },
                {
                    "maxage",
                    maxAgeDays.ToString()
                }
            };
            if (query != null)
            {
                parametersDictionary.Add("q", query);
            }
            if (groups != null && groups.Length > 0)
            {
                parametersDictionary.Add("group", groups);
            }
            if (categories != null && categories.Length > 0)
            {
                parametersDictionary.Add("cat", categories);
            }
            if (attributes != null && attributes.Length > 0)
            {
                parametersDictionary.Add("attrs", attributes);
            }
            if (extended)
            {
                parametersDictionary.Add("extended", "1");
            }
            if (season != null)
            {
                parametersDictionary.Add("season", season);
            }
            if (episode != null)
            {
                parametersDictionary.Add("ep", episode);
            }
            if (rageId != null)
            {
                parametersDictionary.Add("rid", rageId);
            }
            if (tvdbId != null)
            {
                parametersDictionary.Add("tvdbid", tvdbId);
            }
            if (offset > 0)
            {
                parametersDictionary.Add("offset", offset.ToString());
            }
            var response = await GetMethod("tvsearch", parametersDictionary);
            try
            {
                return JsonConvert.DeserializeObject<SearchRoot>(response);
            }
            catch (Exception e)
            {
                throw new Exception(response, e);
            }
        }

        public async Task<SearchRoot> MovieSearch(string query = null, string groups = null, int returnLimit = 100, string categories = null, string attributes = null, bool extended = false, int maxAgeDays = 1, int offset = 0, string genre = null, string imdbId = null)
        {
            var parametersDictionary = new Dictionary<string, string>
            {
                {
                    "limit",
                    returnLimit.ToString()
                },
                {
                    "maxage",
                    maxAgeDays.ToString()
                }
            };
            if (query != null)
            {
                parametersDictionary.Add("q", query);
            }
            if (groups != null && groups.Length > 0)
            {
                parametersDictionary.Add("group", groups);
            }
            if (categories != null && categories.Length > 0)
            {
                parametersDictionary.Add("cat", categories);
            }
            if (attributes != null && attributes.Length > 0)
            {
                parametersDictionary.Add("attrs", attributes);
            }
            if (extended)
            {
                parametersDictionary.Add("extended", "1");
            }
            if (genre != null)
            {
                parametersDictionary.Add("genre", genre);
            }
            if (imdbId != null)
            {
                parametersDictionary.Add("imdbid", imdbId);
            }
            if (offset > 0)
            {
                parametersDictionary.Add("offset", offset.ToString());
            }
            var response = await GetMethod("movie", parametersDictionary);
            try
            {
                return JsonConvert.DeserializeObject<SearchRoot>(response);
            }
            catch (Exception e)
            {
                throw new Exception(response, e);
            }
        }

        public async Task<SearchRoot> MusicSearch(string query = null, string groups = null, int returnLimit = 100, string categories = null, string attributes = null, bool extended = false, int maxAgeDays = 1, int offset = 0, string album = null, string artist = null, string label = null, string track = null, string year = null, string genre = null)
        {
            var parametersDictionary = new Dictionary<string, string>
            {
                {
                    "limit",
                    returnLimit.ToString()
                },
                {
                    "maxage",
                    maxAgeDays.ToString()
                }
            };
            if (query != null)
            {
                parametersDictionary.Add("q", query);
            }
            if (groups != null && groups.Length > 0)
            {
                parametersDictionary.Add("group", groups);
            }
            if (categories != null && categories.Length > 0)
            {
                parametersDictionary.Add("cat", categories);
            }
            if (attributes != null && attributes.Length > 0)
            {
                parametersDictionary.Add("attrs", attributes);
            }
            if (extended)
            {
                parametersDictionary.Add("extended", "1");
            }
            if (genre != null)
            {
                parametersDictionary.Add("genre", genre);
            }
            if (album != null)
            {
                parametersDictionary.Add("album", album);
            }
            if (artist != null)
            {
                parametersDictionary.Add("artist", artist);
            }
            if (label != null)
            {
                parametersDictionary.Add("label", label);
            }
            if (track != null)
            {
                parametersDictionary.Add("track", track);
            }
            if (year != null)
            {
                parametersDictionary.Add("year", year);
            }
            if (offset > 0)
            {
                parametersDictionary.Add("offset", offset.ToString());
            }
            var response = await GetMethod("music", parametersDictionary);
            try
            {
                return JsonConvert.DeserializeObject<SearchRoot>(response);
            }
            catch (Exception e)
            {
                throw new Exception(response, e);
            }
        }

        public async Task<CapsRoot> GetCaps()
        {
            if (_caps == null)
            {
                var response = await GetMethod("caps");
                try
                {
                    _caps = JsonConvert.DeserializeObject<CapsRoot>(response);
                }
                catch (Exception)
                {
                }
            }
            return _caps;
        }
    }
}
