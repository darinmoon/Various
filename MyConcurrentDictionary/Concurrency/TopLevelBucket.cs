using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Concurrency
{
    internal class TopLevelBucket : IDisposable
    {
        #region Private Variables

        private Bucket[] _buckets = null;
        private IEqualityComparer<string> _comparer = EqualityComparer<string>.Default;
        private bool _disposed = false;

        #endregion

        #region Constructor and Finalizer

        public TopLevelBucket(int size)
        {
            // initialize the buckets
            _buckets = new Bucket[size];
            for (int i = 0; i < _buckets.Length; i++)
            {
                _buckets[i] = new Bucket();
            }
        }

        ~TopLevelBucket()
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

        private Random rnd { get; set; } = new Random();
        // This method is solely for testing purposes.
        // It searches a bucket for either the first
        // key, the last key, or a key in the middle
        // to search for.
        public string SearchValue(bool first, bool last)
        {
            int startIdx = rnd.Next(0, _buckets.Length - 1);
            int bucketIdx = startIdx;
            Bucket bucket = _buckets[bucketIdx];
            while (bucketIdx < _buckets.Length && (bucket == null || bucket.Length == 0))
            {
                bucket = _buckets[bucketIdx++];
            }
            bucketIdx = startIdx - 1;
            while (bucketIdx >= 0 && (bucket == null || bucket.Length == 0))
            {
                bucket = _buckets[bucketIdx--];
            }


            if (first)
            {
                return bucket.Keys[0];
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

        public void Insert(Node node)
        {
            int hash = GetBucketHash(node.Key);
            Insert(hash, node);
        }

        public void Delete(string key)
        {
            int hash = GetBucketHash(key);
            Bucket bucket = _buckets[hash];
            if (bucket.Length == 0)
            {
                throw new KeyNotFoundException(key);
            }
            bucket.Delete(key);
        }

        public int Search(string key)
        {
            int hash = GetBucketHash(key);
            return _buckets[hash].Search(key);
        }

        public int Length()
        {
            int size = 0;
            for (int i = 0; i < _buckets.Length; i++)
            {
                Bucket bucket = _buckets[i];
                if (bucket != null)
                {
                    size += bucket.Length;
                }
            }
            return size;
        }

        public int Max()
        {
            int max = Int32.MinValue;
            for (int i = 0; i < _buckets.Length; i++)
            {
                Bucket bucket = _buckets[i];
                if (bucket.Length > 0)
                {
                    int newMax = bucket.Max;
                    if (newMax > max)
                    {
                        max = newMax;
                    }
                }
            }
            return max;
        }

        public int Min()
        {
            int min = Int32.MaxValue;
            for (int i = 0; i < _buckets.Length; i++)
            {
                Bucket bucket = _buckets[i];
                if (bucket.Length > 0)
                {
                    int newMin = bucket.Min;
                    if (newMin < min)
                    {
                        min = newMin;
                    }
                }
            }
            return min;
        }

        //public static TopLevelBucket Build(List<Tuple<string, int>> input)
        //{
        //    if (input == null)
        //    {
        //        throw new ArgumentNullException("input");
        //    }

        //    TopLevelBucket dict = new TopLevelBucket();

        //    int threadListCnt = input.Count / THREAD_COUNT;
        //    var lists = SplitList(input, threadListCnt);

        //    Thread[] threads = new Thread[THREAD_COUNT];
        //    for (int i = 0; i < threads.Length; i++)
        //    {
        //        int idx = i;
        //        threads[i] = new Thread(() => Insert(dict, lists[idx]));
        //    }

        //    for (int i = 0; i < threads.Length; i++)
        //    {
        //        threads[i].Start();
        //    }

        //    Console.WriteLine($"Threads:            {threads.Length}");
        //    Console.WriteLine($"Total Inserts:      {input.Count}");

        //    for (int i = 0; i < threads.Length; i++)
        //    {
        //        threads[i].Join();
        //    }

        //    return dict;
        //}

        #endregion

        #region Private Methods

        private void Insert(int hash, Node newNode)
        {
            Bucket bucket = _buckets[hash];
            bucket.Insert(newNode);
        }

        private int GetBucketHash(string key)
        {
            // Multiplying the has code by -1
            // ensures that we get a different hash
            // than the ConcurrentDictionary class got.
            return ((_comparer.GetHashCode(key) * -1) & 0x7fffffff) % _buckets.Length;
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
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
