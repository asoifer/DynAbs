using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using static DynAbs.Summaries.SummariesParser;

namespace DynAbs.Summaries
{
    public class SummariesVisitor : SummariesBaseVisitor<object>
    {
        public InterpretedAnnotation annotation = null;
        public List<ETF> metf = null;
        public List<ET> met = null;
        public List<ETType> types = null;
        public List<string> kinds = null;
        public List<Connect> mc = null;

        public override object VisitS(SummariesParser.SContext context)
        {
            annotation = new InterpretedAnnotation();
            base.VisitS(context);
            annotation.Annotation = context.GetText();
            return annotation;
        }

        public override object VisitRv(SummariesParser.RvContext context)
        {
            RVType rvEnum = RVType.Null;
            if (context.fresh() != null)
            {
                rvEnum = RVType.Fresh;
                types = new List<ETType>();
                if (context.fresh().types() != null)
                    VisitTypes(context.fresh().types());
                annotation.RVTypes = types;
                if (context.fresh().kind() != null)
                    annotation.RVKind = context.fresh().kind().GetText().ToString();
            }
            else if (context.isIn() != null)
            {
                rvEnum = RVType.IsIn;
                met = new List<ET>();
                VisitMet(context.isIn().met());
                annotation.RVMET = met;
            }
            annotation.RV = rvEnum;
            return null;
        }

        public override object VisitSingle(SingleContext context)
        {
            var ro = new RO();
            ro.Type = ROType.Single;

            types = new List<ETType>();
            if (context.types() != null)
                VisitTypes(context.types());
            ro.ROTypes = types;

            if (context.kind() != null)
                ro.Kind = context.kind().GetText().ToString();

            annotation.RO.Add(ro);

            return null;
        }

        public override object VisitMany(ManyContext context)
        {
            var ro = new RO();
            ro.Type = ROType.Many;

            types = new List<ETType>();
            if (context.types() != null)
                VisitTypes(context.types());
            ro.ROTypes = types;

            if (context.kind() != null)
                ro.Kind = context.kind().GetText().ToString();

            annotation.RO.Add(ro);

            return null;
        }

        public override object VisitR(SummariesParser.RContext context)
        {
            metf = new List<ETF>();
            base.VisitR(context);
            annotation.R = metf;

            return metf;
        }

        public override object VisitW(SummariesParser.WContext context)
        {
            metf = new List<ETF>();
            base.VisitW(context);
            annotation.W = metf;

            return metf;
        }

        public override object VisitCn(SummariesParser.CnContext context)
        {
            mc = new List<Connect>();
            base.VisitCn(context);
            annotation.CN = mc;

            return mc;
        }

        public override object VisitEtf(SummariesParser.EtfContext context)
        {
            var etf = new ETF();
            if (context.b() != null)
            {
                // Base
                BaseET baseET;
                int? index = null;

                var b = context.b().GetText();
                if (!Enum.TryParse(b.Split('[')[0], out baseET))
                    throw new Exception("...");

                if (context.b().number() != null)
                    index = Int32.Parse(context.b().number().GetText());
                
                etf.et = new ET() { @base = baseET, ParamIndex = index };
                etf.field = "";
            }
            else
            {
                met = null;
                var et = context.et();
                var parsed_et = VisitEt(et);
                etf.et = (ET)parsed_et;
                etf.field = context.fa().GetText().ToString();
                base.Visit(context.fa());
            }

            metf.Add(etf);
            return null;
        }

        public override object VisitC(SummariesParser.CContext context)
        {
            var ret = new Connect();
            met = new List<ET>();
            base.VisitMet(context.met().First());
            ret.met1 = met;
            met = new List<ET>();
            base.VisitMet(context.met().Last());
            ret.met2 = met;
            ret.field = context.fa().GetText();
            base.Visit(context.fa());
            mc.Add(ret);
            return ret;
        }

        public override object VisitEt(SummariesParser.EtContext context)
        {
            var et = new ET();

            // Base
            BaseET baseET;
            int? index = null;

            var b = context.bf().b().GetText();
            if (!Enum.TryParse(b.Split('[')[0], out baseET))
                throw new Exception("...");

            if (context.bf().b().number() != null)
                index = Int32.Parse(context.bf().b().number().GetText());

            et.@base = baseET;
            et.ParamIndex = index;

            // Fields sequence
            var fields = context.bf().f();
            if (fields != null)
                et.chainOfFields = fields.GetText().Split('.').ToList();

            // Filters
            if (context.filter() != null)
            {
                // OfKind

                if (context.filter().filterT() != null)
                { 
                    // UntilType
                    var untilType = context.filter().filterT().filterUT();
                    if (untilType != null)
                    {
                        types = new List<ETType>();
                        base.VisitFilterUT(untilType);
                        et.untilType = types;
                    }
                    // OfType
                    var ofType = context.filter().filterT().filterOT();
                    if (ofType != null)
                    {
                        types = new List<ETType>();
                        base.VisitFilterOT(ofType);
                        et.ofType = types;
                    }
                }
                if (context.filter().filterOK() != null)
                {
                    var ofKind = context.filter().filterOK();
                    if (ofKind != null)
                    {
                        kinds = new List<string>();
                        base.VisitFilterOK(ofKind);
                        et.ofKind = kinds;
                    }
                }
            }

            if (met != null)
                met.Add(et);
            return et;
        }

        public override object VisitType(SummariesParser.TypeContext context)
        {
            ETType etType = new ETType();

            if (context.elementType() != null)
            {
                BaseET b;
                var txt = context.elementType().GetText();
                var name = txt.Split('[')[0].Substring(1);
                if (!Enum.TryParse(name, out b))
                    throw new Exception("Bad expression");
                if (context.elementType().number() != null)
                    etType.ParamIndex = Int32.Parse(context.elementType().number().GetText());
                etType.Kind = ETTypeKind.Element;
                etType.@base = b;
            }
            else if (context.parametricType() != null)
            {
                etType.Kind = ETTypeKind.Parametric;
                etType.Name = context.parametricType().GetText().Substring(1);
            }
            else
            {
                etType.Kind = ETTypeKind.Default;
                etType.Name = context.GetText();
            }

            if (etType.Kind != ETTypeKind.Default && !annotation.ToMatch.Any(x => x.Equals(etType)))
                annotation.ToMatch.Add(etType);

            types.Add(etType);
            return null;
        }

        public override object VisitKind(SummariesParser.KindContext context)
        {
            kinds.Add(context.GetText());
            return null;
        }

        public override object VisitFa(FaContext context)
        {
            if (context.GetText().Contains('@'))
                annotation.Parameters.Add(context.GetText().ToString().Substring(1));

            return base.VisitFa(context);
        }
    }
}
