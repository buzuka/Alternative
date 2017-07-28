// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code. Version 3.0.0.19191
//    <NameSpace>Alternative.infragen.Schema</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><EnableLasyLoading>False</EnableLasyLoading><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE><EnableSummaryComment>False</EnableSummaryComment><IncludeSerializeMethod>False</IncludeSerializeMethod><UseBaseClass>False</UseBaseClass><GenerateCloneMethod>False</GenerateCloneMethod><GenerateDataContracts>False</GenerateDataContracts><CodeBaseTag>Net35</CodeBaseTag><SerializeMethodName>Serialize</SerializeMethodName><DeserializeMethodName>Deserialize</DeserializeMethodName><SaveToFileMethodName>SaveToFile</SaveToFileMethodName><LoadFromFileMethodName>LoadFromFile</LoadFromFileMethodName><GenerateXMLAttributes>False</GenerateXMLAttributes><AutomaticProperties>False</AutomaticProperties><DisableDebug>False</DisableDebug><CustomUsings></CustomUsings>
//  </auto-generated>
// ------------------------------------------------------------------------------
namespace Alternative.infragen.Schema {
    using System;
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Xml.Schema;
    using System.ComponentModel;
    using System.Collections.Generic;
    
    
    public partial class infranet {
        
        private List<InstanceType> instanceField;
        
        private List<FieldAttribute> fieldsField;
        
        private List<SessionType> sessionField;
        
        private List<ServiceType> serviceField;
        
        private List<OperationType> operationField;
        
        private List<QueryType> queryField;
        
        private string nameField;
        
        private string pathField;
        
        public infranet() {
            this.queryField = new List<QueryType>();
            this.operationField = new List<OperationType>();
            this.serviceField = new List<ServiceType>();
            this.sessionField = new List<SessionType>();
            this.fieldsField = new List<FieldAttribute>();
            this.instanceField = new List<InstanceType>();
        }
        
        public List<InstanceType> instance {
            get {
                return this.instanceField;
            }
            set {
                this.instanceField = value;
            }
        }
        
        [System.Xml.Serialization.XmlArrayItemAttribute("field", typeof(FieldAttribute), IsNullable=false)]
        public List<FieldAttribute> fields {
            get {
                return this.fieldsField;
            }
            set {
                this.fieldsField = value;
            }
        }
        
        public List<SessionType> session {
            get {
                return this.sessionField;
            }
            set {
                this.sessionField = value;
            }
        }
        
        public List<ServiceType> service {
            get {
                return this.serviceField;
            }
            set {
                this.serviceField = value;
            }
        }
        
        public List<OperationType> operation {
            get {
                return this.operationField;
            }
            set {
                this.operationField = value;
            }
        }
        
        public List<QueryType> query {
            get {
                return this.queryField;
            }
            set {
                this.queryField = value;
            }
        }
        
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        public string path {
            get {
                return this.pathField;
            }
            set {
                this.pathField = value;
            }
        }
    }
    
    public partial class InstanceType {
        
        private InstanceTypeConnection connectionField;
        
        private InstanceTypeTransaction transactionField;
        
        private InstanceTypeLogging loggingField;
        
        private string stdtimeoutField;
        
        private string hawktimeoutField;
        
        public InstanceType() {
            this.loggingField = new InstanceTypeLogging();
            this.transactionField = new InstanceTypeTransaction();
            this.connectionField = new InstanceTypeConnection();
        }
        
        public InstanceTypeConnection connection {
            get {
                return this.connectionField;
            }
            set {
                this.connectionField = value;
            }
        }
        
        public InstanceTypeTransaction transaction {
            get {
                return this.transactionField;
            }
            set {
                this.transactionField = value;
            }
        }
        
        public InstanceTypeLogging logging {
            get {
                return this.loggingField;
            }
            set {
                this.loggingField = value;
            }
        }
        
        public string stdtimeout {
            get {
                return this.stdtimeoutField;
            }
            set {
                this.stdtimeoutField = value;
            }
        }
        
        public string hawktimeout {
            get {
                return this.hawktimeoutField;
            }
            set {
                this.hawktimeoutField = value;
            }
        }
    }
    
    public partial class InstanceTypeConnection {
        
        private string hostField;
        
        private string portField;
        
        private string loginField;
        
        private string passwordField;
        
        private InstanceTypeConnectionAdvanced advancedField;
        
        public InstanceTypeConnection() {
            this.advancedField = new InstanceTypeConnectionAdvanced();
        }
        
        public string host {
            get {
                return this.hostField;
            }
            set {
                this.hostField = value;
            }
        }
        
        public string port {
            get {
                return this.portField;
            }
            set {
                this.portField = value;
            }
        }
        
        public string login {
            get {
                return this.loginField;
            }
            set {
                this.loginField = value;
            }
        }
        
