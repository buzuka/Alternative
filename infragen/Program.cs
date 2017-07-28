using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

using Alternative;
using System.Linq.Expressions;
using Alternative.Infranet;
using System.Xml.Schema;
using System.Xml;
using System.Reflection;

namespace Alternative.infragen
{
    enum ExitCodes : int
    {
        SUCCESS = 0,
        MISSED_PARAMS = 1,
        PROCESSING_ERROR = 2
    }

    class Program
    {
        public static XNamespace ns = "http://tempuri/xsd/infragen";

        static void PrintVersion()
        {
            int major = Assembly.GetExecutingAssembly().GetName().Version.Major;
            int minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;

            Console.WriteLine("Infragen " + major + "." + minor);
            Console.WriteLine("Copyright (c) 2010 Oleg Volkov, http://enterprise-way.com");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            PrintVersion();
            if (args.Length != 4)
            {
                Usage();
                Environment.Exit((int)ExitCodes.MISSED_PARAMS);
            }
            else
            {
                Dictionary<String, String> pairs = ParseArgs(args);
                DoWork(pairs["in"], pairs["out"]);
            }
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
            Console.WriteLine("Usage:");
            Console.WriteLine("  infragen.exe -in <in path> -out <out path>\n");
            Console.WriteLine("    <in path>\t- folder with xmls");
            Console.WriteLine("    <out path>\t- output folder, should be empty");
        }
        
