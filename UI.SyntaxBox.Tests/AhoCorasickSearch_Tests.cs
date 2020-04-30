using Microsoft.VisualStudio.TestTools.UnitTesting;
using UI.SyntaxBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UI.SyntaxBox.Tests
{
    [TestClass()]
    public class AhoCorasickSearch_Tests
    {
        // ...................................................................
        /// <summary>
        /// Implements a very simple scenario appropriate for debugging.
        /// </summary>
        [TestMethod()]
        public void FindAll_AXBY()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "ax",
                "by"
            });

            matcher.FindAll(" if something then if goto J elsif");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_CommonPrefix()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "if",
                "ifa",
                "ifb"
            });

            var actual = matcher.FindAll(" ifa ").ToList();
            Assert.AreEqual(1, actual.Count, "Wrong number of matches");
            Assert.AreEqual(1, actual[0].Position, "Matched wrong position");
            Assert.AreEqual(3, actual[0].Length, "Matched wrong length");
            Assert.AreEqual("ifa", actual[0].Value, "Matched wrong keyword");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_MultipleMatches()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "if",
                "ifa",
                "ifb"
            });

            var actual = matcher.FindAll("Matches 'ifa', 'ifb' and 'if' in a longer text, Keywords: if, ifa, ifb").ToList();
            Assert.AreEqual(6, actual.Count, "Wrong number of matches");

            Assert.AreEqual("ifa", actual[0].Value, "Matched wrong keyword");
            Assert.AreEqual("ifb", actual[1].Value, "Matched wrong keyword");
            Assert.AreEqual("if", actual[2].Value, "Matched wrong keyword");
            Assert.AreEqual("if", actual[3].Value, "Matched wrong keyword");
            Assert.AreEqual("ifa", actual[4].Value, "Matched wrong keyword");
            Assert.AreEqual("ifb", actual[5].Value, "Matched wrong keyword");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_CommonPrefix_NoWordBreakB()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "if",
                "ifa",
                "ifb"
            }, false, true);

            var actual = matcher.FindAll("xifax").ToList();
            Assert.AreEqual(2, actual.Count, "Wrong number of matches");
            Assert.AreEqual(1, actual[0].Position, "Matched wrong position");
            Assert.AreEqual(2, actual[0].Length, "Matched wrong length");
            Assert.AreEqual("if", actual[0].Value, "Matched wrong keyword");
            Assert.AreEqual(3, actual[1].Length, "Matched wrong length");
            Assert.AreEqual("ifa", actual[1].Value, "Matched wrong keyword");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_CommonPrefix_NoWordBreakB_NoOverlap()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "if",
                "ifa",
                "ifb"
            }, false);

            var actual = matcher.FindAll("xifax").ToList();
            Assert.AreEqual(1, actual.Count, "Wrong number of matches");
            Assert.AreEqual(1, actual[0].Position, "Matched wrong position");
            Assert.AreEqual(3, actual[0].Length, "Matched wrong length");
            Assert.AreEqual("ifa", actual[0].Value, "Matched wrong keyword");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_CommonPostfix()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "if",
                "elsif",
            });

            var actual = matcher.FindAll(" elsif ").ToList();
            Assert.AreEqual(1, actual.Count, "Wrong number of matches");
            Assert.AreEqual(1, actual[0].Position, "Matched wrong position");
            Assert.AreEqual(5, actual[0].Length, "Matched wrong length");
            Assert.AreEqual("elsif", actual[0].Value, "Matched wrong keyword");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_CommonPostfix_NoWordBreak()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "if",
                "elsif",
            }, false, true);
            TextReader rdr = new StringReader("aaa");
            var actual = matcher.FindAll(" elsif ").ToList();
            Assert.AreEqual(2, actual.Count, "Wrong number of matches");
            Assert.AreEqual("elsif", actual[0].Value, "Matched wrong keyword matched (1)");
            Assert.AreEqual("if", actual[1].Value, "Matched wrong keyword matched (2)");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_CommonPostfix_NoWordBreak_NoOverlap()
        {
            var matcher = new AhoCorasickSearch(new List<string>
            {
                "if",
                "elsif",
            }, false);

            var actual = matcher.FindAll(" elsif ").ToList();
            Assert.AreEqual(1, actual.Count, "Wrong number of matches");
            Assert.AreEqual("elsif", actual[0].Value, "Matched wrong keyword matched");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_CmpRegex()
        {
            KeywordRule acsRule = new KeywordRule
            {
                Keywords = "abstract,as,base,bool,break,byte,case,catch,char,checked,class,const,continue,decimal,default,delegate,do,double,else,enum,event,explicit,extern,false,finally,fixed,float,for,foreach,goto,if,implicit,in,int,interface,internal,is,lock,long,namespace,new,null,object,operator,out,override,params,private,protected,public,readonly,ref,return,sbyte,sealed,short,sizeof,stackalloc,static,string,struct,switch,this,throw,true,try,typeof,uint,ulong,unchecked,unsafe,ushort,using,using,static,virtual,void,volatile,while,get,set,yield,var"
            };
            RegexRule regexRule = new RegexRule
            {
                Pattern = @"\babstract\b|\bas\b|\bbase\b|\bbool\b|\bbreak\b|\bbyte\b|\bcase\b|\bcatch\b|\bchar\b|\bchecked\b|\bclass\b|\bconst\b|\bcontinue\b|\bdecimal\b|\bdefault\b|\bdelegate\b|\bdo\b|\bdouble\b|\belse\b|\benum\b|\bevent\b|\bexplicit\b|\bextern\b|\bfalse\b|\bfinally\b|\bfixed\b|\bfloat\b|\bfor\b|\bforeach\b|\bgoto\b|\bif\b|\bimplicit\b|\bin\b|\bint\b|\binterface\b|\binternal\b|\bis\b|\block\b|\blong\b|\bnamespace\b|\bnew\b|\bnull\b|\bobject\b|\boperator\b|\bout\b|\boverride\b|\bparams\b|\bprivate\b|\bprotected\b|\bpublic\b|\breadonly\b|\bref\b|\breturn\b|\bsbyte\b|\bsealed\b|\bshort\b|\bsizeof\b|\bstackalloc\b|\bstatic\b|\bstring\b|\bstruct\b|\bswitch\b|\bthis\b|\bthrow\b|\btrue\b|\btry\b|\btypeof\b|\buint\b|\bulong\b|\bunchecked\b|\bunsafe\b|\bushort\b|\busing\b|\busing\b|\bstatic\b|\bvirtual\b|\bvoid\b|\bvolatile\b|\bwhile\b|\bget\b|\bset\b|\byield\b|\bvar\b"
            };

            string file = Path.Combine(Environment.CurrentDirectory, @"..\..\..\UI.SyntaxBox\SyntaxRenderer.cs");
            var input = File.ReadAllLines(file);

            var regex = input
                .SelectMany((x) => regexRule.Match(x))
                .OrderBy((x) => x.FromChar)
                .ThenBy((x) => x.Length)
                .ToList();
            var acs = input
                .SelectMany((x) => acsRule.Match(x))
                .OrderBy((x) => x.FromChar)
                .ThenBy((x) => x.Length)
                .ToList();

            Assert.AreEqual(regex.Count, acs.Count, "Not the same number of matches");
            Assert.IsTrue(Enumerable.SequenceEqual(regex, acs, new FormatInstructionCmp()), "Different matches");
        }
        // ...................................................................
        [TestMethod()]
        public void FindAll_PerformanceVsRegex()
        {
            KeywordRule acsRule = new KeywordRule
            {
                Keywords = "abstract,as,base,bool,break,byte,case,catch,char,checked,class,const,continue,decimal,default,delegate,do,double,else,enum,event,explicit,extern,false,finally,fixed,float,for,foreach,goto,if,implicit,in,int,interface,internal,is,lock,long,namespace,new,null,object,operator,out,override,params,private,protected,public,readonly,ref,return,sbyte,sealed,short,sizeof,stackalloc,static,string,struct,switch,this,throw,true,try,typeof,uint,ulong,unchecked,unsafe,ushort,using,using,static,virtual,void,volatile,while,get,set,yield,var"
            };
            acsRule.Match("empty"); // Warm it up

            RegexRule regexRule = new RegexRule
            {
                Pattern = @"\babstract\b|\bas\b|\bbase\b|\bbool\b|\bbreak\b|\bbyte\b|\bcase\b|\bcatch\b|\bchar\b|\bchecked\b|\bclass\b|\bconst\b|\bcontinue\b|\bdecimal\b|\bdefault\b|\bdelegate\b|\bdo\b|\bdouble\b|\belse\b|\benum\b|\bevent\b|\bexplicit\b|\bextern\b|\bfalse\b|\bfinally\b|\bfixed\b|\bfloat\b|\bfor\b|\bforeach\b|\bgoto\b|\bif\b|\bimplicit\b|\bin\b|\bint\b|\binterface\b|\binternal\b|\bis\b|\block\b|\blong\b|\bnamespace\b|\bnew\b|\bnull\b|\bobject\b|\boperator\b|\bout\b|\boverride\b|\bparams\b|\bprivate\b|\bprotected\b|\bpublic\b|\breadonly\b|\bref\b|\breturn\b|\bsbyte\b|\bsealed\b|\bshort\b|\bsizeof\b|\bstackalloc\b|\bstatic\b|\bstring\b|\bstruct\b|\bswitch\b|\bthis\b|\bthrow\b|\btrue\b|\btry\b|\btypeof\b|\buint\b|\bulong\b|\bunchecked\b|\bunsafe\b|\bushort\b|\busing\b|\busing\b|\bstatic\b|\bvirtual\b|\bvoid\b|\bvolatile\b|\bwhile\b|\bget\b|\bset\b|\byield\b|\bvar\b"
            };
            regexRule.Match("empty"); // Warm it up

            int iter = 10;
            string file = Path.Combine(Environment.CurrentDirectory, @"..\..\..\UI.SyntaxBox\SyntaxRenderer.cs");
            var input = File.ReadAllLines(file);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                input.SelectMany((x) => regexRule.Match(x)).ToList();
            }
            sw.Stop();

            long regex = sw.ElapsedMilliseconds;

            sw.Reset();
            sw.Start();
            for (int i = 0; i < iter; i++)
            {
                input.SelectMany((x) => acsRule.Match(x)).ToList();
            }
            sw.Stop();
            long acs = sw.ElapsedMilliseconds;

            Assert.Inconclusive($"Regex: {regex}ms, Acs: {acs}ms");
        }
        // ...................................................................
    }

    class FormatInstructionCmp : IEqualityComparer<FormatInstruction>
    {
        public bool Equals(FormatInstruction x, FormatInstruction y)
        {
            return (x.FromChar == y.FromChar
                && x.Length == y.Length);
        }

        public int GetHashCode(FormatInstruction obj)
        {
            return (obj.FromChar + obj.Length).GetHashCode();
        }
    }
}