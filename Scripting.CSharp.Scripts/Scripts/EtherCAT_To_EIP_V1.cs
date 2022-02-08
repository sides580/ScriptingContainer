﻿using System.IO;
using System.Xml;
using EnvDTE;
using EnvDTE100;
using EnvDTE80;
using TCatSysManagerLib;
using TwinCAT.SystemManager;
using System;
using ScriptingTest;
using System.Collections.Generic;

namespace Scripting.CSharp
{
    /// <summary>
    /// Demonstrates the Copying of an EtherCAT IO tree and the linking to Ethernet IP (Early Binding), Alternative Box adding via VInfo Structe
    /// </summary>
    public class EtherCAT_To_EIP_V1
        : ScriptEarlyBound
    {

        /// <summary>
        /// System Manager object
        /// </summary>
        private ITcSysManager4 systemManager = null;

        /// <summary>
        /// TwinCAT XAE Project ojbect
        /// </summary>
        private Project project = null;

        /// <summary>
        /// Handler function Initializing the Script (Configuration preparations)
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>Usually used to to the open a prepared or new XAE configuration</remarks>
        protected override void OnInitialize(IContext context)
        {
            base.OnInitialize(context);
        }

        /// <summary>
        /// Handler function called after the Solution object has been created.
        /// </summary>
        protected override void OnSolutionCreated()
        {
            this.project = (Project)CreateNewProject();
            this.systemManager = (ITcSysManager4)project.Object;
            base.OnSolutionCreated();
        }
        /// <summary>
        /// Handler function called after the Solution object has been Opened.
        /// </summary>
        protected override void OnSolutionOpened()
        {
            if (GlobalVariables.OpenExisitngVSProject == true)
            {

                dte = (DTE2)GlobalVariables.ConnectedDTE.DTE;// ROTAccess.GetActiveDTE(this.ScriptName); // Getting DTE of the currently Script-Opened project


                this.project = dte.Solution.Projects.Item(1);
                this.systemManager = (ITcSysManager4)project.Object;
                // this.solution  GlobalVariables.ConnectedDTE.Solution;
            }
            else
            {

                this.project = dte.Solution.Projects.Item(1);
                this.systemManager = (ITcSysManager4)project.Object;
                //this.project = (Project)CreateNewProject();
                //this.systemManager = (ITcSysManager4)project.Object;
            }
            base.OnSolutionOpened();
        }

        /// <summary>
        /// Cleaning up the XAE configuration after script execution.
        /// </summary>
        /// <param name="worker">The worker.</param>
        protected override void OnCleanUp(IWorker worker)
        {
            base.OnCleanUp(worker);
        }

        /// <summary>
        /// Name of the PLC Template
        /// </summary>
        private string plcTemplateName = "PlcFile.tpy";

        /// <summary>
        /// Name of the Project Template
        /// </summary>
        private string xaeProjectTemplateName = "DemoProject.tsp";

        /// <summary>
        /// Gets the Path to the PLC Template
        /// </summary>
        /// <value>The PLC file.</value>
        private string PlcTemplatePath
        {
            get { return Path.Combine(ApplicationDirectory, plcTemplateName); }
        }

        /// <summary>
        /// Gets the Path to the Project Template
        /// </summary>
        /// <value>The TSM file.</value>
        private string XAEProjectTemplatePath
        {
            get { return Path.Combine(ApplicationDirectory, xaeProjectTemplateName); }
        }


        /// <summary>
        /// Handler function Executing the Script code.
        /// </summary>
        /// <param name="worker">The worker.</param>
        protected override void OnExecute(IWorker worker)
        {
            ScriptingTest.CSV_Reader.ReadConfigCSV();
            worker.Progress = 0;

            //IO configuration
            //search for IO Devices
            string EtherCATMasterName = "EtherCAT Master";
            ITcSmTreeItem devices = systemManager.LookupTreeItem("TIID");
            ITcSmTreeItem device;
            bool HasEtherCATNetwork = true;
            device = FindDevice(worker, devices, "EtherCAT Master");
            if (device == null)
                return;
            if (!HasEtherCATNetwork)
            {
                worker.Progress = 10;
                worker.ProgressStatus = "Creating EtherCATnetwork";
                device = CreateEtherCATNetwork(devices, EtherCATMasterName);
            }


            ImportPOUFromList(devices, CSV_Reader.ImportPOU_List, worker);
            worker.ProgressStatus = "Finished Importing PLC code";
            worker.Progress = 30;




            List<IO_Object> IO_List = new List<IO_Object>();
            GetIOList(worker, device, IO_List);

            //create EIP Master
            worker.Progress = 60;
            worker.ProgressStatus = "Finished Scanning EtherCAT network";


            //ITcSmTreeItem eipSlave;// = devices.CreateChild(DeviceEIPSlaveName, 145);
            //AddNewEIPDevice(devices);
            string EL6652Path = GetSingleIoObjectPath(worker, device, "EL6652-0010");
            ImportEIPDevice(worker, devices, IO_List, EL6652Path);
            worker.Progress = 80;
            worker.ProgressStatus = "Finished building Ethernet IP network";


            AddManualLinks(worker);
            worker.ProgressStatus = "Finished extra linking";
            worker.Progress = 100;
            //eipSlave.Child[0].Name = "LetsGoBrandon";

            //ITcSmTreeItem item; //systemManager.LookupTreeItem("TIID^EtherCAT Master^A2P (EK1100)^111 (EL6652-0010)");


            //set the EL6751 Device ID as address in CANopen Master Device
            /*
            worker.Progress = 40; 
            worker.ProgressStatus = "Creating CANopen Master (EL6751)";

            item = systemManager.LookupTreeItem("TIID^CANopen Master (EL6751)");
            item.ConsumeXml("<TreeItem><DeviceDef><AddressInfo><Ecat><EtherCATDeviceId>" + El6751DevId.ToString() + "</EtherCATDeviceId></Ecat></AddressInfo></DeviceDef></TreeItem>");

            //create CANopen Slaves
            //search for CANopen Master
            item = systemManager.LookupTreeItem("TIID^CANopen Master (EL6751)");

            //create BK5150
            item.CreateChild("Box11 (BK5150)", 5008);
            //set node address
            item = systemManager.LookupTreeItem("TIID^CANopen Master (EL6751)^Box11 (BK5150)");
            item.ConsumeXml("<TreeItem><BoxDef><FieldbusAddress>11</FieldbusAddress></BoxDef></TreeItem>");
            //create terminals
            item.CreateChild("Term2 (KL1002)", 1002, "End Term (KL9010)");
            item.CreateChild("Term3 (KL2114)", 2114, "End Term (KL9010)");
            */



            ////PLC configuration
            ////search for PLC project
            //item = systemManager.LookupTreeItem("TIPC^PlcFile");
            ////rescan plc project
            //item.ConsumeXml("<TreeItem><PlcDef><ProjectPath>" + this.PlcTemplatePath + "</ProjectPath><ReScan>1</ReScan></PlcDef></TreeItem>");

            //string plcAxisTemplatePath = Path.Combine(ConfigurationTemplatesFolder, "PlcAxisTemplate.tpzip");


            //worker.ProgressStatus = "Adding PLC project ...";

            //ITcSmTreeItem plcConfig = systemManager.LookupTreeItem("TIPC");
            //ITcSmTreeItem plc = plcConfig.CreateChild("PlcProject", 0, "", plcAxisTemplatePath);

            //ITcSmTreeItem plcProject = systemManager.LookupTreeItem("TIPC^PlcProject");
            // ITcSmTreeItem plcInstances = systemManager.LookupTreeItem("TIPC^PlcProject^PlcProject Project");
        }
        private string GetProjectPathForLinking(IWorker worker)
        {
            try
            {
                ITcSmTreeItem PlcDevice = systemManager.LookupTreeItem("TIPC");
                ITcSmTreeItem plcProject = PlcDevice.Child[1];
                ITcProjectRoot projectRoot = (ITcProjectRoot)plcProject;
                ITcSmTreeItem nestedProject = projectRoot.NestedProject;
                ITcSmTreeItem projectInstance = plcProject.get_Child(1);
                return projectInstance.PathName;
            }
            catch (Exception ex)
            {
                worker.ProgressStatus = "Get PLC Project Path Failed: " + ex.Message;
            }
            return "";
        }
        private bool FindNestedPlcFolderObject(ITcSmTreeItem nestedProject, string objectName)
        {
            for (int x = 1; x < nestedProject.ChildCount; x++)
            {
                ITcSmTreeItem Child = nestedProject.Child[x];
                if (nestedProject.Child[x].Name == objectName)
                {
                    return true;
                }
                else
                {
                    if (Child.ChildCount > 0)
                    { 
                        bool IsNested = FindNestedPlcFolderObject(Child, objectName);
                        if (IsNested)
                            return true;
                    }
                }
            }
            return false;
        }

        private void ImportPOUFromList(ITcSmTreeItem devices, List<string> List, IWorker worker)
        {
            try
            {
                ITcSmTreeItem PlcDevice = systemManager.LookupTreeItem("TIPC");
                ITcSmTreeItem plcProject = PlcDevice.Child[1];
                ITcProjectRoot projectRoot = (ITcProjectRoot)plcProject;
                ITcSmTreeItem nestedProject = projectRoot.NestedProject;
     
                ITcSmTreeItem projectInstance = plcProject.get_Child(1);
                ITcPlcIECProject importExport = (ITcPlcIECProject)nestedProject;
                //string Directory = System.IO.Directory.GetCurrentDirectory();
                foreach (string import in List)
                {
                    string[] importSkimmed =import.Split('\\');
                    string importSkimmedFinal =  importSkimmed[importSkimmed.GetUpperBound(0)].Split('.')[0];
                    if(!FindNestedPlcFolderObject(nestedProject, importSkimmedFinal))
                        importExport.PlcOpenImport(import, (int)PLCIMPORTOPTIONS.PLCIMPORTOPTIONS_SKIP, "", true);
                }
                dte.Solution.SolutionBuild.Build(true);
            }
            catch(Exception ex)
            {
                worker.ProgressStatus = "Import POU and build project failed: " + ex.Message;
            }
        }

            private void AddNewEIPDevice(ITcSmTreeItem devices)
        {
            //int El6652DevId;
            ITcSmTreeItem eipSlave;
            string DeviceEIPSlaveName = "EtherNet/IP Adapter (EL6652-0010)";
            eipSlave = devices.CreateChild(DeviceEIPSlaveName, 145);

            ITcSmTreeItem item;


            item = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName);

            string EIPSlaveSubName = "EIP_Slave1";
            if (item.ChildCount > 0 && item.Child[1].Name != EIPSlaveSubName)
                item.Child[1].Name = EIPSlaveSubName;

            //item.DeleteChild(item.Child[1].Name);

            if (item.ChildCount == 0)
                item.CreateChild(EIPSlaveSubName, 9133);

            //item = systemManager.LookupTreeItem("TIID^"+ DeviceEIPSlaveName+"^"+ EIPSlaveSubName+"^Inputs");
            item = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName + "^" + EIPSlaveSubName);
            item.CreateChild("Assembly 1 (Input/Output)", (int)TreeItemType.EipConnection, EIPSlaveSubName);//NOte subtype 1073741867 name: IO Assembly
        }
        private string AddLinkPathByType(IWorker worker, string link, string type)
        {
            ITcSmTreeItem devices = systemManager.LookupTreeItem("TIID");
            string PlcInstancePathName = GetProjectPathForLinking(worker);
            string EtherCATInstancePathName = FindDevice(worker, devices, "EtherCAT Master").PathName;
            if (type.ToLower() == "plclinkinput")
            {
                return PlcInstancePathName + "^PlcTask Inputs^" + link;
            }
            if (type.ToLower() == "plclinkoutput")
            {
                return PlcInstancePathName + "^PlcTask Outputs^" + link;
            }
            if (type.ToLower() == "ethercatlink")
            {
                return EtherCATInstancePathName + "^" + link;
            }
            return link;
        }
        private void AddManualLinks(IWorker worker)
        {

            foreach (ManualLinks Link in CSV_Reader.Links)
            {
                string Link1 = "";
                string Link2 = "";
                try
                {
                    //ITcSmTreeItem obLink1 = systemManager.LookupTreeItem(Link.Link1);
                    //XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.LoadXml(obLink1.ProduceXml(false));
                    //if(xmlDoc.)
                    Link1 = AddLinkPathByType(worker, Link.Link1, Link.Type1);
                    Link2 = AddLinkPathByType(worker, Link.Link2, Link.Type2);
                    systemManager.LinkVariables(Link1, Link2);
    
                }
                catch(Exception ex)
                {
                    worker.ProgressStatus = "Link Failed: " + Link1 + " To " + Link2;
                    worker.ProgressStatus = ex.Message;
                }
            }
            
        }
        private void ImportEIPDevice(IWorker worker, ITcSmTreeItem devices, List<IO_Object> IoList, string EL6652Path)
        {
            //Get the list of links that are extra to be linked to the
            string PlcInstancePathName = GetProjectPathForLinking(worker);
            for (int x = 0; x < CSV_Reader.EipExtraTags.Count; x++)
            {
                IO_Object NewItem = new IO_Object();
                NewItem.IsOutput = !CSV_Reader.EipExtraTags[x].IsInput;
                NewItem.IsInput = CSV_Reader.EipExtraTags[x].IsInput;
                NewItem.PathName = PlcInstancePathName + CSV_Reader.EipExtraTags[x].LinkPath;
                NewItem.TagName = CSV_Reader.EipExtraTags[x].EIPTagName;
                NewItem.bitsize = GetSizeFromString(CSV_Reader.EipExtraTags[x].DataType);// Convert.ToInt32(EtherCatTerminalNodeSubitems.SelectSingleNode("BitLen").InnerText);
                NewItem.type = GetTypeFromString(CSV_Reader.EipExtraTags[x].DataType);
                NewItem.typedetails = "IsManualAdd";
                IoList.Add(NewItem);
            }

            ITcSmTreeItem eipSlave = null;

            string DeviceEIPSlaveName = "EtherNet/IP Adapter (EL6652-0010)";
            string EIPSlaveSubName = "EIP_Slave1";
            string IOAssemblyName;
            ITcSmTreeItem item;
            ITcSmTreeItem Inputs;
            ITcSmTreeItem Outputs;

            foreach (ITcSmTreeItem device in devices)
            {
                if (device.ItemSubType == 145 || device.ItemSubType == 139)
                {
                    if (ScriptingTest.CSV_Reader.EthernetIPTemplate == null)
                        eipSlave = device;
                    else
                        devices.DeleteChild(device.Name);
                    break;
                }
            }
           
            if(eipSlave == null)
                eipSlave = devices.ImportChild(ScriptingTest.CSV_Reader.EthernetIPTemplate, null, false);
            DeviceEIPSlaveName = eipSlave.Name;


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(eipSlave.ProduceXml(false));


            ITcSmTreeItem eipSlaveBox = eipSlave.Child[1];//Grab first child It's the Box 1 (TC EtherNet/IP Slave




            EIPSlaveSubName = eipSlaveBox.Name;
            item = eipSlaveBox.Child[1]; //Grab IO Assembly
           

            IOAssemblyName = item.Name;
            Inputs = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName + "^" + EIPSlaveSubName + "^" + IOAssemblyName + "^Inputs");
            Outputs = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName + "^" + EIPSlaveSubName + "^" + IOAssemblyName + "^Outputs");


         
            //Remove everything first
            for (int i = Inputs.ChildCount; 1 < i; i--)
            {
                Inputs.DeleteChild(Inputs.Child[i].Name);
            }
            for (int i = Outputs.ChildCount; i > 1; i--)
            {
                Outputs.DeleteChild(Outputs.Child[i].Name);
            }


            int bitTracker = 0;
            int bufferCounter = 0;
            int bufferInputCount = 0;
            int bufferOutputCount = 0;
            for (int x = 0; x < 3; x++)
            {
                for (int i = 0; i < IoList.Count; i++)
                {
                    if (x == 0 && IoList[i].bitsize < 16)
                        continue;
                    else if (x == 1 && (IoList[i].bitsize == 1 || IoList[i].bitsize >= 16))
                        continue;
                    else if (x == 2 && IoList[i].bitsize != 1 )
                        continue;


                    string tagName;
                    if (IoList[i].TagParentName == "")
                    {
                        tagName = IoList[i].TagName;
                    }
                    else
                    {
                        tagName = IoList[i].TagParentName + "_" + IoList[i].TagName;
                        tagName = tagName.Replace(" ", "_");
                    }
                    if (IoList[i].bitsize > 1)
                    {
                        if (IoList[i].IsInput)
                        {
                            bitTracker = bufferInputCount;
                        }
                        else
                            bitTracker = bufferOutputCount;

                        if (bitTracker % 16 != 0) //Checks if this is divisable by 16
                        {
                            while (bitTracker % 16 != 0)
                            {
                                bitTracker++;
                                bufferCounter++;
                                if (IoList[i].IsInput)
                                {
                                    bufferInputCount++;
                                    item = Outputs.CreateChild("Buffer_" + bufferCounter.ToString(), 1, "", 1);

                                }
                                else
                                {
                                    bufferOutputCount++;
                                    item = Inputs.CreateChild("Buffer_" + bufferCounter.ToString(), 1, "", 1);
                                }
                            }
                            //bitTracker--;
                        }
                    }
                    //bitTracker = bitTracker + IoList[i].bitsize;
                    if (IoList[i].IsInput)
                    {
                        item = Outputs.CreateChild(tagName, IoList[i].type, "", 1);
                        //XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(item.ProduceXml(false));
                        bufferInputCount = bufferInputCount + Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);

                    }
                    else
                    {
                        item = Inputs.CreateChild(tagName, IoList[i].type, "", 1);
                        //XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(item.ProduceXml(false));
                        bufferOutputCount = bufferOutputCount + Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);
                    }
                    try
                    {
                        if(IoList[i].PathName != "")
                            systemManager.LinkVariables(IoList[i].PathName, item.PathName);
                        if (char.IsNumber(item.Name.ToCharArray()[0]))
                            item.Name = "_"+ item.Name;
                        AutoRename(worker, item, ScriptingTest.CSV_Reader.AutoRenameInEIPList);
                    }
                    catch (Exception ex)
                    {
                        worker.ProgressStatus = IoList[i].PathName + "     " + item.PathName;
                        worker.ProgressStatus = ex.Message;
                    }
                }
            }
            while (bufferInputCount % 16 != 0)
            {
                bufferInputCount++;
                bufferCounter++;
                item = Outputs.CreateChild("Buffer_" + bufferCounter.ToString(), 1, "", 1);
            }
            while (bufferOutputCount % 16 != 0)
            {
                bufferOutputCount++;
                bufferCounter++;
                item = Inputs.CreateChild("Buffer_" + bufferCounter.ToString(), 1, "", 1);

            }

        }
        private ITcSmTreeItem FindDevice(IWorker worker, ITcSmTreeItem Devices, string Type)
        {
            for (int i = 1; i <= Devices.ChildCount; i++)
            {
                ITcSmTreeItem EtherCATDevice = Devices.Child[i];
                XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
                EtherCATChilds.LoadXml(Devices.Child[i].ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
                XmlNode type = EtherCATChilds.SelectSingleNode("TreeItem/ItemSubTypeName"); //Gets the I/O Type. Example EP1111 or EK1100
                if (Type == type.InnerText)
                    return EtherCATDevice;
                /*string[] splittype = type.InnerText.Split('-');
                string[] splitInputType = Type.Split('-');

                if (splitInputType.Length <= splittype.Length)
                {
                    bool IsMatch = false;
                    for (int x = 0; x < splitInputType.Length; x++)
                    {
                        if (splitInputType[x] == splittype[x])
                            IsMatch = true;
                        else
                            IsMatch = false;

                    }
                    if (IsMatch)
                        return EtherCATDevice.PathName;
                }

            }*/
                
            }
            return null;
        }
        //devices
        private string GetSingleIoObjectPath(IWorker worker, ITcSmTreeItem EtherCATMaster, string Type)
        {
            for (int i = 1; i <= EtherCATMaster.ChildCount; i++)
            {
                ITcSmTreeItem EtherCATDevice = EtherCATMaster.Child[i];
                XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
                EtherCATChilds.LoadXml(EtherCATMaster.Child[i].ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
                XmlNode type = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/ProductRevision"); //Gets the I/O Type. Example EP1111 or EK1100
                string[] splittype = type.InnerText.Split('-');
                string[] splitInputType = Type.Split('-');

                if(splitInputType.Length <= splittype.Length)
                {
                    bool IsMatch = false;
                    for(int x = 0; x < splitInputType.Length;x++)
                    {
                        if (splitInputType[x] == splittype[x])
                            IsMatch = true;
                        else
                            IsMatch = false;
                           
                    }
                    if(IsMatch)
                         return EtherCATDevice.PathName;
                }

            }
            return null;
        }
        private void GetIOList(IWorker worker, ITcSmTreeItem EtherCATMaster, List<IO_Object> IoList)
        {
            for (int i = 1; i <= EtherCATMaster.ChildCount; i++)
            {
                GetIOObject(worker, EtherCATMaster, IoList, i);

            }
        }

        private int GetTypeFromString(string type)
        {
            // 7 = UDINT
            //8 = LINT
            //6 = DINT
            //5 = uint
            //4 = int
            //3 = usint
            //2 = sint
            int number = 0;
            bool canConvert = int.TryParse(type, out number);
            if (canConvert)
                return number;
            if (type == "BIT")
                return 1;
            //else if (type == "Byte")
            //    return 2;//Byte is 2, but we can't use bytes because it messes up EIP L5X generator
            else if (type == "WORD")//done sort of, 
                return 5;
            else if (type == "DWORD")
                return 9;
            else if (type == "SINT")
                return 2;
            else if (type == "USINT") // don
                return 3;
            else if (type == "INT")
                return 4;
            else if (type == "UINT" )
                return 5;
            else if (type == "DINT") //Done
                return 6;
            else if (type == "UDINT")//Done
                return 7;
            else if (type == "REAL")
                return 10;
            else if (type == "LREAL")
                return 11;
            //else if (type == "STRING")
            //    return 13;
            else if (type == "LINT") //Done
                return 8;
            else if (type == "ULINT") //Done
                return 9;
            else if (type == "BIT2") //Done
                return 4;
            else
                return 4;
        }
        private int GetSizeFromString(string type)
        {
            // 9 = ULINT
            //10 = REAL
            //11 = LREAL
            // 7 = UDINT
            //8 = LINT
            //6 = DINT
            //5 = uint
            //4 = int
            //3 = usint
            //2 = sint
            int number = 0;
            bool canConvert = int.TryParse(type, out number);
            if (canConvert)
                return 32;
            if (type == "BIT")
                return 1;
            if (type == "BOOL")
                return 1;
            else if (type == "Byte")
                return 8;//Byte is 2, but we can't use bytes because it messes up EIP L5X generator
            else if (type == "WORD")//done sort of, 
                return 16;
            else if (type == "DWORD")
                return 32;
            else if (type == "SINT")
                return 8;
            else if (type == "USINT") // don
                return 8;
            else if (type == "INT")
                return 16;
            else if (type == "UINT" )
                return 16;
            else if (type == "DINT") //Done
                return 32;
            else if (type == "UDINT")//Done
                return 32;
            else if (type == "REAL")
                return 32;
            else if (type == "LREAL")
                return 64;
            else if (type == "STRING")
                return 656;
            else if (type == "LINT") //Done
                return 64;
            else if (type == "BIT2") //Done
                return 2;
            else
                return 32;
        }


        private void GetIOObjectRecursive(IWorker worker, ITcSmTreeItem item, List<IO_Object> list)
        {
            AutoRename(worker, item);
            foreach (ITcSmTreeItem subitem in item)
            {

                if (subitem.ChildCount == 0)
                {
                    if (subitem.ItemType == 7)
                    {
                        AutoRename(worker, subitem);
                        try
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(subitem.ProduceXml(false));
                            IO_Object NewItem = new IO_Object();
                            //NewItem.IsOutput = false;
                            //NewItem.IsInput = true;
                            NewItem.PathName = subitem.PathName;
                            NewItem.TagName = subitem.Name;
                            NewItem.TagParentName = subitem.Parent.Name;
                            //if(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarType").InnerText)
                            NewItem.typedetails = xmlDoc.SelectSingleNode("TreeItem/VarDef/VarType").InnerText;
                            // 7 = UDINT
                            //8 = LINT
                            //6 = DINT
                            //5 = uint
                            //4 = int
                            //3 = usint
                            //2 = sint
                            if (NewItem.typedetails == "BIT")
                                NewItem.type = 1;
                            //else if (NewItem.typedetails == "Byte")
                            //    NewItem.type = 2;//Byte is 2, but we can't use bytes because it messes up EIP L5X generator
                            else if (NewItem.typedetails == "WORD")//done sort of, 
                                NewItem.type = 5;
                            else if (NewItem.typedetails == "DWORD")
                                NewItem.type = 9;
                            //else if (NewItem.typedetails == "SINT")
                            //    NewItem.type = 2;
                            //else if (NewItem.typedetails == "USINT") // done
                            //    NewItem.type = 3;
                            else if (NewItem.typedetails == "INT" || NewItem.typedetails == "Byte" || NewItem.typedetails == "SINT")
                                NewItem.type = 4;
                            else if (NewItem.typedetails == "UINT" || NewItem.typedetails == "USINT")
                                NewItem.type = 5;
                            else if (NewItem.typedetails == "DINT") //Done
                                NewItem.type = 6;
                            else if (NewItem.typedetails == "UDINT")//Done
                                NewItem.type = 7;
                            else if (NewItem.typedetails == "REAL")
                                NewItem.type = 11;
                            else if (NewItem.typedetails == "LREAL")
                                NewItem.type = 12;
                            else if (NewItem.typedetails == "STRING")
                                NewItem.type = 13;
                            else if (NewItem.typedetails == "LINT") //Done
                                NewItem.type = 8;
                            else if (NewItem.typedetails == "BIT2") //Done
                                NewItem.type = 4;
                            else
                                NewItem.type = 4;
                            NewItem.IndexGroup = xmlDoc.SelectSingleNode("TreeItem/VarDef/AdsInfo/IndexGroup").InnerText;
                            NewItem.IndexOffset = xmlDoc.SelectSingleNode("TreeItem/VarDef/AdsInfo/IndexOffset").InnerText;
                            list.Add(NewItem);
                        }
                        catch (Exception ex)
                        {
                            worker.ProgressStatus = ex.Message;
                        }
                        
                    }
                }
                else
                {
                    
                    //XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.LoadXml(subitem.ProduceXml(false));
                    GetIOObjectRecursive(worker, subitem, list);
                }
            }
        }
        private void AutoRename(IWorker worker, ITcSmTreeItem Device)
        {
            foreach (string[] ReplaceWord in ScriptingTest.CSV_Reader.AutoRenameInEtherCATList)
                if (Device.Name.Contains(ReplaceWord[0]))
                {
                    Device.Name = Device.Name.Replace(ReplaceWord[0], ReplaceWord[1]);
                }
        }
        private void AutoRename(IWorker worker, ITcSmTreeItem Device, List<string[]> AutoRenameList )
        {
            foreach (string[] ReplaceWord in AutoRenameList)
                if (Device.Name.Contains(ReplaceWord[0]))
                {
                    Device.Name = Device.Name.Replace(ReplaceWord[0], ReplaceWord[1]);
                }
        }
        private void GetIOObject(IWorker worker, ITcSmTreeItem EtherCATMaster, List<IO_Object> IoList, int i)
        {
            //int i = 1;
            List<IO_Object> FullListOfIOObjects = new List<IO_Object>();
            ITcSmTreeItem EtherCATDevice = EtherCATMaster.Child[i];
            foreach (ITcSmTreeItem SubItem in EtherCATDevice) //This foreach is just to strip the first object out
            {
                GetIOObjectRecursive(worker, SubItem, FullListOfIOObjects);
            }
            XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
            EtherCATChilds.LoadXml(EtherCATMaster.Child[i].ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
            if (EtherCATChilds.SelectNodes("TreeItem/EtherCAT/Slave/ProcessData/TxPdo") == null)
                return;
            XmlNodeList EtherCatTerminal = EtherCATChilds.SelectNodes("TreeItem/EtherCAT/Slave/ProcessData/TxPdo"); // return with the specific tree item of found devices.       
            //XmlNode TypeOld = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/Type"); //Gets the I/O Type. Example EP1111 or EK1100
            //XmlNode Type = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/ProductRevision"); //Gets the I/O Type. Example EP1111 or EK1100
            

            /******************************************
                 *Search for process data
                 **************************************** */
            //OutputAdd("Linking IO from terminal: " + Type.InnerText);
            bool PdoEnbled = false;
            string SmValue;
            string LinkName;
            foreach (XmlNode EtherCatTerminalNodes in EtherCatTerminal)
            {
                try
                {
                    SmValue = EtherCatTerminalNodes.Attributes["Sm"].Value.ToString();
                    PdoEnbled = true;
                }
                catch
                {
                    PdoEnbled = false;
                }
                if (PdoEnbled == true && EtherCatTerminalNodes.SelectNodes("Entry") != null)
                {
                    foreach (XmlNode EtherCatTerminalNodeSubitems in EtherCatTerminalNodes.SelectNodes("Entry"))
                    {
                        try
                        {
                            if (EtherCatTerminalNodeSubitems.SelectSingleNode("Index").InnerText != "#x0")
                            {
                                //LinkName = EtherCATDevice.PathName;
                                

                                string LinkNamePart1 = EtherCATMaster.Child[i].PathName.ToString();
                                string LinkNamePart2 = EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString();

                                string LinkNamePart3 = EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString();

                                //if (LinkNamePart3.Contains("_"))
                                //{
                                string IndexGroup = EtherCatTerminalNodeSubitems.SelectSingleNode("AdsInfo/IndexGroup").InnerText;
                                string IndexOffset = EtherCatTerminalNodeSubitems.SelectSingleNode("AdsInfo/IndexOffset").InnerText;

                                IO_Object match = FullListOfIOObjects.Find(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset);
                                LinkNamePart3 = match.TagName;
                                //LinkNamePart3.Replace('_', '^');
                                LinkName = match.PathName;

                                // }
                                //else
                                //    LinkName = EtherCATMaster.Child[i].PathName.ToString() + "^" + EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "^" + EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString();
                                IO_Object NewItem = new IO_Object();
                                NewItem.IsOutput = false;
                                NewItem.IsInput = true;
                                NewItem.PathName = LinkName;
                                NewItem.TagName = EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "_" + LinkNamePart3;
                                //NewItem.TagName = NewItem.TagName.Replace(" ", "_");
                                NewItem.TagParentName = EtherCATMaster.Child[i].Name.ToString();
                                NewItem.bitsize = Convert.ToInt32(EtherCatTerminalNodeSubitems.SelectSingleNode("BitLen").InnerText);
                                NewItem.type = match.type;
                                NewItem.typedetails = match.typedetails;
                                IoList.Add(NewItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            worker.ProgressStatus = "terminal" + EtherCATMaster.Child[i].Name + " message:" + ex.Message;
                        }
                        
                    }
                }
            }
            EtherCatTerminal = null;
            EtherCatTerminal = EtherCATChilds.SelectNodes("TreeItem/EtherCAT/Slave/ProcessData/RxPdo"); // return with the specific tree item of found devices.
            foreach (XmlNode EtherCatTerminalNodes in EtherCatTerminal)
            {
                // OutputAdd("Inner xml = " + EtherCatTerminalNodes.Attributes.Count.ToString());
                try
                {
                    SmValue = EtherCatTerminalNodes.Attributes["Sm"].Value.ToString();
                    PdoEnbled = true;
                }
                catch
                {
                    PdoEnbled = false;
                }
                // OutputAdd("Has child nodes = " + EtherCatTerminalNodes.FirstChild.InnerText.ToString());
                //Get the "Name", "Entry/Name" and the "Entry/BitLen" from each subterminal. 
                if (PdoEnbled == true)
                {
                    if (PdoEnbled == true && EtherCatTerminalNodes.SelectNodes("Entry") != null)
                    {
                        foreach (XmlNode EtherCatTerminalNodeSubitems in EtherCatTerminalNodes.SelectNodes("Entry"))
                        {

                            try
                            {
                                if (EtherCatTerminalNodeSubitems.SelectSingleNode("Index").InnerText != "#x0")
                                {
                                    string LinkNamePart1 = EtherCATMaster.Child[i].PathName.ToString();
                                    string LinkNamePart2 = EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString();

                                    string LinkNamePart3 = EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString();

                                    //if (EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString().Contains("_"))
                                    //{
                                    string IndexGroup = EtherCatTerminalNodeSubitems.SelectSingleNode("AdsInfo/IndexGroup").InnerText;
                                    string IndexOffset = EtherCatTerminalNodeSubitems.SelectSingleNode("AdsInfo/IndexOffset").InnerText;

                                    IO_Object match = FullListOfIOObjects.Find(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset);
                                    LinkNamePart3 = match.TagName;
                                    //LinkNamePart3.Replace('_', '^');
                                    LinkName = match.PathName;
                                    //}
                                    //else
                                    //    LinkName = EtherCATMaster.Child[i].PathName.ToString() + "^" + EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "^" + EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString();
                                    IO_Object NewItem = new IO_Object();
                                    NewItem.IsInput = false;
                                    NewItem.IsOutput = true;
                                    NewItem.PathName = LinkName;
                                    NewItem.TagName = EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "_" + LinkNamePart3;
                                    NewItem.TagParentName = EtherCATMaster.Child[i].Name.ToString();
                                    NewItem.bitsize = Convert.ToInt32(EtherCatTerminalNodeSubitems.SelectSingleNode("BitLen").InnerText);
                                    NewItem.type = match.type;
                                    NewItem.typedetails = match.typedetails;
                                    /*
                                                                if (EtherCatTerminalNodeSubitems.SelectSingleNode("BitLen").InnerText == "1")
                                                                {
                                                                    NewItem.type = 1;
                                                                }
                                                                else if (EtherCatTerminalNodeSubitems.SelectSingleNode("BitLen").InnerText == "16")
                                                                {
                                                                    NewItem.type = 6;
                                                                }
                                                                else
                                                                {
                                                                    NewItem.type = 0;
                                                                    NewItem.typedetails = "unknown";
                                                                }*/
                                    IoList.Add(NewItem);
                                }
                            }
                            catch (Exception ex)
                            {
                                worker.ProgressStatus = "terminal" + EtherCATMaster.Child[i].Name + " message:" + ex.Message;
                            }
                        }
                    }
                }
            }
        }
        private ITcSmTreeItem CreateEtherCATNetwork(ITcSmTreeItem devices, string EtherCATMasterName)
        {
            int[] vInfo = new int[4];
            vInfo[0] = 2; // vendorId Beckhoff
            vInfo[1] = 0; // productCode
            vInfo[2] = 0; // revision, only used for EL6731
            vInfo[3] = 0; // serial number

            //create EtherCAT-Master
            ITcSmTreeItem device = devices.CreateChild(EtherCATMasterName, (int)DeviceType.EtherCAT_DirectMode);
            //search for EtherCAT-Master


            //create EK1100 A2P
            

            vInfo[1] = 72100946; //productCode EK1100
            vInfo[2] = 65536;    //revision 0000-0001
            ITcSmTreeItem a2p = device.CreateChild("A2P (EK1100)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            //create Terminals
            vInfo[1] = 66465874; //productCode EL1014
            vInfo[2] = 0;        //revision 0000-0000
            ITcSmTreeItem terminal = device.CreateChild("100 (EL1014)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 616050768; //productCode EL9400
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("101 (EL9400)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 131346514; //productCode EL2004
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("102 (EL2004)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 596389968; //productCode EL9100
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("103 (EL9100)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 131346514; //productCode EL2004
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("104 (EL2004)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 131346514; //productCode EL2004
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("105 (EL2004)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 131346514; //productCode EL2004
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("106 (EL2004)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 131346514; //productCode EL2004
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("107 (EL2004)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 131346514; //productCode EL2004
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("108 (EL2004)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 65810514; //productCode EL1004
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("109 (EL1004)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            vInfo[1] = 66465874; //productCode EL1014
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("110 (EL1014)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);


            terminal = device.CreateChild("111 (EL6652-0010)", (int)TCSYSMANAGERBOXTYPES.TSM_BOX_TYPE_EXXXXX, "", "EL6652-0010");

            vInfo[1] = 72756306; //productCode EL1110
            vInfo[2] = 0;        //revision 0000-0000
            terminal = device.CreateChild("112 (EK1110)", (int)BoxType.EtherCAT_EXXXXX, "", (object)vInfo);

            return device;
        }
    
        private class IO_Object
        {
            public int type;
            public string PathName;
            public string typedetails;
            public string TagName;
            public string TagParentName;
            public bool IsInput = false;
            public bool IsOutput = false;
            public string IndexGroup;
            public string IndexOffset;
            public int bitsize;
        }

        /// <summary>
        /// Gets the Script description
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return "Copy EtherCAT network PDO's to a new Ethernet IP device\nEnjoy!";
            }
        }

        /// <summary>
        /// Gets the keywords, describing the Script features
        /// </summary>
        /// <value>The keywords.</value>
        public override string Keywords
        {
            get
            {
                return "EtherCAT, BoxCreation via VInfo, IO-PLC Mapping,\nScanning Devices, PLC rescan";
            }
        }

        /// <summary>
        /// Gets the Version number of TwinCAT that is necessary for script execution.
        /// </summary>
        /// <value>The TwinCAT version.</value>
        public override Version TwinCATVersion
        {
            get
            {
                return new Version(3, 1);//(3, 0);
            }
        }

        /// <summary>
        /// Gets the build number of TwinCAT that is necessary for script execution.
        /// </summary>
        /// <value>The TwinCAT build.</value>
        public override string TwinCATBuild
        {
            get
            {
                return "4024";//"3100";
            }
        }

        /// <summary>
        /// Gets the category of this script.
        /// </summary>
        /// <value>The script category.</value>
        public override string Category
        {
            get
            {
                return "I/O";
            }
        }
    }
}