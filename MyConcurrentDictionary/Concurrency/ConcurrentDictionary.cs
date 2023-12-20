using System;
using System.Collections.Generic;
using System.Threading;

namespace Concurrency
{
    public class ConcurrentDictionary : IDisposable
    {
        #region Private Constants

        private const int DEFAULT_ARRAY_SIZE = 16384;
        private const int DEFAULT_LOCKS = 128;
        private const int THREAD_COUNT = 10;

        #endregion

        #region Private Variables

        private object[] _locks = new object[DEFAULT_LOCKS];
        private Bucket[] _buckets = null;
        private IEqualityComparer<string> _comparer = EqualityComparer<string>.Default;
        private bool _disposed = false;

        #endregion

        #region Constructor and Finalizer

        public ConcurrentDictionary()
        {
            // initialize the buckets
            _buckets = new Bucket[DEFAULT_ARRAY_SIZE];
            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = new Bucket();
            }

            // initialize the locks
            for (int i = 0; i < _locks.Length; i++)
            {
                _locks[i] = new object();
            }
        }

        ~ConcurrentDictionary()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public Methods

        // This method is solely for testing purposes.
        // It searches a bucket for either the first
        // key, the last key, or a key in the middle
        // to search for.
        public string SearchValue(bool first, bool last, int bucketIdx)
        {
            while (bucketIdx > _buckets.Length - 1)
            {
                bucketIdx /= 2;
            }

            Bucket bucket = _buckets[bucketIdx];
            while (bucket.FirstNode == null)
            {
                bucket = _buckets[++bucketIdx];
            }

            if (first)
            {
                return bucket.FirstNode.Key;
            }
            else if (last)
            {
                return bucket.Keys[bucket.Keys.Length - 1];
            }

            int length = bucket.Keys.Length;
            int midPt = length / 2;
            string key = bucket.Keys[(midPt == 0) ? 0 : midPt - 1];

            return key;
        }

        public void Insert(string key, int value)
        {
            ThreadSafeInsert(key, value);
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
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            ConcurrentDictionary dict = new ConcurrentDictionary();

            int threadListCnt = input.Count / THREAD_COUNT;
            var lists = SplitList(input, threadListCnt);

            Thread[] threads = new Thread[THREAD_COUNT];
            for (int i = 0; i < threads.Length; i++)
            {
                int idx = i;
                threads[i] = new Thread(() => Insert(dict, lists[idx]));
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }

            Console.WriteLine($"Threads:            {threads.Length}");
            Console.WriteLine($"Total Inserts:      {input.Count}");

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            return dict;
        }

        #endregion

        #region Private Methods

        // Splits the list of tuples into multiple lists
        // in order to multi-thread the insertions of the
        // Build method
        private static List<List<Tuple<string, int>>> SplitList(List<Tuple<string, int>> kvPairs, int nSize)
        {
            var list = new List<List<Tuple<string, int>>>();

            for (int i = 0; i < kvPairs.Count; i += nSize)
            {
                list.Add(kvPairs.GetRange(i, Math.Min(nSize, kvPairs.Count - i)));
            }

            return list;
        }

        private static void Insert(ConcurrentDictionary dict, List<Tuple<string, int>> input)
        {
            try
            {
                foreach (var pair in input)
                {
                    dict.ThreadSafeInsert(pair.Item1, pair.Item2);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ThreadSafeInsert(string key, int value)
        {
            int hash = GetBucketHash(key);
            Node node = new Node(key, value);

            lock (_locks[GetLockHash(hash)])
            {
                Insert(hash, node);
            }
        }

        private void Insert(int hash, Node newNode)
        {
            Bucket bucket = _buckets[hash];
            bucket.Insert(newNode);
        }

        private int GetBucketHash(string key)
        {
            return (_comparer.GetHashCode(key) & 0x7fffffff) % _buckets.Length;
        }

        private int GetLockHash(int bucketHash)
        {
            return bucketHash % _locks.Length;
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
