using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Alternative.Infranet
{
    public class InfranetStaticSchema : AESchema
    {
        #region Static

        private static InfranetStaticSchema _self = null;

        public static InfranetStaticSchema Instance()
        {
            if (_self == null)
            {
                _self = new InfranetStaticSchema();
            }
            return _self;
        }

        #endregion

        private InfranetStaticSchema()
            : base("/AESchemas/ae/Infranet/InfranetStaticSchema.aeschema")
        {
            InitDefaultObjects();
        }

        private void InitDefaultObjects()
        {
            // sequences
            Add(new AESequence(this, "sequence[ui4]") { ElementType = StaticSchemas.AE.Lookup("ui4") });
            Add(new AESequence(this, "sequence[any]") { ElementType = StaticSchemas.AE.Lookup("any") });

            // classes
            Add(_private.pin_array.Instance(this));
            Add(_private.pin_command_exec_opcode.Instance(this));
            Add(_private.pin_command_exec_search.Instance(this));
        }
    }

    #region Default entities

    namespace _private
    {
        class pin_array
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "pin_array");
                    _self.Attributes.Add(new AEAttribute("value") { AEType = schema.Lookup("sequence[any]") });
                    _self.Attributes.Add(new AEAttribute("index") { AEType = schema.Lookup("sequence[ui4]") });
                }
                return _self;
            }
        }

        class pin_command_exec_opcode
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "pin_command_exec_opcode");
                    _self.SuperClass = GenericStaticSchema.Instance().Lookup("gen_command");
                }
                return _self;
            }
        }

        class pin_command_exec_search
        {
            private static AEClass _self = null;
            public static AEClass Instance(AERepository schema)
            {
                if (_self == null)
                {
                    _self = new AEClass(schema, "pin_command_exec_search");
                    _self.SuperClass = GenericStaticSchema.Instance().Lookup("gen_command");
                }
                return _self;
            }
        }
    }

    #endregion
}