        private static void DoWork(String inPath, String outPath)
        {
            try
            {
                Infragen res = LoadSchemas(inPath);
                Console.WriteLine("Loaded " + res.InstanceCount + " instance(s)");
                res.BuildAll(outPath);
                Console.WriteLine("Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Console.WriteLine("Fail");
                Environment.Exit((int)ExitCodes.PROCESSING_ERROR);
            }
        }

        private static Infragen LoadSchemas(string folder)
        {
            if (!Directory.Exists(folder))
            {
                throw new ArgumentException("Path does not exist: " + folder);
            }
            DirectoryInfo di = new DirectoryInfo(folder);
            Infragen res = new Infragen();

            TextReader tr = new StringReader(Properties.Resources.schema);
            XmlSchema sch = XmlSchema.Read(tr, null);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(sch);
            settings.ValidationType = ValidationType.Schema;

            foreach (string f in Directory.GetFiles(folder, "*.xml"))
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(f, settings))
                    {
                        XDocument doc = XDocument.Load(reader);

                        if (doc.Root.Name.Namespace == ns && doc.Root.Name.LocalName == "infranet")
                        {
                            LoadSchema(di.Name, res, doc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Exception while processing " + f, ex);
                }
            }
            return res;
        }

        private static void LoadSchema(String defInstance, Infragen gen, XDocument doc)
        {
            XElement root = doc.Root;
            String instance = ValueOrDefault(root.Attribute("name"), x => x.Value, defInstance);
            String path = ValueOrDefault(root.Attribute("path"), x => x.Value, "");
            InfragenInstance ins = gen.GetInstance(instance);
            ins.SetAdinfraPath(path);
            // instance details first
            ParseGlobals(ins, root.Element(ns + "instance"));
            // custom fields
            ParseFields(gen, root.Element(ns + "fields"));
            // sessions
            foreach (XElement el in root.Elements(ns + "session"))
            {
                ParseSession(ins, el);
            }
            // services
            foreach (XElement el in root.Elements(ns + "service"))
            {
                ParseService(ins, el);
            }
            // operations
            foreach (XElement el in root.Elements(ns + "operation"))
            {
                ParseOperation(ins, el);
            }
            // queries
            foreach (XElement el in root.Elements(ns + "query"))
            {
                ParseQuery(ins, el);
            }
        }

        private static String ValueOrDefault<T>(T o, Func<T, String> d, String def)
        {
            if (o == null)
            {
                return def;
            }
            return d(o);
        }

        private static void ParseGlobals(InfragenInstance gen, XElement el)
        {
            if (el == null)
            {
                // stop here
                return;
            }

            Schema.InstanceType ii = gen.CreateInstance();

            ParseConnection(ii, el.Element(ns + "connection"));
            ParseTransaction(ii, el.Element(ns + "transaction"));
            ParseLogging(ii, el.Element(ns + "logging"));

            ii.stdtimeout = ValueOrDefault(el.Element(ns + "std-timeout"), x => x.Value, null);
            ii.hawktimeout = ValueOrDefault(el.Element(ns + "hawk-timeout"), x => x.Value, null);
        }

        private static void ParseConnection(Schema.InstanceType ii, XElement el)
        {
            if (el == null)
            {
                // stop here
                return;
            }
            // connection props are ignored in this version
            String host = ValueOrDefault(el.Element(ns + "host"), x => x.Value, "localhost");
            String port = ValueOrDefault(el.Element(ns + "port"), x => x.Value, "11960");
            String login = ValueOrDefault(el.Element(ns + "login"), x => x.Value, "root.0.0.0.1");
            String password = ValueOrDefault(el.Element(ns + "password"), x => x.Value, "password");

            // advanced props should work
            ii.connection = new Schema.InstanceTypeConnection();
            ParseAdvanced(ii.connection, el.Element(ns + "advanced"));
        }

        private static void ParseAdvanced(Schema.InstanceTypeConnection ii, XElement el)
        {
            if (el == null)
            {
                // stop here
                return;
            }
            ii.advanced = new Schema.InstanceTypeConnectionAdvanced();
            ii.advanced.suspendafter = ValueOrDefault(el.Element(ns + "suspend-after"), x => x.Value, "5");
            ii.advanced.retries = ValueOrDefault(el.Element(ns + "retries"), x => x.Value, "-1");
            ii.advanced.timer = ValueOrDefault(el.Element(ns + "timer"), x => x.Value, "0");
            ii.advanced.sleep = ValueOrDefault(el.Element(ns + "sleep"), x => x.Value, "100");
        }

        private static void ParseTransaction(Schema.InstanceType ii, XElement el)
        {
            if (el == null)
            {
                // stop here
                return;
            }
            ii.transaction = new Schema.InstanceTypeTransaction();
            ii.transaction.timeout = ValueOrDefault(el.Element(ns + "timeout"), x => x.Value, "60000");
            ii.transaction.max = ValueOrDefault(el.Element(ns + "max"), x => x.Value, "3");
        }

        private static void ParseLogging(Schema.InstanceType ii, XElement el)
        {
            if (el == null)
            {
                // stop here
                return;
            }
            ii.logging = new Schema.InstanceTypeLogging();
            ParseLogging(ii.logging, el.Element(ns + "file"));
            ParseLogging(ii.logging, el.Element(ns + "stdio"));
        }

        private static void ParseLogging(Schema.InstanceTypeLogging iiLog, XElement el)
        {
            if (el == null)
            {
                // stop here
                return;
            }
            if (el.Name.LocalName == "file")
            {
                iiLog.file = new List<string>();
                ParseRoles(iiLog.file, el);
            }
            else if (el.Name.LocalName == "stdio")
            {
                iiLog.stdio = new List<string>();
                ParseRoles(iiLog.stdio, el);
            }
            else
            {
                throw new NotImplementedException("Unknown sink type: " + el.Name.LocalName);
            }
        }

        private static void ParseRoles(List<string> list, XElement el)
        {
            list.AddRange(from r in el.Elements(ns + "role") select r.Value);
        }

        private static void ParseFields(Infragen gen, XElement el)
        {
            if (el == null)
            {
                // stop here
                return;
            }
            foreach (XElement f in el.Elements(Program.ns + "field"))
            {
                CustomField fld = new CustomField();
                fld.name = ValueOrDefault(f.Attribute("name"), x => x.Value, null);
                fld.id = Int32.Parse(ValueOrDefault(f.Attribute("id"), x => x.Value, "0"));
                fld.type = ValueOrDefault(f.Attribute("type"), x => x.Value, null);

                gen.AddField(fld);
            }
        }

        private static void ParseSession(InfragenInstance gen, XElement el)
        {
            SessionInfo si = new SessionInfo();
            si.name = el.Attribute("name").Value;
            si.type = ValueOrDefault(el.Attribute("type"), x => x.Value, "rv");

            si.dispCount = ValueOrDefault(el.Element(ns + "dispatchers"), x => x.Value, "0");
            si.daemon = ValueOrDefault(el.Element(ns + "daemon"), x => x.Value, "%%RvDaemon%%");
            si.service = ValueOrDefault(el.Element(ns + "service"), x => x.Value, "%%RvService%%");
            si.network = ValueOrDefault(el.Element(ns + "network"), x => x.Value, "%%RvNetwork%%");

            gen.AddSession(si);
        }

        private static void ParseService(InfragenInstance gen, XElement el)
        {
            ServiceInfo si = new ServiceInfo();
            si.name = el.Attribute("name").Value;
            si.isDefault = ValueOrDefault(el.Attribute("default"), x => x.Value, "false") == "true";

            si.session = ValueOrDefault(el.Element(ns + "session"), x => x.Value, "");
            si.inTransaction = ValueOrDefault(el.Element(ns + "transaction"), x => x.Value, "false") == "true";
            si.subject = ValueOrDefault(el.Element(ns + "subject"), x => x.Value, "");

            gen.AddService(si);
        }

        private static void ParseOperation(InfragenInstance gen, XElement el)
        {
            OperationInfo oi = new OperationInfo();
            oi.type = OpType.OPCODE;
            oi.name = el.Attribute("name").Value;
            oi.opcode = el.Attribute("opcode").Value;
            oi.flags = ValueOrDefault(el.Attribute("flags"), x => x.Value, "0");
            oi.service = ValueOrDefault(el.Attribute("service"), x => x.Value, "");

            oi.inSchema = new XElement(el.Element(ns + "in"));
            oi.outSchema = new XElement(el.Element(ns + "out"));

            gen.AddOperation(oi);
        }

        private static void ParseQuery(InfragenInstance gen, XElement el)
        {
            OperationInfo oi = new OperationInfo();
            oi.type = OpType.QUERY;
            oi.name = el.Attribute("name").Value;
            oi.flags = ValueOrDefault(el.Attribute("flags"), x => x.Value, "0");
            oi.service = ValueOrDefault(el.Attribute("service"), x => x.Value, "");
            oi.opcode = el.Element(Program.ns + "template").Value;
            oi.storableClass = ValueOrDefault(el.Element(Program.ns + "class"), x => x.Value, "");

            oi.outSchema = new XElement(el.Element(ns + "out"));

            gen.AddOperation(oi);
        }
    }