        public string password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
            }
        }
        
        public InstanceTypeConnectionAdvanced advanced {
            get {
                return this.advancedField;
            }
            set {
                this.advancedField = value;
            }
        }
    }
    
    public partial class InstanceTypeConnectionAdvanced {
        
        private string suspendafterField;
        
        private string retriesField;
        
        private string timerField;
        
        private string sleepField;
        
        public string suspendafter {
            get {
                return this.suspendafterField;
            }
            set {
                this.suspendafterField = value;
            }
        }
        
        public string retries {
            get {
                return this.retriesField;
            }
            set {
                this.retriesField = value;
            }
        }
        
        public string timer {
            get {
                return this.timerField;
            }
            set {
                this.timerField = value;
            }
        }
        
        public string sleep {
            get {
                return this.sleepField;
            }
            set {
                this.sleepField = value;
            }
        }
    }
    
    public partial class QueryType {
        
        private string templateField;
        
        private string classField;
        
        private System.Xml.XmlElement outField;
        
        private string nameField;
        
        private string flagsField;
        
        private string serviceField;
        
        public string template {
            get {
                return this.templateField;
            }
            set {
                this.templateField = value;
            }
        }
        
        public string @class {
            get {
                return this.classField;
            }
            set {
                this.classField = value;
            }
        }
        
        public System.Xml.XmlElement @out {
            get {
                return this.outField;
            }
            set {
                this.outField = value;
            }
        }
        
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        public string flags {
            get {
                return this.flagsField;
            }
            set {
                this.flagsField = value;
            }
        }
        
        public string service {
            get {
                return this.serviceField;
            }
            set {
                this.serviceField = value;
            }
        }
    }
    
    public partial class OperationType {
        
        private System.Xml.XmlElement inField;
        
        private System.Xml.XmlElement outField;
        
        private string nameField;
        
        private string opcodeField;
        
        private string flagsField;
        
        private string serviceField;
        
        public System.Xml.XmlElement @in {
            get {
                return this.inField;
            }
            set {
                this.inField = value;
            }
        }
        
        public System.Xml.XmlElement @out {
            get {
                return this.outField;
            }
            set {
                this.outField = value;
            }
        }
        
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        public string opcode {
            get {
                return this.opcodeField;
            }
            set {
                this.opcodeField = value;
            }
        }
        
        public string flags {
            get {
                return this.flagsField;
            }
            set {
                this.flagsField = value;
            }
        }
        
        public string service {
            get {
                return this.serviceField;
            }
            set {
                this.serviceField = value;
            }
        }
    }
    
    public partial class ServiceType {
        
        private string sessionField;
        
        private bool transactionField;
        
        private bool transactionFieldSpecified;
        
        private string subjectField;
        
        private string nameField;
        
        private bool defaultField;
        
        private bool defaultFieldSpecified;
        
        public string session {
            get {
                return this.sessionField;
            }
            set {
                this.sessionField = value;
            }
        }
        
        public bool transaction {
            get {
                return this.transactionField;
            }
            set {
                this.transactionField = value;
            }
        }
        
        public bool transactionSpecified {
            get {
                return this.transactionFieldSpecified;
            }
            set {
                this.transactionFieldSpecified = value;
            }
        }
        
        public string subject {
            get {
                return this.subjectField;
            }
            set {
                this.subjectField = value;
            }
        }
        
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        public bool @default {
            get {
                return this.defaultField;
            }
            set {
                this.defaultField = value;
            }
        }
        
        public bool defaultSpecified {
            get {
                return this.defaultFieldSpecified;
            }
            set {
                this.defaultFieldSpecified = value;
            }
        }
    }
    
    public partial class SessionType {
        
        private string daemonField;
        
        private string serviceField;
        
        private string networkField;
        
        private int dispatchersField;
        
        private bool dispatchersFieldSpecified;
        
        private string nameField;
        
        private TypeEnum typeField;
        
        private bool defaultField;
        
        private bool defaultFieldSpecified;
        
        public string daemon {
            get {
                return this.daemonField;
            }
            set {
                this.daemonField = value;
            }
        }
        
        public string service {
            get {
                return this.serviceField;
            }
            set {
                this.serviceField = value;
            }
        }
        
        public string network {
            get {
                return this.networkField;
            }
            set {
                this.networkField = value;
            }
        }
        
        public int dispatchers {
            get {
                return this.dispatchersField;
            }
            set {
                this.dispatchersField = value;
            }
        }
        
        public bool dispatchersSpecified {
            get {
                return this.dispatchersFieldSpecified;
            }
            set {
                this.dispatchersFieldSpecified = value;
            }
        }
        
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        public TypeEnum type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        public bool @default {
            get {
                return this.defaultField;
            }
            set {
                this.defaultField = value;
            }
        }
        
        public bool defaultSpecified {
            get {
                return this.defaultFieldSpecified;
            }
            set {
                this.defaultFieldSpecified = value;
            }
        }
    }
    
    public enum TypeEnum {
        
        /// <remarks/>
        rv,
        
        /// <remarks/>
        rvcm,
        
        /// <remarks/>
        rvcmq,
    }
    
    public partial class FieldAttribute {
        
        private string nameField;
        
        private int idField;
        
        private string typeField;
        
        private string valueField;
        
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        public int id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    public partial class InstanceTypeTransaction {
        
        private string timeoutField;
        
        private string maxField;
        
        public string timeout {
            get {
                return this.timeoutField;
            }
            set {
                this.timeoutField = value;
            }
        }
        
        public string max {
            get {
                return this.maxField;
            }
            set {
                this.maxField = value;
            }
        }
    }
    
    public partial class InstanceTypeLogging {
        
        private List<string> stdioField;
        
        private List<string> fileField;
        
        public InstanceTypeLogging() {
            this.fileField = new List<string>();
            this.stdioField = new List<string>();
        }
        
        [System.Xml.Serialization.XmlArrayItemAttribute("role", IsNullable=false)]
        public List<string> stdio {
            get {
                return this.stdioField;
            }
            set {
                this.stdioField = value;
            }
        }
        
        [System.Xml.Serialization.XmlArrayItemAttribute("role", IsNullable=false)]
        public List<string> file {
            get {
                return this.fileField;
            }
            set {
                this.fileField = value;
            }
        }
    }
}
