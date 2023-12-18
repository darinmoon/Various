using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Concurrency
{
    public class ConcurrentDictionary : IDisposable
    {
        private const int DEFAULT_ARRAY_SIZE = 2048;
        private const int DEFAULT_LOCKS = 128;

        private object[] _locks = new object[DEFAULT_LOCKS];
        private Bucket[] _buckets = new Bucket[DEFAULT_ARRAY_SIZE];
        private IEqualityComparer<string> _comparer = EqualityComparer<string>.Default;
        //private LockPool _lockPool = new LockPool(DEFAULT_LOCKS);
        private bool _disposed = false;

        public ConcurrentDictionary()
        {
            for (int i = 0; i < _locks.Length; i++)
            {
                _locks[i] = new object();
            }
        }

        ~ConcurrentDictionary()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Insert(string key, int value)
        {
            Insert(key, value, true);
        }

        public void Delete(string key)
        {
            int hash = GetBucketHash(key);
            lock (_locks[GetLockHash(hash)])
            {
                Bucket bucket = _buckets[hash];
                if (bucket == null)
                {
                    throw new KeyNotFoundException(key);
                }
                _buckets[hash] = bucket.Delete(key);
                _buckets[hash].CalcLength();
            }
        }

        //public int Search(string key)
        //{
        //    int hash = GetBucketHash(key);
        //    lock (_locks[GetLockHash(hash)])
        //    {
        //        Bucket bucket = _buckets[hash];
        //        if (bucket == null)
        //        {
        //            throw new KeyNotFoundException(key);
        //        }
        //        return bucket.Search(key);
        //    }
        //}

        public int Search(string key)
        {
            int hash = GetBucketHash(key);
            lock (_locks[GetLockHash(hash)])
            {
                Bucket start = _buckets[hash];
                if (start == null)
                {
                    throw new KeyNotFoundException(key);
                }
                Dictionary<string, Bucket> dict = CreateDict(start);
                if (dict.TryGetValue(key, out Bucket found))
                {
                    return found.Value;
                }
                throw new KeyNotFoundException(key);
            }
        }

        private Dictionary<string, Bucket> CreateDict(Bucket bucket)
        {
            Dictionary<string, Bucket> dict = new Dictionary<string, Bucket>();
            while (bucket != null)
            {
                dict.Add(bucket.Key, bucket);
                bucket = bucket.NextBucket;
            }
            return dict;
        }

        public int Size()
        {
            int size = 0;
            for (int i = 0; i < _buckets.Length; i++)
            {
                lock (_locks[GetLockHash(i)])
                {
                    Bucket bucket = _buckets[i];
                    if (bucket != null)
                    {
                        size += bucket.Length;
                    }
                }
            }
            return size;
        }

        public int Max()
        {
            int max = 0;
            for (int i = 0; i < _buckets.Length; i++)
            {
                lock (_locks[GetLockHash(i)])
                {
                    if (_buckets[i] != null)
                    {
                        if (max < _buckets[i].Max)
                        {
                            max = _buckets[i].Max;
                        }
                    }
                }
            }
            return max;
        }

        public int Min()
        {
            int min = 0;
            for (int i = 0; i < _buckets.Length; i++)
            {
                lock (_locks[GetLockHash(i)])
                {
                    if (_buckets[i] != null)
                    {
                        if (min > _buckets[i].Max)
                        {
                            min = _buckets[i].Max;
                        }
                    }
                }
            }
            return min;
        }

        public static ConcurrentDictionary Build(List<Tuple<string, int>> input)
        {
            ConcurrentDictionary dict = new ConcurrentDictionary();

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            foreach (var pair in input)
            {
                dict.Insert(pair.Item1, pair.Item2, false);
            }

            foreach (Bucket b in dict._buckets)
            {
                if (b != null)
                {
                    b.CalcLength();
                }
            }

            return dict;
        }

        #region Private Methods

        private void Insert(string key, int value, bool threadSafe)
        {
            int hash = GetBucketHash(key);
            Bucket bucket = new Bucket(key, value);

            if (threadSafe)
            {
                lock (_locks[GetLockHash(hash)])
                {
                    Insert(hash, bucket, true);
                }
            }
            else
            {
                Insert(hash, bucket, false);
            }
        }

        private void Insert(int hash, Bucket bucket, bool calcLength)
        {
            Bucket current = _buckets[hash];
            if (current == null)
            {
                _buckets[hash] = bucket;
            }
            else
            {
                _buckets[hash] = current.Insert(bucket);
            }
            if (calcLength)
            {
                _buckets[hash].CalcLength();
            }
        }

        private int GetBucketHash(string key)
        {
            //int a = _comparer.GetHashCode(key);
            //int b = a & 0x7fffffff;
            //int hash = b % _buckets.Length;
            return (_comparer.GetHashCode(key) & 0x7fffffff) % _buckets.Length;
        }

        private int GetLockHash(int bucketHash)
        {
            return bucketHash % _locks.Length;
        }

        private Bucket FindMidPointBucket(Bucket start, int midPt)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }

            Bucket midBucket = start;
            for (int i = 0; i < midPt; i++)
            {
                midBucket = start.NextBucket;
            }

            return midBucket;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                this._comparer = null;

                if (_buckets != null)
                {
                    for (int i = 0; i < _buckets.Length; i++)
                    {
                        Bucket bucket = _buckets[i];
                        if (bucket != null)
                        {
                            bucket.Dispose();
                            _buckets[i] = null;
                        }
                    }
                    _buckets = null;
                }

                if (_locks != null)
                {
                    for (int i = 0; i < _locks.Length; i++)
                    {
                        if (_locks[i] != null)
                        {
                            _locks[i] = null;
                        }
                    }
                    _locks = null;
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
