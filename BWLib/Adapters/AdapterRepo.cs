using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace Alternative
{
    public partial class Namespaces
    {
        public static XNamespace sample = "http://sample/adapter/schema/2011";
    }
}

namespace Alternative
{
    public interface IAdapter
    {
        XElement Config { get; }
        IEnumerable<AESession> Sessions { get; }
        IEnumerable<XElement> Consumers { get; }
        IEnumerable<XElement> Servers { get; }
    }

    public class AdapterRepository : AERepository
    {
        private IAdapter _adapter;

        public AdapterRepository(IAdapter a)
        {
            _adapter = a;
        }

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(Namespaces.repo + "repository"
                , new XAttribute(XNamespace.Xmlns + "AESDK", Namespaces.aesdk)
                , new XAttribute(XNamespace.Xmlns + "xsi", Namespaces.xsi)
                , new XAttribute(XNamespace.Xmlns + "AEService", Namespaces.aesvc)
                , new XAttribute(XNamespace.Xmlns + "Repository", Namespaces.repo)
                , _adapter.Config
                , from s in _adapter.Sessions select s.ToXML(Namespaces.adinfra + "el")
                );
        }

        #endregion

        #region AERepository Members

        public AELinkable Lookup(string s)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region AEItem Members

        public string FullPath
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }

    public class SampleAdapter : IAdapter
    {
        #region IAdapter Members

        public XElement Config
        {
            get
            {
                return new XElement(Namespaces.sample + "adapter"
                    , new XAttribute(XNamespace.Xmlns + "adsample", Namespaces.sample)
                    , new XAttribute("name", "SampleAdapter"));
            }
        }

        public IEnumerable<AESession> Sessions
        {
            get { yield return new RvSession(null, "DefaultRVSession"); }
        }

        public IEnumerable<XElement> Consumers
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<XElement> Servers
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
