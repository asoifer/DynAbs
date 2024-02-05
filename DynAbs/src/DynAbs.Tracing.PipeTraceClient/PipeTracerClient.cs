using DynAbs.Tracing;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tracer
{
    internal class PipeTracerClient : ITracerClient
    {
        private NamedPipeClientStream pipeClient;

        public PipeTracerClient()
        {
            AlreadyClosed = false;
        }

        private bool AlreadyClosed { get; set; }

        public void Initialize()
        {
            //Estas 2 lineas evitan que se necesite el backend para testear si funciona el programa instrumentado
            //Console.WriteLine("Inicializando tracer");
            //return;

            if (AlreadyClosed) return;

            var connected = false;
            while (!connected)
            {
                try
                {
                    pipeClient = new NamedPipeClientStream("SlicerPipe");
                    pipeClient.Connect();
                    connected = true;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);
                }
            }
        }

        public void Trace(int fileId, int traceType, int spanStart, int spanEnd)
        {
            //Estas 2 lineas evitan que se necesite el backend para testear si funciona el programa instrumentado
            //Console.WriteLine("Trazando " + traceType);
            //return;

            if (AlreadyClosed) return;
            //saca la traza a un archivo, para propósitos de debugging
            //using (System.IO.StreamWriter file = System.IO.File.AppendText(@"C:/temp/traza.txt"))
            //{
            //    TraceInfo info = new TraceInfo() { FileId = fileId, SpanStart = spanStart, SpanEnd = spanEnd, TraceType = (TraceType)traceType, RuntimeNames = runtimeNames };
            //    file.WriteLine("{0}:{1}/{2}.{3}", info.TraceType.ToString(), info.FileId.ToString(), info.SpanStart.ToString(), info.SpanEnd.ToString());
            //}
            try
            {
                //Encoding.
                StreamWriter writer = new StreamWriter(pipeClient);
                StreamReader reader = new StreamReader(pipeClient);
                var info = new TraceInfo() { FileId = fileId, SpanStart = spanStart, SpanEnd = spanEnd, TraceType = (TraceType)traceType };
                Serializer.SerializeWithLengthPrefix(writer.BaseStream, info, PrefixStyle.Fixed32);
                writer.Flush();

                bool continueSending = Serializer.DeserializeWithLengthPrefix<bool>(reader.BaseStream, PrefixStyle.Fixed32);

                if (!continueSending)
                {
                    AlreadyClosed = true;
                    pipeClient.Close();
                }
            }
            catch (Exception ex)
            {
                AlreadyClosed = true;
                pipeClient.Close();
            }
        }
    }
}
