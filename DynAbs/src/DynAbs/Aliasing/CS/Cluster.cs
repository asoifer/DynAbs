using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Aliasing.CS
{
    class Cluster
    {
        public int ID = Globals.NextClusterID++;
        public bool IsHavocRegion = false;
        public bool CurrentHavocRegion = false;

        public Dictionary<TypeKind, Dictionary<Field, HashSet<uint>>> LastDefs
            = new Dictionary<TypeKind, Dictionary<Field, HashSet<uint>>>();
        public Dictionary<TypeKind, Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>> Targets
            = new Dictionary<TypeKind, Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>>();
        public bool Single = true;

        public HashSet<TypeKind> TypeKinds = new HashSet<TypeKind>();

        // Operations

        // CreateEmpty
        public Cluster(TypeKind tk, bool Single) { this.Single = Single; this.TypeKinds.Add(tk); }

        // CreateTargetsFromFact (tk :: TypeKind, f :: Field, n :: Set<PtgNode>, t’ :: Type)
        public Cluster(TypeKind t, Field f, Dictionary<TypeKind, HashSet<PtgVertex>> to)
        {
            Targets[t] = new Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>();
            Targets[t][f] = to;
            TypeKinds.Add(t);
        }

        // CreateLastDefFromFact (tk :: TypeKind, f :: Field, s :: Set<DGNode>)
        public Cluster(TypeKind t, Field f, HashSet<uint> s)
        {
            LastDefs[t] = new Dictionary<Field, HashSet<uint>>();
            LastDefs[t][f] = s;
            TypeKinds.Add(t);
        }

        // Invariante: no hace falta unir por tipo compatible porque siempre se traen los niveles que compatibilizan por lo que actualizás y consultás siempre lo que está bien
        public void Union(Cluster other)
        {
            if (Globals.optimize_types && this.IsHavocRegion)
            {
                this.UnionIfHavocRegion(other);
                return;
            }

            // LastDefs
            foreach (var ld in other.LastDefs)
            {
                if (!this.LastDefs.ContainsKey(ld.Key))
                    this.LastDefs[ld.Key] = new Dictionary<Field, HashSet<uint>>();

                foreach (var fs in ld.Value)
                {
                    if (!this.LastDefs[ld.Key].ContainsKey(fs.Key))
                        this.LastDefs[ld.Key][fs.Key] = new HashSet<uint>();

                    this.LastDefs[ld.Key][fs.Key].UnionWith(other.LastDefs[ld.Key][fs.Key]);
                }
            }

            // Targets
            foreach (var t in other.Targets)
            {
                if (!this.Targets.ContainsKey(t.Key))
                    this.Targets[t.Key] = new Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>();

                foreach (var f in t.Value)
                {
                    if (!this.Targets[t.Key].ContainsKey(f.Key))
                        this.Targets[t.Key][f.Key] = new Dictionary<TypeKind, HashSet<PtgVertex>>();

                    foreach (var t2 in f.Value)
                    {
                        //if (!this.Targets[t.Key][f.Key].ContainsKey(t2.Key))
                        //    this.Targets[t.Key][f.Key][t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet();

                        //this.Targets[t.Key][f.Key][t2.Key].UnionWith(other.Targets[t.Key][f.Key][t2.Key]);

                        //if (!this.Targets[t.Key][f.Key].ContainsKey(t2.Key))
                        //    this.Targets[t.Key][f.Key][t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet();

                        //this.Targets[t.Key][f.Key][t2.Key].UnionWith(other.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find()));

                        if (!this.Targets[t.Key][f.Key].ContainsKey(t2.Key))
                            this.Targets[t.Key][f.Key][t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet(other.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find()));
                        else
                            this.Targets[t.Key][f.Key][t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet(
                                this.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find()).Concat(other.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find())));
                    }
                }
            }

            // General (esto está yendo por afuera)
            this.Single = this.Single && other.Single;
            this.TypeKinds.UnionWith(other.TypeKinds);
        }

        public void UnionIfHavocRegion(Cluster other)
        {
            var singleTypeKindFields = this.LastDefs.Single().Value;
            // Last def
            foreach (var ld in other.LastDefs)
                foreach (var fs in ld.Value)
                {
                    if (!singleTypeKindFields.ContainsKey(fs.Key))
                        singleTypeKindFields[fs.Key] = new HashSet<uint>();
                    singleTypeKindFields[fs.Key].UnionWith(fs.Value);
                }

            // Targets
            var singleTK = this.Targets.Single().Value;
            foreach (var t in other.Targets)
                foreach (var f in t.Value)
                {
                    if (!singleTK.ContainsKey(f.Key))
                        singleTK[f.Key] = new Dictionary<TypeKind, HashSet<PtgVertex>>();

                    foreach (var t2 in f.Value)
                    {
                        if (!singleTK[f.Key].ContainsKey(t2.Key))
                            singleTK[f.Key][t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet(other.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find()));
                        else
                            singleTK[f.Key][t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet(
                                singleTK[f.Key][t2.Key].Select(x => x.Find()).Concat(other.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find())));
                    }
                }

        }

        // This is called ONLY in a worst havoc scenario
        public void UnionOPT(Cluster other)
        {
            // Assert ==> this.IsHavocRegion
            // Then: this Cluster will receive everything in one type.

            // LastDefs ==> We don't need this, we've already read these values and we have a new last def

            // Targets ==> Everything for the single type
            var mySingleDictionary = this.Targets.Single().Value.Single().Value;
            foreach (var t in other.Targets)
            {
                foreach (var f in t.Value)
                {
                    foreach (var t2 in f.Value)
                    {
                        if (!mySingleDictionary.ContainsKey(t2.Key))
                            mySingleDictionary[t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet(
                                other.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find()));
                        else
                            mySingleDictionary[t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet(
                                mySingleDictionary[t2.Key].Select(x => x.Find()).Concat(
                                    other.Targets[t.Key][f.Key][t2.Key].Select(x => x.Find())));
                    }
                }
            }
        }

        public HashSet<uint> GetLastDefs(TypeKind t, Field f)
        {
            var toReturn = new HashSet<uint>();
            foreach (var tld in LastDefs)
                if (tld.Key.Compatibles(t))
                    foreach (var ld in tld.Value)
                        if (f == Field.SIGMA_FIELD || ld.Key == Field.SIGMA_FIELD || f.Value == ld.Key.Value)
                            toReturn.UnionWith(ld.Value);
            return toReturn;
        }
    }
}
