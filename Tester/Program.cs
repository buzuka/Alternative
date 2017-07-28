using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Alternative;
using Alternative.Expression;
using Alternative.Infranet;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test1();
            //Test2();
            //Test3();
            //TestAdapter();
            //TestSinks();
            //TestSchema();
            //TestRpc();
            //Decode();
            TestSiebel();
        }

        private static void TestSinks()
        {
            StdioLogSink s = new StdioLogSink();
            s.AddRole("infoRole");
            s.AddRole("warnRole");
            s.AddRole("errorRole");

            FileLogSink f = new FileLogSink();
            f.AddRole("infoRole");
            f.AddRole("warnRole");
            f.AddRole("errorRole");
            f.AddRole("debugRole");

            System.Console.WriteLine(s.ToXML(s.Name).ToString());
            System.Console.WriteLine(f.ToXML(f.Name).ToString());
        }

        private static void Test1()
        {
            AERPCServerActivity starter = new AERPCServerActivity
                {
                    name = "Adapter Request-Response Server",
                    adapterService = "/CRM.adsbl#adapterService.TemplateRRIService",
                    endpointName = "TemplateRRIServiceEndpoint",
                    rvSubject = "%%Domain%%.%%Deployment%%.adsbl.CRM.TemplateRRIService",
                    outputMeta = "/AESchemas/ae/siebel/CRM/businessDocument/operation.aeschema#rpcClass.TemplateRRIServiceEvent",
                    x = 80,
                    y = 76,
                };

            AERPCServerReplyActivity respond = new AERPCServerReplyActivity("TemplateRRIServiceEvent")
                {
                    name = "Respond to Adapter Request",
                    endpoint = "Adapter Request-Response Server",
                    outputMeta = "/AESchemas/ae/siebel/CRM/businessDocument/operation.aeschema#rpcClass.TemplateRRIServiceEvent",
                    x = 734,
                    y = 68,
                };
            respond.inputBindings.Bind(new ValueOfBinder(new IntegerExpression(3)), "/*/advisoryDoc/Version");

            MapperActivity id = new MapperActivity
                {
                    name = "Id",
                    config = new ConfigSchema
                    {
                        element = new XsdElement
                        {
                            xsd = new XElement(Namespaces.xsd + "element"
                                , new XElement(Namespaces.xsd + "complexType"
                                    , new XElement(Namespaces.xsd + "sequence"
                                        , new XElement(Namespaces.xsd + "element"
                                            , new XAttribute("name", "siebel-id")
                                            , new XAttribute("type", "xsd:string")
                                        )
                                    )
                                )
                            ),
                        },
                    },
                    inputBindings = new Bindings(null),
                    x = 246,
                    y = 60,
                };

            RVRequestActivity sendRR = new RVRequestActivity
                {
                    name = "Send Rendezvous Request",
                    channel = new ResourceReference("/Rendezvous Transport.rvtransport"),
                    inputBindings = new Bindings(
                        new XElement("ActivityInput"
                            , new XElement("subject")
                            , new XElement("timeout")
                            , new XElement("body"
                                , new XElement(Namespaces.pfx + "request"
                                    , new XElement(Namespaces.pfx + "id")
                                )
                            )
                        )
                    ),
                    input = new XsdElement
                    {
                        xsd = new XElement(Namespaces.xsd + "element"
                            , new XElement(Namespaces.xsd + "complexType"
                                , new XElement(Namespaces.xsd + "sequence"
                                    , new XElement(Namespaces.xsd + "element"
                                        , new XAttribute("ref", Namespaces.pfx + "request")
                                    )
                                )
                            )
                        ),
                    },
                    output = new XsdElement
                    {
                        xsd = new XElement(Namespaces.xsd + "element"
                            , new XElement(Namespaces.xsd + "complexType"
                                , new XElement(Namespaces.xsd + "sequence"
                                    , new XElement(Namespaces.xsd + "element"
                                        , new XAttribute("ref", Namespaces.pfx + "response")
                                    )
                                )
                            )
                        ),
                    },
                    x = 419,
                    y = 67,
                };
            sendRR.inputBindings.Bind(new ValueOfBinder(new ConcatExpression(new StringExpression("TemplateSubject"), new LinkExpression("$_globalVariables/ns1:GlobalVariables/Application/Suffix"))),
                "subject");
            sendRR.inputBindings.Bind(new ValueOfBinder(new LinkExpression("$_globalVariables/ns1:GlobalVariables/Application/Timeout")),
                "timeout");

            ProcessDefinition pd = new ProcessDefinition
            {
                name = "Starters/TemplateProcess.process",
                startName = "Adapter Request-Response Server",
                starter = starter,
                activities = new Activity[] { respond, id, sendRR }
            };
            Serialize(pd);
        }

        private static void Serialize(ProcessDefinition pd)
        {
            XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", ""));
            xd.Add(pd.ToXML("ProcessDefinition"));
            xd.Save(Console.Out);
        }

        private static void Test2()
        {
            DefaultSettingsFactory settings = new DefaultSettingsFactory("SLTemplate");

            XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", ""));
            xd.Add(settings.GetGlobalVars().AsXML());
            xd.Save(Console.Out);
        }

        private static void Test3()
        {
            ProjectDefinition prj = new ProjectDefinition("SLTemplate");
            XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", ""));
            xd.Add(prj.RootFolder.AsXML());
            xd.Save(Console.Out);
        }

        private static void TestAdapter()
        {
            InfranetAdapterConfig iac = new InfranetAdapterConfig();
            InfranetAdapterInstance ins = iac.AddInstance("Billing");
            ins.SetTransactionInfo(30000, 300);
            ins.SetStdMicroAgentTimeout("%%MicroAgentTimeout%%");
            ins.SetClassMicroAgentTimeout("%%MicroAgentTimeout%%");
            InfranetServerService ss = ins.AddServerService("Search");
            ss.InTransaction = true;

            XElement inSchema = new XElement("root"
                , new XElement("POID")
                , new XElement("FLAGS")
                , new XElement("TEMPLATE")
                , new XElement("ARGS"
                    , new XElement("ACCOUNT_OBJ")
                    , new XElement("POID")
                    )
                , new XElement("RESULTS"
                    , new XElement("SERVICE_INFO"
                        , new XElement("SERVICE_OBJ")
                        )
                    )
                );
            XElement outSchema = new XElement("root"
                , new XElement("POID")
                , new XElement("RESULTS"
                    , new XElement("POID")
                    )
                );
            InfranetOperation op = ss.AddOperation("Opcode", "PCM_OP_GLOBAL_SEARCH", 0, inSchema, outSchema);
            InfranetOperation q = ss.AddQuery("Query", "select X from /account where F1 = V1 ", 768, "/account", outSchema);

            //AdapterConfigExporter ex = new AdapterConfigExporter("c:\\projects\\sample");
            AdapterConfigExporter ex = new AdapterConfigExporter("c:\\temp\\11");
            ex.Export(iac);
        }

        private static void TestSchema()
        {
            AESchema s = new AESchema("/AESchemas/ae/Infranet/CommandSchema.aeschema");
            s.Add(new AEClass(s, "DummyClass"));

            AEClass Read_output_results = InfranetFactory.CreateClass(s, "Read^output^PIN_FLD_RESULTS", InfranetFunctions.query);
            Read_output_results.Attributes.Add(InfranetFactory.CreateAttribute(DefaultFields.AAC_ACCESS));
            AEClass Read_output = InfranetFactory.CreateClass(s, "Read^output", InfranetFunctions.query);
            Read_output.Attributes.Add(InfranetFactory.CreateAttribute(DefaultFields.RESULTS, Read_output_results, false, "false"));

            s.Add(Read_output);
            s.Add(Read_output_results);

            XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", ""));
            xd.Add(s.ToXML("repository"));
            xd.Save(Console.Out);
        }

        private static void TestRpc()
        {
            AESchema s = new AESchema("/AESchemas/ae/Infranet/Billing/Search.aeschema");
            AERPCClass cls = new AERPCClass(s, "RequestResponseServiceOperationSchema");
            AERPCOperation op = new AERPCOperation("GetParent")
            {
                ReturnType = new ForwardLink(s, "GetParent^PCM_OP_BILL_GROUP_GET_PARENT^output", "class")
            };
            op.Parameters.Add(new AERPCOperationParameter("input")
            {
                Direction = ParameterDirection.IN
                ,
                Type = new ForwardLink(s, "GetParent^PCM_OP_BILL_GROUP_GET_PARENT^input", "class")
            });
            cls.AddOperation(op);
            s.Add(cls);

            AEClass Read_output_results = InfranetFactory.CreateClass(s, "Read^output^PIN_FLD_RESULTS", InfranetFunctions.query);
            Read_output_results.Attributes.Add(InfranetFactory.CreateAttribute(DefaultFields.AAC_ACCESS));
            AEClass Read_output = InfranetFactory.CreateClass(s, "Read^output", InfranetFunctions.query);
            Read_output.Attributes.Add(InfranetFactory.CreateAttribute(DefaultFields.RESULTS, Read_output_results, false, "false"));

            s.Add(Read_output);
            s.Add(Read_output_results);

            XDocument xd = new XDocument(new XDeclaration("1.0", "utf-8", ""));
            xd.Add(s.ToXML("repository"));
            xd.Save(Console.Out);
        }

        private static void Decode()
        {
            System.IO.StreamReader inFile;
            string base64String;

            try
            {
                char[] base64CharArray;
                inFile = new System.IO.StreamReader("c:\\temp\\14\\base64copy.txt",
                                                System.Text.Encoding.ASCII);
                base64CharArray = new char[inFile.BaseStream.Length];
                inFile.Read(base64CharArray, 0, (int)inFile.BaseStream.Length);
                base64String = new string(base64CharArray);
            }
            catch (System.Exception exp)
            {
                // Error creating stream or reading from it.
                System.Console.WriteLine("{0}", exp.Message);
                return;
            }

            // Convert the Base64 UUEncoded input into binary output.
            byte[] binaryData;
            try
            {
                binaryData =
                    System.Convert.FromBase64String(base64String);
            }
            catch (System.ArgumentNullException)
            {
                System.Console.WriteLine("Base 64 string is null.");
                return;
            }
            catch (System.FormatException)
            {
                System.Console.WriteLine("Base 64 string length is not " +
                    "4 or is not an even multiple of 4.");
                return;
            }

            // Write out the decoded data.
            System.IO.FileStream outFile;
            try
            {
                outFile = new System.IO.FileStream("c:\\temp\\14\\copy.zip",
                                                   System.IO.FileMode.Create,
                                                   System.IO.FileAccess.Write);
                outFile.Write(binaryData, 0, binaryData.Length);
                outFile.Close();
            }
            catch (System.Exception exp)
            {
                // Error creating stream or writing to it.
                System.Console.WriteLine("{0}", exp.Message);
            }
        }

        private static void TestSiebel()
        {
            AdapterRepository repo = new AdapterRepository(new SampleAdapter());

            Console.WriteLine(repo.ToXML("dummy").ToString());
        }
    }

}
