using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Alternative.Infranet
{
    public class GenericStaticSchema : AESchema
    {
        #region Static

        private static GenericStaticSchema _self = null;

        public static GenericStaticSchema Instance()
        {
            if (_self == null)
            {
                _self = new GenericStaticSchema();
            }
            return _self;
        }

        #endregion

        private GenericStaticSchema()
            : base("/AESchemas/ae/Infranet/GenericStaticSchema.aeschema")
        {
            InitDefaultObjects();
        }

        private void InitDefaultObjects()
        {
            Add(_private.gen_reply_base.Instance(this));
            Add(_private.gen_basework.Instance(this));
            Add(_private.gen_command.Instance(this));
            Add(_private.transaction_Input.Instance(this));
            Add(_private.transaction_Output.Instance(this));
            Add(_private.RequestInputGenericSearch.Instance(this));
        }
    }

    #region Default entities

    namespace _private
    {
        class gen_reply_base
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "gen_reply_base");
                    _self.Attributes.Add(new AEAttribute("hasException") { Default = "false", AEType = StaticSchemas.AE.Lookup("boolean") });
                    _self.Attributes.Add(new AEAttribute("exceptionName") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("exceptionValue") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("exceptionCategory") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("exceptionNumber") { AEType = StaticSchemas.AE.Lookup("string") });
                }
                return _self;
            }
        }

        class gen_basework
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "gen_basework");
                    _self.Attributes.Add(new AEAttribute("name") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("inputClassName") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("outputClassName") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("inputMapName") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("outputMapName") { AEType = StaticSchemas.AE.Lookup("string") });
                }
                return _self;
            }
        }

        class gen_command
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "gen_command");
                    _self.SuperClass = schema.Lookup("gen_basework");
                }
                return _self;
            }
        }

        class transaction_Input
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "transaction_Input");
                    _self.Attributes.Add(new AEAttribute("Connection_Id") { AEType = StaticSchemas.AE.Lookup("string"), Key = "true" });
                    _self.Attributes.Add(new AEAttribute("Run_In_Transaction") { Default = "TRUE", AEType = StaticSchemas.AE.Lookup("boolean") });
                    _self.Attributes.Add(new AEAttribute("Transaction_Type") { AEType = StaticSchemas.AE.Lookup("string") });
                }
                return _self;
            }
        }

        class transaction_Output
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "transaction_Output");
                    _self.Attributes.Add(new AEAttribute("Connection_Id") { AEType = StaticSchemas.AE.Lookup("string"), Key = "true" });
                }
                return _self;
            }
        }

        class RequestInputGenericSearch
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "RequestInputGenericSearch");
                    _self.Attributes.Add(new AEAttribute("paraName1") { AEType = StaticSchemas.AE.Lookup("string") });
                    _self.Attributes.Add(new AEAttribute("paraValue1") { AEType = StaticSchemas.AE.Lookup("any") });
                    _self.Attributes.Add(new AEAttribute("paraName2") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue2") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName3") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue3") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName4") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue4") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName5") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue5") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName6") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue6") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName7") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue7") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName8") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue8") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName9") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue9") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraName10") { AEType = StaticSchemas.AE.Lookup("string"), Key = "false" });
                    _self.Attributes.Add(new AEAttribute("paraValue10") { AEType = StaticSchemas.AE.Lookup("any"), Key = "false" });
                }
                return _self;
            }
        }
    }

    #endregion
}
