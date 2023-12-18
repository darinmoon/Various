using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Concurrency
{
    internal class Bucket : IDisposable
    {
        public bool _disposed = false;
        public string Key { get; private set; }
        public int Value { get; set; }
        //public int Hashcode { get; private set; }
        public Bucket NextBucket { get; set; } = null;

        public int Length { get; private set; }

        public int Max
        {
            get 
            {
                if (NextBucket == null)
                {
                    return Value;
                }
                return (Value >= NextBucket.Value) ? Value : NextBucket.Value;
            }
        }

        public int Min
        {
            get
            {
                if (NextBucket == null)
                {
                    return Value;
                }
                return (Value <= NextBucket.Value) ? Value : NextBucket.Value;
            }
        }

        internal Bucket(string key, int value)
        {
            this.Key = key;
            this.Value = value;
            //this.Hashcode = hashcode;
            CalcLength();
        }

        ~Bucket()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void CalcLength()
        {
            if (NextBucket == null)
            {
                Length = 1;
            }
            else
            {
                NextBucket.CalcLength();
                Length = 1 + NextBucket.Length;
            }
            //Length = (NextNode == null) ? 1 : 1 + NextNode.Length;
        }

        public Bucket Insert(Bucket bucket)
        {
            int comp = Key.CompareTo(bucket.Key);
            if (comp == 0)
            {
                Value = bucket.Value;
                //Interlocked.Increment(ref collisionCnt);
                return this;
            }
            else if (comp > 0)
            {
                bucket.NextBucket = this;
                return bucket;
            }
            else if (NextBucket == null)
            {
                NextBucket = bucket;
            }
            else
            {
                NextBucket = NextBucket.Insert(bucket);
            }
            return this;
        }

        public Bucket Delete(string key)
        {
            int comp = Key.CompareTo(key);
            if (comp == 0)
            {
                return NextBucket;
            }
            else if (comp > 0 || NextBucket == null)
            {
                throw new KeyNotFoundException(key);
            }
            NextBucket = NextBucket.Delete(key);
            return this;
        }

        public int Search(string key)
        {
            int comp = Key.CompareTo(key);
            if (comp == 0)
            {
                return Value;
            }
            else if (comp > 0 || NextBucket == null)
            {
                throw new KeyNotFoundException(key);
            }
            return NextBucket.Search(key);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed) 
            {
                if (disposing) 
                {
                }
                if (NextBucket != null)
                {
                    NextBucket.Dispose();
                    NextBucket = null;
                    Key = null;
                }
                _disposed = true;
            }
        }
    }
}
