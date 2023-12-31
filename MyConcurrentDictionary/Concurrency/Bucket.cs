using System;
using System.Collections.Generic;

namespace Concurrency
{
    internal class Bucket : IDisposable
    {
        #region Private Variables

        private bool _disposed = false;

        #endregion

        #region Public Properties

        public Node Head { get; private set; } = null;
        public int Length { get; private set; } = 0;
        public int Min { get; private set; } = Int32.MaxValue;
        public int Max { get; private set; } = Int32.MinValue;

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

        public Node GetTail()
        {
            Node node = Head;
            if (node == null)
            {
                return null;
            }
            while (node.NextNode != null)
            {
                node = node.NextNode;
            }

            return node;
        }

        // Insert a node in the correct position
        public void Insert(Node newNode)
        {
            if (Head == null)
            {
                Head = newNode;
            }
            else
            {
                newNode.NextNode = Head;
                Head = newNode;
            }

            Length = 0;
            Min = Int32.MaxValue;
            Max = Int32.MinValue;
            Node node = Head;
            while (node != null)
            {
                Length++;
                if (node.Value < Min)
                {
                    Min = node.Value;
                }
                if (node.Value > Max)
                {
                    Max = node.Value;
                }
                node = node.NextNode;
            }
        }

        public void Delete(string key)
        {
            if (Head == null)
            {
                throw new KeyNotFoundException($"Key \"{key}\" could not be found in the dictionary");
            }

            Node prevNode = null;
            Node node = Head;
            while (node != null)
            {
                if (key.Equals(node.Key))
                {
                    if (prevNode == null)
                    {
                        Head = node.NextNode;
                        node.NextNode = null;
                    }
                    else
                    {
                        prevNode.NextNode = node.NextNode;
                        node.NextNode = null;
                    }

                    Length = 0;
                    Min = Int32.MaxValue;
                    Max = Int32.MinValue;
                    Node tempNode = Head;
                    while (tempNode != null)
                    {
                        Length++;
                        if (tempNode.Value < Min)
                        {
                            Min = tempNode.Value;
                        }
                        if (tempNode.Value > Max)
                        {
                            Max = tempNode.Value;
                        }
                        tempNode = tempNode.NextNode;
                    }


                    return;
                }
                prevNode = node;
                node = node.NextNode;
            }
            throw new KeyNotFoundException($"Key \"{key}\" could not be found in the dictionary");
        }

        public int Search(string key)
        {
            if (Head == null)
            {
                throw new KeyNotFoundException($"Key \"{key}\" could not be found in the dictionary");
            }

            Node node = Head;
            while (node != null) 
            {
                if (key.Equals(node.Key))
                {
                    return node.Value;
                }
                node = node.NextNode;
            }
            throw new KeyNotFoundException($"Key \"{key}\" could not be found in the dictionary");
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Node node = Head;
                    while (node != null)
                    {
                        Node tempNode = node.NextNode;
                        node.NextNode = null;
                        node = tempNode;
                    }
                }
                _disposed = true;
            }
        }

        #endregion

    }
}
