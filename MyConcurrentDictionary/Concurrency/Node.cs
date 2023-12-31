using System;
using System.Collections.Generic;

namespace Concurrency
{
    internal class Node
    {
        #region Public Properties

        public string Key { get; private set; }
        public int Value { get; set; }
        public Node NextNode { get; set; } = null;

        public int Length
        {
            get
            {
                return (NextNode == null) ? 1 : NextNode.Length + 1;
            }
        }

        public int Max
        {
            get
            {
                if (NextNode == null)
                {
                    return Value;
                }
                return (NextNode.Max > Value) ? NextNode.Max : Value;
            }
        }

        public int Min
        {
            get
            {
                if (NextNode == null)
                {
                    return Value;
                }
                return (NextNode.Max < Value) ? NextNode.Max : Value;
            }
        }

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
