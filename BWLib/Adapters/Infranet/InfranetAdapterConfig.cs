using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;

namespace Alternative
{
    public partial class Namespaces
    {
        public static XNamespace adinfra = "http://www.tibco.com/xmlns/adapter/adinfra/2002";
    }
}

namespace Alternative.Infranet
{
    public class InfranetAdapterConfig : AdapterConfig
    {
        private List<InfranetField> _customFields = new List<InfranetField>();
        private List<AERepository> _schemas = new List<AERepository>();
        private List<InfranetAdapterInstance> _instances = new List<InfranetAdapterInstance>();
        private AESchema _commandSchema;

        public List<AERepository> Schemas
        {
            get { return _schemas; }
        }

        public List<InfranetAdapterInstance> Instances
        {
            get { return _instances; }
        }

        public AESchema CommandSchema
        {
            get { return _commandSchema; }
        }

        public InfranetAdapterConfig()
        {
            InitDefaultItems();
        }

        private void InitDefaultItems()
        {
            // default schemas
            _schemas.Add(GenericStaticSchema.Instance());
            _schemas.Add(InfranetStaticSchema.Instance());

            _commandSchema = new AESchema("/AESchemas/ae/Infranet/CommandSchema.aeschema", true);
            _commandSchema.Add(new AEClass(_commandSchema, "DummyClass") { Properties = new ExplicitNilProperties() });
            _schemas.Add(_commandSchema);
        }

        public InfranetAdapterInstance AddInstance(String name)
        {
            InfranetAdapterInstance ins = new InfranetAdapterInstance(this, name);
            _instances.Add(ins);
            return ins;
        }

        public InfranetAdapterInstance AddInstance(String name, String path)
        {
            InfranetAdapterInstance ins = new InfranetAdapterInstance(this, name, path);
            _instances.Add(ins);
            return ins;
        }

        public void AddRepo(AERepository schema)
        {
            _schemas.Add(schema);
        }

        public void AddCustomField(String name, int id, String type)
        {
            int i = (from t in _customFields
                     where t.Name == name
                     select t).Count();
            if (i == 0)
            {
                FieldImpl n = new FieldImpl(name, id, type);
                _customFields.Add(n);
            }
        }

        public InfranetField GetInfranetField(String name)
        {
            FieldInfo fld = typeof(DefaultFields).GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (fld != null)
            {
                return (InfranetField)fld.GetValue(null);
            }
            var l = from t in _customFields
                    where t.Name == name
                    select t;
            if (l.Count() == 0)
            {
                throw new ArgumentException("Field " + name + " not found");
            }
            return l.First();
        }

        #region AdapterConfig Members

        public string Family
        {
            get { return "Infranet"; }
        }

        public IEnumerable<AEItem> AllItems
        {
            get
            {
                foreach (AERepository it in _schemas)
                {
                    yield return it;
                }
                foreach (InfranetAdapterInstance it in _instances)
                {
                    yield return it;
                }
            }
        }

        #endregion
    }

    public class InfranetConnectionInfo
    {
        public String host = "%%adinfraHost%%";
        public String port = "%%adinfraPortNo%%";
        public String login = "%%adinfraLogin%%";
        public String password = "%%adinfraPassword%%";

        public String suspendAfter = "5";
        public String retires = "-1";
        public String timer = "0";
        public String sleep = "100";
    }

    public class InfranetAdapterInstance : IXmlExportable, AERepository
    {
        private List<InfranetService> _services = new List<InfranetService>();
        private List<AERepoElement> _system = new List<AERepoElement>();
        private InfranetAdapterConfig _owner;
        private String _name;
        private String _path = "/";

        private bool exporting = false;

        private int _transactionTimeout = 60000;
        private int _maxTransactions = 3;

        private String _stdMicroAgentTimeout = "10000";
        private String _classMicroAgentTimeout = "10000";

        private InfranetConnectionInfo _con = new InfranetConnectionInfo();

        private const String TERMINATOR = "TerminateSubscriberRv";
        private const String DEFAULT_RV_SESSION = "DefaultRVSession";
        private String _terminateSubject = "%%Domain%%.%%Deployment%%.adinfra.%%InstanceId%%.exit";

