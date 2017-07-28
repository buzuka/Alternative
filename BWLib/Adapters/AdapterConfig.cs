using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace Alternative
{
    public struct LookupInfo
    {
        public String context;
        public String name;
    }

    public class Util
    {
        public static XElement MakeRef(XName n, AELinkable o)
        {
            if (o == null)
            {
                return null;
            }
            String text = o.Schema.FullPath;
            if (o.LocalType != "" && o.Name != "")
            {
                text += "#" + o.LocalType + "." + o.Name;
            }
            else if (o.LocalType != "")
            {
                text += "#" + o.LocalType;
            }
            return new XElement(n
                , new XAttribute("isRef", true)
                , new XText(text)
                );
        }

        public static XElement MakeRef(XName n, AERepository o)
        {
            if (o == null)
            {
                return null;
            }
            return new XElement(n
                , new XAttribute("isRef", true)
                , new XText(o.FullPath)
                );
        }

        public static LookupInfo ParseLookup(string s)
        {
            int i = s.IndexOf('.');
            if (i == -1)
            {
                return new LookupInfo() { context = "", name = s };
            }
            return new LookupInfo() { context = s.Substring(0, i), name = s.Substring(i + 1) };
        }
    }

    public class ForwardLink : AELinkable
    {
        private AERepository _schema;
        private string _path;
        private String _type;

        public ForwardLink(AERepository sc, string path, String type)
        {
            _schema = sc;
            _path = path;
            _type = type;
        }

        #region AELinkable Members

        public AERepository Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return _path; }
        }

        public string LocalType
        {
            get { return _type; }
        }

        #endregion
    }

    public abstract class AERepoElement : AELinkable, IXmlExportable
    {
        private AERepository _schema;
        private String _name;

        public AERepoElement(AERepository schema, String name)
        {
            _schema = schema;
            _name = name;
        }

        protected abstract String GetLocalType();
        protected abstract XElement ToXMLInternal(XName el);

        #region AELinkable Members

        public AERepository Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return _name; }
        }

        public String LocalType
        {
            get { return GetLocalType(); }
        }

        #endregion

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return ToXMLInternal(el);
        }

        #endregion
    }

    public class AERepoElementFactory<T>
    {
        private Func<AERepository, T> _callback;
        private String _name;

        public String Name
        {
            get { return _name; }
        }

        public AERepoElementFactory(String name, Func<AERepository, T> c)
        {
            _name = name;
            _callback = c;
        }

        public T Create(AERepository schema)
        {
            return _callback(schema);
        }
    }

    public class AEClass : AERepoElement
    {
        private List<AEAttribute> _attr = new List<AEAttribute>();

        #region Public properties

        public AELinkable SuperClass { get; set; }
        public ExtendedProperties Properties { get; set; }

        public List<AEAttribute> Attributes
        {
            get { return _attr; }
        }

        #endregion

        public AEClass(AERepository schema, String name)
            : base(schema, name)
        {
        }

        protected override String GetLocalType()
        {
            return "class";
        }

        protected override XElement ToXMLInternal(XName el)
        {
            return new XElement(el
                , new XAttribute("name", Name)
                , Util.MakeRef(Namespaces.meta + "superclass", SuperClass)
                , from AEAttribute a in _attr
                  select a.ToXML(Namespaces.meta + "attribute")
                , Properties == null ? null : Properties.ToXML(Namespaces.meta + "extendedProperties")
                );
        }
    }

    public class AEAttribute : IXmlExportable
    {
        public String Name { get; private set; }
        public AELinkable AEType { get; set; }
        public String Default { get; set; }
        public String Key { get; set; }
        public ExtendedProperties Properties { get; set; }

        public AEAttribute(String name)
        {
            Name = name;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.meta + "isReadable", true)
                , new XElement(Namespaces.meta + "isWriteable", true)
                , new XElement(Namespaces.meta + "name", Name)
                , Util.MakeRef(Namespaces.meta + "attributeType", AEType)
                , Default != null ? new XElement(Namespaces.meta + "default", Default) : null
                , Key != null ? new XElement(Namespaces.meta + "isKey", Key) : null
                , Properties == null ? null : Properties.ToXML(Namespaces.meta + "extendedProperties")
                );
        }

        #endregion
    }

    public class AEScalar : AERepoElement
    {
        public String DataType { get; set; }
        public String JavaClass { get; set; }

        public AEScalar(AERepository schema, String name)
            : base(schema, name)
        {
        }

        protected override String GetLocalType()
        {
            return "scalar";
        }

        protected override XElement ToXMLInternal(XName el)
        {
            throw new NotImplementedException();
        }

    }

    public class AESequence : AERepoElement
    {
        public AELinkable ElementType { get; set; }

        public AESequence(AERepository schema, String name)
            : base(schema, name)
        {
        }

        protected override String GetLocalType()
        {
            return "sequence";
        }

        protected override XElement ToXMLInternal(XName el)
        {
            return new XElement(el
                , new XAttribute("name", Name)
                , Util.MakeRef(Namespaces.meta + "elementType", ElementType)
                );
        }
    }

    public class AERPCClass : AERepoElement
    {
        private List<AERPCOperation> _operations = new List<AERPCOperation>();

        public AERPCClass(AERepository schema, String name)
            : base(schema, name)
        {
        }

        public void AddOperation(AERPCOperation op)
        {
            _operations.Add(op);
        }

        protected override String GetLocalType()
        {
            return "rpcClass";
        }

        protected override XElement ToXMLInternal(XName el)
        {
            return new XElement(el
                , new XAttribute("name", Name)
                , from o in _operations
                  select o.ToXML(Namespaces.meta + "operation")
            );
        }
    }

    public class AERPCOperation : IXmlExportable
    {
        private string _name;
        private List<AERPCOperationParameter> _params = new List<AERPCOperationParameter>();
        private List<AERPCOperationException> _exs = new List<AERPCOperationException>();

        public String Name { get { return _name; } }
        public AELinkable ReturnType { get; set; }
        public List<AERPCOperationParameter> Parameters
        {
            get { return _params; }
        }

        public List<AERPCOperationException> Exceptions
        {
            get { return _exs; }
        }

        public AERPCOperation(String name)
        {
            _name = name;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.meta + "oneway", "false")
                , new XElement(Namespaces.meta + "name", _name)
                , Util.MakeRef(Namespaces.meta + "returnType", ReturnType)
                , from t in _params select t.ToXML(Namespaces.meta + "parameter")
                , from p in _exs select p.ToXML(Namespaces.meta + "raises")
            );
        }

        #endregion
    }

    public enum ParameterDirection
    {
        IN,
        OUT
    }

    public class AERPCOperationParameter : IXmlExportable
    {
        private string _name;

        public ParameterDirection Direction { get; set; }
        public AELinkable Type { get; set; }

        public AERPCOperationParameter(string name)
        {
            _name = name;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.meta + "direction", Direction.ToString().ToLower())
                , new XElement(Namespaces.meta + "name", _name)
                , Util.MakeRef(Namespaces.meta + "parameterType", Type)
            );
        }

        #endregion
    }

    public class AERPCOperationException : IXmlExportable
    {
        private string _name;

        public AELinkable Type { get; set; }

        public AERPCOperationException(String name)
        {
            _name = name;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.meta + "name", _name)
                , Util.MakeRef(Namespaces.meta + "raisesType", Type)
            );
        }

        #endregion
    }

    public abstract class AESession : AERepoElement
    {
        public AESession(AERepository schema, String name)
            : base(schema, name)
        {
        }

        abstract public String TransportType { get; }
        abstract public String QoS { get; }
    }

    public class RvSession : AESession
    {
        public String Daemon { get; set; }
        public String Service { get; set; }
        public String Network { get; set; }
        public ExtendedProperties Properties { get; set; }

        public RvSession(AERepository schema, String name)
            : base(schema, name)
        {
        }

        protected override String GetLocalType()
        {
            return "rvSession";
        }

        protected override XElement ToXMLInternal(XName el)
        {
            return new XElement(el
                , new XAttribute("objectType", "session.RV")
                , new XAttribute("name", Name)
                , new XElement(Namespaces.aesvc + "daemon", Daemon)
                , new XElement(Namespaces.aesvc + "service", Service)
                , new XElement(Namespaces.aesvc + "network", Network)
                , Properties == null ? null : Properties.ToXML(Namespaces.aesvc + "extendedProperties")
                );
        }

        #region AESession Members

        public override string TransportType
        {
            get { return "rv"; }
        }

        public override string QoS
        {
            get { return "ae.sessions.rvsession"; }
        }

        #endregion
    }

    public class Server : AERepoElement
    {
        public AELinkable Class { get; set; }
        public String Subject { get; set; }
        public AELinkable Session { get; set; }

        public Server(AERepository schema, String name)
            : base(schema, name)
        {
        }

        protected override String GetLocalType()
        {
            return "server";
        }

        protected override XElement ToXMLInternal(XName el)
        {
            return new XElement(el
                , new XAttribute("objectType", "endpoint.RVRPCServer")
                , new XAttribute("name", Name)
                , Util.MakeRef(Namespaces.aesvc + "class", Class)
                , new XElement(Namespaces.aesvc + "subject", Subject)
                , new XElement(Namespaces.aesvc + "wireFormat", "aeRvMsg")
                , Util.MakeRef(Namespaces.aesvc + "session", Session)
                );
        }
    }

    public class Consumer : AERepoElement
    {
        public String Subject { get; set; }
        public AELinkable Session { get; set; }

        public Consumer(AERepository schema, String name)
            : base(schema, name)
        {
        }

        protected override String GetLocalType()
        {
            return "consumer";
        }

        protected override XElement ToXMLInternal(XName el)
        {
            return new XElement(el
                , new XAttribute("objectType", "endpoint.RVSubscriber")
                , new XAttribute("name", Name)
                , new XElement(Namespaces.aesvc + "subject", Subject)
                , new XElement(Namespaces.aesvc + "wireFormat", "aeRvMsg")
                , Util.MakeRef(Namespaces.aesvc + "session", Session)
                );
        }
    }

    public class AdapterConfigExporter
    {
        private string _base;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basepath">Output folder</param>
        public AdapterConfigExporter(String basepath)
        {
            _base = basepath;
        }

        /// <summary>
        /// Stores all items to filesystem.
        /// </summary>
        public void Export(AdapterConfig ac)
        {
            foreach (AEItem it in ac.AllItems)
            {
                String name = _base + it.FullPath;
                using (StreamWriter fl = OpenStream(name))
                {
                    XDocument xd = new XDocument(new XDeclaration("1.0", "UTF-8", ""));
                    xd.Add(it.ToXML("dummy"));
                    XmlWriterSettings sett = new XmlWriterSettings();
                    sett.Indent = true;
                    sett.IndentChars = "    ";
                    using (XmlWriter wr = XmlWriter.Create(fl, sett))
                    {
                        xd.WriteTo(wr);
                    }
                }
            }
        }

        private StreamWriter OpenStream(string name)
        {
            String folder = name.Substring(0, name.LastIndexOf('/'));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            FileStream fs = new FileStream(name, FileMode.Create);
            return new StreamWriter(fs);
        }
    }

    #region Static schemas

    public sealed class StaticSchemas
    {
        public static AERepository AE = new STATIC_ae();
    }

    sealed class STATIC_ae : AESchema
    {
        public STATIC_ae()
            : base("/AESchemas/ae.aeschema")
        {
            Add(new AEScalar(this, "any") { DataType = "any" });
            Add(new AEScalar(this, "assocList") { DataType = "assocList" });
            Add(new AEScalar(this, "binary") { DataType = "binary", JavaClass = "java.lang.Byte[]" });
            Add(new AEScalar(this, "boolean") { DataType = "boolean", JavaClass = "java.lang.Boolean" });
            Add(new AEScalar(this, "char") { DataType = "char", JavaClass = "java.lang.Character" });
            Add(new AEScalar(this, "date") { DataType = "date", JavaClass = "java.util.Date" });
            Add(new AEScalar(this, "dateTime") { DataType = "dateTime", JavaClass = "java.util.Date" });
            Add(new AEScalar(this, "i1") { DataType = "i1", JavaClass = "java.lang.Byte" });
            Add(new AEScalar(this, "i2") { DataType = "i2", JavaClass = "java.lang.Short" });
            Add(new AEScalar(this, "i4") { DataType = "i4", JavaClass = "java.lang.Integer" });
            Add(new AEScalar(this, "i8") { DataType = "i8", JavaClass = "java.lang.Long" });
            Add(new AEScalar(this, "interval") { DataType = "interval" });
            Add(new AEScalar(this, "r4") { DataType = "i4", JavaClass = "java.lang.Float" });
            Add(new AEScalar(this, "r8") { DataType = "i8", JavaClass = "java.lang.Double" });
            Add(new AEScalar(this, "string") { DataType = "string", JavaClass = "java.lang.String" });
            Add(new AEScalar(this, "time") { DataType = "time", JavaClass = "java.util.Date" });
            Add(new AEScalar(this, "ui1") { DataType = "ui1", JavaClass = "java.lang.Short" });
            Add(new AEScalar(this, "ui2") { DataType = "ui2", JavaClass = "java.lang.Integer" });
            Add(new AEScalar(this, "ui4") { DataType = "ui4", JavaClass = "java.lang.Long" });
            Add(new AEScalar(this, "ui8") { DataType = "ui8", JavaClass = "java.lang.Long" });
            Add(new AEScalar(this, "void") { DataType = "void" });
        }
    }

    #endregion
}
