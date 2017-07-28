using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Xml.Linq;

using Alternative.Expression;

namespace Alternative
{
    public partial class Namespaces
    {
        public static XNamespace pd = "http://xmlns.tibco.com/bw/process/2003";
        public static XNamespace xsl = "http://www.w3.org/1999/XSL/Transform";
        public static XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
        public static XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
        public static XNamespace pfx = "http://KTTK/TemplateSchema";
        public static XNamespace ns = "http://www.tibco.com/xmlns/ae2xsd/2002/05/ae/siebel/CRM/businessDocument/operation";
        public static XNamespace gvars = "http://www.tibco.com/pe/DeployedVarsType";
        public static XNamespace repo = "http://www.tibco.com/xmlns/repo/types/2002";
        public static XNamespace meta = "http://www.tibco.com/xmlns/aemeta/types/2002";
        public static XNamespace aesdk = "http://www.tibco.com/xmlns/aemeta/adapter/2002";
        public static XNamespace aesvc = "http://www.tibco.com/xmlns/aemeta/services/2002";
    }

    public class NoElement : Exception
    {
    }

    public interface IXmlExportable
    {
        XElement ToXML(XName el);
    }

    public class ProcessDefinition : IXmlExportable
    {
        public XElement ToXML(XName el)
        {
            XElement res = new XElement(Namespaces.pd + el.LocalName
                , new XAttribute(XNamespace.Xmlns + "pd", Namespaces.pd)
                , new XAttribute(XNamespace.Xmlns + "xsl", Namespaces.xsl)
                , new XAttribute(XNamespace.Xmlns + "xsd", Namespaces.xsd)
                , new XAttribute(XNamespace.Xmlns + "pfx", Namespaces.pfx)
                , new XAttribute(XNamespace.Xmlns + "ns", Namespaces.ns)
                , new XAttribute(XNamespace.Xmlns + "gvars", Namespaces.gvars)
                );
            res.Add(new XElement(Namespaces.pd + "name", name)
                , new XElement(Namespaces.pd + "startName", startName)
                , new XElement(Namespaces.pd + "startX", startX)
                , new XElement(Namespaces.pd + "startY", startY)
                , new XElement(Namespaces.pd + "returnBindings", null)
                , starter.ToXML(Namespaces.pd + "starter")
                , new XElement(Namespaces.pd + "endName", endName)
                , new XElement(Namespaces.pd + "endX", endX)
                , new XElement(Namespaces.pd + "endY", endY)
                , new XElement(Namespaces.pd + "errorSchemas")
                , new XElement(Namespaces.pd + "processVariables")
                , new XElement(Namespaces.pd + "targetNamespace", "http://xmlns.example.com/1225439237719")
                );
            foreach (Activity a in activities)
            {
                res.Add(a.ToXML(Namespaces.pd + "activity"));
            }
            return res;
        }

        public string name;
        public string startName;
        public string endName = "End";
        public int startX = 0;
        public int startY = 0;
        public int endX = 882;
        public int endY = 144;

        public string returnBindings = "";

        public Activity starter;
        public Activity[] activities;
    }

    public abstract class Activity : IXmlExportable
    {
        public int x;
        public int y;
        public string name;
        public Bindings inputBindings;

        public abstract XElement ToXML(XName el);

        protected XElement[] CommonProps(string type, string resType)
        {
            return new XElement[]
            {
                  new XElement(Namespaces.pd + "type", type)
                , new XElement(Namespaces.pd + "resourceType", resType)
                , new XElement(Namespaces.pd + "x", x)
                , new XElement(Namespaces.pd + "y", y)
            };
        }
    }

    public abstract class AERPCActivity : Activity
    {
        public string transportChoice = "default";
        public string useRequestReply = "false";
        public string transportType = "rv";
        public string rvSessionService = "%%RvService%%";
        public string rvSessionNetwork = "%%RvNetwork%%";
        public string rvSessionDaemon = "%%RvDaemon%%";
        public string operation = "processEvent";

        public string outputMeta;
    }

    public class AERPCServerActivity : AERPCActivity
    {
        public AERPCServerActivity()
        {
        }

        public override XElement ToXML(XName el)
        {
            return new XElement(el
                , new XAttribute("name", name)
                , CommonProps("com.tibco.plugin.ae.AERPCServerActivity", "ae.aepalette.aeRRServer")
                , new XElement("config"
                    , new XElement("ae.aepalette.sharedProperties.transportChoice", transportChoice)
                    , new XElement("ae.aepalette.sharedProperties.useRequestReply", useRequestReply)
                    , new XElement("ae.aepalette.sharedProperties.adapterService", adapterService)
                    , new XElement("tpPluginEndpointName", endpointName)
                    , new XElement("ae.aepalette.sharedProperties.transportType", transportType)
                    , new XElement("ae.aepalette.sharedProperties.rvSubject", rvSubject)
                    , new XElement("ae.aepalette.sharedProperties.rvSessionService", rvSessionService)
                    , new XElement("ae.aepalette.sharedProperties.rvSessionNetwork", rvSessionNetwork)
                    , new XElement("ae.aepalette.sharedProperties.rvSessionDaemon", rvSessionDaemon)
                    , new XElement("ae.aepalette.aeOpClientReqActivity.ops", operation)
                    , new XElement("ae.aepalette.sharedProperties.outputMeta"
                        , new XElement("aeMeta", outputMeta))
                    )
                , new XElement(Namespaces.pd + "inputBindings")
                );
        }

