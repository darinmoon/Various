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
        private const int DEFAULT_INITIAL_SIZE = 40960;
        private const int DEFAULT_LOCKS = 128;

        private object[] _locks = new object[DEFAULT_LOCKS];
        private Bucket[] _buckets = null;
        private IEqualityComparer<string> _comparer = EqualityComparer<string>.Default;
        //private LockPool _lockPool = new LockPool(DEFAULT_LOCKS);
        private bool _disposed = false;

        public ConcurrentDictionary()
            : this(DEFAULT_INITIAL_SIZE)
        {
        }

        public ConcurrentDictionary(int initialSize)
        {
            int arraySize = initialSize / 20;
            _buckets = new Bucket[arraySize];

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

        public string SearchValue(bool first, bool last)
        {
            int fifth = _buckets.Length / 5;
            Bucket bucket = _buckets[fifth];
            if (first)
            {
                return bucket.Key;
            }
            else if (last)
            {
                return GetEndBucket(bucket).Key;
            }

            int length = bucket.Length;
            int midPt = length / 2;
            for (int i = 0; i < midPt; i++)
            {
                bucket = bucket.NextBucket;
            }
            return bucket.Key;
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
                _buckets[hash].CalcSuperlatives();
            }
        }

        private Bucket[] MakeBucketArray(Bucket start)
        {
            Bucket[] buckets = new Bucket[start.Length];
            Bucket bucket = start;
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = bucket;
                bucket = bucket.NextBucket;
                if (bucket == null)
                {
                    break;
                }
            }
            return buckets;
        }

        private string[] MakeKeyArray(Bucket[] buckets)
        {
            string[] keys = new string[buckets.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (buckets[i] != null)
                {
                    keys[i] = buckets[i].Key;
                }
            }

            return keys;
        }

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
                Bucket[] buckets = new Bucket[start.Length];
                string[] keys = new string[start.Length];

                Bucket bucket = start;
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = bucket;
                    keys[i] = bucket.Key;
                    bucket = bucket.NextBucket;
                    if (bucket == null)
                    {
                        break;
                    }
                }


                int idx = Array.BinarySearch<string>(keys, key);
                if (idx < 0)
                {
                    throw new KeyNotFoundException(key);
                }

                return buckets[idx].Value;
            }
        }

        //public int Search(string key)
        //{
        //    int hash = GetBucketHash(key);
        //    lock (_locks[GetLockHash(hash)])
        //    {
        //        Bucket start = _buckets[hash];
        //        if (start == null)
        //        {
        //            throw new KeyNotFoundException(key);
        //        }
        //        Bucket end = GetEndBucket(start);
        //        Bucket match = FindMidPointBucket(start, end, key);

        //        if (match == null)
        //        {
        //            throw new KeyNotFoundException(key);
        //        }

        //        return match.Value;
        //    }
        //}

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

        //public int Search(string key)
        //{
        //    int hash = GetBucketHash(key);
        //    lock (_locks[GetLockHash(hash)])
        //    {
        //        Bucket start = _buckets[hash];
        //        if (start == null)
        //        {
        //            throw new KeyNotFoundException(key);
        //        }
        //        Dictionary<string, Bucket> dict = CreateDict(start);
        //        if (dict.TryGetValue(key, out Bucket found))
        //        {
        //            return found.Value;
        //        }
        //        throw new KeyNotFoundException(key);
        //    }
        //}

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
                        int newMax = _buckets[i].Max;
                        if (max < newMax)
                        {
                            max = newMax;
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
                        int newMin = _buckets[i].Min;
                        if (min > newMin)
                        {
                            min = newMin;
                        }
                    }
                }
            }
            return min;
        }

        public static ConcurrentDictionary Build(List<Tuple<string, int>> input)
        {
            ConcurrentDictionary dict = new ConcurrentDictionary(input.Count);

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
                    b.CalcSuperlatives();
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

        private void Insert(int hash, Bucket newBucket, bool calcSuperlatives)
        {
            Bucket start = _buckets[hash];
            if (start == null)
            {
                _buckets[hash] = newBucket;
            }
            else
            {
                Bucket[] buckets = new Bucket[start.Length];
                string[] keys = new string[start.Length];

                Bucket bucket = start;
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = bucket;
                    keys[i] = bucket.Key;
                    bucket = bucket.NextBucket;
                    if (bucket == null)
                    {
                        break;
                    }
                }

                if (start.Length > 10)
                {
                    int x;
                    x = 0;
                }


                int idx = Array.BinarySearch<string>(keys, newBucket.Key);
                if (idx >= 0)
                {
                    buckets[idx].Value = newBucket.Value;
                }
                else 
                {
                    int i;
                    for (i = 0; i < keys.Length; i++)
                    {
                        int comp = newBucket.Key.CompareTo(keys[i]);
                        if (comp == 0)
                        {
                            throw new Exception("Oops");
                        }
                        else if (comp < 0)
                        {
                            newBucket.NextBucket = buckets[i];
                            if (i == 0)
                            {
                                _buckets[hash] = newBucket;
                            }
                            else
                            {
                                buckets[i - 1].NextBucket = newBucket;
                            }
                            break;
                        }
                    }
                    if (i == keys.Length)
                    {
                        buckets[keys.Length - 1].NextBucket = newBucket;
                    }
                }

                if (calcSuperlatives)
                {
                    _buckets[hash].CalcSuperlatives();
                }

            }

        }

        //private void Insert(int hash, Bucket newBucket, bool calcLength)
        //{
        //    Bucket start = _buckets[hash];
        //    if (start == null)
        //    {
        //        _buckets[hash] = newBucket;
        //    }
        //    else
        //    {
        //        _buckets[hash] = start.Insert(newBucket);
        //    }
        //    if (calcLength)
        //    {
        //        _buckets[hash].CalcLength();
        //    }
        //}

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

        private Bucket FindMidPointBucket(Bucket start, Bucket end, string key)
        {
            if (start == null && end == null)
            {
                throw new ArgumentNullException("start and end");
            }
            else if (start == null)
            {
                if (end.Key.CompareTo(key) == 0)
                {
                    return end;
                }
                return null;
            }
            else if (end == null)
            {
                if (start.Key.CompareTo(key) == 0)
                {
                    return start;
                }
                return null;
            }

            int length = GetBucketLength(start, end);
            int midPt = length / 2;

            Bucket midBucket = start;
            for (int i = 0; i < midPt; i++)
            {
                midBucket = midBucket.NextBucket;
            }

            int comp = midBucket.Key.CompareTo(key);
            if (comp == 0)
            {
                return midBucket;
            }
            else if (comp < 0)
            {
                if (Bucket.ReferenceEquals(start, midBucket))
                {
                    return FindMidPointBucket(null, end, key);
                }
                return FindMidPointBucket(midBucket, end, key);
            }
            else
            {
                if (Bucket.ReferenceEquals(end, midBucket))
                {
                    return FindMidPointBucket(start, null, key);
                }
                return FindMidPointBucket(start, midBucket, key);
            }

            return midBucket;
        }

        //private Bucket FindMidPointBucket(Bucket start, Bucket end, string key)
        //{
        //    if (start == null && end == null)
        //    {
        //        throw new ArgumentNullException("start and end");
        //    }
        //    else if (start == null)
        //    {
        //        if (end.Key.CompareTo(key) == 0)
        //        {
        //            return end;
        //        }
        //        return null;
        //    }
        //    else if (end == null)
        //    {
        //        if (start.Key.CompareTo(key) == 0)
        //        {
        //            return start;
        //        }
        //        return null;
        //    }

        //    int length = GetBucketLength(start, end);
        //    int midPt = length / 2;

        //    Bucket midBucket = start;
        //    for (int i = 0; i < midPt; i++)
        //    {
        //        midBucket = midBucket.NextBucket;
        //    }

        //    int comp = midBucket.Key.CompareTo(key); 
        //    if (comp == 0)
        //    {
        //        return midBucket;
        //    }
        //    else if (comp < 0)
        //    {
        //        if (Bucket.ReferenceEquals(start, midBucket))
        //        {
        //            return FindMidPointBucket(null, end, key);
        //        }
        //        return FindMidPointBucket(midBucket, end, key);
        //    }
        //    else
        //    {
        //        if (Bucket.ReferenceEquals(end, midBucket))
        //        {
        //            return FindMidPointBucket(start, null, key);
        //        }
        //        return FindMidPointBucket(start, midBucket, key);
        //    }

        //    return midBucket;
        //}

        private int GetBucketLength(Bucket start, Bucket end)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            int length = 1;
            Bucket bucket = start;
            while (bucket != null && !Bucket.ReferenceEquals(bucket, end))
            {
                if (bucket.NextBucket == null)
                {
                    break;
                }
                length++;
                bucket = bucket.NextBucket;
            }

            return length;
        }

        private Bucket GetEndBucket(Bucket start)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }

            Bucket end = start;
            while (end.NextBucket != null)
            {
                end = end.NextBucket;
            }
            return end;
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
