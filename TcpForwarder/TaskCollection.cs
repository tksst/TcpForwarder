using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TcpForwarder
{
    class TaskCollection
    {

        private ConcurrentDictionary<int, Task> dic = new ConcurrentDictionary<int, Task>();
        private int counter = 0;

        public void Add(Task t){
            var i = Interlocked.Increment(ref counter);
            dic.TryAdd(i, t);
            t.ContinueWith( it => dic.TryRemove(i, out var dummy));
        }
    }
}
