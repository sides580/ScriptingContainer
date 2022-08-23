﻿using System.IO;
using System.Xml;
using EnvDTE;
using EnvDTE100;
using EnvDTE80;
using TCatSysManagerLib;
using TwinCAT.SystemManager;
using System;
using ScriptingTest;
using System.Linq;
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
            GlobalVariables.NumberOfDevicesInEtherCATTree = GetIOCount(worker, device);

            ReferenceCode.ImportPOUFromList(dte, systemManager, devices, CSV_Reader.ImportPOU_List, worker);
            ReferenceCode.AddLibrarys(systemManager, devices, worker);
            worker.ProgressStatus = "Finished Importing PLC code";
            worker.Progress = 30;
            List<ReferenceCode.IO_Object> IO_List = new List<ReferenceCode.IO_Object>();
            GetIOList(worker, device, IO_List);

            AddAutoGeneratedCode(devices, worker);

            worker.ProgressStatus = "Compile";
            dte.Solution.SolutionBuild.Clean(true);
            dte.Solution.SolutionBuild.Build(true);


            //create EIP Master
            worker.Progress = 60;
            worker.ProgressStatus = "Finished Scanning EtherCAT network";
            //Need to compile once



            //ITcSmTreeItem eipSlave;// = devices.CreateChild(DeviceEIPSlaveName, 145);
            //AddNewEIPDevice(devices);
            string EL6652Path = GetSingleIoObjectPath(worker, device, "EL6652-0010");
            if (EL6652Path == null)
            {
                worker.ProgressStatus = "EL6652 not found in etherCAT network. Can't continue till a EL6652 is added.";
            }



            ImportEIPDevice(worker, devices, IO_List, EL6652Path);
            worker.Progress = 80;
            worker.ProgressStatus = "Finished building Ethernet IP network";


            AddManualLinks(worker);
            worker.ProgressStatus = "Finished extra linking";
            worker.Progress = 100;
   
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

        private void AddAutoGeneratedCode(ITcSmTreeItem devices, IWorker worker)
        {
            ITcSmTreeItem device = FindDevice(worker, devices, "EtherCAT Master");
            ITcSmTreeItem PlcDevice = systemManager.LookupTreeItem("TIPC");
            ITcSmTreeItem plcProject = PlcDevice.Child[1];
            ITcProjectRoot projectRoot = (ITcProjectRoot)plcProject;
            ITcSmTreeItem nestedProject = projectRoot.NestedProject;
            string projectName = plcProject.Name;
            ITcSmTreeItem plcGVLsItem = systemManager.LookupTreeItem("TIPC^" + projectName + "^" + projectName + " Project^GVLs");
            

            List<List<AutoAddCode>> CodeToAdd = new List<List<AutoAddCode>>();

            //First sort these into an extra dimensional list to's split between all the locations we need to generate a file for.
            foreach (AutoAddCode line in CSV_Reader.AutoAddVariables)
            {
                if (CodeToAdd.Count == 0)
                {
                    CodeToAdd.Add(new List<AutoAddCode>());
                    CodeToAdd[0].Add(line);
                }
                else
                {
                    List<AutoAddCode> Match = CodeToAdd.Find(a => a[0].Location == line.Location);
                    if (Match == null)
                    {
                        CodeToAdd.Add(new List<AutoAddCode>());
                        CodeToAdd[0].Add(line);
                    }
                    else
                    {
                        Match.Add(line);
                    }
                }
            }
            foreach (List<AutoAddCode> macro in CodeToAdd)
            {
                List<string> CodeToGenerateBySection = new List<string>();
                string CodeToGenerate = "";
                /*string CodeToGenerate = "VAR_GLOBAL\n" + "\t//WARNING, Important!\n" +
            "\t//Do not add or remove code from this Global Variable List. Use another Global Variable List.\n" +
            "\t//Code added by the programmer here will be deleted.\n";*/
                
                string location = macro[0].Location;
                foreach (AutoAddCode line in macro)
                {
                   
                    int index = 0;
                    bool IsNewIndex = false;
                    if (CodeToGenerateBySection.Count == 0)
                    {
                        IsNewIndex = true;

                    }
                    else
                    {
                        index = FindMatchingIndex(CodeToGenerateBySection, line.Type);    
                        if(index >= CodeToGenerateBySection.Count)
                        {
                            IsNewIndex = true;
                        }
                    }
                    if(IsNewIndex)
                    {
                        CodeToGenerateBySection.Add(line.Type + "\n" + "\t//WARNING, Important!\n" +
"\t//Do not add or remove code from this Global Variable List. Use another Global Variable List.\n" +
"\t//Code added by the programmer here will be deleted.\n");
                    }

                    if (line.Option.ToLower() == "numberofethercatdevices")
                        CodeToGenerateBySection[index] = CodeToGenerateBySection[index] + "\t" + line.Name + ":INT:=" + GlobalVariables.NumberOfDevicesInEtherCATTree.ToString() + ";\n";
                    else
                    {
                        CodeToGenerateBySection[index] = CodeToGenerateBySection[index] + "\t" + line.Name + ";\n";
                    }
                }
                int VARGlobal = FindMatchingIndex(CodeToGenerateBySection, "VAR_GLOBAL");
                int VARGlobalConstant = FindMatchingIndex(CodeToGenerateBySection, "VAR_GLOBAL CONSTANT");
                string lines;
                
                string TwinCATIntName = "CoEReadInt";
                string TwinCATByteName = "CoEReadByte";
                string TwinCATUintName = "CoEReadUint";
                string TwinCATUdintName = "CoEReadUdint";
                //string TwinCATDintName = "CoEReadDint";
                string TwinCATBoolName = "CoEReadBool";
               //int BYTECount = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEDataType.ToLower() == "BYTE".ToLower()).Count();
               //int INTCount = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEDataType.ToLower() == "INT".ToLower()).Count();
               //int UINTCount = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEDataType.ToLower() == "UINT".ToLower()).Count();
               //int UDINTCount = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEDataType.ToLower() == "UDINT".ToLower()).Count();
               //int DINTCount = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEDataType.ToLower() == "DINT".ToLower()).Count();
               //int BOOLCount = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEDataType.ToLower() == "BOOL".ToLower()).Count();
                int BYTEPos = 0;
                int INTPos = 0;
                int UINTPos =0;
                int UDINTPos =0;
                //int DINTPos =0;
                int BOOLPos =0;


                List<int> MarkedForDeletion = new List<int>();
                for (int x = 0; x < CSV_Reader.CoEAutoReadToEIP.Count; x++)
                {
                    
                    if (CSV_Reader.CoEAutoReadToEIP[x].EthSlaveFindByMethod.ToLower() == "Name".ToLower())
                    {
                        foreach (string item in GlobalVariables.IOListNames)
                        {
                            if (item.ToLower() == CSV_Reader.CoEAutoReadToEIP[x].EthSlave.ToLower())
                            {
                                try
                                {
                                    ITcSmTreeItem match = FindTreeItem(worker, device, item);
                                    if (match != null)
                                    {
                                        XmlDocument XMLData = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
                                        XMLData.LoadXml(match.ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 

                                        XmlNode Address = XMLData.SelectSingleNode("TreeItem/BoxDef/FieldbusAddress"); //Gets the I/O Type. Example EP1111 or EK1100
                                        CSV_Reader.CoEAutoReadToEIP[x].EthSlave = Address.InnerText;
                                    }
                                }
                                catch { }

                            }
                        }
                    }
                    if (CSV_Reader.CoEAutoReadToEIP[x].EthSlaveFindByMethod.ToLower() == "Type".ToLower())
                    {
                        MarkedForDeletion.Add(x); //We delete this CoEAutoReadToEIP because it's only a place holder to tell me what IO to look for.
                        int counter = 0;
                        List<string> Address = GetEtherCatAddressOfType(worker, device, CSV_Reader.CoEAutoReadToEIP[x].EthSlave);
                        foreach (string add in Address)
                        {
                            counter++;
                            CoEAutoRead NewCoERead = new CoEAutoRead();
                            NewCoERead.CoEDataType = CSV_Reader.CoEAutoReadToEIP[x].CoEDataType;
                            NewCoERead.CoESubIndex = CSV_Reader.CoEAutoReadToEIP[x].CoESubIndex;
                            NewCoERead.CoEIndex = CSV_Reader.CoEAutoReadToEIP[x].CoEIndex;
                            NewCoERead.EIP_Var_Name = CSV_Reader.CoEAutoReadToEIP[x].EIP_Var_Name + "_" + counter.ToString();
                            NewCoERead.EthSlave = add;
                            NewCoERead.EthSlaveFindByMethod = "address";
                            NewCoERead.PLC_Var_Name = CSV_Reader.CoEAutoReadToEIP[x].PLC_Var_Name;
                            NewCoERead.VariableInsideTwinCATLinkName = CSV_Reader.CoEAutoReadToEIP[x].VariableInsideTwinCATLinkName;
                            CSV_Reader.CoEAutoReadToEIP.Add(NewCoERead);
                        }
                    }
                    else
                    {
                        CSV_Reader.CoEAutoReadToEIP[x].CoEIndex = "16#" + CSV_Reader.CoEAutoReadToEIP[x].CoEIndex; //modify to get into hex
                        if (CSV_Reader.CoEAutoReadToEIP[x].CoEDataType.ToLower() == "BYTE".ToLower())
                        {
                            BYTEPos++;
                            //CSV_Reader.CoEAutoReadToEIP[x].PLC_Var_Name = TwinCATByteName;
                            CSV_Reader.CoEAutoReadToEIP[x].VariableInsideTwinCATLinkName = TwinCATByteName + "[" + BYTEPos.ToString() + "]";
                        }
                        if (CSV_Reader.CoEAutoReadToEIP[x].CoEDataType.ToLower() == "UINT".ToLower())
                        {
                            UINTPos++;
                            //CSV_Reader.CoEAutoReadToEIP[x].PLC_Var_Name = TwinCATUintName;
                            CSV_Reader.CoEAutoReadToEIP[x].VariableInsideTwinCATLinkName = TwinCATUintName + "[" + UINTPos.ToString() + "]";
                        }
                        if (CSV_Reader.CoEAutoReadToEIP[x].CoEDataType.ToLower() == "INT".ToLower())
                        {
                            INTPos++;
                            //CSV_Reader.CoEAutoReadToEIP[x].PLC_Var_Name = TwinCATIntName;
                            CSV_Reader.CoEAutoReadToEIP[x].VariableInsideTwinCATLinkName = TwinCATIntName + "[" + INTPos.ToString() + "]";
                        }
                        if (CSV_Reader.CoEAutoReadToEIP[x].CoEDataType.ToLower() == "UDINT".ToLower())
                        {
                            UDINTPos++;
                            //CSV_Reader.CoEAutoReadToEIP[x].PLC_Var_Name = TwinCATUdintName;
                            CSV_Reader.CoEAutoReadToEIP[x].VariableInsideTwinCATLinkName = TwinCATUdintName + "[" + UDINTPos.ToString() + "]";
                        }
                        if (CSV_Reader.CoEAutoReadToEIP[x].CoEDataType.ToLower() == "BOOL".ToLower())
                        {
                            BOOLPos++;
                            //CSV_Reader.CoEAutoReadToEIP[x].PLC_Var_Name = TwinCATBoolName;
                            CSV_Reader.CoEAutoReadToEIP[x].VariableInsideTwinCATLinkName = TwinCATBoolName + "[" + BOOLPos.ToString() + "]";
                        }
                        CSV_Reader.EipExtraTags.Add(new EipExtraTagDetails(CSV_Reader.CoEAutoReadToEIP[x].EIP_Var_Name, "^PlcTask Outputs^" + location + "." + CSV_Reader.CoEAutoReadToEIP[x].VariableInsideTwinCATLinkName, CSV_Reader.CoEAutoReadToEIP[x].CoEDataType.ToUpper(), true, true));
                    }
                }

                
                for (int x = MarkedForDeletion.Count()-1; x >= 0; x--)
                {
                    CSV_Reader.CoEAutoReadToEIP.RemoveAt(MarkedForDeletion[x]);
                }

                    List<string> CoEIndex = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEIndex).ToList();
                List<string> CoESubIndex = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoESubIndex).ToList();
                List<string> CoESlave = CSV_Reader.CoEAutoReadToEIP.Select(a => a.EthSlave).ToList();
                List<string> CoEEIPName = CSV_Reader.CoEAutoReadToEIP.Select(a => a.EIP_Var_Name).ToList();
                List<string> CoEDataType = CSV_Reader.CoEAutoReadToEIP.Select(a => a.CoEDataType).ToList();
                List<string> CoESlaveAddressFindMethod = CSV_Reader.CoEAutoReadToEIP.Select(a => a.EthSlaveFindByMethod).ToList();


                lines = GenerateCodeBuildGlobalVarArraywithValues("CoEReadIndex", "1.." + CoEIndex.Count.ToString(), "WORD", CoEIndex);
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + lines;
                lines = GenerateCodeBuildGlobalVarArraywithValues("CoEReadSubIndex", "1.." + CoEIndex.Count.ToString(), "BYTE", CoESubIndex);
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + lines;
                lines = GenerateCodeBuildGlobalVarArraywithValues("CoEReadSlaveAddress", "1.." + CoEIndex.Count.ToString(), "STRING", CoESlave);
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + lines;
                lines = GenerateCodeBuildGlobalVarArraywithValues("CoEReadDataType", "1.." + CoEIndex.Count.ToString(), "STRING", CoEDataType);
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + lines;
                lines = GenerateCodeBuildGlobalVarArraywithValues("CoEReadSlaveType", "1.." + CoEIndex.Count.ToString(), "STRING", CoESlaveAddressFindMethod);
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + lines;
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + TwinCATByteName + " AT %Q*:ARRAY[1.." + BYTEPos.ToString() + "] OF BYTE;\n";
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + TwinCATUintName + " AT %Q*:ARRAY[1.." + UINTPos.ToString() + "] OF UDINT;\n";
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + TwinCATIntName + " AT %Q*:ARRAY[1.." + INTPos.ToString() + "] OF INT;\n";
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + TwinCATUdintName + " AT %Q*:ARRAY[1.." + UDINTPos.ToString() + "] OF UDINT;\n";
                CodeToGenerateBySection[VARGlobal] = CodeToGenerateBySection[VARGlobal] + "\t" + TwinCATBoolName + " AT %Q*:ARRAY[1.." + BOOLPos.ToString() + "] OF BOOL;\n";
                CodeToGenerateBySection[VARGlobalConstant] = CodeToGenerateBySection[VARGlobalConstant] + "\tCoEReadTotalCount : UDINT := " + CoEIndex.Count().ToString() +";\n";



                foreach (string line in CodeToGenerateBySection)
                {
                     CodeToGenerate = CodeToGenerate + "\n" + line + "END_VAR\n";
                }

                    //CodeToGenerateBySection[index] = CodeToGenerateBySection[index] + "END_VAR";

                    try
                {
                    ITcSmTreeItem plcGVLsNameItemSub = systemManager.LookupTreeItem("TIPC^" + projectName + "^" + projectName + " Project^GVLs^" + macro[0].Location);
                    if (plcGVLsNameItemSub != null)
                        plcGVLsItem.DeleteChild(plcGVLsNameItemSub.Name);
                }
                catch { }
                plcGVLsItem.CreateChild(macro[0].Location, TreeItemType.PlcGvl.AsInt32(), "", CodeToGenerate);

            }

        }
        private string GenerateCodeBuildGlobalVarArraywithValues(string varname, string arraysize, string type, List<string> defaultValues)
        {
            string line = varname+" : ARRAY["+arraysize+"] OF "+ type + ":= [";
            foreach (string Index in defaultValues)
            {
                if (type.ToLower() == "string")
                    line = line + "'" + Index.ToUpper() + "',";
                else 
                    line = line + Index + ",";
            }
            line = line.TrimEnd(new Char[] {','});
            line = line + "];\n";
                return line;
        }
        private int FindMatchingIndex(List<string> input,string match)
        {
            int x = 0;
            for(x = 0; x< input.Count;x++)
            {
                if (input[x].StartsWith(match + "\n"))
                    return x;
            }
            return x;
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
        private void ImportEIPDevice(IWorker worker, ITcSmTreeItem devices, List<ReferenceCode.IO_Object> IoList, string EL6652Path)
        {
            //Get the list of links that are extra to be linked to the

            string PlcInstancePathName = GetProjectPathForLinking(worker);
            for (int x = 0; x < CSV_Reader.EipExtraTags.Count; x++)
            {
                
                if (CSV_Reader.EipExtraTags[x].IsARRAY)
                {
                    int endPos = 0;
                    int startPos = 0;
                    bool result = Int32.TryParse(CSV_Reader.EipExtraTags[x].ArrayEnd, out endPos);
                    bool result2 = Int32.TryParse(CSV_Reader.EipExtraTags[x].ArrayStart, out startPos);
                    if (result == false)//must be a tag then.
                    {
                        if (CSV_Reader.EipExtraTags[x].ArrayEnd == "EtherDeviceCount")
                            endPos = GlobalVariables.NumberOfDevicesInEtherCATTree;
                        else
                            endPos = startPos + 1;
                    }
                        for (int i = startPos; i <= endPos; i++)
                        {
                        ReferenceCode.IO_Object NewItem = new ReferenceCode.IO_Object();
                            NewItem.IsOutput = !CSV_Reader.EipExtraTags[x].IsInput;
                            NewItem.IsInput = CSV_Reader.EipExtraTags[x].IsInput;
                            NewItem.TagParentName = "";
                            NewItem.PathName = PlcInstancePathName + CSV_Reader.EipExtraTags[x].LinkPath + "[" + i.ToString() + "]";
                            NewItem.TagName = CSV_Reader.EipExtraTags[x].EIPTagName;// + i.ToString();
                            NewItem.bitsize = GetSizeFromString(CSV_Reader.EipExtraTags[x].DataType);// Convert.ToInt32(EtherCatTerminalNodeSubitems.SelectSingleNode("BitLen").InnerText);
                            NewItem.type = GetTypeFromString(CSV_Reader.EipExtraTags[x].DataType);
                            NewItem.typedetails = "IsManualAdd";
                        if (CSV_Reader.EipExtraTags[x].ArrayEnd == "EtherDeviceCount" && GlobalVariables.IOListNames.Count - 1 >= i)
                        {
                            NewItem.Description = "EtherCAT Device: " + GlobalVariables.IOListNames[i];
                            NewItem.TagName = NewItem.TagName + GlobalVariables.IOListNames[i];
                        }
                        ReferenceCode.AutoRename(worker, NewItem, " ", "_");
                        ReferenceCode.AutoRename(worker, NewItem, "-", "_");
                        IoList.Add(NewItem);
                        }              
                }
                else
                {
                    ReferenceCode.IO_Object NewItem = new ReferenceCode.IO_Object();
                    NewItem.IsOutput = !CSV_Reader.EipExtraTags[x].IsInput;
                    NewItem.IsInput = CSV_Reader.EipExtraTags[x].IsInput;
                    NewItem.PathName = PlcInstancePathName + CSV_Reader.EipExtraTags[x].LinkPath;
                    NewItem.TagParentName = "";
                    NewItem.TagName = CSV_Reader.EipExtraTags[x].EIPTagName;
                    NewItem.bitsize = GetSizeFromString(CSV_Reader.EipExtraTags[x].DataType);// Convert.ToInt32(EtherCatTerminalNodeSubitems.SelectSingleNode("BitLen").InnerText);
                    NewItem.type = GetTypeFromString(CSV_Reader.EipExtraTags[x].DataType);
                    NewItem.typedetails = "IsManualAdd";
                    IoList.Add(NewItem);
                }
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
                    //XmlDocument xmlDoc3 = new XmlDocument();
                    //xmlDoc3.LoadXml(device.ProduceXml(false));
                    if (ScriptingTest.CSV_Reader.EthernetIPTemplate == null)
                        eipSlave = device;
                    else
                    {
                        //testing code remove
                        /*
                        XmlDocument xmlDoc1 = new XmlDocument();
                        ITcSmTreeItem Inputs1;
                        ITcSmTreeItem Outputs1;
                        xmlDoc1.LoadXml(device.Child[1].ProduceXml(false));

                        xmlDoc1.LoadXml(device.Child[1].Child[1].ProduceXml(false));
                        IOAssemblyName = device.Child[1].Child[1].Name;
                        Inputs1 = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName + "^" + EIPSlaveSubName + "^" + IOAssemblyName + "^Inputs");
                        Outputs1 = systemManager.LookupTreeItem("TIID^" + DeviceEIPSlaveName + "^" + EIPSlaveSubName + "^" + IOAssemblyName + "^Outputs");
                        for (int i = Inputs1.ChildCount; 1 < i; i--)
                        {
                            XmlDocument xmlDoc2 = new XmlDocument();
                            xmlDoc2.LoadXml(Inputs1.Child[i].ProduceXml());
                        }
                        */
                        //end testing

                        devices.DeleteChild(device.Name);
                    
                    
                    }
                    break;
                }
            }
           
            if(eipSlave == null)
                eipSlave = devices.ImportChild(ScriptingTest.CSV_Reader.EthernetIPTemplate, null, false);
            DeviceEIPSlaveName = eipSlave.Name;


            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(eipSlave.ProduceXml(false));


            //XmlNode EtherCATID = xmlDoc["TreeItem"]["DeviceDef"]["AddressInfo"]["Ecat"]["EtherCATDeviceId"];
            //XmlNode EtherCATDeviceNameID = xmlDoc["TreeItem"]["DeviceDef"]["AddressInfo"]["Ecat"]["EtherCATDeviceName"];

            //EtherCATID.InnerText = 3.ToString();
            //EtherCATDeviceNameID.InnerText = EL6652Path;

            eipSlave.ConsumeXml("<TreeItem><DeviceDef><AddressInfo><Ecat><EtherCATDeviceId>" + "3" + "</EtherCATDeviceId></Ecat></AddressInfo></DeviceDef></TreeItem>");
            eipSlave.ConsumeXml("<TreeItem><DeviceDef><AddressInfo><Ecat><EtherCATDeviceName>" + EL6652Path + "</EtherCATDeviceName></Ecat></AddressInfo></DeviceDef></TreeItem>");

            //xmlDoc.LoadXml(xmlItem.OuterXml);
            //eipSlave.ConsumeXml(xmlDoc.OuterXml);

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
            int TotalInputs = 0;
            int TotalOutputs = 0;

            int bitTracker = 0;
            int InputsbitCounter = 4*8;
            int OutputsbitCounter = 4*8;
            int bufferCounter = 0;
            int bufferInputCount = 0;
            int bufferOutputCount = 0;
            int tagtrimcounter = 0;



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

                    //renames the tagname
                    ReferenceCode.AutoRename(worker, IoList[i], ScriptingTest.CSV_Reader.AutoRenameInEIPList);

                    string tagName;
                    if (IoList[i].TagParentName == "")
                    {
                        tagName = IoList[i].TagName;
                    }
                    else
                    {
                        tagName = /*IoList[i].TagParentName + "_" + */IoList[i].TagName;
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
                                    InputsbitCounter++;

                                }
                                else
                                {
                                    bufferOutputCount++;
                                    item = Inputs.CreateChild("Buffer_" + bufferCounter.ToString(), 1, "", 1);

                                    OutputsbitCounter++;
                                }
                            }
                            //bitTracker--;
                        }
                    }
                    //bitTracker = bitTracker + IoList[i].bitsize;
                    if (tagName.Length > 40)
                    {
                        int lenghtToBeRemoved = tagName.Length - 40;
                        worker.ProgressStatus = "TagName exceeds maximum length: " + tagName + " Remove " + lenghtToBeRemoved.ToString() + " Characters so RS logics 5000 can import correctly";
                        tagName = tagName.Remove(40);
                        tagtrimcounter++;
                    }
                    if (tagName.Length > 39 && tagName.Substring(39,1) == "_")
                    {
                        worker.ProgressStatus = "TagName ends with a _. This is not allowed: " + tagName;
                        tagName = tagName.Remove(39);
                        tagName = tagName.Insert(39, "1");// = "1";
                        
                    }

                    for(int p = 0; p< CSV_Reader.AddDescription.Count; p++)
                    {
                        AddDescriptions description = CSV_Reader.AddDescription[p];
                        if (description.Device.ToLower() == "ethercat")
                        {
                            bool matches = true;
                            for (int z = 0; z < description.SearchName.Length; z++)
                            {
                                if (!IoList[i].PathName.ToLower().Contains(description.SearchName[z].ToLower()))
                                {
                                    matches = false;
                                    break;
                                }
                            }
                            if (matches)
                            {
                                IoList[i].Description = description.Description;
                                //CSV_Reader.AddDescription.RemoveAt(p);
                                break;
                            }
                        }
                        if (description.Device.ToLower() == "eip" || description.Device.ToLower() == "ethernetip")
                        {
                            bool matches = true;
                            for (int z = 0; z < description.SearchName.Length; z++)
                            {
                                if (!tagName.ToLower().Contains(description.SearchName[z].ToLower()))
                                {
                                    matches = false;
                                    break;
                                }
                                else
                                {
                                    if (description.SearchName[z].ToLower() == "InDev")
                                        matches = true;// don't need this, just so I can breakpoint.
                                }//so I can put a breakpoint here.
                            }
                            if (matches)
                            {
                                IoList[i].Description = description.Description;
                                //CSV_Reader.AddDescription.RemoveAt(p);
                                break;
                            }
                        }
                    }


                    if (IoList[i].IsInput)
                    {
                        //Outputs is a Tree item
                        //Maximum tag length = MDR100_InDev_Chl_1_short_circuit_MDR100_
                        //40 chars
                        TotalInputs++;
                        item = Outputs.CreateChild(tagName, IoList[i].type, "", 1); // This adds the Output to the EIP network PDO
                        xmlDoc.LoadXml(item.ProduceXml(false));
                        
                        bufferInputCount = bufferInputCount + Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);
                        int bitsize = Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);
                        InputsbitCounter = InputsbitCounter + bitsize;// IoList[i].bitsize;
                    }
                    else
                    {
                        TotalOutputs++;
                        item = Inputs.CreateChild(tagName, IoList[i].type, "", 1);

                        xmlDoc.LoadXml(item.ProduceXml(false));
                        int bitsize = Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);
                        bufferOutputCount = bufferOutputCount + Convert.ToInt32(xmlDoc.SelectSingleNode("TreeItem/VarDef/VarBitSize").InnerText);
                        OutputsbitCounter = OutputsbitCounter + bitsize;//IoList[i].bitsize;
                    }

                    

                    if (IoList[i].Description != null && IoList[i].Description != "")
                    {
                        if (IoList[i].Description.Length > 420)
                        {
                            //int lenghtToBeRemoved = IoList[i].Description.Length - 420;
                            //worker.ProgressStatus = "TagName exceeds maximum length: " + tagName + " Remove " + lenghtToBeRemoved.ToString() + " Characters so RS logics 5000 can import correctly";
                            IoList[i].Description = IoList[i].Description.Remove(420);

                        }
                        XmlDocument xmlItem = new XmlDocument();
                        xmlItem.LoadXml(item.ProduceXml());
                        XmlNode xmlTargetRef = xmlItem["TreeItem"]["ItemName"];
                        XmlNode xmlTarget = xmlItem["TreeItem"];
                        XmlElement Comment1 = xmlItem.CreateElement("Comment");
                        Comment1.InnerXml = "<![CDATA[" + IoList[i].Description + "]]>";
                        //Comment1.InnerXml = "<![CDATA[Hello]]>"; //IoList[i].Description; //"![CDATA[" +  IoList[i].Description + "]";
                        //xmlTarget.AppendChild(Comment1);
                        xmlTarget.InsertAfter(Comment1, xmlTargetRef);
                        xmlDoc.LoadXml(xmlItem.OuterXml);
                        item.ConsumeXml(xmlDoc.OuterXml);
                    }

                    try
                    {
                        if(IoList[i].PathName != "")
                            systemManager.LinkVariables(IoList[i].PathName, item.PathName);
                        if (char.IsNumber(item.Name.ToCharArray()[0]))
                            item.Name = "_"+ item.Name;
                        //ReferenceCode.AutoRename(worker, item, ScriptingTest.CSV_Reader.AutoRenameInEIPList);
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
                InputsbitCounter++;
                bufferCounter++;
                item = Outputs.CreateChild("Buffer_" + bufferCounter.ToString(), 1, "", 1);
            }
            while (bufferOutputCount % 16 != 0)
            {
                bufferOutputCount++;
                bufferCounter++;
                OutputsbitCounter++;
                item = Inputs.CreateChild("Buffer_" + bufferCounter.ToString(), 1, "", 1);

            }
            if (true)
            {
                var randomclass = new Random();
                int random = randomclass.Next();

                string comment =  "Total Inputs: " + TotalInputs.ToString() + " / Size In Bytes: "+ ((InputsbitCounter+16)/8).ToString() + "\n" + "Total Outputs: " + TotalOutputs.ToString() + " / Size In Bytes: " + ((OutputsbitCounter) / 8).ToString() + "\n" + "The following checks to make sure the correct L5X file is loaded. The following number is unque:\n" + random.ToString() ;
               
                item = Outputs.CreateChild("CheckSum" + random.ToString(), 4, "", 1);
                XmlDocument xmlItem = new XmlDocument();
                xmlItem.LoadXml(item.ProduceXml());
                XmlNode xmlTargetRef = xmlItem["TreeItem"]["ItemName"];
                XmlNode xmlTarget = xmlItem["TreeItem"];
                XmlElement Comment1 = xmlItem.CreateElement("Comment");
                Comment1.InnerXml = "<![CDATA[" + comment + "]]>";
                //Comment1.InnerXml = "<![CDATA[Hello]]>"; //IoList[i].Description; //"![CDATA[" +  IoList[i].Description + "]";
                //xmlTarget.AppendChild(Comment1);
                xmlTarget.InsertAfter(Comment1, xmlTargetRef);
                xmlDoc.LoadXml(xmlItem.OuterXml);
                item.ConsumeXml(xmlDoc.OuterXml);
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
            }
            return null;
        }
        private ITcSmTreeItem FindTreeItem(IWorker worker, ITcSmTreeItem Devices, string Name)
        {
            for (int i = 1; i <= Devices.ChildCount; i++)
            {
                ITcSmTreeItem EtherCATDevice = Devices.Child[i];
                //XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
                //EtherCATChilds.LoadXml(Devices.Child[i].ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
                //XmlNode type = EtherCATChilds.SelectSingleNode("TreeItem/ItemSubTypeName"); //Gets the I/O Type. Example EP1111 or EK1100
                if (Name == EtherCATDevice.Name)
                    return EtherCATDevice;
            }
            return null;
        }
        //devices
        private string GetSingleIoObjectPath(IWorker worker, ITcSmTreeItem EtherCATMaster, string Type)
        {
            for (int i = 1; i <= EtherCATMaster.ChildCount; i++)
            {
                try
                {
                    ITcSmTreeItem EtherCATDevice = EtherCATMaster.Child[i];
                    XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
                    EtherCATChilds.LoadXml(EtherCATMaster.Child[i].ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
                    XmlNode type = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/ProductRevision"); //Gets the I/O Type. Example EP1111 or EK1100
                    if (type == null)
                        continue;
                    string[] splittype = type.InnerText.Split('-');
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
                }
                catch (Exception ex)
                {
                    worker.ProgressStatus = "Error searching IO tree for EL6652 " + ex.Message.ToString();
                }
            }
            return null;
        }
        private List<string> GetEtherCatAddressOfType(IWorker worker, ITcSmTreeItem EtherCATMaster, string IOType)
        {
            List<string> found = new List<string>();
            for (int i = 1; i <= EtherCATMaster.ChildCount; i++)
            {
                try
                {
                    ITcSmTreeItem EtherCATDevice = EtherCATMaster.Child[i];
                    XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
                    EtherCATChilds.LoadXml(EtherCATMaster.Child[i].ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
                    XmlNode type = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/ProductRevision"); //Gets the I/O Type. Example EP1111 or EK1100
                    XmlNode ItemSubTypeName = EtherCATChilds.SelectSingleNode("TreeItem/ItemSubTypeName"); //Gets the I/O Type. Example EP1111 or EK1100 
                    XmlNode Address = EtherCATChilds.SelectSingleNode("TreeItem/BoxDef/FieldbusAddress"); //Gets the I/O Type. Example EP1111 or EK1100

                    string[] splittype;// = type.InnerText.Split('-');
                    string[] splitInputType = IOType.Split('-');
                    if (type == null)
                    {
                        if (ItemSubTypeName == null)
                        {
                            continue;
                        }
                        else
                        {
                            splittype = ItemSubTypeName.InnerText.Split(' ')[0].Split('-');
                        }
                    }
                    else
                    {
                        splittype = type.InnerText.Split('-');
                    }
                    
                    bool IsMatch = false;
                    for (int x = 0; x < splitInputType.Length; x++)
                    {
                        if (splitInputType[x].ToLower() == splittype[x].ToLower())
                            IsMatch = true;
                        else
                        {
                            IsMatch = false;
                            break;
                        }
                    }

                    if (IsMatch)
                         found.Add(Address.InnerText);

                }
                catch (Exception ex)
                {
                    worker.ProgressStatus = "Error finding etherCAT address for specific IO types " + ex.Message.ToString();
                }
            }
            return found;
        }
        private void GetIOList(IWorker worker, ITcSmTreeItem EtherCATMaster, List<ReferenceCode.IO_Object> IoList)
        {
            GlobalVariables.IOListNames = new List<string>();
            //GlobalVariables.IOListNames = new List<string>();
            for (int i = 1; i <= EtherCATMaster.ChildCount; i++)
            {

                ReferenceCode.GetIOObject(worker, EtherCATMaster, IoList, i);

            }
        }
        private int GetIOCount(IWorker worker, ITcSmTreeItem EtherCATMaster)
        {
            int totalcount = 0;
            for (int i = 2; i <= EtherCATMaster.ChildCount; i++)
            {
                totalcount++;

            }
            return totalcount;
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
            if (type == "BIT" || type == "BOOL")
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


        

        /*
        private void GetIOObjectOld(IWorker worker, ITcSmTreeItem EtherCATMaster, List<ReferenceCode.IO_Object> IoList, int i)
        {
            //int i = 1;
            List<ReferenceCode.IO_Object> FullListOfIOObjects = new List<ReferenceCode.IO_Object>();
            ITcSmTreeItem EtherCATDevice = EtherCATMaster.Child[i];
            GlobalVariables.IOListNames.Add(EtherCATDevice.Name);

            XmlDocument EtherCATChilds = new XmlDocument(); //Convert the EtherCAT master childs into XML documents for parsing
            EtherCATChilds.LoadXml(EtherCATDevice.ProduceXml(false));//Convert the EtherCAT master childs into XML documents for parsing 
            //Find if device is disabled
            if (EtherCATChilds.SelectNodes("TreeItem/Disabled") != null && !CSV_Reader.DoNotIgnoreDisabledEtherCATDevices)
            {
                if (EtherCATChilds.SelectNodes("TreeItem/Disabled").Count > 0)
                    if (EtherCATChilds.SelectNodes("TreeItem/Disabled").Item(0).InnerText == "true")
                        return;
            }
            if (EtherCATDevice.ItemSubTypeName == "EL6652-0010 EtherNet/IP Adapter (Slave)" || CSV_Reader.IgnoreList.Exists(a => a.Type == EtherCATDevice.ItemSubTypeName))
                return;
            if (CSV_Reader.IgnoreList.Exists(a => a.Name == EtherCATDevice.Name))
                return;

            foreach (ITcSmTreeItem SubItem in EtherCATDevice) //This foreach is just to strip the first object out
            {
                GetIOObjectRecursive(worker, SubItem, FullListOfIOObjects);
            }
            if (CSV_Reader.AddAllWcStatesToEIP)
            {

                ReferenceCode.IO_Object WcStateOjbect = FullListOfIOObjects.Find(x => x.TagName == "WcState");
                
                if (WcStateOjbect != null)
                {
                    string[] split1 = WcStateOjbect.PathName.Split('^');
                    string[] split2 = EtherCATDevice.PathName.Split('^');
                    if (split1.Length == split2.Length +2)
                    {
                        ReferenceCode.IO_Object NewItem = new ReferenceCode.IO_Object();
                        NewItem.IsOutput = false;
                        NewItem.IsInput = true;
                        NewItem.PathName = WcStateOjbect.PathName;
                        NewItem.TagName = WcStateOjbect.TagName;
                        NewItem.Description = WcStateOjbect.Description;
                        //NewItem.TagName = NewItem.TagName.Replace(" ", "_");
                        NewItem.TagParentName = EtherCATMaster.Child[i].Name.ToString();
                        NewItem.bitsize = WcStateOjbect.bitsize;
                        NewItem.type = WcStateOjbect.type;
                        NewItem.typedetails = WcStateOjbect.typedetails;
                        IoList.Add(NewItem);
                    }
                }
            }
            if (CSV_Reader.AddAllStatesToEIP)
            {
                ReferenceCode.IO_Object WcStateOjbect = FullListOfIOObjects.Find(x => x.TagName == "State");
                if (WcStateOjbect != null)
                {
                    string[] split1 = WcStateOjbect.PathName.Split('^');
                    string[] split2 = EtherCATDevice.PathName.Split('^');
                    if (split1.Length == split2.Length + 2)
                    {
                        ReferenceCode.IO_Object NewItem = new ReferenceCode.IO_Object();
                        NewItem.IsOutput = false;
                        NewItem.IsInput = true;
                        NewItem.PathName = WcStateOjbect.PathName;
                        NewItem.TagName = WcStateOjbect.TagName;
                        NewItem.Description = WcStateOjbect.Description;
                        //NewItem.TagName = NewItem.TagName.Replace(" ", "_");
                        NewItem.TagParentName = EtherCATMaster.Child[i].Name.ToString();
                        NewItem.bitsize = WcStateOjbect.bitsize;
                        NewItem.type = WcStateOjbect.type;
                        NewItem.typedetails = WcStateOjbect.typedetails;
                        IoList.Add(NewItem);
                    }
                }
            }

          
            if (EtherCATChilds.SelectNodes("TreeItem/EtherCAT/Slave/ProcessData/TxPdo") == null)
                return;

            XmlNodeList EtherCatTerminal = EtherCATChilds.SelectNodes("TreeItem/EtherCAT/Slave/ProcessData/TxPdo"); // return with the specific tree item of found devices.       
            //XmlNode TypeOld = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/Type"); //Gets the I/O Type. Example EP1111 or EK1100
            //XmlNode Type = EtherCATChilds.SelectSingleNode("TreeItem/EtherCAT/Slave/Info/ProductRevision"); //Gets the I/O Type. Example EP1111 or EK1100
            

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
                                string LinkNamePart1 = EtherCATMaster.Child[i].PathName.ToString();
                                string LinkNamePart2 = EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString();

                                string LinkNamePart3 = EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString();

                                //if (LinkNamePart3.Contains("_"))
                                //{
                                string IndexGroup = EtherCatTerminalNodeSubitems.SelectSingleNode("AdsInfo/IndexGroup").InnerText;
                                string IndexOffset = EtherCatTerminalNodeSubitems.SelectSingleNode("AdsInfo/IndexOffset").InnerText;

                                ReferenceCode.IO_Object match = FullListOfIOObjects.Find(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset);
                                List<ReferenceCode.IO_Object> matchall = FullListOfIOObjects.FindAll(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset);
                                if (matchall.Count > 1) // This was added because some XML files must be screwed up and indexgroup and infexoffset matched on multiple PDO's
                                {
                                    match = matchall.Find(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset && x.TagParentName == LinkNamePart2);
                                }
                                LinkNamePart3 = match.TagName;
                                //LinkNamePart3.Replace('_', '^');
                                LinkName = match.PathName;

                                // }
                                //else
                                //    LinkName = EtherCATMaster.Child[i].PathName.ToString() + "^" + EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "^" + EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString();
                                ReferenceCode.IO_Object NewItem = new ReferenceCode.IO_Object();
                                NewItem.IsOutput = false;
                                NewItem.IsInput = true;
                                NewItem.PathName = LinkName;
                                NewItem.TagName = EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "_" + LinkNamePart3;
                                NewItem.Description = match.Description;
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

                                    ReferenceCode.IO_Object match = FullListOfIOObjects.Find(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset);
                                    List<ReferenceCode.IO_Object> matchall = FullListOfIOObjects.FindAll(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset);
                                    if(matchall.Count > 1 ) // This was added because some XML files must be screwed up and indexgroup and infexoffset matched on multiple PDO's
                                    {
                                        match = matchall.Find(x => x.IndexGroup == IndexGroup && x.IndexOffset == IndexOffset && x.TagParentName == LinkNamePart2);                                      
                                    }
                                   
                                    LinkNamePart3 = match.TagName;
                                    //LinkNamePart3.Replace('_', '^');
                                    LinkName = match.PathName;
                                    //}
                                    //else
                                    //    LinkName = EtherCATMaster.Child[i].PathName.ToString() + "^" + EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "^" + EtherCatTerminalNodeSubitems.SelectSingleNode("Name").InnerText.ToString();
                                    ReferenceCode.IO_Object NewItem = new ReferenceCode.IO_Object();
                                    NewItem.IsInput = false;
                                    NewItem.IsOutput = true;
                                    NewItem.PathName = LinkName;
                                    NewItem.Description = match.Description;
                                    NewItem.TagName = EtherCatTerminalNodes.SelectSingleNode("Name").InnerText.ToString() + "_" + LinkNamePart3;
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
            }
        }
*/
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
