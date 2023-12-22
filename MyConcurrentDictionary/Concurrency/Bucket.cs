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
        public int Length { get { return (Nodes == null) ? 0 : Nodes.Length; } }
        public int Min { get; private set; }
        public int Max { get; private set; }

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
            if (Nodes == null || Nodes.Length == 0 || Nodes[0] == null)
            {
                Nodes = new Node[1] { newNode };
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
                    Node[] tempNodes = new Node[Nodes.Length + 1];

                    // if the key is not currently in the array of keys,
                    // then the BinarySearch method returns the negative
                    // value of (the insertion point + 1)
                    // to get the correct insertion point, we have to add
                    // 1 to the negative value and then get the absolute value
                    int insertIdx = Math.Abs(idx + 1);

                    int j = 0;
                    for (int i = 0; i < tempNodes.Length; i++)
                    {
                        if (i == insertIdx)
                        {
                            tempNodes[i] = newNode;
                        }
                        else
                        {
                            tempNodes[i] = Nodes[j++];
                        }
                    }
                    Nodes = tempNodes;
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

            Node[] tempNodes = new Node[Nodes.Length - 1];
            // delete the node
            int j = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (i != idx)
                {
                    tempNodes[j++] = Nodes[i];
                }
            }
            Nodes = tempNodes;

            // recalculate all of the values that
            // make inserting, searching, or getting
            // size, min, or max fast
            Recalculate();
        }

        public int Search(string key)
        {
            if (Nodes == null || Nodes.Length == 0 || Nodes[0] == null)
            {
                throw new KeyNotFoundException($"Key \"{key}\" could not be found in the dictionary");
            }

            int idx = Array.BinarySearch<string>(Keys, key);
            if (idx < 0)
            {
                throw new KeyNotFoundException($"Key \"{key}\" could not be found in the dictionary");
            }

            return Nodes[idx].Value;
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
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
                }
                _disposed = true;
            }
        }

        #endregion

        #region Private Methods

        // recalculate all of the values that
        // make inserting, searching, or getting
        // size, min, or max fast
        private void Recalculate()
        {
            if (Nodes != null && Nodes.Length > 0 && Nodes[0] != null)
            {
                // Reset the Node and Keys arrays
                Keys = new string[Nodes.Length];

                // initialize min and max
                Min = 0;
                Max = 0;

                for (int i = 0; i < Keys.Length; i++)
                {
                    Node node = Nodes[i];
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
                    Keys[i] = node.Key;
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
