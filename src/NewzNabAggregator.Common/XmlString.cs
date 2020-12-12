using System.Web;

namespace NewzNabAggregator.Common
{
    public static class XmlString
    {
        public static string Encode(string toEncode)
        {
            return HttpUtility.HtmlEncode(toEncode);
        }
    }
}