        public string adapterService;
        public string endpointName;
        public string rvSubject;
    }

    public class AERPCServerReplyActivity : AERPCActivity
    {
        public AERPCServerReplyActivity(string eventName)
        {
            _eventName = eventName;
            inputBindings = new Bindings(new XElement("ProcessStarterReply"
                , new XElement(Namespaces.ns + "__caret_reply_caret_" + eventName + "_caret_processEvent"
                    , new XElement("advisoryDoc"
                        , new XElement("Version")
                        , new XElement("Data")
                    )
                )
            ));
        }

        public override XElement ToXML(XName el)
        {
            return new XElement(el
                , new XAttribute("name", name)
                , CommonProps("com.tibco.plugin.ae.AERPCServerReplyActivity", "ae.aepalette.aeOpServerReplyActivity")
                , new XElement("config"
                    , new XElement("ae.aepalette.sharedProperties.transportChoice", transportChoice)
                    , new XElement("ae.aepalette.sharedProperties.useRequestReply", useRequestReply)
                    , new XElement("ae.aepalette.aeOpServerReplyActivity.endpoint", endpoint)
                    , new XElement("ae.aepalette.aeOpClientReqActivity.ops", operation)
                    , new XElement("ae.aepalette.sharedProperties.outputMeta", outputMeta)
                    )
                , inputBindings.ToXML(Namespaces.pd + "inputBindings")
                );
        }

        public string endpoint;

        // privates
        private string _eventName;
    }

    public class MapperActivity : Activity
    {
        public override XElement ToXML(XName el)
        {
            return new XElement(el
                , new XAttribute("name", name)
                , CommonProps("com.tibco.plugin.mapper.MapperActivity", "ae.activities.MapperActivity")
                , config.ToXML("config")
                , inputBindings.ToXML(Namespaces.pd + "inputBindings")
            );
        }

        public ConfigSchema config;
    }

    public class ConfigSchema : IXmlExportable
    {
        public XElement ToXML(XName el)
        {
            return new XElement(el
                , element.ToXML("element"));
        }

        public XsdElement element;
    }

    public class XsdElement : IXmlExportable
    {
        public XElement ToXML(XName el)
        {
            return new XElement(el
                , xsd);
        }

        public XElement xsd;
    }

    public class RVRequestActivity : Activity
    {
        public override XElement ToXML(XName el)
        {
            return new XElement(el
                , new XAttribute("name", name)
                , CommonProps("com.tibco.plugin.tibrv.RVRequestActivity", "ae.rvpalette.RVRequestActivity")
                , new XElement("config"
                    , new XElement("wantsValidationForOutput", true)
                    , new XElement("wantsFiltrationForOutput", true)
                    , new XElement("wantsXMLCompliantFieldNames", true)
                    , new XElement("wantsXMLCompliantFieldNamesOutput", true)
                    , new XElement("sharedChannel", channel.ToString())
                    , input.ToXML("inputXsdString")
                    , output.ToXML("outputXsdString")
                )
                , inputBindings.ToXML(Namespaces.pd + "inputBindings")
            );
        }

        public ResourceReference channel;
        public XsdElement input;
        public XsdElement output;
    }

    public class ResourceReference
    {
        public ResourceReference(string path)
        {
            _path = path;
        }

        public override string ToString()
        {
            return _path;
        }

        private string _path;
    }

    public class Bindings : IXmlExportable
    {
        public Bindings(XElement emptyDoc)
        {
            _doc = emptyDoc;
        }

        public void Bind(IXslBinder value, string xpathTo)
        {
            XElement el = _doc.XPathSelectElement(xpathTo);
            if (el == null)
            {
                throw new NoElement();
            }
            value.Bind(el);
        }

        public void Copy(Activity src, string xpathFrom, string xpathTo)
        {
            string str = "$" + src.name + xpathFrom;
            XElement el = _doc.XPathSelectElement(xpathTo);
            el.Add(
                new XElement(
                    Namespaces.xsl + "copy-of"
                    , new XAttribute("select", str + "/ancestor-or-self::*/namespace::node()")
                )
                , new XElement(
                    Namespaces.xsl + "copy-of"
                    , new XAttribute("select", str + "/@*")
                )
                , new XElement(
                    Namespaces.xsl + "copy-of"
                    , new XAttribute("select", str + "/node()")
                )
            );
        }

        public XElement ToXML(XName name)
        {
            return new XElement(name, _doc);
        }

        private XElement _doc;
    }
}
