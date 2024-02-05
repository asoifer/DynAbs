using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynAbs.Tracing
{
    [ProtoContract]
    public class TraceInfo
    {
        [ProtoMember(1)]
        public int FileId { get; set; }
        [ProtoMember(2)]
        public int SpanStart { get; set; }
        [ProtoMember(3)]
        public int SpanEnd { get; set; }
        [ProtoMember(4)]
        public TraceType TraceType { get; set; }

        public override bool Equals(object obj)
        {
            var other = (TraceInfo)obj;
            return this.TraceType == other.TraceType &&
                this.FileId == other.FileId &&
                this.SpanStart == other.SpanStart &&
                this.SpanEnd == other.SpanEnd;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + FileId;
            result = prime * result + SpanStart;
            result = prime * result + SpanEnd;
            result = prime * result + (int)TraceType;
            return result;
        }
    }
}