using System;
using System.Collections.Generic;
using System.Linq;

namespace Alternative
{
    public interface AEItem : IXmlExportable
    {
        String FullPath { get; }
    }

    /// <summary>
    /// Adapter schema file
    /// </summary>
    public interface AERepository : AEItem
    {
        AELinkable Lookup(String s);
    }

    /// <summary>
    /// Represents any linkable entity
    /// </summary>
    public interface AELinkable
    {
        AERepository Schema { get; }
        String Name { get; }
        String LocalType { get; }
    }

    /// <summary>
    /// Typedef
    /// </summary>
    public interface ExtendedProperties : IXmlExportable
    {
    }

    /// <summary>
    /// Generates several files.
    /// </summary>
    public interface AdapterConfig
    {
        /// <summary>
        /// Adapter family name:
        /// AESchemas/ae/Family
        /// </summary>
        String Family { get; }

        /// <summary>
        /// Flat list of adapter configuration items
        /// </summary>
        IEnumerable<AEItem> AllItems { get; }
    }
}

