using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace DynAbs.Tracing
{
    internal class PipeTracerServer : ITracerServer
    {
        public ITraceReceiver Receiver { get; set; }
        private NamedPipeServerStream pipeListener;

        public PipeTracerServer(ITraceReceiver receiver)
        {
            this.Receiver = receiver;
        }

        public void Initialize()
        {
            try
            {
                //Initializes the Listener.
                pipeListener = new NamedPipeServerStream("SlicerPipe");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void StartReceivingTrace()
        {
            pipeListener.WaitForConnection();

            var receiving = true;
            bool receiversListening = true;
            while (receiving)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(pipeListener);
                	StreamReader reader = new StreamReader(pipeListener);
                    TraceInfo info = Serializer.DeserializeWithLengthPrefix<TraceInfo>(reader.BaseStream, PrefixStyle.Fixed32);
                    if (info == null)
                    {
                        receiving = false;
                        continue;
                    }
                    bool receiverIsListening = Receiver.TraceReceived(info);
                    Serializer.SerializeWithLengthPrefix(writer.BaseStream, receiverIsListening, PrefixStyle.Fixed32);
                }
                catch
                {
                    //TODO: Maybe we want to catch the specific exception, now it is
                    //used only to avoid exit the function without close the listener.
                    receiving = false;
                    continue;
                }

                if (!receiversListening)
                {
                    receiving = false;
                }
            }
            Receiver.StopReceiving();
            pipeListener.Close();
        }
    }
}
