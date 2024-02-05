using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class NodesOperationData
    {
        public int final_nodes { get; set; }
        public int involved_nodes { get; set; }
        public int involved_hubs { get; set; }
        public int visiting_nodes { get; set; }
        public List<TermData> term_data { get; set; }
        public double elapsed_milliseconds { get; set; }

        public NodesOperationData() { }
        public NodesOperationData(int _final_nodes, int _involved_nodes, int _involved_hubs, int _visiting_nodes, int _file_id, int _span_start, int _span_end, string _term_name, bool _static_term, double _elapsed_milliseconds) 
        {
            final_nodes = _final_nodes;
            involved_nodes = _involved_nodes;
            involved_hubs = _involved_hubs;
            visiting_nodes = _visiting_nodes;
            term_data = new List<TermData>() {
                new TermData() {
                    file_id = _file_id,
                    span_start = _span_start,
                    span_end = _span_end,
                    term_name = _term_name,
                    static_term = _static_term,
                }
            };
            elapsed_milliseconds = _elapsed_milliseconds;
        }
        public NodesOperationData(int _final_nodes, int _involved_nodes, int _involved_hubs, int _visiting_nodes, double _elapsed_milliseconds, params TermData[] terms)
        {
            final_nodes = _final_nodes;
            involved_nodes = _involved_nodes;
            involved_hubs = _involved_hubs;
            visiting_nodes = _visiting_nodes;
            term_data = terms.ToList();
            elapsed_milliseconds = _elapsed_milliseconds;
        }
    }

    public class TermData
    {
        public int file_id { get; set; }
        public int span_start { get; set; }
        public int span_end { get; set; }
        public string term_name { get; set; }
        public bool static_term { get; set; }

        public TermData() { }
        public TermData(int _file_id, int _span_start, int _span_end, string _term_name, bool _static_term) 
        {
            file_id = _file_id;
            span_start = _span_start;
            span_end = _span_end;
            term_name = _term_name;
            static_term = _static_term;
        }
    }
}