        private List<LogSink> _logs = new List<LogSink>();

        public String Name
        {
            get { return _name; }
        }

        public InfranetConnectionInfo Connection
        {
            get { return _con; }
        }

        public InfranetAdapterInstance(InfranetAdapterConfig owner, String name) : this(owner, name, "/")
        {
        }

        public InfranetAdapterInstance(InfranetAdapterConfig owner, String name, String path)
        {
            _owner = owner;
            _name = name;
            _path = path;

            // create default entities
            RvSession hawk = new RvSession(this, "InfranetHawkDefault");
            hawk.Daemon = "%%TIBHawkDaemon%%";
            hawk.Service = "%%TIBHawkService%%";
            hawk.Network = "%%TIBHawkNetwork%%";
            _system.Add(hawk);

            //RvSession rv = new RvSession(this, DEFAULT_RV_SESSION);
            //rv.Daemon = "%%RvDaemon%%";
            //rv.Service = "%%RvService%%";
            //rv.Network = "%%RvNetwork%%";
            //rv.Properties = new SessionProperties() { DispatcherCount = 5 };
            //_system.Add(rv);

            //Consumer term = new Consumer(this, "TerminateSubscriberRv");
            //term.Subject = _terminateSubject;
            //term.Session = rv;
            //_system.Add(term);

            // Default logging settings
            StdioLogSink s = new StdioLogSink();
            s.AddRole("errorRole");
            s.AddRole("infoRole");
            s.AddRole("warnRole");
            s.AddRole("debugRole");

            FileLogSink f = new FileLogSink();
            f.AddRole("errorRole");
            f.AddRole("infoRole");
            f.AddRole("warnRole");
            f.AddRole("debugRole");

            _logs.Add(s);
            _logs.Add(f);
        }

        public void AddRepo(AERepository repo)
        {
            _owner.AddRepo(repo);
        }

        public InfranetField GetInfranetField(String name)
        {
            return _owner.GetInfranetField(name);
        }

        public void SetTransactionInfo(int timeout, int maxCount)
        {
            _transactionTimeout = timeout;
            _maxTransactions = maxCount;
        }

        public void SetStdMicroAgentTimeout(String value)
        {
            _stdMicroAgentTimeout = value;
        }

        public void SetClassMicroAgentTimeout(String value)
        {
            _classMicroAgentTimeout = value;
        }

        public void SetLogSinks(List<LogSink> newList)
        {
            _logs = newList;
        }

        public void SetConnectionAdvanced(string suspendAfter, string retries, string timer, string sleep)
        {
            _con.suspendAfter = suspendAfter;
            _con.retires = retries;
            _con.timer = timer;
            _con.sleep = sleep;
        }

        public void CreateTerminator(String session, String subject)
        {
            AELinkable exists = Lookup(TERMINATOR);
            if (exists == null)
            {
                AELinkable sess = Lookup(session);
                if (sess == null)
                {
                    sess = GetDefaultSession(session);
                }
                Consumer term = new Consumer(this, TERMINATOR);
                term.Subject = subject == "" ? _terminateSubject : subject;
                term.Session = sess;
                _system.Add(term);
            }
        }

        public AESession GetDefaultSession(String session)
        {
            if (session == "")
            {
                session = DEFAULT_RV_SESSION;
            }
            AESession found = (AESession)this.Lookup(session);
            if (found == null)
            {
                RvSession rv = new RvSession(this, session);
                rv.Daemon = "%%RvDaemon%%";
                rv.Service = "%%RvService%%";
                rv.Network = "%%RvNetwork%%";
                rv.Properties = new SessionProperties() { DispatcherCount = 0 };
                _system.Add(rv);

                found = rv;
            }

            return found;
        }

        public InfranetServerService AddServerService(String name)
        {
            return AddServerService(name, new AERepoElementFactory<AESession>(DEFAULT_RV_SESSION, x => GetDefaultSession("")), "");
        }

