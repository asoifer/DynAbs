using DynAbs.Tracing;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace Tracer
{
    internal class FileTracerClient : ITracerClient
    {
        const int PackageSize = 1500;
        // Bounds memory if the writer falls behind; Add blocks the traced program instead of reordering packages.
        const int MaxPendingPackages = 8;

        static long counter = 1;
        static int initial_thread_id;

        static StringBuilder current;
        static int line_count;
        static BlockingCollection<StringBuilder> pending;

        static FileStream fs;

        Thread writer;
        static Thread client;

        public void Initialize()
        {
            initial_thread_id = Thread.CurrentThread.ManagedThreadId;

            current = new StringBuilder();
            line_count = 0;
            pending = new BlockingCollection<StringBuilder>(new ConcurrentQueue<StringBuilder>(), MaxPendingPackages);

            fs = new FileStream(TracerGlobals.FileTraceInput, FileMode.Create, FileAccess.Write, FileShare.Write, 4096);

            writer = new Thread(TraceWriter);
            client = Thread.CurrentThread;
            writer.Start();
        }

        public void Trace(int fileId, int traceType, int spanStart, int spanEnd)
        {
            if (initial_thread_id == Thread.CurrentThread.ManagedThreadId)
            {
                string separator = ",";
                string trace = fileId + separator + traceType + separator + spanStart + separator + spanEnd + separator + (counter++);

                current.AppendLine(trace);
                if (++line_count == PackageSize)
                {
                    pending.Add(current);
                    current = new StringBuilder();
                    line_count = 0;
                }
            }
        }

        static void TraceWriter()
        {
            StringBuilder package;
            while (client.IsAlive)
            {
                if (pending.TryTake(out package, 50))
                    write(package);
            }

            /* Una vez finalizado el programa cliente, se escriben los paquetes pendientes en orden FIFO
             * y por último el paquete incompleto que nunca llegó a despacharse
             */
            while (pending.TryTake(out package))
                write(package);
            write(current);
            fs.Close();
        }

        static void write(StringBuilder package)
        {
            var bytes = Encoding.UTF8.GetBytes(package.ToString());
            if (bytes.Length > 0)
                fs.Write(bytes, 0, bytes.Length);
        }
    }
}
