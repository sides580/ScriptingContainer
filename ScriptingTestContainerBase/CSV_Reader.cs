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
    /// Actual status of the Script
    /// </summary>
    static public class CSV_Reader
    {
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
        static public List<string> ImportPOU_List;// = new List<string[]>();
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
            ImportPOU_List = new List<string>();
            AutoRenameInEtherCATList = new List<string[]>();
            AutoRenameInEIPList = new List<string[]>();
            AutoEtherCATSettings = new List<StartupListObject>();
            EipExtraTags = new List<EipExtraTagDetails>();
            Links = new List<ManualLinks>();
            SettingFiles = new List<string>();
            SettingFiles.Add("Settings.txt");
            int x = 0;
            while(x < SettingFiles.Count)
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

                        //listA.Add(values[0]);
                        //listB.Add(values[1]);
                        if (values[0].ToLower() == "settingsfile" && values.Count() > 1)
                        {
                            SettingFiles.Add(values[1]);
                        }
                        if (values[0] == "TwinCATProjectPath")
                        {
                            if (values[1] == "UseLocalPath")
                            {
                                ProjectPath = localFolder + @"\" + values[2];
                            }
                            else
                                ProjectPath = values[1];
                        }
                        if (values[0] == "EthernetIPTemplate")
                        {
                            if (values[1] == "UseLocalPath")
                            {
                                EthernetIPTemplate = localFolder + @"\" + values[2];
                            }
                            else
                                EthernetIPTemplate = values[1];
                        }
                        if (values[0].ToLower() == "importpou")
                        {
                            if (values[1] == "UseLocalPath")
                            {
                                ImportPOU_List.Add(localFolder + @"\" + values[2]);
                            }
                            else
                                ImportPOU_List.Add(values[1]);
                        }
                        if (values[0] == "CharReplaceInEtherCAT" && values.Count() == 3)
                        {
                            string[] replacechars = new string[2];
                            replacechars[0] = values[1];
                            replacechars[1] = values[2];
                            AutoRenameInEtherCATList.Add(replacechars);
                        }
                        if (values[0] == "CharReplaceInEIP" && values.Count() == 3)
                        {
                            string[] replacechars = new string[2];
                            replacechars[0] = values[1];
                            replacechars[1] = values[2];
                            AutoRenameInEIPList.Add(replacechars);
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
                                        Type = subcolumn[1];
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

                    }
                }
            }
        }
    }
}
