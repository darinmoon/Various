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
    internal class Node : IDisposable
    {
        public bool _disposed = false;
        public string Key { get; private set; }
        public int Value { get; set; }
        public Node NextNode { get; set; } = null;

        public int Length
        {
            get
            {
                if (NextNode == null)
                {
                    return 1;
                }
                return 1 + NextNode.Length;
            }
        }

        public Node(string key, int value)
        {
            this.Key = key;
            this.Value = value;
        }

        ~Node()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Node Insert(Node bucket)
        {
            int comp = Key.CompareTo(bucket.Key);
            if (comp == 0)
            {
                Value = bucket.Value;
                return this;
            }
            else if (comp > 0)
            {
                bucket.NextNode = this;
                return bucket;
            }
            else if (NextNode == null)
            {
                NextNode = bucket;
            }
            else
            {
                NextNode = NextNode.Insert(bucket);
            }
            return this;
        }

        public Node Delete(string key)
        {
            int comp = Key.CompareTo(key);
            if (comp == 0)
            {
                return NextNode;
            }
            else if (comp > 0 || NextNode == null)
            {
                throw new KeyNotFoundException(key);
            }
            NextNode = NextNode.Delete(key);
            return this;
        }

        public int Search(string key)
        {
            int comp = Key.CompareTo(key);
            if (comp == 0)
            {
                return Value;
            }
            else if (comp > 0 || NextNode == null)
            {
                throw new KeyNotFoundException(key);
            }
            return NextNode.Search(key);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed) 
            {
                if (disposing)
                {
                    if (NextNode != null)
                    {
                        NextNode.Dispose();
                        NextNode = null;
                        Key = null;
                    }
                }
                _disposed = true;
            }
        }
    }
}
