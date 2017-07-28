using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Alternative
{
    public class AESchema : AERepository
    {
        private String _path = "";
        private List<AERepoElement> _elements = new List<AERepoElement>();
        private bool _hasDesigner = false;

        public AESchema(String path) : this(path, false)
        {
        }

        public AESchema(String path, bool hasDesigner)
        {
            _path = path;
            _hasDesigner = hasDesigner;
        }

        public void Add(AERepoElement el)
        {
            _elements.Add(el);
        }

        #region AERepository Members

        public AELinkable Lookup(string s)
        {
            LookupInfo li = Util.ParseLookup(s);
            var list = from e in _elements
                       where e.Name == li.name
                       select e;
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

        #region AEItem Members

        public string FullPath
        {
            get { return _path; }
        }

        #endregion

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            XElement result = new XElement(Namespaces.repo + "repository"
                , new XAttribute(XNamespace.Xmlns + "Repository", Namespaces.repo)
                , new XAttribute("xmlns", Namespaces.meta)
                , new XAttribute(XNamespace.Xmlns + "xsi", Namespaces.xsi)
                );
            result.Add(from e in _elements select e.ToXML(Namespaces.meta + e.LocalType));
            if (_hasDesigner)
            {
                result.Add(new XElement(Namespaces.meta + "designer"
                    , new XElement(Namespaces.meta + "lockedProperties")
                    , new XElement(Namespaces.meta + "fixedChildren")
                    , new XElement(Namespaces.meta + "resourceDescriptions")
                    )
                );
            }
            return result;
        }

        #endregion
    }
}