    struct SessionInfo
    {
        public String name;
        public String type;
        public String daemon;
        public String service;
        public String network;
        public String dispCount;
    }

    struct ServiceInfo
    {
        public String name;
        public bool isDefault;
        public String session;
        public bool inTransaction;
        public String subject;
    }

    enum OpType
    {
        OPCODE,
        QUERY
    }

    struct OperationInfo
    {
        public OpType type;
        public String name;
        public String opcode;
        public String flags;
        public String service;
        public XElement inSchema;
        public XElement outSchema;
        public String storableClass;
    }

    struct CustomField
    {
        public String name;
        public int id;
        public String type;
    }

    class Infragen
    {
        private Dictionary<String, InfragenInstance> _instances = new Dictionary<string, InfragenInstance>();
        private List<CustomField> _fields = new List<CustomField>();

        public int InstanceCount
        {
            get { return _instances.Count; }
        }

        public InfragenInstance GetInstance(String name)
        {
            if (!_instances.ContainsKey(name))
            {
                InfragenInstance res = new InfragenInstance(name);
                _instances[name] = res;
                return res;
            }
            return _instances[name];
        }

        public void AddField(CustomField fld)
        {
            if (fld.name == null || fld.id == 0 || fld.type == null)
            {
                throw new ArgumentNullException("Invalid field definition");
            }
            var res = from f in _fields
                      where f.name == fld.name
                      select f;
            if (res.Count() > 0)
            {
                throw new ArgumentException("Field already defined: " + fld.name);
            }
            _fields.Add(fld);
        }

        public void BuildAll(String basepath)
        {
            InfranetAdapterConfig iac = new InfranetAdapterConfig();
            foreach (CustomField fld in _fields)
            {
                iac.AddCustomField(fld.name, fld.id, fld.type);
            }
            foreach (InfragenInstance ins in _instances.Values)
            {
                ins.Build(iac);
            }
            AdapterConfigExporter ex = new AdapterConfigExporter(basepath);
            ex.Export(iac);
        }
    }

    class InfragenInstance
    {
        private Schema.infranet _model = new Schema.infranet();

        private List<SessionInfo> _sessions = new List<SessionInfo>();
        private List<ServiceInfo> _services = new List<ServiceInfo>();
        private List<OperationInfo> _ops = new List<OperationInfo>();

        private String _defaultSession = "";

        public InfragenInstance(String instance)
        {
            _model.name = instance;
        }

        public Schema.InstanceType CreateInstance()
        {
            if (_model.instance.Count > 0)
            {
                // duplicate instance found
                throw new ArgumentException("Duplicate instance info found in working folder for instance " + _model.name);
            }
            Schema.InstanceType ins = new Schema.InstanceType();
            _model.instance.Add(ins);
            return ins;
        }

        public void SetAdinfraPath(String path)
        {
            _model.path = path;
        }

        public void AddSession(SessionInfo si)
        {
            var res = from e in _sessions
                      where e.name == si.name
                      select e;
            if (res.Count() > 0)
            {
                throw new ArgumentException("Session already defined: " + si.name);
            }
            if (_defaultSession == "" && si.type == "rv")
            {
                _defaultSession = si.name;
            }
            _sessions.Add(si);
        }

