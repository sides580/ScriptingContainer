using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ScriptingTest;

namespace ScriptingTest
{
    /// <summary>
    /// bla
    /// </summary>
    public class StartupListObject
    {
        /// <summary>
        /// bla
        /// </summary>
        public string NetworkPosition;
        /// <summary>
        /// bla
        /// </summary>
        public string Channel;
        /// <summary>
        /// bla
        /// </summary>
        public string Comment;
        /// <summary>
        /// bla
        /// </summary>
        public string Timeout;
        /// <summary>
        /// bla
        /// </summary>
        public string Ccs;
        /// <summary>
        /// bla
        /// </summary>
        public string Index;
        /// <summary>
        /// bla
        /// </summary>
        public string SubIndex;
        /// <summary>
        /// bla
        /// </summary>
        public string Data;
        /// <summary>
        /// bla
        /// </summary>
        public string Size;
        /// <summary>
        /// bla
        /// </summary>
        public StartupListObject(string networkposition, string channel, string comment, string timeout, string ccs, string index, string subindex, string data, string size)
        {
            Comment = comment; Timeout = timeout; Ccs = ccs; Index = index; SubIndex = subindex; Data = data; NetworkPosition = networkposition; Channel = channel; ; Size = size;
        }


    }
    /// <summary>
    /// bla
    /// </summary>
    public class MotorSetting
    {
        /// <summary>
        /// bla
        /// </summary>
        public string Channel;
        /// <summary>
        /// bla
        /// </summary>
        public string Comment;
        /// <summary>
        /// bla
        /// </summary>
        public string Index;
        /// <summary>
        /// bla
        /// </summary>
        public string SubIndex;
        /// <summary>
        /// bla
        /// </summary>
        public string Data;
        /// <summary>
        /// bla
        /// </summary>
        public string Size;
        /// <summary>
        /// bla
        /// </summary>
        public MotorSetting(string channel, string index, string subindex, string data, string size)
        {
            Index = index; SubIndex = subindex; Data = data; Channel = channel; ; Size = size;
        }
    }
    /// <summary>
    /// bla
    /// </summary>
    public class MotorStartupList
    {
        /// <summary>
        /// bla
        /// </summary>
        public MotorStartupDataBase MotorSettings;
        /// <summary>
        /// bla
        /// </summary>
        public string MotorTargetName;
        /// <summary>
        /// bla
        /// </summary>
        public string MotorChannel;
        /// <summary>
        /// bla
        /// </summary>
        public string MotorTargetPosition;
        /// <summary>
        /// bla
        /// </summary>
        public string MotorType;
    }
    /// <summary>
    /// bla
    /// </summary>
    public class MotorStartupDataBase
    {
        /// <summary>
        /// bla
        /// </summary>
        public List<MotorSetting> MotorSettings;
        /// <summary>
        /// bla
        /// </summary>
        public string MotorType;
    }
    /// <summary>
    /// bla
    /// </summary>
    public class EipExtraTagDetails
    {
        /// <summary>
        /// bla
        /// </summary>
        public string EIPTagName;
        /// <summary>
        /// bla
        /// </summary>
        public string LinkPath;
        /// <summary>
        /// bla
        /// </summary>
        public string DataType;
        /// <summary>
        /// bla
        /// </summary>
        public bool IsInput;
        /// <summary>
        /// bla
        /// </summary>
        public bool IsARRAY;
        /// <summary>
        /// bla
        /// </summary>
        public string ArrayStart;
        /// <summary>
        /// bla
        /// </summary>
        public string ArrayEnd;
        /// <summary>
        /// bla
        /// </summary>
        public bool UseFirstPlcInstancePathName;
        /// <summary>
        /// bla
        /// </summary>
        public EipExtraTagDetails(string eIPTagName, string pLCLinkName, string dataType, bool isInput, bool useFirstPlcInstancePathName)
        {
            EIPTagName = eIPTagName; LinkPath = pLCLinkName; DataType = dataType; IsInput = isInput; UseFirstPlcInstancePathName = useFirstPlcInstancePathName;
        }
    }
    /// <summary>
    /// for adding links from the settings file
    /// </summary>
    public class ManualLinks
    {
        /// <summary>
        /// bla
        /// </summary>
        public string Link1;
        /// <summary>
        /// bla
        /// </summary>
        public string Type1;
        /// <summary>
        /// bla
        /// </summary>
        public string Link2;
        /// <summary>
        /// bla
        /// </summary>
        public string Type2;
        /// <summary>
        /// bla
        /// </summary>
        public ManualLinks(string link1, string type1, string link2, string type2)
        {
            Link1 = link1; Type1 = type1; Link2 = link2; Type2 = type2;
        }
    }
    /// <summary>
    /// for adding links from the settings file
    /// </summary>
    public class CoEAutoRead
    {
        /// <summary>
        /// bla
        /// </summary>
        public string CoEIndex;
        /// <summary>
        /// bla
        /// </summary>
        public string CoESubIndex;
        /// <summary>
        /// bla
        /// </summary>
        public string EthSlave;// ether a name, address or type.
        /// <summary>
        /// bla
        /// </summary>
        public string EIP_Var_Name;
        /// <summary>
        /// bla
        /// </summary>
        public string PLC_Var_Name;
        /// <summary>
        /// bla
        /// </summary>
        public string CoEDataType;
        /// <summary>
        /// bla
        /// </summary>
        public string EthSlaveFindByMethod;//string will be ether Address or Name. Could add "Type" in the future
        /// <summary>
        /// bla
        /// </summary>
        public string VariableInsideTwinCATLinkName;//Is populated when parsing through all these at end.
        /// <summary>
        /// bla
        /// </summary>
        public CoEAutoRead()
        {
            ;
        }
    }
    /// <summary>
    /// for adding links from the settings file
    /// </summary>
    public class IngnoreObject
    {
        /// <summary>
        /// bla
        /// </summary>
        public string Name;
        /// <summary>
        /// bla
        /// </summary>
        public string Type;

