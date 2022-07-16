using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Manager
{
    public class TagCache
    {
        private ReaderWriterLockSlim cacheLock = new();
        private Dictionary<string, Da.OpcItem> innerCache = new();
        public int Count
        {
            get
            {
                return innerCache.Count;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return innerCache.Count == 0;
            }
        }
        public object? Read(string tagName)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache[tagName].Value;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
        public void Add(string tagName, Da.OpcItem value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Add(tagName, value);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
        public void Add(Da.OpcItem value)
        {
            Add(value.Name, value);
        }
        public bool AddWithTimeout(string tagName, Da.OpcItem value, int timeout)
        {
            if (cacheLock.TryEnterWriteLock(timeout))
            {
                try
                {
                    innerCache.Add(tagName, value);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool AddWithTimeout(Da.OpcItem value, int timeout)
        {
            return AddWithTimeout(value.Name, value, timeout);
        }
        public AddOrUpdateStatus AddOrUpdate(string tagName, Da.OpcItem value)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                Da.OpcItem? result = null;
                if (innerCache.TryGetValue(tagName, out result))
                {
                    if (result == value)
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    else
                    {
                        cacheLock.EnterWriteLock();
                        try
                        {
                            innerCache[tagName] = value;
                        }
                        finally
                        {
                            cacheLock.ExitWriteLock();
                        }
                        return AddOrUpdateStatus.Updated;
                    }
                }
                else
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        innerCache.Add(tagName, value);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }
        public AddOrUpdateStatus AddOrUpdate(Da.OpcItem value)
        {
            return AddOrUpdate(value.Name, value);
        }
        public string[] GetAllTagNames()
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache.Keys.ToArray();
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }
        public void Delete(string tagName)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Remove(tagName);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }
        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        };      
        ~TagCache()
        {
            if (cacheLock != null) cacheLock.Dispose();
            if (innerCache != null) innerCache.Clear();
        }
    }
}
