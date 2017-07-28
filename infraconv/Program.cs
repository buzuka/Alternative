using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Alternative.infraconv
{
    enum OpType
    {
        OPCODE,
        QUERY
    }

    struct RpcOperation
    {
        public OpType type;
        public string name;
        public string opcode;
        public string template;
        public string flags;
        public string storableClass;
    }

    class Program
    {
        public static XNamespace alt = "http://tempuri/xsd/infragen";

        static void PrintVersion()
        {
            int major = Assembly.GetExecutingAssembly().GetName().Version.Major;
            int minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;

            Console.WriteLine("Conversion tool for Infragen, version " + major + "." + minor);
            Console.WriteLine("Copyright (c) 2010 Oleg Volkov, http://enterprise-way.com");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            PrintVersion();
            Dictionary<String, String> pairs = ParseArgs(args);
            String inFolder = "";
            String outFolder = "";
            try
            {
                inFolder = pairs["in"];
                outFolder = pairs["out"];
            }
            catch
            {
                Usage();
                return;
            }
            try
            {
                DirectoryInfo di = new DirectoryInfo(inFolder);
                List<String> adinfras = new List<string>();
                adinfras.AddRange(FindAdinfra(di));
                if (adinfras.Count() > 1)
                {
                    throw new NotImplementedException("Too many instances found");
                }
                else if (adinfras.Count == 0)
                {
                    throw new ArgumentException("No instance found");
                }
                DoWork(di, adinfras[0], new DirectoryInfo(outFolder));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private static IEnumerable<String> FindAdinfra(DirectoryInfo di)
        {
            foreach (FileInfo fi in di.GetFiles("*.adinfra"))
            {
                yield return fi.FullName;
            }
            foreach (DirectoryInfo d in di.GetDirectories())
            {
                foreach (String a in FindAdinfra(d))
                {
                    yield return a;
                }
            }
        }

        private static void DoWork(DirectoryInfo root, string adinfra, DirectoryInfo outdir)
        {
            XmlReader reader = XmlReader.Create(adinfra);
            XDocument doc = XDocument.Load(reader);
            XElement docRoot = doc.Root;
            XmlNamespaceManager man = new XmlNamespaceManager(reader.NameTable);
            man.AddNamespace("adinfra", Namespaces.adinfra.NamespaceName);
            man.AddNamespace("AESDK", Namespaces.aesdk.NamespaceName);
            man.AddNamespace("AEService", Namespaces.aesvc.NamespaceName);
            man.AddNamespace("alt", Program.alt.NamespaceName);

            var list = docRoot.XPathSelectElements("//adinfra:adapter/adinfra:adapterServices/*", man);

            string instance = docRoot.XPathSelectElement("//adinfra:adapter/AESDK:instanceId", man).Value;
            // calculating path
            if (adinfra.IndexOf(root.FullName) == -1)
            {
                throw new ArgumentException("Impossible here: cannot calculate path");
            }
            string path = adinfra.Substring(root.FullName.Length);
            path = path.Substring(0, path.LastIndexOf('\\'));
            path = path.Replace('\\', '/');

            System.Console.WriteLine("Found " + list.Count() + " services");

            XDocument globals = new XDocument();
            XElement globalsRoot = InitDocument(globals, instance, docRoot, path);

            Dictionary<String, XDocument> services = new Dictionary<string, XDocument>();
            
            foreach (XElement l in list)
            {
                // checking service type
                if (l.Name.LocalName != "serverService")
                {
                    throw new NotImplementedException("Not supported service type: " + l.Name.LocalName);
                }
                // getting name
                XElement name = ValidatedRead(l, Namespaces.adinfra + "name"
                    , () => "Wrong schema: unknown name");
                // checking transport type
                XElement tt = ValidatedRead(l, Namespaces.adinfra + "transportType"
                    , () => "Wrong schema: no transport in service " + name.Value);

                if (tt.Value != "rv")
                {
                    throw new NotImplementedException("Not supported transport: " + tt.Value);
                }
                // getting session
                XElement s = ValidatedRead(l, Namespaces.adinfra + "sessionReference"
                    , () => "Wrong schema: no session reference for service " + name.Value);
                // TODO: read session props here

                // getting other props (transaction, subject)
                XElement inTrans = l.Element(Namespaces.adinfra + "runInTransaction");
                XElement msgSubj = ValidatedRead(l, Namespaces.adinfra + "messageSubject"
                    , () => "Wrong schema: expected 'messageSubject' while reading service " + name.Value);

                // getting operations
                var ops = l.XPathSelectElements("adinfra:Operations/*", man);
                System.Console.WriteLine("Ops count: " + ops.Count());

                // adding service
                if (ops.Count() > 0)
                {
                    String session = s.Value.Split('.')[1];
                    XElement ex = globalsRoot.XPathSelectElement("//alt:session[@name='" + session + "']", man);
                    if (ex == null)
                    {
                        // adding session
                        XElement def = docRoot.XPathSelectElement("//AEService:rvSession[@name='" + session + "']", man);
                        if (def != null)
                        {
                            XElement add = new XElement(Program.alt + "session"
                                , new XAttribute("name", session)
                                , new XAttribute("type", "rv")
                                , new XElement(Program.alt + "daemon"
                                    , ValidatedRead(def, Namespaces.aesvc + "daemon", () => "Daemon not found for session " + session).Value
                                    )
                                , new XElement(Program.alt + "service"
                                    , ValidatedRead(def, Namespaces.aesvc + "service", () => "Service not found for session " + session).Value
                                    )
                                , new XElement(Program.alt + "network"
                                    , ValidatedRead(def, Namespaces.aesvc + "network", () => "Network not found for session " + session).Value
                                    )
                                , new XElement(Program.alt + "dispatchers"
                                    , ValidatedRead(ValidatedRead(def
                                            , Namespaces.aesvc + "extendedProperties"
                                            , () => "ExtendedProperties not found for session " + session)
                                        , "dispatcherCount"
                                        , () => "DispatcherCount not found for session " + session).Value
                                    )
                                );
                            globalsRoot.Add(add);
                            //System.Console.WriteLine("Added: " + def);
                        }
                        else
                        {
                            throw new InvalidOperationException("Unknown session " + session + " for service " + name.Value);
                        }
                    }
                    globalsRoot.Add(new XElement(Program.alt + "service"
                        , new XAttribute("name", name.Value)
                        , new XElement(Program.alt + "session", session)
                        , new XElement(Program.alt + "transaction", inTrans != null ? inTrans.Value : "false")
                        , new XElement(Program.alt + "subject", msgSubj.Value)
                        )
                    );

                    XElement opsGlobal = l.Element(Namespaces.adinfra + "Operations");
                    if (opsGlobal != null)
                    {
                        Dictionary<string, RpcOperation> opsMap = new Dictionary<string, RpcOperation>();
                        foreach (XElement op in opsGlobal.Elements())
                        {
                            String opname = ValidatedRead(op, Namespaces.adinfra + "operationName", () => "No operation name").Value;
                            String optype = ValidatedRead(op, Namespaces.adinfra + "operationType", () => "No operation type").Value;
                            RpcOperation rpc = new RpcOperation();
                            rpc.name = opname;
                            if (optype == "opcode")
                            {
                                rpc.type = OpType.OPCODE;
                                rpc.opcode = ValidatedRead(op, Namespaces.adinfra + "opcode", () => "No opcode name").Value;
                                rpc.flags = ValidatedRead(op, Namespaces.adinfra + "opflag", () => "No opcode flag").Value;
                            }
                            else if (optype == "query")
                            {
                                rpc.type = OpType.QUERY;
                                rpc.template = ValidatedRead(op, Namespaces.adinfra + "searchTemplate", () => "No search template").Value;
                                rpc.flags = ValidatedRead(op, Namespaces.adinfra + "searchFlag", () => "No search flag").Value;
                                XElement sc = op.Element(Namespaces.adinfra + "storableClass");
                                if (sc != null)
                                {
                                    rpc.storableClass = sc.Value;
                                }
                            }
                            else
                            {
                                throw new ArgumentException("Unknown operation type " + optype);
                            }
                            opsMap[opname] = rpc;
                        }

                        String schemaRef = ValidatedRead(l, Namespaces.adinfra + "schemaReference", () => "No schema reference for service " + name.Value).Value;
                        schemaRef = schemaRef.Split('#')[0];
                        schemaRef = schemaRef.Replace('/', '\\');

                        XDocument opsDoc = AddOperations(instance, name.Value, root.FullName + schemaRef, opsMap);
                        services[name.Value] = opsDoc;
                    }
                }
            }
            globals.Save(outdir + "\\services.xml");
            System.Console.WriteLine(globalsRoot);
            foreach (string key in services.Keys)
            {
                services[key].Save(outdir + "\\" + key + ".xml");
                System.Console.WriteLine(key + ": \n" + services[key]);
            }
        }

        private static XDocument AddOperations(string instance, string srvName, string pathToRpc, Dictionary<string, RpcOperation> opsMap)
        {
            XDocument opsDoc = new XDocument();
            using (XmlReader srvReader = XmlReader.Create(pathToRpc))
            {
                XDocument srvDoc = XDocument.Load(srvReader);
                XmlNamespaceManager srvMan = new XmlNamespaceManager(srvReader.NameTable);
                srvMan.AddNamespace("meta", Namespaces.meta.NamespaceName);

                XElement opsRoot = InitDocument(opsDoc, instance);

                XElement rpc = srvDoc.Root.XPathSelectElement("//meta:rpcClass", srvMan);
                if (rpc == null)
                {
                    throw new InvalidOperationException("RPC class not found");
                }
                foreach (XElement op in rpc.Elements(Namespaces.meta + "operation"))
                {
                    String name = ValidatedRead(op, Namespaces.meta + "name", () => "Missed operation name").Value;
                    if (!opsMap.ContainsKey(name))
                    {
                        throw new InvalidOperationException("Operation " + name + " is not defined in service " + srvName);
                    }

                    if (opsMap[name].type == OpType.OPCODE)
                    {
                        String outType = ValidatedRead(op, Namespaces.meta + "returnType", () => "Missed return type").Value;
                        String inType = ValidatedRead(op, "meta:parameter[meta:name='input']/meta:parameterType", srvMan, () => "Missed input type").Value;
                        System.Console.WriteLine("Operation (opcode): " + name);
                        System.Console.WriteLine("  IN:  " + inType);
                        System.Console.WriteLine("  OUT: " + outType);

                        opsRoot.Add(new XElement(Program.alt + "operation"
                            , new XAttribute("name", name)
                            , new XAttribute("opcode", opsMap[name].opcode)
                            , new XAttribute("flags", opsMap[name].flags)
                            , new XAttribute("service", srvName)
                            , CreateTree(srvDoc, srvMan, Program.alt + "in", inType)
                            , CreateTree(srvDoc, srvMan, Program.alt + "out", outType)
                            )
                        );
                    }
                    else if (opsMap[name].type == OpType.QUERY)
                    {
                        String outType = ValidatedRead(op, Namespaces.meta + "returnType", () => "Missed return type").Value;
                        System.Console.WriteLine("Operation (query): " + name);
                        System.Console.WriteLine("  OUT: " + outType);

                        opsRoot.Add(new XElement(Program.alt + "query"
                            , new XAttribute("name", name)
                            , new XAttribute("flags", opsMap[name].flags)
                            , new XAttribute("service", srvName)
                            , new XElement(Program.alt + "template", opsMap[name].template)
                            , opsMap[name].storableClass == "" ? null : new XElement(Program.alt + "class", opsMap[name].storableClass)
                            , CreateTree(srvDoc, srvMan, Program.alt + "out", outType, true)
                            )
                        );
                    }
                    else
                    {
                        throw new NotImplementedException("Something wrong...");
                    }
                }
            }
            return opsDoc;
        }

        private static XElement CreateTree(XDocument root, XmlNamespaceManager man, XName el, string path)
        {
            return CreateTree(root, man, el, path, false);
        }

        private static XElement CreateTree(XDocument root, XmlNamespaceManager man, XName el, string path, bool skipTop)
        {
            string local = GetLocalPath(path);
            XElement f = root.XPathSelectElement("//meta:class[@name='" + local + "']", man);
            if (f == null)
            {
                throw new InvalidOperationException("Invalid schema");
            }
            List<XElement> values = new List<XElement>();
            foreach (XElement att in f.Elements(Namespaces.meta + "attribute"))
            {
                XElement type = att.XPathSelectElement("meta:extendedProperties/meta:targetFieldType", man);
                if (type == null)
                {
                    // index attribute, skip
                    continue;
                }
                String name = ToShortName(ValidatedRead(att, Namespaces.meta + "name", () => "Invalid schema").Value);
                switch (type.Value)
                {
                    case "PIN_FLDT_SUBSTRUCT":
                        {
                            XElement at = ValidatedRead(att, Namespaces.meta + "attributeType", () => "Invalid schema");
                            values.Add(CreateTree(root, man, Program.alt + name, at.Value));
                        }
                        break;
                    case "PIN_FLDT_ARRAY":
                        {
                            XElement at = ValidatedRead(att, Namespaces.meta + "attributeType", () => "Invalid schema");
                            string seqLocal = GetLocalPath(at.Value);
                            XElement seq = root.XPathSelectElement("//meta:sequence[@name='" + seqLocal + "']/meta:elementType", man);
                            if (seq == null)
                            {
                                throw new InvalidOperationException("Invalid schema");
                            }
                            if (skipTop)
                            {
                                values.AddRange(CreateTree(root, man, el, seq.Value).Elements());
                            }
                            else
                            {
                                values.Add(CreateTree(root, man, Program.alt + name, seq.Value));
                            }
                        }
                        break;
                    default:
                        values.Add(new XElement(Program.alt + name));
                        break;
                }
            }
            return new XElement(el, values.ToArray());
        }

        private static string ToShortName(string fld)
        {
            if (fld.StartsWith("PIN_FLD_"))
            {
                return fld.Substring(8);
            }
            return fld;
        }

        private static string GetLocalPath(string path)
        {
            String p = path.Split('#')[1];
            return p.Substring(p.IndexOf('.') + 1);
        }

        private static void AddOperation(XElement schemaRoot, XElement opsRoot, XElement op, string service, XmlNamespaceManager man)
        {
            String name = ValidatedRead(op, Namespaces.adinfra + "operationName", () => "No operation name").Value;
            String type = ValidatedRead(op, Namespaces.adinfra + "operationType", () => "No operation type").Value;
            if (type != "opcode")
            {
                System.Console.WriteLine("Operation " + name + " is not opcode, skipped");
                return;
            }
            String opcode = ValidatedRead(op, Namespaces.adinfra + "opcode", () => "No opcode name").Value;
            String flags = ValidatedRead(op, Namespaces.adinfra + "opflag", () => "No opcode flag").Value;

            XElement rpcOp = schemaRoot.XPathSelectElement("//meta:rpcClass[meta:name='RequestResponseServiceOperationSchema']/meta:operation[meta:name='" + name + "']", man);
            System.Console.WriteLine("Operation " + rpcOp);
        }

        private static XElement ValidatedRead(XElement r, XName el, Func<String> msg)
        {
            XElement res = r.Element(el);
            if (res == null)
            {
                throw new InvalidOperationException(msg());
            }
            return res;
        }

        private static XElement ValidatedRead(XElement r, string xpath, XmlNamespaceManager man, Func<String> msg)
        {
            XElement res = r.XPathSelectElement(xpath, man);
            if (res == null)
            {
                throw new InvalidOperationException(msg());
            }
            return res;
        }

        private static String ValueOrDefault(XElement r, string xpath, XmlNamespaceManager man, String def)
        {
            XElement res = r.XPathSelectElement(xpath, man);
            if (res == null)
            {
                return def;
            }
            return res.Value;
        }

        private static XElement InitDocument(XDocument globals, String name, XElement root, String path)
        {
            XmlNamespaceManager man = new XmlNamespaceManager(new NameTable());
            man.AddNamespace("adinfra", Namespaces.adinfra.NamespaceName);
            man.AddNamespace("AESDK", Namespaces.aesdk.NamespaceName);
            man.AddNamespace("AEService", Namespaces.aesvc.NamespaceName);

            String suspendAfter = ValueOrDefault(root, "//adinfra:adapter/adinfra:ConnectionInfo/adinfra:retriesBeforeSuspend", man, null);
            String retries = ValueOrDefault(root, "//adinfra:adapter/adinfra:ConnectionInfo/adinfra:maxRetries", man, null);
            String conTimer = ValueOrDefault(root, "//adinfra:adapter/adinfra:ConnectionInfo/adinfra:connectionTimer", man, null);
            String sleep = ValueOrDefault(root, "//adinfra:adapter/adinfra:ConnectionInfo/adinfra:sleepBetweenRetries", man, null);

            String transTimer = ValueOrDefault(root, "//adinfra:adapter/adinfra:Options/adinfra:transactionTimer", man, "60000");
            String max = ValueOrDefault(root, "//adinfra:adapter/adinfra:Options/adinfra:maxTransConnections", man, "3");

            String stdTimeout = ValidatedRead(root, "//adinfra:adapter/AESDK:startup/AESDK:stdMicroAgentTimeout", man, () => "Std agent timeout not found").Value;
            String hawkTimeout = ValidatedRead(root, "//adinfra:adapter/AESDK:startup/AESDK:classMicroAgentTimeout", man, () => "Class agent timeout not found").Value;

            XElement logging = ValidatedRead(root, "//adinfra:adapter/AESDK:reporting", man, () => "Reporting not found");

            globals.Add(new XElement(Program.alt + "infranet"
                , new XAttribute("name", name)
                , new XAttribute("path", path)
                , new XElement(Program.alt + "instance"
                    , new XElement(Program.alt + "connection"
                        , new XElement(Program.alt + "advanced"
                            , suspendAfter == null ? null : new XElement(Program.alt + "suspend-after", suspendAfter)
                            , retries == null ? null : new XElement(Program.alt + "retries", retries)
                            , conTimer == null ? null : new XElement(Program.alt + "timer", conTimer)
                            , sleep == null ? null : new XElement(Program.alt + "sleep", sleep)
                            )
                        )
                    , new XElement(Program.alt + "transaction"
                        , new XElement(Program.alt + "timeout", transTimer)
                        , new XElement(Program.alt + "max", max)
                        )
                    , new XElement(Program.alt + "logging"
                        , from l in logging.Elements(Namespaces.aesdk + "fileSink")
                          select new XElement(Program.alt + "file"
                              , from r in l.Elements(Namespaces.aesdk + "role")
                                select new XElement(Program.alt + "role", r.Element(Namespaces.aesdk + "name").Value)
                              )
                        , from l in logging.Elements(Namespaces.aesdk + "stdioSink")
                          select new XElement(Program.alt + "stdio"
                              , from r in l.Elements(Namespaces.aesdk + "role")
                                select new XElement(Program.alt + "role", r.Element(Namespaces.aesdk + "name").Value)
                              )
                        )
                    , new XElement(Program.alt + "std-timeout", stdTimeout)
                    , new XElement(Program.alt + "hawk-timeout", hawkTimeout)
                    )
                ));
            return globals.Root;
        }

        private static XElement InitDocument(XDocument globals, String name)
        {
            globals.Add(new XElement(Program.alt + "infranet"
                , new XAttribute("name", name)
                ));
            return globals.Root;
        }

        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<String, String> res = new Dictionary<string, string>();
            String last = null;
            int counter = 0;
            foreach (string a in args)
            {
                if (last == null)
                {
                    if (a.StartsWith("-"))
                    {
                        string b = a.Substring(1);
                        res[b] = null;
                        last = b;
                    }
                    else
                    {
                        res["unnamed" + counter] = a;
                        ++counter;
                    }
                }
                else
                {
                    res[last] = a;
                    last = null;
                }
            }
            return res;
        }

        private static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  infraconv.exe -in <project root> -out <out path>\n");
            Console.WriteLine("    <project root>\t- main project folder");
            Console.WriteLine("    <out path>\t- output folder, should be empty");
            Console.WriteLine("  Adapter instances will be found by extension *.adinfra");
        }
    }
}