        /// <summary>
        /// bla
        /// </summary>
        public IngnoreObject(string name, string type)
        {
            Name = name; Type = type; 
        }
    }
    /// <summary>
    /// for adding links from the settings file
    /// </summary>
    public class AddDescriptions
    {
        /// <summary>
        /// bla
        /// </summary>
        public string Description;
        /// <summary>
        /// bla
        /// </summary>
        public string Device;
        /// <summary>
        /// bla
        /// </summary>
        public string[] SearchName;
        /// <summary>
        /// bla
        /// </summary>
        public string Target;
        /// <summary>
        /// bla
        /// </summary>
        public AddDescriptions(string device, string[] searchName, string target, string description)
        {
            Description = description; Device = device; SearchName = searchName; Target = target;
        }
    }
    /// <summary>
    /// for adding links from the settings file
    /// </summary>
    public class AutoAddCode
    {
        /// <summary>
        /// bla
        /// </summary>
        public string Option;
        /// <summary>
        /// bla
        /// </summary>
        public string Type;
        /// <summary>
        /// bla
        /// </summary>
        public string Name;
        /// <summary>
        /// bla
        /// </summary>
        public string Location;
        /// <summary>
        /// bla
        /// </summary>
        public AutoAddCode(string name, string type, string location, string option)
        {
            Name = name; Type = type; Location = location; Option = option;
        }

    }
    /// <summary>
    /// for adding links from the settings file
    /// </summary>
    public class ImportPOU
    {
        /// <summary>
        /// bla
        /// </summary>
        public string Source;
        /// <summary>
        /// bla
        /// </summary>
        public string[] Option;
        /// <summary>
        /// bla
        /// </summary>
        public string Name;
        /// <summary>
        /// bla
        /// </summary>
        public string Location;
        /// <summary>
        /// bla
        /// </summary>
        public ImportPOU(string source, string[] option)
        {
            Source = source; Option = option;
        }

    }
    /// <summary>
    /// Actual status of the Script
    /// </summary>
    static public class CSV_Reader
    {
        /// <summary>
        /// bla
        /// </summary>
        static public List<MotorStartupList> MotorSettingsList;

