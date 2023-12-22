using System;
using System.Collections.Generic;

namespace Concurrency
{
    internal class Node
    {
        #region Public Properties

        public string Key { get; private set; }
        public int Value { get; set; }

        #endregion

        #region Constructor

        public Node(string key, int value)
        {
            this.Key = key;
            this.Value = value;
        }

        #endregion
    }
}
