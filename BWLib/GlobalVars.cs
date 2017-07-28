using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace Alternative
{
    public interface ISettingsFactory
    {
        GlobalVars GetGlobalVars();
    }

    public class Variable
    {
        public Variable(string name, string value, string type)
            : this(name, value, type, true, false)
        {
        }

        public Variable(string name, string value, string type, bool deploymentSettable, bool serviceSettable)
        {
            _name = name;
            _value = value;
            _deploymentSettable = deploymentSettable;
            _serviceSettable = serviceSettable;
            _type = type;
        }

        public string Name
        {
            get { return _name; }
        }

        public XElement AsXML()
        {
            return new XElement(Namespaces.repo + "globalVariable"
                , new XElement(Namespaces.repo + "name", _name)
                , new XElement(Namespaces.repo + "value", _value)
                , new XElement(Namespaces.repo + "deploymentSettable", _deploymentSettable)
                , new XElement(Namespaces.repo + "serviceSettable", _serviceSettable)
                , new XElement(Namespaces.repo + "type", _type)
                , new XElement(Namespaces.repo + "modTime", (DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10000)
            );
        }

        private string _name;
        private string _value;
        private bool _deploymentSettable;
        private bool _serviceSettable;
        private string _type;
    }

    public class VariableComparer : IEqualityComparer<Variable>
    {
        public bool Equals(Variable x, Variable y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Variable obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    public class GlobalVarsRepo : AERepository
    {
        private String _path;
        private List<Variable> _vars = new List<Variable>();

        public GlobalVarsRepo(String path)
        {
            _path = path;
        }

        public void AddVariable(string name, string value, string type
            , bool deploymentSettable, bool serviceSettable)
        {
            _vars.Add(new Variable(name, value, type, deploymentSettable, serviceSettable));
        }

        #region AERepository Members

        public AELinkable Lookup(string s)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region AEItem Members

        public string FullPath
        {
            get { return _path; }
        }

        #endregion

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(Namespaces.repo + "repository"
                , new XAttribute("xmlns", Namespaces.repo)
                , new XAttribute(XNamespace.Xmlns + "xsi", Namespaces.xsi)
                , new XElement(Namespaces.repo + "globalVariables"
                    , from v in _vars
                      orderby v.Name
                      select v.AsXML()
                )
            );
        }

        #endregion
    }

    public class GlobalVars
    {
        public GlobalVars()
            : this("")
        {
        }

        public GlobalVars(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public void AddVariable(Variable var)
        {
            _vars.Add(var);
        }

        public void AddNode(GlobalVars n)
        {
            _tree.Add(n);
        }

        public IEnumerable<GlobalVars> GetNestedNodes()
        {
            return _tree;
        }

        public XElement AsXML()
        {
            XElement result = new XElement(Namespaces.repo + "repository"
                , new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance")
            );
            XElement list = new XElement(Namespaces.repo + "globalVariables");
            foreach (Variable v in from t in _vars orderby t.Name select t)
            {
                list.Add(v.AsXML());
            }
            result.Add(list);
            return result;
        }

        private HashSet<Variable> _vars = new HashSet<Variable>(new VariableComparer());
        private List<GlobalVars> _tree = new List<GlobalVars>();
        private string _name;
    }

    public class DefaultSettingsFactory : ISettingsFactory
    {
        public DefaultSettingsFactory(string appname)
        {
            _vars = new GlobalVars();
            _vars.AddVariable(new Variable("Deployment", appname, "String"));
            AddBWDefaults();
        }

        public GlobalVars GetGlobalVars()
        {
            return _vars;
        }

        private void AddBWDefaults()
        {
            Debug.Assert(_vars != null);
            _vars.AddVariable(new Variable("DirLedger", ".", "String"));
            _vars.AddVariable(new Variable("DirTrace", ".", "String"));
            _vars.AddVariable(new Variable("Domain", "dummy", "String"));
            _vars.AddVariable(new Variable("HawkEnabled", "false", "String"));
            _vars.AddVariable(new Variable("JmsProviderUrl", "tcp://localhost:7222", "String"));
            _vars.AddVariable(new Variable("JmsSslProviderUrl", "ssl://localhost:7243", "String"));
            _vars.AddVariable(new Variable("RemoteRvDaemon", "", "String"));
            _vars.AddVariable(new Variable("RvDaemon", "tcp:7500", "String"));
            _vars.AddVariable(new Variable("RvNetwork", "", "String"));
            _vars.AddVariable(new Variable("RvService", "", "String"));
            _vars.AddVariable(new Variable("RvaHost", "localhost", "String"));
            _vars.AddVariable(new Variable("RvaPort", "7600", "String"));
            _vars.AddVariable(new Variable("TIBHawkDaemon", "tcp:7474", "String"));
            _vars.AddVariable(new Variable("TIBHawkNetwork", "", "String"));
            _vars.AddVariable(new Variable("TIBHawkService", "7474", "String"));
        }

        private GlobalVars _vars = null;
    }
}
