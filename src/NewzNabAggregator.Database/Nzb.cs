using System;

namespace NewzNabAggregator.Database
{
    public class Nzb
    {
        public Guid id
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

        public string pubDate
        {
            get;
            set;
        }

        public long length
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
}