        public InfranetServerService AddServerService(String name, AERepoElementFactory<AESession> sessionFactory, String subject)
        {
            AESchema s = new AESchema("/AESchemas/ae/Infranet/" + _name + "/" + name + ".aeschema");
            InfranetServerService result = new InfranetServerService(this, name, s);

            AESession foundSession = (AESession)Lookup(sessionFactory.Name);
            if (foundSession == null)
            {
                AESession session = sessionFactory.Create(this);
                _system.Add(session);
                foundSession = session;
            }

            Server endpoint = new Server(this, name + "EndPoint");
            endpoint.Class = result.RPC;
            endpoint.Subject = subject == "" ? "%%Domain%%.%%Deployment%%.adinfra." + _name + "." + name : subject;
            endpoint.Session = foundSession;
            _system.Add(endpoint);
            AETransport tr = new AETransport()
            {
                Endpoint = endpoint,
                Session = foundSession,
            };
            result.Transport = tr;
            _services.Add(result);
            _owner.AddRepo(s);
            return result;
        }

        #region IXmlExportable Members

        private XElement StartupXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.aesdk + "defaultStartup", "active")
                , new XElement(Namespaces.aesdk + "banner", true)
                , new XElement(Namespaces.aesdk + "hasStdMicroAgent", true)
                , new XElement(Namespaces.aesdk + "stdMicroAgentName", "COM.TIBCO.ADAPTER.adinfra.%%Deployment%%.%%InstanceId%%")
                , new XElement(Namespaces.aesdk + "stdMicroAgentTimeout", _stdMicroAgentTimeout)
                , new XElement(Namespaces.aesdk + "hasClassMicroAgent", "%%HawkEnabled%%")
                , new XElement(Namespaces.aesdk + "classMicroAgentTimeout", _classMicroAgentTimeout)
                , new XElement(Namespaces.aesdk + "classMicroAgentName", "COM.TIBCO.adinfra.%%Deployment%%.%%InstanceId%%")
                , new XElement(Namespaces.aesdk + "defaultMicroAgentSession", "InfranetHawkDefault")
                , from s in _system
                  select
                    new XElement(Namespaces.aesdk + "startComponent"
                        , new XElement(Namespaces.aesdk + "state", "active")
                        , new XElement(Namespaces.aesdk + "name", s.Name)
                        )
                );
        }

        private XElement DeploymentXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.aesdk + "advisories", new XAttribute(Namespaces.xsi + "nil", true))
                , new XElement(Namespaces.aesdk + "sessions"
                    , new XElement(Namespaces.aesdk + "messaging"
                        , from s in _system
                          where s.LocalType == "rvSession" || s.LocalType == "rcCmSession"
                          select Util.MakeRef(Namespaces.aesdk + s.LocalType, s)
                        )
                    )
                , new XElement(Namespaces.aesdk + "consumers"
                    , from s in _system
                          where s.LocalType == "consumer"
                          select Util.MakeRef(Namespaces.aesdk + "rvSubscriber", s)
                    )
                , new XElement(Namespaces.aesdk + "servers"
                    , from s in _system
                      where s.LocalType == "server"
                      select Util.MakeRef(Namespaces.aesdk + "rvRpcServer", s)
                    )
                , new XElement(Namespaces.aesdk + "producers", new XAttribute(Namespaces.xsi + "nil", true))
                );
        }

        private XElement ReportingXML(XName el)
        {
            return new XElement(el
                , from l in _logs
                  select l.ToXML(l.Name)
                );
        }

        private XElement DesignerXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.aesdk + "advancedLogging", true)
                , new XElement(Namespaces.aesdk + "adapterVersion", "sdk51")
                , new XElement(Namespaces.aesdk + "serviceOwnedSessions"
                    , from s in _services
                      group s by s.Session.Name into sGroup
                      select new XElement(Namespaces.aesdk + "session", sGroup.First().Session.Name)
                    )
                , new XElement(Namespaces.aesdk + "fixedChildren")
                , new XElement(Namespaces.aesdk + "lockedProperties")
                , new XElement(Namespaces.aesdk + "adapterTester"
                    , new XElement(Namespaces.aesdk + "args", "--run --propFile")
                    , new XElement(Namespaces.aesdk + "exe", "C:\\Tibco\\adapter\\adinfra\\5.2\\bin\\adinfra.exe")
                    , new XElement(Namespaces.aesdk + "workingDir", "c:\\temp")
                    )
                , new XElement(Namespaces.aesdk + "resourceDescriptions")
                );
        }

        public XElement ToXML(XName el)
        {
            exporting = true;
            XElement res = new XElement(Namespaces.repo + "repository"
                , new XAttribute(XNamespace.Xmlns + "AESDK", Namespaces.aesdk)
                , new XAttribute(XNamespace.Xmlns + "xsi", Namespaces.xsi)
                , new XAttribute(XNamespace.Xmlns + "AEService", Namespaces.aesvc)
                , new XAttribute(XNamespace.Xmlns + "Repository", Namespaces.repo)
                , new XElement(Namespaces.adinfra + "adapter"
                    , new XAttribute(XNamespace.Xmlns + "adinfra", Namespaces.adinfra)
                    , new XAttribute("name", "InfranetAdapter")
                    , new XElement(Namespaces.aesdk + "instanceId", _name)
                    , StartupXML(Namespaces.aesdk + "startup")
                    , DeploymentXML(Namespaces.aesdk + "deployment")
                    , new XElement(Namespaces.aesdk + "timers", new XAttribute(Namespaces.xsi + "nil", "true"))
                    , new XElement(Namespaces.aesdk + "txControls", new XAttribute(Namespaces.xsi + "nil", "true"))
                    , ReportingXML(Namespaces.aesdk + "reporting")
                    , new XElement(Namespaces.aesdk + "metadata"
                        // TODO: filter only instance-related schemas
                        , from s in _owner.Schemas select Util.MakeRef(Namespaces.aesdk + "loadUrl", s)
                        )
                    , DesignerXML(Namespaces.aesdk + "designer")
                    , new XElement(Namespaces.adinfra + "adapterServices"
                        , from s in _services select s.ToXML(s.ServiceType)
                        )
                    , new XElement(Namespaces.adinfra + "DesignTimeConnectionInfo"
                        , new XElement(Namespaces.adinfra + "rememberPassword", true)
                        , new XElement(Namespaces.adinfra + "useDesignTimeSettings", false)
                        , new XElement(Namespaces.adinfra + "designTimeHostName", _con.host)
                        , new XElement(Namespaces.adinfra + "designTimePortNo", _con.port)
                        , new XElement(Namespaces.adinfra + "designTimeLoginName", _con.login)
                        , new XElement(Namespaces.adinfra + "designTimePassword", _con.password)
                        )
                    , new XElement(Namespaces.adinfra + "ConnectionInfo"
                        , new XElement(Namespaces.adinfra + "hostName", _con.host)
                        , new XElement(Namespaces.adinfra + "portNo", _con.port)
                        , new XElement(Namespaces.adinfra + "loginName", _con.login)
                        , new XElement(Namespaces.adinfra + "password", _con.password)
                        , new XElement(Namespaces.adinfra + "retriesBeforeSuspend", _con.suspendAfter)
                        , new XElement(Namespaces.adinfra + "maxRetries", _con.retires)
                        , new XElement(Namespaces.adinfra + "connectionTimer", _con.timer)
                        , new XElement(Namespaces.adinfra + "sleepBetweenRetries", _con.sleep)
                        )
                    , new XElement(Namespaces.adinfra + "Options"
                        , new XElement(Namespaces.adinfra + "errorCodeList", "PIN_ERR_STREAM_IO,PIN_ERR_NAP_CONNECT_FAILED,PIN_ERR_BAD_LOGIN_REDIRECT_INFO,PIN_ERR_TOO_MANY_LOGIN_REDIRECTS,PIN_ERR_STORAGE_DISCONNECT,PIN_ERR_DM_CONNECT_FAILED")
                        , new XElement(Namespaces.adinfra + "adapterEncoding", "UTF8")
                        , new XElement(Namespaces.adinfra + "transactionTimer", _transactionTimeout)
                        , new XElement(Namespaces.adinfra + "maxTransConnections", _maxTransactions)
                        , new XElement(Namespaces.adinfra + "terminateSubject", _terminateSubject)
                        )
                    )
                , from s in _system select s.ToXML(Namespaces.aesvc + s.LocalType)
                );
            exporting = false;
            return res;
        }

        #endregion

        #region AEItem Members

        public string FullPath
        {
            get { return exporting ? "" : _path + "/" + Name + ".adinfra"; }
        }

        #endregion

        #region AERepository Members

        public AELinkable Lookup(string s)
        {
            // TODO: implement search
            LookupInfo li = Util.ParseLookup(s);
            var list = from e in _system
                       where e.Name == li.name
                       select e;
            if (list.Count() == 0)
            {
                return null;
            }
            if (li.context != "")
            {
                foreach (var t in list)
                {
                    if (t.LocalType == li.context)
                    {
                        return t;
                    }
                }
            }
            return list.First();
        }

        #endregion
    }

    /// <summary>
    /// type only
    /// </summary>
    public interface InfranetService : IXmlExportable
    {
        XName ServiceType { get; }

        InfranetAdapterInstance Owner { get; }

        AESession Session { get; }
    }

    public class InfranetOperation
    {
        private InfranetOperationView _view;
        private AERPCOperation _rpc;

        public InfranetOperation(InfranetOperationView view, AERPCOperation rpc)
        {
            _view = view;
            _rpc = rpc;
        }
    }

    public interface InfranetOperationView : IXmlExportable
    {
    }

    public class InfranetOperationOpcode : InfranetOperationView
    {
        private InfranetService _owner = null;

        public String Name { get; private set; }
        public String Opcode { get; set; }
        public int Flags { get; set; }

        public AELinkable InputReference { get; set; }
        public AELinkable OutputReference { get; set; }
        //public AELinkable CMDInputReference { get; set; }
        //public AELinkable CMDOutputReference { get; set; }

        public InfranetOperationOpcode(InfranetService owner, String name)
        {
            _owner = owner;
            Name = name;
            Flags = 0;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.adinfra + "operationName", Name)
                , new XElement(Namespaces.adinfra + "operationType", "opcode")
                , new XElement(Namespaces.adinfra + "opcode", Opcode)
                , new XElement(Namespaces.adinfra + "opflag", Flags)
                , Util.MakeRef(Namespaces.adinfra + "inputSchemaReference", InputReference)
                , Util.MakeRef(Namespaces.adinfra + "outputSchemaReference", OutputReference)
                //, Util.MakeRef(Namespaces.adinfra + "inputCommandSchemaReference", CMDInputReference)
                //, Util.MakeRef(Namespaces.adinfra + "outputCommandSchemaReference", CMDOutputReference)
                );
        }

        #endregion
    }

    public class InfranetOperationQuery : InfranetOperationView
    {
        private InfranetService _owner = null;

        public String Name { get; private set; }
        public String Template { get; set; }
        public int Flags { get; set; }
        public String Class { get; set; }

        public AELinkable OutputReference { get; set; }
        //public AELinkable CMDOutputReference { get; set; }

        public InfranetOperationQuery(InfranetService owner, String name)
        {
            _owner = owner;
            Name = name;
            Flags = 0;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.adinfra + "operationName", Name)
                , new XElement(Namespaces.adinfra + "operationType", "query")
                , new XElement(Namespaces.adinfra + "queryName", Name)
                , new XElement(Namespaces.adinfra + "searchTemplate", Template)
                , new XElement(Namespaces.adinfra + "searchFlag", Flags)
                , Class != "" ? new XElement(Namespaces.adinfra + "storableClass", Class) : null
                , Util.MakeRef(Namespaces.adinfra + "outputSchemaReference", OutputReference)
                //, Util.MakeRef(Namespaces.adinfra + "outputCommandSchemaReference", CMDOutputReference)
                );
        }

        #endregion
    }

    public class AETransport : IXmlExportable
    {
        public AESession Session { get; set; }
        public Server Endpoint { get; set; }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.adinfra + "transportType", Session.TransportType)
                , new XElement(Namespaces.adinfra + "qualityOfService", Session.QoS)
                , new XElement(Namespaces.adinfra + "wireFormat", "aeRvMsg")
                , Util.MakeRef(Namespaces.adinfra + "sessionReference", Session)
                , Util.MakeRef(Namespaces.adinfra + "endpointReference", Endpoint)
                , new XElement(Namespaces.adinfra + "messageSubject", Endpoint.Subject)
                );
        }

        #endregion
    }

    public enum InfranetFunctions
    {
        query,
        opcode
    }

    public class InfranetAttributeProperties : ExtendedProperties
    {
        public Boolean Mandatory { get; set; }
        public InfranetField Field { get; set; }
        public String ContainedClass { get; set; }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(el.Namespace + "isMandatory", Mandatory)
                , new XElement(el.Namespace + "targetFieldType", Field.Type)
                , new XElement(el.Namespace + "targetFieldName", Field.ID)
                , ContainedClass == null ? null : new XElement(el.Namespace + "containedClass", ContainedClass)
                );
        }

        #endregion
    }

    public class InfranetClassProperties : ExtendedProperties
    {
        public String TargetSchema { get; set; }
        public String TargetClass { get; set; }
        public InfranetFunctions FunctionType { get; set; }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(el.Namespace + "targetSchema", TargetSchema)
                , new XElement(el.Namespace + "targetClass", TargetClass)
                , new XElement(el.Namespace + "functionType", FunctionType.ToString())
                );
        }

        #endregion
    }

    public class ExplicitNilProperties : ExtendedProperties
    {
        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XAttribute(Namespaces.xsi + "nil", true)
                );
        }

        #endregion
    }

    public class SessionProperties : ExtendedProperties
    {
        public int DispatcherCount { get; set; }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement("dispatcherCount", DispatcherCount)
                );
        }

        #endregion
    }

    public class InfranetFactory
    {
        public static AEClass CreateClass(AERepository schema, String name, InfranetFunctions func)
        {
            AEClass res = new AEClass(schema, name);
            res.Properties = new InfranetClassProperties()
            {
                TargetSchema = "FLIST",
                TargetClass = name,
                FunctionType = func
            };
            return res;
        }

        public static AEAttribute CreateAttribute(InfranetField fld)
        {
            return CreateAttribute(fld, null, false, null);
        }

        public static AEAttribute CreateAttribute(InfranetField fld, AEClass cls)
        {
            return CreateAttribute(fld, cls, false, null);
        }

        public static AEAttribute CreateAttribute(InfranetField fld, AEClass cls, bool mandatory)
        {
            return CreateAttribute(fld, cls, mandatory, null);
        }

        public static AEAttribute CreateAttribute(InfranetField fld, AEClass cls, String isKey)
        {
            return CreateAttribute(fld, cls, false, isKey);
        }

        public static AEAttribute CreateAttribute(InfranetField fld, AEClass cls, bool mandatory, String isKey)
        {
            AEAttribute res = new AEAttribute(fld.Name);
            res.Properties = new InfranetAttributeProperties()
            {
                Mandatory = mandatory,
                Field = fld,
                ContainedClass = cls == null ? null : cls.Name
            };
            switch (fld.Type)
            {
                case "PIN_FLDT_STR":
                case "PIN_FLDT_POID":
                    res.AEType = StaticSchemas.AE.Lookup("string");
                    break;
                case "PIN_FLDT_INT":
                case "PIN_FLDT_ENUM":
                    res.AEType = StaticSchemas.AE.Lookup("i4");
                    break;
                case "PIN_FLDT_DECIMAL":
                    res.AEType = StaticSchemas.AE.Lookup("r8");
                    break;
                case "PIN_FLDT_TSTAMP":
                    res.AEType = StaticSchemas.AE.Lookup("dateTime");
                    break;
                case "PIN_FLDT_BUF":
                    res.AEType = StaticSchemas.AE.Lookup("binary");
                    break;
                case "PIN_FLDT_ARRAY":
                    res.AEType = InfranetStaticSchema.Instance().Lookup("pin_array");
                    break;
                case "PIN_FLDT_SUBSTRUCT":
                    // post creation
                    res.AEType = StaticSchemas.AE.Lookup("void");
                    break;
                default:
                    throw new NotImplementedException("Implement this type: " + fld.Type);
            }
            res.Key = isKey;
            return res;
        }
    }
}
