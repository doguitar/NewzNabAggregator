using LiteDB;
using System;
using System.IO;

namespace NewzNabAggregator.Database
{
    public class Database : IDisposable
    {
        private LiteDatabase _db;

        private bool _disposed;

        private ILiteCollection<Nzb> Nzbs
        {
            get;
        }

        public Database(string databasePath)
        {
            var file = new FileInfo(databasePath);
            if (!file.Directory!.Exists)
            {
                file.Directory!.Create();
            }
            _db = new LiteDatabase(databasePath);
            Nzbs = _db.GetCollection<Nzb>("nzbs");
            Nzbs.EnsureIndex((Nzb n) => n.id);
            Nzbs.EnsureIndex((Nzb n) => n.link);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
                _disposed = true;
            }
        }

        public Nzb GetNzb(Guid id)
        {
            return Nzbs.FindOne((Nzb n) => n.id == id);
        }

        public Nzb SaveNzb(Nzb nzb)
        {
            var existing = Nzbs.FindOne((Nzb n) => n.link == nzb.link);
            if (existing == null)
            {
                nzb.id = Guid.NewGuid();
                Nzbs.Insert(nzb);
                return nzb;
            }
            Nzbs.Update(nzb);
            return existing;
        }
    }
}
