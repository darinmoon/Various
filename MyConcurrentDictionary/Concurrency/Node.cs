using System;
using System.Collections.Generic;

namespace Concurrency
{
    internal class Node : IDisposable
    {
        #region Private Variables

        private bool _disposed = false;

        #endregion

        #region Public Properties

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

        #endregion

        #region Constructor and Finalizer

        public Node(string key, int value)
        {
            this.Key = key;
            this.Value = value;
        }

        ~Node()
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

        #region Private Methods

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

        #endregion
    }
}
