using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Alternative
{
    /// <summary>
    /// Contains full project information and structure.
    /// Basic structure:
    /// PROJECT_NAME
    /// D  AESchemas
    /// D  defaultVars
    /// F  .folder
    /// </summary>
    public class ProjectDefinition
    {
        public ProjectDefinition(string name)
        {
            _name = name;
            _rootFolder = new FolderDesc(_name, "ae.rootfolder");
            _rootFolder.AddFixedChild("AESchemas");
            InitPalettes();
        }

        public string Name
        {
            get { return _name; }
        }

        public FolderDesc RootFolder
        {
            get { return _rootFolder; }
        }

        // privates
        private void InitPalettes()
        {
            _rootFolder.AddProjectProperty("ae.pallette.tcp.pallette.version", "5.6.0");
            _rootFolder.AddProjectProperty("palette.generalpalette.version", "5.3.0");
            _rootFolder.AddProjectProperty("ae.manualwork.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("aa.adapteradministrator.palette.version", "5.3.0");
            _rootFolder.AddProjectProperty("ae.palette.SOAPpalette.version", "5.6.0");
            _rootFolder.AddProjectProperty("projectName", "SLTemplate");
            _rootFolder.AddProjectProperty("ae.palette.Mail.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.rvpalette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.JMS.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("service.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("infranet.palette.version", "5.2.0.8");
            _rootFolder.AddProjectProperty("ae.palette.jrmi.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("r3.aer3adapter.palette.version", "5.4.1.2");
            _rootFolder.AddProjectProperty("ae.palette.Filepalette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.FTPpalette.version", "5.6.0");
            _rootFolder.AddProjectProperty("wsdlresource.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.process.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.JDBCpalette.version", "5.6.0");
            _rootFolder.AddProjectProperty("httppalette.httpplugins.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.parsepalette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.designerapp.version", "5.5.2.2");
            _rootFolder.AddProjectProperty("siebel.palette.version", "5.3.1.8");
            _rootFolder.AddProjectProperty("ae.repo.palette.version", "5.3.0");
            _rootFolder.AddProjectProperty("ae.aepalette.ae.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("turbo.palette.version", "5.3.1");
            _rootFolder.AddProjectProperty("ae.palette.Policy.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.java.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.transaction.palette.version", "5.6.0");
            _rootFolder.AddProjectProperty("ae.palette.GeneralPalette.version", "5.6.0");
            _rootFolder.AddProjectProperty("palette.ae.metadata.version", "5.3.0");
            _rootFolder.AddProjectProperty("ae.palette.xml.palette.version", "5.6.0");
        }

        // fields
        private string _name;
        private FolderDesc _rootFolder;
    }

    /// <summary>
    /// .folder file representation
    /// </summary>
    public class FolderDesc
    {
        public FolderDesc(string name, string resType)
        {
            _name = name;
            _resType = resType;
        }

        public void AddFixedChild(string child)
        {
            _fixedChildren.Add(child);
        }

        public void AddProjectProperty(string key, string value)
        {
            _projectProperties.Add(new KeyValuePair<string, string>(key, value));
        }

        public XElement AsXML()
        {
            return new XElement(Namespaces.repo + "repository"
                , new XAttribute(XNamespace.Xmlns + "Repository", Namespaces.repo)
                , new XElement("folder"
                    , new XAttribute("resourceType", _resType)
                    , new XAttribute("name", _name)
                    , new XAttribute("propLocks", "")
                    , new XAttribute("allowUserToChangeIcon", "false")
                    , new XAttribute("sortContents", "false")
                    , new XAttribute("acceptsResources", "true")
                    , new XAttribute("allowDuplicates", "false")
                )
                , new XElement("fixedChildren", _fixedChildren.Count > 0 ? _fixedChildren.Aggregate<string>((a, b) => a + ", " + b) : null)
                , _projectProperties.Count == 0 ? null : new XElement("projectProperties"
                    , (from pp in _projectProperties select new XElement("property"
                        , new XAttribute("key", pp.Key)
                        , new XAttribute("value", pp.Value)
                    ))
                )
            );
        }

        // fields
        private string _name;
        private string _resType;
        private List<string> _fixedChildren = new List<string>();
        private List<KeyValuePair<string, string>> _projectProperties = new List<KeyValuePair<string, string>>();
    }
}
