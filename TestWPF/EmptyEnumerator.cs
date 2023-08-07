using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWPF
{
    public class EmptyEnumerator : IEnumerator
    {
        public void Reset() { }

        public bool MoveNext() { return false; }

        public object Current
        {
            get
            {
                throw new InvalidOperationException();
            }
        }
    }
}