using System.Threading;

namespace IoFx.Runtime
{
    class SList
    {
        private SListNode _headNode;

        public SList()
        {
            var tail = new SListNode();
            tail.Next = tail;
            _headNode = tail;
        }

        private void Insert(SListNode node)
        {
            bool success;
            do
            {
                node.Next = _headNode;
                success = Interlocked.CompareExchange(ref _headNode, node, node.Next) == node.Next;
            } while (!success);
        }

        private SListNode Dequeue()
        {
            do
            {
                SListNode slot = _headNode;
                var previous = Interlocked.CompareExchange(ref _headNode, slot.Next, slot);
                if (previous == slot) // Successful compare exchange
                {
                    if (slot.Next == slot)
                    {
                        // reached the end of the list 
                        return null;
                    }

                    return slot;
                }

            } while (true);

        }
    }

    internal class SListNode
    {
        internal SListNode Next;
    }
}
