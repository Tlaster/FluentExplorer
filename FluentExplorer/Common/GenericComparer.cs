using System;
using System.Collections.Generic;

namespace FluentExplorer.Common
{
    public class GenericComparer<T> : IEqualityComparer<T> where T : class
    {
        public GenericComparer(Func<T, object> expr)
        {
            _expr = expr;
        }

        private Func<T, object> _expr { get; }

        public bool Equals(T x, T y)
        {
            var first = _expr.Invoke(x);
            var sec = _expr.Invoke(y);
            return first != null && first.Equals(sec);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}