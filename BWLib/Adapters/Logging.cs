using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Alternative
{

    public abstract class LogSink : IXmlExportable
    {
        private List<String> _roles = new List<string>();

        public void AddRole(String role)
        {
            _roles.Add(role);
        }

        public void AddRoles(IEnumerable<String> roles)
        {
            _roles.AddRange(roles);
        }

        #region Abstracts

        public abstract XName Name { get; }
        internal abstract XElement[] GetSpecific();

        #endregion

        #region IXmlExportable Members

        public XElement ToXML(XName el)
        {
            return new XElement(el
                , GetSpecific()
                , from r in _roles
                  select new XElement(Namespaces.aesdk + "role"
                        , new XElement(Namespaces.aesdk + "name", r)
                        )
                );
        }

        #endregion
    }

    public class StdioLogSink : LogSink
    {
        private const string NAME = "stdioSink";

        private string _ioName = "stdout";

        public StdioLogSink()
        {
        }

        public StdioLogSink(String ioName)
        {
            _ioName = ioName;
        }

        public override XName Name
        {
            get { return Namespaces.aesdk + NAME; }
        }

        internal override XElement[] GetSpecific()
        {
            return new XElement[] { new XElement(Namespaces.aesdk + "ioName", _ioName)
                , new XElement(Namespaces.aesdk + "name", NAME)
            };
        }
    }

    public class FileLogSink : LogSink
    {
        private const string NAME = "fileSink";

        private string _fileName = "%%DirTrace%%/%%Deployment%%.%%InstanceId%%.log";
        private string _fileCount = "5";
        private string _fileLimit = "20000000";
        private string _append = "false";

        public FileLogSink()
        {
        }

        public FileLogSink(String fileName, String fileCount, String fileLimit, String append)
        {
            _fileName = fileName;
            _fileCount = fileCount;
            _fileLimit = fileLimit;
            _append = append;
        }

        public override XName Name
        {
            get { return Namespaces.aesdk + NAME; }
        }

        internal override XElement[] GetSpecific()
        {
            return new XElement[] {
                  new XElement(Namespaces.aesdk + "fileName", _fileName)
                , new XElement(Namespaces.aesdk + "fileCount", _fileCount)
                , new XElement(Namespaces.aesdk + "fileLimit", _fileLimit)
                , new XElement(Namespaces.aesdk + "appendMode", _append)
                , new XElement(Namespaces.aesdk + "name", NAME)
            };
        }
    }

}