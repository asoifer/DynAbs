using DynAbs.Tracing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tracer
{
    internal class FileTracerClient : ITracerClient
    {
        static int initial_thread_id;

        static StringBuilder[] hub;
        static int index;
        int line_count, package_size;

        static FileStream fs;

        Thread writer;
        static Thread client;

        static Object writing, dispatched;

        public void Initialize()
        {
            initial_thread_id = System.Threading.Thread.CurrentThread.ManagedThreadId;

            hub = new StringBuilder[2] { new StringBuilder(), new StringBuilder() };
            index = 0;
            line_count = 0;
            package_size = 1500;

            fs = new FileStream(TracerGlobals.FileTraceInput, FileMode.Create, FileAccess.Write, FileShare.Write, 4096);

            writer = new Thread(TraceWriter);
            client = Thread.CurrentThread;

            writing = false;
            dispatched = false;
            writer.Start();
        }

        public void Trace(int fileId, int traceType, int spanStart, int spanEnd)
        {
            if (initial_thread_id == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                string separator = ",";
                string trace = fileId + separator + traceType + separator + spanStart + separator + spanEnd;

                hub[index].AppendLine(trace);
                if (++line_count == package_size)
                {
                    while ((bool)writing) { }

                    index = (index + 1) % 2;
                    lock (dispatched) { dispatched = true; }
                    line_count = 0;
                }
            }
        }

        static void TraceWriter()
        {
            while (client.IsAlive)
            {
                if ((bool)dispatched)
                {
                    lock (writing) { writing = true; }
                    lock (dispatched) { dispatched = false; };
                    write();
                }
            }

            /* Si la cantidad total de líneas de traza no es un múltiplo del tamaño de paquete,
             * una vez finalizada la ejecucuion del programa cliente quedan líneas que nunca terminan siendo despachadas
             */
            index = (index + 1) % 2;
            write();
            fs.Close();
        }

        static void write()
        {
            int write_index = (index + 1) % 2;

            string content = hub[write_index].ToString();
            var bytes = Encoding.UTF8.GetBytes(content);
            if (bytes.Length > 0)
                fs.Write(bytes, 0, bytes.Length);

            hub[write_index].Clear();
            lock (writing) { writing = false; }
        }
    }
}
