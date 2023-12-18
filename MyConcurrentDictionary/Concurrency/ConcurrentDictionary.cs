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
        private const int DEFAULT_ARRAY_SIZE = 16384;
        private const int DEFAULT_LOCKS = 128;

        private object[] _locks = new object[DEFAULT_LOCKS];
        private Bucket[] _buckets = null;
        private IEqualityComparer<string> _comparer = EqualityComparer<string>.Default;
        private bool _disposed = false;

        public ConcurrentDictionary()
        {
            _buckets = new Bucket[DEFAULT_ARRAY_SIZE];
            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = new Bucket();
            }

            for (int i = 0; i < _locks.Length; i++)
            {
                _locks[i] = new object();
            }
        }

        public ConcurrentDictionary(int initialSize)
        {
            int arraySize = initialSize / 20;
            _buckets = new Bucket[arraySize];
            for(int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = new Bucket();
            }

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
            GC.SuppressFinalize(this);
        }

        public string SearchValue(bool first, bool last)
        {
            int fifth = _buckets.Length / 5;
            Bucket bucket = _buckets[fifth];
            if (first)
            {
                return bucket.FirstNode.Key;
            }
            else if (last)
            {
                return bucket.Keys[bucket.Keys.Length - 1]; // GetEndBucket(bucket).Key;
            }

            int length = bucket.Keys.Length;
            int midPt = length / 2;
            string key = bucket.Keys[midPt - 1];
            return key;
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
                if (bucket.FirstNode == null)
                {
                    throw new KeyNotFoundException(key);
                }
                bucket.Delete(key);
            }
        }

        public int Search(string key)
        {
            int hash = GetBucketHash(key);
            lock (_locks[GetLockHash(hash)])
            {
                return _buckets[hash].Search(key);
            }
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
                        if (newMax > max)
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
                        if (newMin < min)
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

            return dict;
        }

        #region Private Methods

        private void Insert(string key, int value, bool threadSafe)
        {
            int hash = GetBucketHash(key);
            Node node = new Node(key, value);

            if (threadSafe)
            {
                lock (_locks[GetLockHash(hash)])
                {
                    Insert(hash, node, true);
                }
            }
            else
            {
                Insert(hash, node, true);
            }
        }

        private void Insert(int hash, Node newNode, bool recalculate)
        {
            Bucket bucket = _buckets[hash];
            bucket.Insert(newNode, recalculate);
        }

        private int GetBucketHash(string key)
        {
            return (_comparer.GetHashCode(key) & 0x7fffffff) % _buckets.Length;
        }

        private int GetLockHash(int bucketHash)
        {
            return bucketHash % _locks.Length;
        }

        private Node FindMidPointBucket(Node start, Node end, string key)
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

            Node midBucket = start;
            for (int i = 0; i < midPt; i++)
            {
                midBucket = midBucket.NextNode;
            }

            int comp = midBucket.Key.CompareTo(key);
            if (comp == 0)
            {
                return midBucket;
            }
            else if (comp < 0)
            {
                if (Node.ReferenceEquals(start, midBucket))
                {
                    return FindMidPointBucket(null, end, key);
                }
                return FindMidPointBucket(midBucket, end, key);
            }
            else
            {
                if (Node.ReferenceEquals(end, midBucket))
                {
                    return FindMidPointBucket(start, null, key);
                }
                return FindMidPointBucket(start, midBucket, key);
            }
        }

        private int GetBucketLength(Node start, Node end)
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
            Node bucket = start;
            while (bucket != null && !Node.ReferenceEquals(bucket, end))
            {
                if (bucket.NextNode == null)
                {
                    break;
                }
                length++;
                bucket = bucket.NextNode;
            }

            return length;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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

                }
                _disposed = true;
            }
        }

        #endregion
    }
}
