using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Concurrency
{
    internal class Bucket : IDisposable
    {
        private bool _disposed = false;

        public Node[] Nodes { get; private set; } = null;
        public string[] Keys { get; private set; } = null;
        public Node FirstNode { get; private set; } = null;
        public int Length 
        { 
            get 
            {
                if (FirstNode == null)
                {
                    return 0;
                }
                return Nodes.Length; 
            }
        }
        public int Min { get; private set; } = 0;
        public int Max { get; private set; } = 0;

        ~Bucket()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Recalculate()
        {
            if (FirstNode != null)
            {
                int length = FirstNode.Length;
                Nodes = new Node[length];
                Keys = new string[length];

                Min = FirstNode.Value;
                Max = FirstNode.Value;
                int i = 0;
                Node node = FirstNode;
                while (node != null)
                {
                    if (node.Value < Min)
                    {
                        Min = node.Value;
                    }
                    if (node.Value > Max)
                    {
                        Max = node.Value;
                    }
                    Nodes[i] = node;
                    Keys[i] = node.Key;
                    i++;
                    node = node.NextNode;
                }
            }
            else
            {
                Nodes = null;
                Keys = null;
                Min = 0;
                Max = 0;
            }
        }

        public void Insert(Node newNode)
        {
            Node start = FirstNode;
            if (start == null)
            {
                FirstNode = newNode;
            }
            else
            {
                int idx = BinarySearch(Keys, newNode.Key);
                if (idx >= 0)
                {
                    Nodes[idx].Value = newNode.Value;
                }
                else
                {
                    int insertIdx = Math.Abs(idx + 1);
                    if (insertIdx == 0)
                    {
                        newNode.NextNode = FirstNode;
                        FirstNode = newNode;
                    }
                    else if (insertIdx == Keys.Length)
                    {
                        Nodes[Nodes.Length - 1].NextNode = newNode;
                    }
                    else
                    {
                        Nodes[insertIdx - 1].NextNode = newNode;
                        newNode.NextNode = Nodes[insertIdx];
                    }
                }
            }

            Recalculate();
        }

        public void Delete(string key)
        {
            int idx = Array.BinarySearch<string>(Keys, key);
            if (idx < 0)
            {
                throw new KeyNotFoundException($"Key \"{key}\" could not be found in the dictionary");
            }

            if (idx == 0)
            {
                Node node = FirstNode.NextNode;
                FirstNode.NextNode = null;
                FirstNode.Dispose();
                FirstNode = node;
            }
            else
            {
                Node node = Nodes[idx];
                Nodes[idx - 1].NextNode = node.NextNode;
                node.NextNode = null;
                node.Dispose();
                node = null;
            }

            Recalculate();
        }

        public int Search(string key)
        {
            if (FirstNode == null)
            {
                throw new KeyNotFoundException(key);
            }

            int idx = Array.BinarySearch<string>(Keys, key);
            if (idx < 0)
            {
                throw new KeyNotFoundException(key);
            }

            return Nodes[idx].Value;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) 
                {
                    if (Nodes != null)
                    {
                        for (int i = 0; i < Nodes.Length; i++)
                        {
                            Nodes[i] = null;
                            Keys[i] = null;
                        }

                        Nodes = null;
                        Keys = null;
                    }

                    if (FirstNode != null)
                    {
                        FirstNode.Dispose();
                        FirstNode = null;
                    }
                }
                _disposed = true;
            }
        }

        private static int BinarySearch(string[] a, string key)
        {
            return BinarySearch(a, 0, a.Length, key);
        }

        private static int BinarySearch(string[] a, int fromIndex, int toIndex, string key)
        {
            int low = fromIndex;
            int high = toIndex - 1;

            while (low <= high)
            {
                int mid = (int)((uint)(low + high) >> 1);
                string midVal = a[mid];

                int comp = midVal.CompareTo(key);
                if (comp < 0)
                    low = mid + 1;
                else if (comp > 0)
                    high = mid - 1;
                else
                    return mid; // key found
            }
            return -(low + 1);  // key not found.
        }
    }
}