        public void AddService(ServiceInfo si)
        {
            var res = from e in _services
                      where e.name == si.name
                      select e;
            if (res.Count() > 0)
            {
                throw new ArgumentException("Service already defined: " + si.name);
            }
            if (si.isDefault)
            {
                var def = from e in _services
                          where e.isDefault
                          select e;
                if (def.Count() > 0)
                {
                    throw new ArgumentException("Service '" + si.name + "' declared as default but there is another default service: '" + def.First().name + "'");
                }
            }
            _services.Add(si);
        }

        public void AddOperation(OperationInfo oi)
        {
            var res = from e in _ops
                      where e.name == oi.name && e.service == oi.service
                      select e;
            if (res.Count() > 0)
            {
                throw new ArgumentException("Operation already defined: " + oi.name
                    + ", service " + oi.name == "" ? "'default'" : oi.name);
            }
            _ops.Add(oi);
        }

        public void Build(InfranetAdapterConfig cfg)
        {
            InfranetAdapterInstance ins = cfg.AddInstance(_model.name, _model.path == null ? "/" : _model.path);
            Schema.InstanceType mdl = _model.instance.FirstOrDefault();
            if (mdl != null)
            {
                Schema.InstanceTypeLogging log = mdl.logging;
                if (log != null)
                {
                    List<LogSink> sinks = new List<LogSink>();
                    if (log.file != null)
                    {
                        FileLogSink f = new FileLogSink();
                        f.AddRoles(log.file);
                        sinks.Add(f);
                    }
                    if (log.stdio != null)
                    {
                        StdioLogSink s = new StdioLogSink();
                        s.AddRoles(log.stdio);
                        sinks.Add(s);
                    }
                    if (sinks.Count > 0)
                    {
                        ins.SetLogSinks(sinks);
                    }
                }
                Schema.InstanceTypeTransaction tr = mdl.transaction;
                if (tr != null)
                {
                    ins.SetTransactionInfo(Int32.Parse(tr.timeout), Int32.Parse(tr.max));
                }
                if (mdl.hawktimeout != null)
                {
                    ins.SetClassMicroAgentTimeout(mdl.hawktimeout);
                }
                if (mdl.stdtimeout != null)
                {
                    ins.SetStdMicroAgentTimeout(mdl.stdtimeout);
                }
                if (mdl.connection != null && mdl.connection.advanced != null)
                {
                    ins.SetConnectionAdvanced(mdl.connection.advanced.suspendafter
                        , mdl.connection.advanced.retries
                        , mdl.connection.advanced.timer
                        , mdl.connection.advanced.sleep
                        );
                }
            }

            foreach (ServiceInfo si in _services)
            {
                AERepoElementFactory<AESession> ss = GetSession(si.session);
                InfranetServerService srv = ins.AddServerService(si.name, ss, si.subject);
                srv.InTransaction = si.inTransaction;

                var ops = from o in _ops
                          where o.service == si.name || (si.isDefault && o.service == "")
                          select o;

                foreach (var o in ops)
                {
                    if (o.type == OpType.OPCODE)
                    {
                        srv.AddOperation(o.name, o.opcode, Int32.Parse(o.flags), o.inSchema, o.outSchema);
                    }
                    else
                    {
                        XElement queryOut = new XElement("dummy", new XElement("RESULTS", o.outSchema.Elements()));
                        srv.AddQuery(o.name, o.opcode, Int32.Parse(o.flags), o.storableClass, queryOut);
                    }
                }
            }

            // last step
            ins.CreateTerminator(_defaultSession, "");
        }

        private AERepoElementFactory<AESession> GetSession(string name)
        {
            var sessions = from s in _sessions
                          where s.name == name || (name == "" && s.type == "rv")
                          select s;
            if (sessions.Count() == 0)
            {
                throw new ArgumentException("Session not found: " + name);
            }
            SessionInfo si = sessions.FirstOrDefault();
            return new AERepoElementFactory<AESession>(si.name, x => CreateSession(x, si));
        }

        private AESession CreateSession(AERepository x, SessionInfo si)
        {
            switch (si.type)
            {
                case "rv":
                    RvSession rv = new RvSession(x, si.name);
                    rv.Daemon = si.daemon;
                    rv.Service = si.service;
                    rv.Network = si.network;
                    rv.Properties = new SessionProperties() { DispatcherCount = Int32.Parse(si.dispCount) };
                    return rv;
                default:
                    throw new NotImplementedException("Not supported session type: " + si.type);
            }
        }
    }
}
