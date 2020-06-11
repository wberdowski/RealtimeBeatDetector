using System.Collections.Generic;

namespace RealtimeBeatDetector
{
    public class BufferList<T> : List<T>
    {
        public int Length { get; set; }

        public BufferList(int length)
        {
            Length = length;
        }

        public new void Add(T item)
        {
            Insert(0, item);

            if (Count > Length)
            {
                RemoveAt(Count - 1);
            }
        }
    }
}
