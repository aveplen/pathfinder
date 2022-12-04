using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStar
{
    public struct Pair<T>
    {
        public T first, second;

        public static Pair<T> FromList(List<T> list)
        {
            if (list.Count < 2)
            {
                throw new Exception($"not enough elements in list, found: {list.Count}");
            }

            return new Pair<T>()
            {
                first = list[0],
                second = list[1],
            };
        }

        public override int GetHashCode()
        {
            // (x << 2) ^ y;
            return first.GetHashCode() ^ second.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            Pair<T> pair = (Pair<T>)obj;
            return first.Equals(pair.first) && second.Equals(pair.second);
        }
    }
}
