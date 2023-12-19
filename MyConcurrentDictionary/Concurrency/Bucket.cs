using System;
using System.Collections.Generic;

namespace Concurrency
{
    internal class Bucket : IDisposable
    {
        #region Private Variables

        private bool _disposed = false;

        #endregion

        #region Private Properties

        private Node[] Nodes { get; set; } = null;

        #endregion

        #region Public Properties

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

        #endregion

        #region Finalizer

        ~Bucket()
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

        // Insert a node in the correct position
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
                    // the key already exists, so just overwrite the value
                    Nodes[idx].Value = newNode.Value;
                }
                else
                {
                    // if the key is not currently in the array of keys,
                    // then the BinarySearch method returns the negative
                    // value of (the insertion point + 1)
                    // to get the correct insertion point, we have to add
                    // 1 to the negative value and then get the absolute value
                    int insertIdx = Math.Abs(idx + 1);

                    if (insertIdx == 0)
                    {
                        // the new node goes at the head of the list
                        newNode.NextNode = FirstNode;
                        FirstNode = newNode;
                    }
                    else if (insertIdx == Keys.Length)
                    {
                        // the new node goes at the end of the list
                        Nodes[Nodes.Length - 1].NextNode = newNode;
                    }
                    else
                    {
                        // the new node goes somewhere in the middle
                        Nodes[insertIdx - 1].NextNode = newNode;
                        newNode.NextNode = Nodes[insertIdx];
                    }
                }
            }

            // recalculate all of the values that
            // make inserting, searching, or getting
            // size, min, or max fast
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
                // delete the first Node
                Node node = FirstNode.NextNode;
                FirstNode.NextNode = null;
                FirstNode.Dispose();
                FirstNode = node;
            }
            else
            {
                // delete the node anywhere
                // but the head
                Node node = Nodes[idx];
                Nodes[idx - 1].NextNode = node.NextNode;
                node.NextNode = null;
                node.Dispose();
                node = null;
            }

            // recalculate all of the values that
            // make inserting, searching, or getting
            // size, min, or max fast
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

        #endregion

        #region Private Methods

        // recalculate all of the values that
        // make inserting, searching, or getting
        // size, min, or max fast
        private void Recalculate()
        {
            if (FirstNode != null)
            {
                // Recalculate the length of the bucket
                int length = FirstNode.Length;

                // Reset the Node and Keys arrays
                Nodes = new Node[length];
                Keys = new string[length];

                // initialize min and max
                Min = FirstNode.Value;
                Max = FirstNode.Value;
                int i = 0;
                Node node = FirstNode;

                while (node != null)
                {
                    if (node.Value < Min)
                    {
                        // new min value
                        Min = node.Value;
                    }
                    if (node.Value > Max)
                    {
                        // new max value
                        Max = node.Value;
                    }
                    // load the arrays
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

        // Binary search that will give us the index of the matching
        // entry in the array or, if it is not founnd, gives us a
        // negative value that indicates the potential insertion point
        private static int BinarySearch(string[] a, string key)
        {
            return BinarySearch(a, 0, a.Length, key);
        }

        // Binary search that will give us the index of the matching
        // entry in the array or, if it is not founnd, gives us a
        // negative value that indicates the potential insertion point
        private static int BinarySearch(string[] a, int fromIndex, int toIndex, string key)
        {
            int low = fromIndex;
            int high = toIndex - 1;

            while (low <= high)
            {
                // get the mid point
                int mid = (int)((uint)(low + high) >> 1);
                string midVal = a[mid];

                // does it match, if not,
                // which section do we search next?
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

        #endregion
    }
}