        /// <summary>
        /// bla
        /// </summary>
        static public List<CoEAutoRead> CoEAutoReadToEIP;
        /// <summary>
        /// bla
        /// </summary>
        static public List<MotorStartupDataBase> MotorDataBase;
        
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public List<AutoAddCode> AutoAddVariables;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public List<IngnoreObject> IgnoreList;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public List<AddDescriptions> AddDescription;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public bool DoNotIgnoreDisabledEtherCATDevices;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public bool AddAllWcStatesToEIP;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public bool WcStatesPackIntoBits;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public bool AddAllStatesToEIP;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public bool PackBitsIntoBytes;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public bool AutoBackupProject;
        /// <summary>
        /// Project Path if already exist
        /// </summary>
        static public string ProjectPath;
        /// <summary>
        /// EthernetIP adaptor template. Must have this
        /// </summary>
        static public string EthernetIPTemplate;
        /// <summary>
        /// auto
        /// </summary>
        static public List<ImportPOU> ImportPOU_List;// = new List<string[]>();
        /// <summary>
        /// auto
        /// </summary>
        static public List<string> AddLibrary_List;// = new List<string[]>();
        /// <summary>
        /// auto
        /// </summary>
        static public List<string[]> AutoRenameInEtherCATList;// = new List<string[]>();
        /// <summary>
        /// auto
        /// </summary>
        static public List<string[]> AutoRenameInEIPList;// = new List<string[]>();
        /// <summary>
        /// auto
        /// </summary>
        static public List<StartupListObject> AutoEtherCATSettings;// = new List<StartupListObject>();
        /// <summary>
        /// List the contains tags that need added to the EIP and then linked.
        /// </summary>
        static public List<EipExtraTagDetails> EipExtraTags;// = new List<StartupListObject>();
        /// <summary>
        /// List the contains tags that need added to the EIP and then linked.
        /// </summary>
        static public List<string> SettingFiles;// = new List<StartupListObject>();
        /// <summary>
        /// List the contains tags that need added to the EIP and then linked.
        /// </summary>
        static public List<ManualLinks> Links;// = new List<StartupListObject>();

        
        /// <summary>
        /// Actual status of the Script
        /// </summary>
        static public void ReadConfigCSV()
        {
            CoEAutoReadToEIP = new List<CoEAutoRead>();
            MotorDataBase = new List<MotorStartupDataBase>();
            MotorSettingsList = new List<MotorStartupList>();
            IgnoreList = new List<IngnoreObject>();
            DoNotIgnoreDisabledEtherCATDevices = false;
            AddAllStatesToEIP = false;
            PackBitsIntoBytes = false;
            AddAllWcStatesToEIP = false;
            WcStatesPackIntoBits = false;
            AddDescription = new List<AddDescriptions>();
            AddLibrary_List = new List<string>();
            AutoAddVariables = new List<AutoAddCode>();
            ImportPOU_List = new List<ImportPOU>();
            AutoRenameInEtherCATList = new List<string[]>();
            AutoRenameInEIPList = new List<string[]>();
            AutoEtherCATSettings = new List<StartupListObject>();
            EipExtraTags = new List<EipExtraTagDetails>();
            Links = new List<ManualLinks>();
            SettingFiles = new List<string>();
            SettingFiles.Add("Settings.txt");
            List<string> SettingFilesMotor;// = new List<string>();
            string MotorFilesDirectory = System.IO.Directory.GetCurrentDirectory() + @"\MotorFiles";
            int x = 0;
            if (System.IO.Directory.Exists(MotorFilesDirectory))
            {
                
                SettingFilesMotor = Directory.GetFiles(MotorFilesDirectory).ToList();
                while (x < SettingFilesMotor.Count)
                {
                    string FileName = SettingFilesMotor[x];
                    ReadCSV_FileMotorFile(FileName);
                    x++;
                }
            }

            x = 0;
            while (x < SettingFiles.Count)
            {
                string FileName = SettingFiles[x];
                ReadCSV_File(FileName);
                x++;
            }



            //ReadCSV_File("Settings.txt");
            //ReadCSV_File("EtherCAT_StartupList.txt");
            //ReadCSV_File("EIP_Tags.txt");


        }
        /// <summary>
        /// Actual status of the Script
        /// </summary>
        /// 
        static public void ReadCSV_File(string fileName)
        {
            string defaultPLCInstance = "";


            string localFolder = System.IO.Directory.GetCurrentDirectory();
            string filePath = localFolder + @"\" + fileName;//"@\Settings.txt";
            if (System.IO.File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    //List<string> listA = new List<string>();
                    //List<string> listB = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        string firststring = values[0];
                        if (values[0].Length == 0 || firststring.Substring(0, 1) == "/")
                        {
                            continue;
                        }
                        //listA.Add(values[0]);
                        //listB.Add(values[1]);
                        if (values[0].ToLower() == "settingsfile" && values.Count() > 1)
                        {
                            SettingFiles.Add(values[1]);
                        }
                        if (values[0].ToLower() == "TwinCATProjectPath".ToLower())
                        {
                            if (values[1].ToLower() == "UseLocalPath".ToLower())
                            {
                                ProjectPath = localFolder + @"\" + values[2];
                            }
                            else
                                ProjectPath = values[1];
                        }
                        if (values[0].ToLower() == "EthernetIPTemplate".ToLower())
                        {
                            if (values[1].ToLower() == "UseLocalPath".ToLower())
                            {
                                EthernetIPTemplate = localFolder + @"\" + values[2];
                            }
                            else
                                EthernetIPTemplate = values[1];
                        }
                        if (values[0].ToLower() == "BackupProject".ToLower())
                        {
                            if (values[1].ToLower() == "true" || values[1].ToLower() == "enable" || values[1].ToLower() == "yes" || values[1] == "1")
                                AutoBackupProject = true;
                        }
                        if (values[0].ToLower() == "donotignoredisabledethercatitems")
                        {
                            if (values[1].ToLower() == "true" || values[1].ToLower() == "enable" || values[1].ToLower() == "yes" || values[1] == "1")
                                DoNotIgnoreDisabledEtherCATDevices = true;
                        }
                        if (values[0].ToLower() == "importpou")
                        {
                            //bool UseLocal = false;
                            string[] options = null;
                            string Path = "";
                            string[] smartsplit = SplitString(line).ToArray();

                            for (int x = 0; x < smartsplit.Count(); x++)
                            {
                                string column = smartsplit[x];
                                if (column.StartsWith("{"))
                                {
                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    if (subcolumn[0].ToLower() == "option" && subcolumn.Count() > 1)
                                    {
                                        options = subcolumn.Where((source, index) => index != 0).ToArray(); //remove first index
                                        //options = subcolumn[1];
                                    }
                                    if (subcolumn[0].ToLower() == "path" && subcolumn.Count() > 1)
                                    {
                                        Path = subcolumn[1];
                                    }
                                }
                            }

                            if (smartsplit.Contains("UseLocalPath"))
                            {
                                //UseLocal = true;
                                Path = localFolder + @"\" + Path;
                            }

                            ImportPOU_List.Add(new ImportPOU(Path, options));
                        }
                        if (values[0].ToLower() == "CharReplaceInEtherCAT".ToLower() && values.Count() == 3)
                        {
                            string[] replacechars = new string[2];
                            replacechars[0] = values[1];
                            replacechars[1] = values[2];
                            AutoRenameInEtherCATList.Add(replacechars);
                        }
                        if (values[0].ToLower() == "addlibrary" && values.Count() > 1)
                        {
                            AddLibrary_List.Add(values[1]);
                        }
                        if (values[0].ToLower() == "CharReplaceInEIP".ToLower() && values.Count() == 3)
                        {
                            string[] replacechars = new string[2];
                            replacechars[0] = values[1];
                            replacechars[1] = values[2];
                            AutoRenameInEIPList.Add(replacechars);
                        }
                        if (values[0].ToLower() == "diagnostics" && values[1].ToLower() == "addallwcstatestoeip")
                        {
                            AddAllWcStatesToEIP = true;
                            if (values.Count() > 2 && values[2].ToLower() == "PackIntoBits".ToLower())
                            {
                                WcStatesPackIntoBits = true;
                            }
                        }
                        if (values[0].ToLower() == "diagnostics" && values[1].ToLower() == "addallstatestoeip")
                        {
                            AddAllStatesToEIP = true;
                        }
                        if (values[0].ToLower() == "PackBitsIntoBytes".ToLower() && (values[1].ToLower() == "true" || values[1].ToLower() == "1"))
                        {
                            PackBitsIntoBytes = true;
                        }

                        
                        if (values[0] == "EP7402" && values.Count() > 2)
                        {
                            for (int x = 0; x < values.Count(); x++)
                            {
                                string column = values[x];
                                if (column.StartsWith("{"))
                                {
                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    StartupListObject StartupListObject2 = new StartupListObject(values[1], values[2], subcolumn[0], "0", "1", subcolumn[1].Split(':')[0], subcolumn[1].Split(':')[1], subcolumn[2], subcolumn[3]);
                                    AutoEtherCATSettings.Add(StartupListObject2);
                                }
                            }
                        }

                        if (values[0] == "DefaultPLCInstance" && values.Count() > 1)
                        {
                            defaultPLCInstance = values[1];
                        }
                        if ((values[0] == "EIPTagAddOutput" || values[0] == "EIPTagAddInput") && values.Count() > 2)
                        {
                            string Name = "";
                            string Type = "";
                            string Link = "";
                            bool isArray = false;
                            string ArrayStart = "";
                            string ArrayEnd = "";
                            bool UsePlcInstanceName = false;
                            bool isInput = false;// = true;
                            if (values[0] == "EIPTagAddInput")
                                isInput = true;
                            for (int x = 0; x < values.Count(); x++)
                            {
                                string column = values[x];
                                if (column.StartsWith("{"))
                                {

                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    if (subcolumn[0] == "Name" && subcolumn.Count() > 1)
                                    {
                                        Name = subcolumn[1];
                                    }
                                    else if (subcolumn[0] == "Type" && subcolumn.Count() > 1)
                                    {
                                        if (subcolumn.Count() == 2)
                                            Type = subcolumn[1];
                                        else if (subcolumn.Count() > 3)
                                        {
                                            if (subcolumn[1] == "ARRAY")
                                            {
                                                isArray = true;
                                                Type = subcolumn[2];
                                                ArrayStart = subcolumn[3].Split(':')[0];
                                                ArrayEnd = subcolumn[3].Split(':')[1];
                                            }
                                        }
                                    }
                                    else if (subcolumn[0] == "LinkToPlc" && subcolumn.Count() > 1)
                                    {
                                        UsePlcInstanceName = true;
                                        if (isInput == false)
                                            Link = "^PlcTask Inputs^" + subcolumn[1];// defaultPLCInstance + "^PlcTask Inputs^" + subcolumn[1];
                                        else
                                            Link = "^PlcTask Outputs^" + subcolumn[1];
                                    }
                                    else if (subcolumn[0] == "LinkFullAddress" && subcolumn.Count() > 1)
                                    {
                                        Link = subcolumn[1];
                                    }
                                }
                            }

                            EipExtraTags.Add(new EipExtraTagDetails(Name, Link, Type, isInput, UsePlcInstanceName));
                            if (isArray)
                            {
                                EipExtraTags.Last().ArrayEnd = ArrayEnd;
                                EipExtraTags.Last().ArrayStart = ArrayStart;
                                EipExtraTags.Last().IsARRAY = isArray;
                            }
                        }
                        if (values[0].ToLower() == "MotorSettings".ToLower() && values.Count() == 3)
                        {
                            MotorStartupList NewMotor = new MotorStartupList();
                            string TargetName = "";
                            string Channel = "";
                            string MotorType = "";
                            if (values.Count() == 3)
                            {
               
                                MotorType = values[2];
                                string[] split = values[1].Split(':');
                                if (split.Count() == 2)
                                {
                                    TargetName = split[0];
                                    Channel = split[1];
                                }
                            }
                            NewMotor.MotorType = MotorType;
                            NewMotor.MotorTargetName = TargetName;
                            NewMotor.MotorTargetPosition = "";
                            NewMotor.MotorChannel = Channel;
                            MotorStartupDataBase MotorDataBaseIndex = MotorDataBase.FindLast(a => a.MotorType == MotorType);
                            NewMotor.MotorSettings = MotorDataBaseIndex;
                            MotorSettingsList.Add(NewMotor);
                        }
                        if ((values[0].ToLower() == "autogeneratecode") && values.Count() > 3)
                        {
                            string Location = "";
                            string Option = "";
                            string Type = "";
                            string Name = "";
                            for (int x = 0; x < values.Count(); x++)
                            {
                                string column = values[x];
                                if (column.StartsWith("{"))
                                {
                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    if (subcolumn[0] == "Name" && subcolumn.Count() > 1)
                                    {
                                        Name = subcolumn[1];
                                    }
                                    else if (subcolumn[0].ToLower() == "type" && subcolumn.Count() > 1)
                                    {
                                        Type = subcolumn[1];
                                    }
                                    else if (subcolumn[0].ToLower() == "option" && subcolumn.Count() > 1)
                                    {
                                        Option = subcolumn[1];
                                    }
                                    else if (subcolumn[0].ToLower() == "location" && subcolumn.Count() > 1)
                                    {

                                        Location = subcolumn[1];
                                    }

                                }
                            }
                            AutoAddVariables.Add(new AutoAddCode(Name, Type, Location, Option));
                        }
                        if ((values[0].ToLower() == "ignore") && values.Count() > 0)
                        {
                            var values2 = line.Split(new char[] { ',' },2);
                            string Type = "";
                            string Name = "";
                            for (int x = 0; x < values2.Count(); x++)
                            {
                                string column = values2[x];
                                if (column.StartsWith("{"))
                                {
                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    if (subcolumn[0].ToLower() == "name" && subcolumn.Count() > 1)
                                    {
                                        Name = subcolumn[1];
                                    }
                                    else if (subcolumn[0].ToLower() == "type" && subcolumn.Count() > 1)
                                    {
                                        Type = subcolumn[1];
                                    }
                                }
                            }
                            IgnoreList.Add(new IngnoreObject(Name, Type));
                        }
                        if ((values[0].ToLower() == "adddescription") && values.Count() > 3)
                        {
                            string Device = "";
                            string[] SearchName = null; //= new string[1];
                            string Target = "";
                            string Description = "";
                            //AutoAddCode
                            string[] smartsplit = SplitString(line).ToArray();
                            for (int x = 0; x < smartsplit.Count(); x++)
                            {
                                string column = smartsplit[x];
                                if (column.StartsWith("{"))
                                {
                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    if (subcolumn[0].ToLower() == "devicename" && subcolumn.Count() > 0)
                                    {
                                        Device = subcolumn[1];
                                    }
                                    else if (subcolumn[0].ToLower() == "sourcename" && subcolumn.Count() > 0)
                                    {
                                        SearchName = subcolumn.Where((source, index) => index != 0).ToArray(); //remove first index
                                        //SearchName = subcolumn[1];
                                    }
                                    else if (subcolumn[0].ToLower() == "target" && subcolumn.Count() > 0)
                                    {
                                        Target = subcolumn[1];
                                    }
                                    else if (subcolumn[0].ToLower() == "description" && subcolumn.Count() > 0)
                                    {
                                        Description = subcolumn[1];
                                    }

                                }
                            }
                            if (SearchName != null && SearchName.Count() > 0)
                                AddDescription.Add(new AddDescriptions(Device, SearchName, Target, Description));
                        }
                        if ((values[0].ToLower() == "adddescriptiontoeip") && values.Count() > 1)
                        {
                            string Device = "eip";


                            string temp = values[1];
                            temp = temp.TrimStart('{');
                            temp = temp.TrimEnd('}');
                            string[] SearchName = temp.Split(';'); //= new string[1];
                            string Target = "";
                            string Description = values[2];
                            //AutoAddCode

                            if (SearchName != null && SearchName.Count() > 0)
                                AddDescription.Add(new AddDescriptions(Device, SearchName, Target, Description));
                        }
                        if ((values[0].ToLower() == "addlink") && values.Count() > 2)
                        {
                            string Link1 = "";
                            string Type1 = "";
                            string Link2 = "";
                            string Type2 = "";
                            int z = 0;
                            for (int x = 1; x < values.Count(); x++)
                            {
                                string column = values[x];
                                if (column.StartsWith("{"))
                                {
                                    z++;
                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    if (z == 1)
                                    {
                                        Type1 = subcolumn[0];
                                        Link1 = subcolumn[1];
                                    }
                                    if (z == 2)
                                    {
                                        Type2 = subcolumn[0];
                                        Link2 = subcolumn[1];
                                    }
                                }
                            }
                            Links.Add(new ManualLinks(Link1, Type1, Link2, Type2));
                        }
                        if ((values[0].ToLower() == "CoERead_To_EIP".ToLower()) && values.Count() > 2)
                        {
                            CoEAutoRead CoE = new CoEAutoRead();
                            for (int x = 1; x < values.Count(); x++)
                            {
                                string column = values[x];
                                if (column.StartsWith("{"))
                                {
                                    column = column.TrimStart('{');
                                    column = column.TrimEnd('}');

                                    string[] subcolumn = column.Split(';');
                                    if (subcolumn[0].ToLower() == "Index".ToLower())
                                    {
                                        CoE.CoEIndex = subcolumn[1];
                                    }
                                    if (subcolumn[0].ToLower() == "SubIndex".ToLower())
                                    {
                                        CoE.CoESubIndex = subcolumn[1];
                                    }
                                    if (subcolumn[0].ToLower() == "SlaveAddress".ToLower() || subcolumn[0].ToLower() == "DeviceAddress".ToLower())
                                    {
                                        CoE.EthSlave = subcolumn[1];
                                        CoE.EthSlaveFindByMethod = "address";
                                    }
                                    if (subcolumn[0].ToLower() == "SlaveType".ToLower() || subcolumn[0].ToLower() == "DeviceType".ToLower())
                                    {
                                        CoE.EthSlave = subcolumn[1];
                                        CoE.EthSlaveFindByMethod = "type";
                                    }
                                    if (subcolumn[0].ToLower() == "SlaveName".ToLower() || subcolumn[0].ToLower() == "DeviceName".ToLower())
                                    {
                                        CoE.EthSlave = subcolumn[1];
                                        CoE.EthSlaveFindByMethod = "Name";
                                    }
                                    if (subcolumn[0].ToLower() == "VarType".ToLower() || subcolumn[0].ToLower() == "DataType".ToLower())
                                    {
                                        CoE.CoEDataType = subcolumn[1];
                                    }
                                    if (subcolumn[0].ToLower() == "VarName".ToLower() || subcolumn[0].ToLower() == "EIPName".ToLower())
                                    {

                                        CoE.EIP_Var_Name = subcolumn[1];
                                    }
                                }
                           
                            }
                            CoEAutoReadToEIP.Add(CoE);


                        }

                    }
                }
            }
        }
        /// <summary>
        /// /////
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        static public void ReadCSV_FileMotorFile(string fileName)
        {

            string localFolder = System.IO.Directory.GetCurrentDirectory();
            string filePath = fileName;//"@\Settings.txt";
            MotorStartupDataBase MotorDataBaseItem = new MotorStartupDataBase();
            MotorDataBaseItem.MotorSettings = new List<MotorSetting>();
            //MotorDataBase


            if (System.IO.File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                   
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        string firststring = values[0];
                        if (values[0].Length == 0 || firststring.Substring(0, 1) == "/")
                        {
                            continue;
                        }



                        if (values[0].ToLower() == "EUseSimpleMode".ToLower())
                        {
                            if (values[1].ToLower() == "true".ToLower())
                            {
                                ;//EthernetIPTemplate = localFolder + @"\" + values[2];
                            }
                        }
                        if (values[0].ToLower() == "MotorSettingsName".ToLower() && values.Count() >1)
                        {
                            MotorDataBaseItem.MotorType = values[1];
                        }

                        if (values[0].ToLower() == "StartupListItem".ToLower() && values.Count() > 2)
                        {
                            MotorSetting NewSetting;
                            string comment = "";
                            string index ="";
                            string offset = "";
                            string bytesize = "";
                            string value = "";
                            if (values.Count() == 4)
                            {
                            comment = values[1];
                            value = values[3];
                            string[] split = values[2].Split(':');
                                if (split.Count() == 3)
                                {
                                    index = split[0];
                                    offset = split[1];
                                    bytesize = split[2];
                                }
                            }
                            NewSetting = new MotorSetting("1", index, offset, value, bytesize);
                            MotorDataBaseItem.MotorSettings.Add(NewSetting);
                        }
                        
                    }
                    MotorDataBase.Add(MotorDataBaseItem);
                }
            }
        }
        static private List<string> SplitString(string input)//This splitting system keeps "," between {}. Will build on this if I need to
        {
            string splitchar = ",";
            string[] FirstSplit = input.Split(splitchar.ToCharArray()[0]);
            //string joinchar = "{"
            List<string> split2 = new List<string>();
            int startindex = 0;
            for (int i = 0; i < FirstSplit.Count(); i++)
            {
                if(startindex > 0)
                {
                    if (FirstSplit[i].EndsWith("}"))
                    {
                        string concat = "";
                        for (int x = startindex; x<= i; x++)
                        {
                            
                            concat = concat + FirstSplit[x] + splitchar;
                           
                        }
                        splitchar.TrimEnd(splitchar.ToArray()[0]);
                        splitchar.TrimEnd('}');
                        split2.Add(concat);
                    }
                }
                else if (FirstSplit[i].StartsWith("{"))
                {
                    if (FirstSplit[i].EndsWith("}"))
                    {
                        startindex = 0;
                        split2.Add(FirstSplit[i]);
                    }
                    else
                        startindex = i;
                }
                else
                {
                    startindex = 0;
                    split2.Add(FirstSplit[i]);
                }
             }
            return split2;
        }
    }
}
