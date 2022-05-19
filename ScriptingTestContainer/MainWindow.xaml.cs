using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using ScriptingTest;
using EnvDTE;
using EnvDTE100;
using EnvDTE80;
using System.IO.Compression;

using Microsoft.Win32;
 

namespace ScriptingTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        /// <summary>
        /// Indicates, that the main window is initializing.
        /// </summary>
        List<OpenVsList> _OpenVsList = new List<OpenVsList>();
        
        /// <summary>
        /// Indicates, that the main window is initializing.
        /// </summary>
        bool _initializing = false;

        /// <summary>
        /// Gets a value Indicating that the <see cref="MainWindow"/> is currently initializing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is initializing]; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitializing
        {
            get { return _initializing; }
        }
        /// <summary>
        /// Customer Selected Path <see cref="MainWindow"/> is currently initializing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is initializing]; otherwise, <c>false</c>.
        /// </value>
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            string localFolder = System.IO.Directory.GetCurrentDirectory();
            //MessageBox.Show(localFolder);
            //var dllDirectory = @"C:/some/path";
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + localFolder);
            _initializing = true;
            //System.Windows.MessageBox.Show("This is Version Beta 1.0");
            InitializeComponent();

            ScriptingTest.GlobalVariables.OpenExisitngVSProject = true;
            UpdateOpenTwinCATProjectMode();

            int defaultIndex = -1;
            int currentIndex = -1;
            //MessageBox.Show("OMG Work?");
            IDictionary<string, DTEInfo> progIdDict;
            try
            {
                progIdDict = ConfigurationFactory.GetVisualStudioProgIds(out currentIndex, out defaultIndex);
                if(progIdDict.Count == 0)
                     System.Windows.MessageBox.Show("Could not find Visual studio installs");
                foreach (DTEInfo key in progIdDict.Values)
                {
                    this.cBProgID.Items.Add(key);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("No visual studio installed? Error:" + ex.Message.ToString());
            }
            int actualIndex = defaultIndex;

            if (actualIndex == -1)
                actualIndex = currentIndex;


            //obsolete this.cBProgID.Items.Add(ConfigurationGenerator.ProgID_XAE);

            this.cBProgID.SelectedIndex = actualIndex;

            this.cbProgVisible.IsChecked = true;
            this.cbUserControl.IsChecked = true;
            this.cbSuppressUI.IsChecked = true;

            _initializing = false;

            // Setting Script filter (Filter Out all Exes)

            ScriptLoader.ScriptFilter = new Func<Type, bool>(

                (type) =>
                {
                    return (type.Assembly.EntryPoint == null);
                }
            );

            setScripts(ScriptLoader.Scripts); // Setting the Scripts to the ListView
            enableDisableControls(); // EnableDisable Visual Elements
            CSV_Reader.ReadConfigCSV(); //Loads settings for customer to have some ability to easly adjust machine
            txtSelectedProject.Text = ScriptingTest.CSV_Reader.ProjectPath;
            loadOpenVisualStudio();

            ///// Set open project from folder as defaults
            ScriptingTest.GlobalVariables.OpenExisitngVSProject = false;
            rbOpenTwinCATProject.IsChecked = true;
            rbUseActiveTwinCATProject.IsChecked = false;
            UpdateOpenTwinCATProjectMode();
            ///// Set open project from folder as defaults
        }

        /// <summary>
        /// Sets the scripts within the ListView
        /// </summary>
        /// <param name="scripts">The scripts.</param>
        private void setScripts(IList<Script> scripts)
        {
            //this.lVScripts.DataContext = scripts;
            this.lVScripts.ItemsSource = scripts;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lVScripts.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("ScriptName", ListSortDirection.Ascending));
           
            lVScripts.SelectedItem = scripts.Last();
        }
        /// <summary>
        /// Sets the scripts within the ListView
        /// </summary>
        private void loadOpenVisualStudio()
        {
           // using EnvDTE


            //The Solution must be saved one time, otherwise the SolutionPath is not stored properly
            //within the DTE Object.
            //string fullName1 = this.dte.Solution.FullName;

            //Access COM Running Objects Table
            Dictionary<string, List<object>> rot = ROTAccess.GetRunningObjectTable();
            Dictionary<ROTDteInfo, DTE> dteTable = ROTAccess.GetRunningDTETable();
            _OpenVsList.Clear();

            
            //List<OpenVsList> test = new List<OpenVsList>();
            //for(int x= 0; x< rot.Count;x++)
            //foreach (Dictionary<string, List<object>> rotTableItem in rot)
            for (int x = 0; x < rot.Count; x++)
            {
                //string Trim1 = rot.ElementAt(x).Keys.ToString().TrimStart('{').TrimStart('[');
                //string Trim2 = rot.ElementAt(x).Key.ToString().TrimEnd('}').TrimStart(']');
                string[] Trim1 = rot.ElementAt(x).Key.ToString().Split('.');
                string[] Trim2 = Trim1[0].Split('\\');
                if (Trim1.Last() == "sln")
                {
                    _OpenVsList.Add(new OpenVsList());
                    _OpenVsList.Last().Name = Trim2.Last();
                    _OpenVsList.Last().FolderLocation = rot.ElementAt(x).Key;
                    dynamic Converter = rot.ElementAt(x).Value[0];
                    _OpenVsList.Last().dTE = (DTE)Converter.DTE;
                }
            }
            lVActiveVS.ItemsSource = _OpenVsList;
            lVActiveVS.Items.Refresh();
            /*
            foreach (KeyValuePair<ROTDteInfo, DTE> dteTableItem in dteTable)
            {
                _OpenVsList.Add(new OpenVsList());
                _OpenVsList.Last().Name = dteTableItem.Key.SolutionName;
                _OpenVsList.Last().FolderLocation = dteTableItem.Key.SolutionPath;
                _OpenVsList.Last().dTE = dteTableItem.Value;

            }
            lVActiveVS.ItemsSource = _OpenVsList;
            lVActiveVS.Items.Refresh();
            ;
            */
           // DTE dte = ROTAccess.GetActiveDTE(this.ScriptName); // Getting DTE of the currently Script-Opened project

            //Debug.Assert(dte == (DTE)this._context.DTE); // Should be the same Object!
        }

        /// <summary>
        /// Gets the currently configured Prog ID for TwinCAT 3
        /// </summary>
        /// <value>The prog ID.</value>
        public DTEInfo DTEInfo
        {
            get { return (DTEInfo)cBProgID.SelectedValue; }
        }

        /// <summary>
        /// Gets a value indicating whether the IDE is visible during script
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is IDE visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsIDEVisible
        {
            get { return cbProgVisible.IsChecked.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether the IDE is controlled by User
        /// </summary>
        /// <value>
        /// </value>
        public bool IsIDEUserControl
        {
            get { return cbUserControl.IsChecked.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether VisualStudio UI is suppressed.
        /// </summary>
        /// <value>
        /// </value>
        public bool SupressUI
        {
            get { return cbSuppressUI.IsChecked.Value; }
        }

        /// <summary>
        /// Background script worker
        /// </summary>
        IWorker _worker = null;

        /// <summary>
        /// Configuration Generator
        /// </summary>
        ConfigurationFactory _factory = null;

        /// <summary>
        /// Currently runnig script
        /// </summary>
        Script _runningScript = null;

        /// <summary>
        /// Handles the Click event of the btnExecute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptingTest.GlobalVariables.OpenExisitngVSProject == false)//open from file
            {
                Dictionary<string, List<object>> rot = ROTAccess.GetRunningObjectTable();
                bool projectalreadyopened = false;
                for (int x = 0; x < rot.Count; x++)
                {
                    if (rot.ElementAt(x).Key == ScriptingTest.CSV_Reader.ProjectPath)
                    {
                        projectalreadyopened = true;
                    }
                }
                if (projectalreadyopened == true)
                {
                    MessageBox.Show("Project is already opened: " + ScriptingTest.CSV_Reader.ProjectPath + "... Please close before continuing");
                    return;
                }
                /*if (_OpenVsList.Exists(a => a.FolderLocation == ScriptingTest.CSV_Reader.ProjectPath))
                {
                    MessageBox.Show("Project is already opened. Please close before continuing");
                    return;
                }*/
            }

            //comment out test
            //Debug.Assert(this._factory == null);



            if (this.SelectedScript != null)
            {
                _runningScript = this.SelectedScript;
                _runningScript.StatusChanged += new EventHandler<ScriptStatusChangedEventArgs>(script_StatusChanged);

                VsFactory vsFactory = new VsFactory();
                //MessageBox.Show("test");
                if (_runningScript is ScriptEarlyBound)
                    this._factory = new EarlyBoundFactory(vsFactory);
                else if (_runningScript is ScriptLateBound)
                    this._factory = new LateBoundFactory(vsFactory);

                if (this._factory == null)
                {
                    throw new ApplicationException("Generator not found!");
                }
                
                Dictionary<string, dynamic> parameterSet = new Dictionary<string, dynamic>();
                if (File.Exists(txtSelectedProject.Text))
                     ScriptingTest.CSV_Reader.ProjectPath = txtSelectedProject.Text;
                else ScriptingTest.CSV_Reader.ProjectPath = null;

                if (lVActiveVS.SelectedIndex >= 0)
                {
                    DTE dteToConnect;
                    int SelectedIndex = lVActiveVS.SelectedIndex;
                    //if (OpenExisitngVSProject)
                    dteToConnect = _OpenVsList[SelectedIndex].dTE;
                    ScriptingTest.GlobalVariables.ConnectedDTE = dteToConnect;
                }
                ScriptContext context = new ScriptContext(_factory, ScriptingTest.CSV_Reader.ProjectPath, parameterSet);



                    //Backup Project
                    if (ScriptingTest.GlobalVariables.OpenExisitngVSProject == false && ScriptingTest.CSV_Reader.AutoBackupProject)//open from file
                {

                    if (_OpenVsList.Exists(a => a.FolderLocation == ScriptingTest.CSV_Reader.ProjectPath))
                    {
                        MessageBox.Show("Project is already opened. Please close before continuing");
                        return;
                    }


                    string localFolder = System.IO.Directory.GetCurrentDirectory() + @"\ProjectBackup";
                    string ProjectLocation = Path.GetDirectoryName(ScriptingTest.CSV_Reader.ProjectPath);

             
                    System.IO.Directory.CreateDirectory(localFolder);
                    int fileIndex = 1;
                    string zipPath = localFolder + @"\Backup" + fileIndex.ToString() + ".zip";
                    while (File.Exists(zipPath))
                    {
                        fileIndex++;
                        zipPath = localFolder + @"\Backup" + fileIndex.ToString() + ".zip";
                    }
                    // string extractPath = @"c:\example\extract";
                    //System.IO.Compression.ZipFile.CreateFromDirectory
     
                    ZipFile.CreateFromDirectory(ProjectLocation, zipPath);
                    //ZipFile.ExtractToDirectory(zipPath, extractPath);
                }


                _worker = new ScriptBackgroundWorker(/*this._factory,*/ _runningScript,context);
                this.Update(_runningScript);
                _worker.ConfigurationInitialized += new EventHandler(_worker_ConfigurationInitialized);
                _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
                _worker.ProgressStatusChanged += new EventHandler<ProgressStatusChangedArgs>(_worker_ProgressStatusChanged);
                _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
                _factory.AppID = this.DTEInfo.ProgId;
                _factory.IsIdeVisible = this.IsIDEVisible;
                _factory.IsIdeUserControl = this.IsIDEUserControl;
                _factory.SuppressUI = this.SupressUI;
                //txtSelectedProject.Text
                SetExecution(true);
                _worker.BeginScriptExecution();
            }
        }
 

        void _worker_ConfigurationInitialized(object sender, EventArgs e)
        {
            Action action = () =>
                {
                    this.cbProgVisible.IsChecked = _factory.IsIdeVisible;
                    this.cbSuppressUI.IsChecked = _factory.SuppressUI;
                    this.cbUserControl.IsChecked = _factory.IsIdeUserControl;
                };

            this.Dispatcher.Invoke(action, new object[] { });
        }

        void _worker_ProgressStatusChanged(object sender, ProgressStatusChangedArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                int index = this.lvMessages.Items.Add(e.Status);
                object item = this.lvMessages.Items[index];
                this.lvMessages.ScrollIntoView(item);
            }
            else
            {
                Action<string> action = (str) =>
                {
                    int index = this.lvMessages.Items.Add(str);
                    object item = this.lvMessages.Items[index];
                    this.lvMessages.ScrollIntoView(item);
                };

                this.Dispatcher.Invoke(action, new object[] { e.Status });
            }
        }

        /// <summary>
        /// Handles the StatusChanged event of the script control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ScriptingTest.ScriptStatusChangedEventArgs"/> instance containing the event data.</param>
        void script_StatusChanged(object sender, ScriptStatusChangedEventArgs e)
        {
            Action<Script> action = (script) =>
            {
                if (e.NewState == ScriptStatus.Initializing)
                {
                    this.lvMessages.Items.Clear();
                }

                Update(script);
            };

            this.Dispatcher.Invoke(action, new object[] { this._runningScript });
        }

        /// <summary>
        /// Updates the Main window with the Script status
        /// </summary>
        /// <param name="script">The script.</param>
        private void Update(Script script)
        {
            switch (script.Status)
            {
                case ScriptStatus.None:
                    if (script.Result == ScriptResult.None)
                        statusBox.Background = Brushes.Silver;
                    else if (script.Result == ScriptResult.Fail)
                        statusBox.Background = Brushes.Red;
                    else if (script.Result == ScriptResult.Ok)
                        statusBox.Background = Brushes.Green;
                    break;
                case ScriptStatus.Initializing:
                case ScriptStatus.Cleanup:
                    statusBox.Background = Brushes.Gray;
                    break;
                case ScriptStatus.Executing:
                    statusBox.Background = Brushes.Yellow;
                    break;
            }
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the _worker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Action<Script> action = (script) =>
            {
                SetExecution(false);
                _factory = null;
                Update(script);
                _runningScript.StatusChanged -= new EventHandler<ScriptStatusChangedEventArgs>(script_StatusChanged);
                _runningScript = null;
            };

            this.Dispatcher.BeginInvoke(action, new object[] { this._runningScript });
        }

        /// <summary>
        /// Indicates that the Script engine is running
        /// </summary>
        bool _executing = false;

        /// <summary>
        /// Gets a value indicating whether a script is executing.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a script is executing; otherwise, <c>false</c>.
        /// </value>
        public bool IsExecuting
        {
            get { return _executing; }
        }

        /// <summary>
        /// Sets the execution flag.
        /// </summary>
        /// <param name="set">if set to <c>true</c> [set].</param>
        private void SetExecution(bool set)
        {
            _executing = set;
            enableDisableControls();
        }

        /// <summary>
        /// Enables / Disables (updates) the Main form controls
        /// </summary>
        private void enableDisableControls()
        {
            bool scriptSelected = (this.SelectedScript != null);
            bool isExecuting = this.IsExecuting;
            bool isCancelPending = this.IsExecuting && this._worker.CancellationPending;

            this.btnCancel.IsEnabled = isExecuting && !isCancelPending;
            this.btnExecute.IsEnabled = !isExecuting && scriptSelected;
            //this.gbBinding.IsEnabled = !isExecuting;
            this.gbProgID.IsEnabled = !isExecuting;
            this.lVScripts.IsEnabled = !isExecuting;

            gBSettings.IsEnabled = !isExecuting;
        }

        /// <summary>
        /// Handles the ProgressChanged event of the _worker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.progressBar1.Value = e.ProgressPercentage;
            }
            else
            {
                Action<int> action = (i) =>
                {
                    this.progressBar1.Value = i;
                };

                this.Dispatcher.Invoke(action, new object[] { e.ProgressPercentage });
            }
        }

        /// <summary>
        /// Gets the currently selected script.
        /// </summary>
        /// <value>The selected script.</value>
        public Script SelectedScript
        {
            get
            {
                return (Script)this.lVScripts.SelectedItem;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this._worker != null)
            {
                this._worker.CancelRequest();
                this.enableDisableControls();
                //this._worker.CancelAndWait(TimeSpan.FromSeconds(0.1));
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (this._worker != null)
                this._worker.CancelAndWait(TimeSpan.FromSeconds(1.0));

            base.OnClosing(e);
        }

        /// <summary>
        /// Handles the SelectionChanged event of the lVScripts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void lVScripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToolTip toolTip = new ToolTip();

            Script selected = this.SelectedScript;
            if (selected != null)
                toolTip.Content = selected.DetailedDescription;
            else
                toolTip.Content = string.Empty;

            lVScripts.ToolTip = toolTip;
            enableDisableControls();
            Update(selected);
        }

        private void btnSelectProject_Click(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = txtSelectedProject.Text;//"c:\\";
                openFileDialog.Filter = "sln files (*.sln)|*.sln|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;             
            }
            ScriptingTest.CSV_Reader.ProjectPath = filePath;
            txtSelectedProject.Text = ScriptingTest.CSV_Reader.ProjectPath;
            //MessageBox.Show(fileContent, "File Content at path: " + filePath, MessageBoxButtons.OK);
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            ScriptingTest.GlobalVariables.OpenExisitngVSProject = true;
            rbOpenTwinCATProject.IsChecked = false;
            rbUseActiveTwinCATProject.IsChecked = true;
            loadOpenVisualStudio();
            UpdateOpenTwinCATProjectMode();
        }

        private void rbOpenTwinCATProject_Click(object sender, RoutedEventArgs e)
        {
            ScriptingTest.GlobalVariables.OpenExisitngVSProject = false;
            rbOpenTwinCATProject.IsChecked = true;
            rbUseActiveTwinCATProject.IsChecked = false;
            UpdateOpenTwinCATProjectMode();
        }
        private void UpdateOpenTwinCATProjectMode()
        {
            if (rbUseActiveTwinCATProject.IsChecked == true)
            {
                gBActiveVS.IsEnabled = true;
                txtSelectedProject.IsEnabled = false;
                btnSelectProject.IsEnabled = false;
            }
            else
            {
                gBActiveVS.IsEnabled = false;
                txtSelectedProject.IsEnabled = true;
                btnSelectProject.IsEnabled = true;
            }
        }

        private void EditSettings_Click(object sender, RoutedEventArgs e)
        {
            string localFolder = System.IO.Directory.GetCurrentDirectory();
            string filePath = localFolder + "\\Settings.txt";
            System.Diagnostics.Process.Start("notepad.exe", filePath);
        }
    }
}


/*
 1. Auto back project before editing so don't ever loss code. 
2. Auto set IP address from constant inside PLC program. Pull from text file. Do both network cards. Leave option for 1 port to be DHCP
3. Leave dips alone
4. Find a way to deal with arrays for linking diagnostics.
5. Auto detect number of etherCAT slaves.
6. Add working counter and give name from device in tree.
 
 
 */