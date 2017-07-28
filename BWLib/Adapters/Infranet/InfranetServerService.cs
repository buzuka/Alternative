using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;

namespace Alternative.Infranet
{
    public class InfranetServerService : InfranetService
    {
        private InfranetAdapterInstance _owner = null;
        private List<InfranetOperationView> _operations = new List<InfranetOperationView>();
        private AESchema _schema;
        private AERPCClass _rpc;

        public String Name { get; private set; }
        public AETransport Transport { get; set; }
        public bool InTransaction { get; set; }

        public AERPCClass RPC
        {
            get { return _rpc; }
        }

        public InfranetServerService(InfranetAdapterInstance owner, String name, AESchema schema)
        {
            _owner = owner;
            Name = name;
            _schema = schema;

            // schema init
            _rpc = new AERPCClass(_schema, "RequestResponseServiceOperationSchema");
            _schema.Add(_rpc);
        }

        public InfranetOperation AddQuery(String name, String template, int flags, String cls
            , XElement outFlist)
        {
            String className = name + "^" + name;
            IEnumerable<AERepoElement> outs = CreateTypes(_schema, outFlist, className + "^output");

            AERPCOperation op = new AERPCOperation(name);
            op.ReturnType = outs.Last();
            if (op.ReturnType.GetType() == typeof(AEClass))
            {
                ((AEClass)op.ReturnType).SuperClass = GenericStaticSchema.Instance().Lookup("gen_reply_base");
            }
            op.Parameters.Add(new AERPCOperationParameter("input")
            {
                Direction = ParameterDirection.IN
                ,
                Type = GenericStaticSchema.Instance().Lookup("RequestInputGenericSearch")
            });
            op.Exceptions.Add(new AERPCOperationException("REQUESTREPLY_ERROR")
            {
                Type = StaticSchemas.AE.Lookup("string")
            });
            if (InTransaction)
            {
                op.Parameters.Add(new AERPCOperationParameter("transaction_Input")
                {
                    Direction = ParameterDirection.IN
                    ,
                    Type = GenericStaticSchema.Instance().Lookup("transaction_Input")
                });
                op.Parameters.Add(new AERPCOperationParameter("transaction_Output")
                {
                    Direction = ParameterDirection.OUT
                    ,
                    Type = GenericStaticSchema.Instance().Lookup("transaction_Output")
                });
            }
            _rpc.AddOperation(op);

            foreach (AERepoElement re in outs)
            {
                _schema.Add(re);
            }

            InfranetOperationQuery res = new InfranetOperationQuery(this, name);
            res.Template = template;
            res.Flags = flags;
            res.OutputReference = outs.Last();
            res.Class = cls;

            _operations.Add(res);
            return new InfranetOperation(res, null);
        }

        public InfranetOperation AddOperation(String name, String opcode, int flags
            , XElement inFlist, XElement outFlist)
        {
            String className = name + "^" + opcode;
            IEnumerable<AERepoElement> ins = CreateTypes(_schema, inFlist, className + "^input");
            IEnumerable<AERepoElement> outs = CreateTypes(_schema, outFlist, className + "^output");

            AERPCOperation op = new AERPCOperation(name);
            op.ReturnType = outs.Last();
            if (op.ReturnType.GetType() == typeof(AEClass))
            {
                ((AEClass)op.ReturnType).SuperClass = GenericStaticSchema.Instance().Lookup("gen_reply_base");
            }
            op.Parameters.Add(new AERPCOperationParameter("input")
            {
                Direction = ParameterDirection.IN
                ,
                Type = ins.Last()
            });
            op.Exceptions.Add(new AERPCOperationException("REQUESTREPLY_ERROR")
            {
                Type = StaticSchemas.AE.Lookup("string")
            });
            if (InTransaction)
            {
                op.Parameters.Add(new AERPCOperationParameter("transaction_Input")
                {
                    Direction = ParameterDirection.IN
                    ,
                    Type = GenericStaticSchema.Instance().Lookup("transaction_Input")
                });
                op.Parameters.Add(new AERPCOperationParameter("transaction_Output")
                {
                    Direction = ParameterDirection.OUT
                    ,
                    Type = GenericStaticSchema.Instance().Lookup("transaction_Output")
                });
            }
            _rpc.AddOperation(op);

            foreach (AERepoElement re in ins)
            {
                _schema.Add(re);
            }

            foreach (AERepoElement re in outs)
            {
                _schema.Add(re);
            }

            InfranetOperationOpcode res = new InfranetOperationOpcode(this, name);
            res.Opcode = opcode;
            res.Flags = flags;
            res.InputReference = ins.Last();
            res.OutputReference = outs.Last();

            _operations.Add(res);
            return new InfranetOperation(res, null);
        }

        private List<AERepoElement> CreateTypes(AESchema s, XElement root, String name)
        {
            List<AERepoElement> result = new List<AERepoElement>();

            AEClass cl = new AEClass(s, name);
            foreach (XElement el in root.Elements())
            {
                String attrName = el.Name.LocalName;
                InfranetField i = _owner.GetInfranetField(attrName);
                AEAttribute attr = InfranetFactory.CreateAttribute(i);
                switch (i.Type)
                {
                    case "PIN_FLDT_ARRAY":
                        // get nested
                        List<AERepoElement> arr = CreateTypes(s, el, name + "^" + i.Name);
                        result.AddRange(arr);
                        // create sequence
                        AESequence seq = new AESequence(s, "sequence[" + arr.Last().Name + "]");
                        seq.ElementType = arr.Last();
                        result.Add(seq);
                        // add index
                        AEAttribute index = new AEAttribute("index");
                        index.AEType = StaticSchemas.AE.Lookup("i4");
                        ((AEClass)arr.Last()).Attributes.Insert(0, index);
                        // switch type
                        attr.AEType = seq;
                        break;
                    case "PIN_FLDT_SUBSTRUCT":
                        // get nested
                        List<AERepoElement> sub = CreateTypes(s, el, name + "^" + i.Name);
                        result.AddRange(sub);
                        // switch type
                        attr.AEType = sub.Last();
                        break;
                }
                cl.Attributes.Add(attr);
            }
            result.Add(cl);

            return result;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , new XElement(Namespaces.adinfra + "resourceType", "infranet.instance.server")
                , new XElement(Namespaces.adinfra + "name", Name)
                , new XElement(Namespaces.adinfra + "serviceName", Name)
                , Transport == null ? null : Transport.ToXML(Namespaces.adinfra + "transport").Nodes()
                , Util.MakeRef(Namespaces.adinfra + "schemaReference", _rpc)
                , new XElement(Namespaces.adinfra + "runInTransaction", InTransaction)
                , new XElement(Namespaces.adinfra + "Operations"
                    , from o in _operations select o.ToXML(Namespaces.adinfra + "operation")
                    )
                );
        }

        #endregion

        #region InfranetService Members

        public XName ServiceType
        {
            get { return Namespaces.adinfra + "serverService"; }
        }

        public InfranetAdapterInstance Owner
        {
            get { return _owner; }
        }

        public AESession Session
        {
            get { return Transport.Session; }
        }

        #endregion
    }
}