using System.IO;
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
    ///Set MDR settings in project
    /// </summary>
    public class SetMDRSettings
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
            this.project = dte.Solution.Projects.Item(1);
            this.systemManager = (ITcSysManager4)project.Object;
            //this.project = (Project)CreateNewProject();
            //this.systemManager = (ITcSysManager4)project.Object;
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

            //string XmlStr;
            //int El6731DevId;
            //int El6652DevId;

            //IO configuration
            //search for IO Devices
            string EtherCATMasterName = "EtherCAT Master";
            ITcSmTreeItem devices = systemManager.LookupTreeItem("TIID");
            ITcSmTreeItem device;
            device = FindDevice(worker, devices, "EtherCAT Master");
            bool HasEtherCATNetwork = true;

            if (device == null)
                return;
            if (!HasEtherCATNetwork)
            {
                worker.Progress = 10;
                worker.ProgressStatus = "Creating EtherCATnetwork";
                device = CreateEtherCATNetwork(devices, EtherCATMasterName);
            }

            List<IO_Object> IO_List = new List<IO_Object>();
            GetIOList(worker, device, IO_List);

            //create EIP Master
            worker.ProgressStatus = "Finished Scanning EtherCAT network";

            
            worker.Progress = 70;
            worker.ProgressStatus = "Adding PLC project ...";

            ITcSmTreeItem plcConfig = systemManager.LookupTreeItem("TIPC");

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
            }
            return null;
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
        private void ImportEIPDevice(IWorker worker, ITcSmTreeItem devices, List<IO_Object> IoList)
        {

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

            item = eipSlave.Child[1];//Grab first child It's the Box 1 (TC EtherNet/IP Slave
            EIPSlaveSubName = item.Name;
            item = item.Child[1]; //Grab IO Assembly
           

            IOAssemblyName = item.Name;
            Inputs = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName + "^" + EIPSlaveSubName + "^" + IOAssemblyName + "^Inputs");
            Outputs = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName + "^" + EIPSlaveSubName + "^" + IOAssemblyName + "^Outputs");


            //ITcSmTreeItem item2 = item.Child[1]; //This should be first tag

            //Remove everything first
            for (int i = Inputs.ChildCount; 1 < i; i--)
            {
                Inputs.DeleteChild(Inputs.Child[i].Name);
            }
            for (int i = Outputs.ChildCount; i > 1; i--)
            {
                Outputs.DeleteChild(Outputs.Child[i].Name);
            }

            //bool SortByteIntsFirst = true;
            //if(SortByteIntsFirst)
            //{
            //    IoList.Sort((x, y) => y.bitsize.CompareTo(x.bitsize));
            //}

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

                    
                    string tagName = IoList[i].TagParentName + "_" + IoList[i].TagName;
                    tagName = tagName.Replace(" ", "_");
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
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(item.ProduceXml(false));
                        //xmlDoc.SelectSingleNode("TreeItem/VarDef/VarType").InnerText = "UDINT";
                        //xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText = "32";
                        // item.ConsumeXml(xmlDoc.OuterXml);
                        //node1.InnerText = "UDINT";
                        //xmlDoc.ReplaceChild("<TreeItem><VarDef><VarType>UDINT</VarType></VarDef></TreeItem>", xmlDoc.SelectSingleNode("TreeItem/VarDef/VarType"));
                        //item.ConsumeXml(xmlDoc.InnerXml.ToString());

                        //string convertedData = IoList[i].typedetails;
                        //item.ConsumeXml("<TreeItem><VarDef><VarType>" + convertedData + "</VarType></VarDef></TreeItem>");
                        //xmlDoc.LoadXml(item.ProduceXml(false));

                        bufferInputCount = bufferInputCount + Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);

                    }
                    else
                    {
                        item = Inputs.CreateChild(tagName, IoList[i].type, "", 1);
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(item.ProduceXml(false));
                        bufferOutputCount = bufferOutputCount + Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);
                    }
                    try
                    {
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


            //string XmlStr;
            //XmlDocument IOAssemblyNodes = new XmlDocument();
            //IOAssemblyNodes.LoadXml(item.ProduceXml(false));
            //XmlNodeList Nodes = IOAssemblyNodes.SelectNodes("Var");

            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(XmlStr);



        }

        private void GetIOList(IWorker worker, ITcSmTreeItem EtherCATMaster, List<IO_Object> IoList)
        {
            Int32 MDR_Counter = 0;
            for (int i = 1; i <= EtherCATMaster.ChildCount; i++)
            {
                GetIOObject(worker, EtherCATMaster, IoList, i, ref MDR_Counter);

            }
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
                            NewItem.bitsize = 0;
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
        private void GetIOObject(IWorker worker, ITcSmTreeItem EtherCATMaster, List<IO_Object> IoList, int i, ref Int32 mdrCount)
        {
            //int i = 1;
            //List<IO_Object> FullListOfIOObjects = new List<IO_Object>();
            ITcSmTreeItem EtherCATDevice = EtherCATMaster.Child[i];
            //foreach (ITcSmTreeItem SubItem in EtherCATDevice) //This foreach is just to strip the first object out
            //{
            //    GetIOObjectRecursive(worker, SubItem, FullListOfIOObjects);
            //}
            XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
            EtherCATChilds.LoadXml(EtherCATMaster.Child[i].ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
            if (EtherCATChilds.SelectNodes("TreeItem/EtherCAT/Slave/ProcessData/TxPdo") == null)
                return;
            //XmlNodeList EtherCatTerminal = EtherCATChilds.SelectNodes("TreeItem/EtherCAT/Slave/ProcessData/TxPdo"); // return with the specific tree item of found devices.       
            //XmlNode TypeOld = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/Type"); //Gets the I/O Type. Example EP1111 or EK1100
            XmlNode Type = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/ProductRevision"); //Gets the I/O Type. Example EP1111 or EK1100
            
            if(Type != null && Type.InnerText.Split('-')[0] == "EP7402")
            {
                mdrCount++;
                List<StartupListObject> StartupList = CSV_Reader.AutoEtherCATSettings;
                //StartupList.Add(new StartupListObject("Peak Current Ch1", "0", "1", "32800", "1", "8813"));
                //StartupList.Add(new StartupListObject("Peak Current Ch2", "0", "1", "32816", "1", "8813"));
                
                XmlDocument MDR = new XmlDocument();
                MDR.LoadXml("<TreeItem><EtherCAT><Slave><Mailbox><CoE><InitCmds></InitCmds></CoE></Mailbox></Slave></EtherCAT></TreeItem>");
                //MDR.LoadXml("<TreeItem><EtherCAT><Slave><Mailbox><CoE><InitCmds><InitCmd><Transition>PS</Transition><Comment><![CDATA[Peak current]]></Comment><Timeout>0</Timeout><Ccs>1</Ccs><Index>32800</Index><SubIndex>1</SubIndex><Data>8813</Data></InitCmd></InitCmds></CoE></Mailbox></Slave></EtherCAT></TreeItem>");
                foreach (StartupListObject singleStartup in StartupList)
                {
                    if ("All" == singleStartup.NetworkPosition)
                        AddToStartupList(MDR, singleStartup);
                }
                foreach (StartupListObject singleStartup in StartupList)
                {
                    int value;
                    var isNumeric = int.TryParse(singleStartup.NetworkPosition, out value);
                    if (isNumeric == true)
                        if (mdrCount == value)
                            AddToStartupList(MDR, singleStartup);
                }
                foreach (MotorStartupList MotorSetting in CSV_Reader.MotorSettingsList)
                {
                    //int value;
                    //var isNumeric = int.TryParse(MotorSetting.MotorTargetPosition, out value);
                    //if (isNumeric == true)
                    if (MotorSetting.MotorTargetName == EtherCATDevice.Name)
                    {
                        for (int x = 0; x < MotorSetting.MotorSettings.MotorSettings.Count; x++)
                        {
                            string channel = "1";
                            string index = MotorSetting.MotorSettings.MotorSettings[x].Index;
                            if (MotorSetting.MotorChannel.ToLower() == "chl2" || MotorSetting.MotorChannel.ToLower() == "2")
                            {
                                channel = "2";
                                int value;
                                var isNumeric = int.TryParse(index, out value);
                                if (isNumeric == true)
                                {
                                    index = (value + 10).ToString();
                                }
                            }
                            StartupListObject singleStartup = new StartupListObject("", channel, "", "0", "1", index, MotorSetting.MotorSettings.MotorSettings[x].SubIndex, MotorSetting.MotorSettings.MotorSettings[x].Data, MotorSetting.MotorSettings.MotorSettings[x].Size);
                            AddToStartupList(MDR, singleStartup);
                        }
                    }
                }


            EtherCATDevice.ConsumeXml(MDR.InnerXml);
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
            public int bitsize = 0;
        }
        
        private void AddToStartupList(XmlDocument MDR, StartupListObject SingleObject)
        {

            XmlNode InitCmds = MDR["TreeItem"]["EtherCAT"]["Slave"]["Mailbox"]["CoE"]["InitCmds"];
            XmlElement InitCmd = MDR.CreateElement("InitCmd");

            //if (MDR.SelectSingleNode("TreeItem/EtherCAT/Slave/Mailbox/CoE/InitCmds/InitCmd/Transition") != null)
           // {
                XmlElement Transition = MDR.CreateElement("Transition");
                Transition.InnerText = "PS";
                InitCmd.AppendChild(Transition);
            //}
            //else
           //     MDR["TreeItem"]["EtherCAT"]["Slave"]["Mailbox"]["CoE"]["InitCmds"]["InitCmd";
            XmlElement Comment = MDR.CreateElement("Comment");
            Comment.InnerText = SingleObject.Comment;//"![CDATA[Peak current]]";
            InitCmd.AppendChild(Comment);
            XmlElement Timeout = MDR.CreateElement("Timeout");
            Timeout.InnerText = SingleObject.Timeout;//"0";
            InitCmd.AppendChild(Timeout);
            XmlElement Ccs = MDR.CreateElement("Ccs");
            Ccs.InnerText = SingleObject.Ccs;//"1";
            InitCmd.AppendChild(Ccs);
            XmlElement Index = MDR.CreateElement("Index");
            Index.InnerText = int.Parse(SingleObject.Index, System.Globalization.NumberStyles.HexNumber).ToString(); //Convert.ToInt32(SingleObject.Index).ToString("X");// "32816";
            InitCmd.AppendChild(Index);
            XmlElement SubIndex = MDR.CreateElement("SubIndex");
            SubIndex.InnerText = int.Parse(SingleObject.SubIndex, System.Globalization.NumberStyles.HexNumber).ToString(); ;//"32816";
            InitCmd.AppendChild(SubIndex);
            XmlElement Data = MDR.CreateElement("Data");
            //Delete node if allready in here
            foreach (XmlNode node in InitCmds.SelectNodes("InitCmd"))
            {
                if (node.SelectSingleNode("Index") != null && node.SelectSingleNode("SubIndex") != null)
                    if (node["Index"].InnerText == Index.InnerText && node["SubIndex"].InnerText == SubIndex.InnerText)
                        InitCmds.RemoveChild(node);
            }
            if (SingleObject.Data.ToLower() == "true")
                SingleObject.Data = "1";
            if (SingleObject.Data.ToLower() == "false")
                SingleObject.Data = "0";
            char[] charArrayTemp = Convert.ToInt32(SingleObject.Data).ToString("X").ToCharArray();//SingleObject.Data.ToCharArray();
            char[] charArray = new char[] { '0', '0' };//, '0', '0' };//, '0', '0', '0', '0' };
            if (Convert.ToInt32( SingleObject.Size) == 4)
                charArray = new char[] { '0', '0', '0', '0', '0', '0', '0', '0' };
            if (Convert.ToInt32(SingleObject.Size) == 2)
                charArray = new char[] { '0', '0', '0', '0' };//, '0', '0', '0', '0' };

            //Copy the Array Temp to the new array. Add "0"'s in area that th ToString("X") didn't fill in. If we don't do this it fucks up the final number
            int i = 0;
            int x = 0;
            //if(charArrayTemp.Length > charArray.Length)
                
            for (i = 0; i< charArrayTemp.Length; i++)
            {
                x = i + charArray.Length - charArrayTemp.Length;
                    if(x >= 0 && i >= 0 && x < charArray.Length && i < charArrayTemp.Length)
                charArray[x] = charArrayTemp[i];                  
            }

            //Byte swap. Oh god, what a pain in the ass
            i = 0;
            while (i < charArray.Length - 3)
            {

                char buffer1 = charArray[i];
                char buffer2 = charArray[i + 1];
                char buffer3 = charArray[i + 2];
                char buffer4 = charArray[i + 3];
                charArray[i] = buffer3;
                charArray[i + 1] = buffer4;
                charArray[i + 2] = buffer1;
                charArray[i + 3] = buffer2;
                i++;
                i++;
                i++;
                i++;
            }
   
            if( charArray.Length >4)
            {

                char buffer1 = charArray[0];
                char buffer2 = charArray[1];
                char buffer3 = charArray[2];
                char buffer4 = charArray[3];
                charArray[0] = charArray[4];
                charArray[1] = charArray[5];
                charArray[2] = charArray[6];
                charArray[3] = charArray[7];
                charArray[4] = buffer1;
                charArray[5] = buffer2;
                charArray[6] = buffer3;
                charArray[7] = buffer4;
            }
            Data.InnerText = new string(charArray);//Convert.ToInt32(SingleObject.Data).ToString("X"); ;// "8813";
            InitCmd.AppendChild(Data);

            
            InitCmds.AppendChild(InitCmd);
        }

        /// <summary>
        /// Gets the Script description
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return "Edit all EP7402's startup list\nSee motor files directory and EtherCAT_StartupList.txt";
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
