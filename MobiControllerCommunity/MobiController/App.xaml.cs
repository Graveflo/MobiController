using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Tools;
using MobiControllerBlackBox.Controllers;
using ModServer;
using WinAPIWrapper;

// TODO: directx / multi monitor support for remote desktop app
// TODO: Download increment

// File Controller dragable / multi select
// TODO: have controllers download witrh hyperlink

//TODO: when program is double clicked and in tray have it return into veiw

// before first release:
//Yannick Lung <- images
// TODO: hide all protected field and methods
// TODO: fix remote exceptions
// TODO: kill proccess by filter or select multiple //premium?
// look in system.reflection.obfusa
// TODO: double tap select bug on iphone
// todo: dialouge error on iphone
// todo: loading errir on fileremote
// todo: discover assemblies for custom controllers

//TODO: Wake the screen with mouse instead of the system signal.
//TODO: check if the internal Ip address has changed (check if port is open through router) (better notification)

//IDEAS
// Have a temporary dictionary of pictures for the controller icons

// Conceive a good structure for the tools


// maybe have status bar for controllers
// static fields are not serialized. Use to advantage by having a tool also need a gui pannel for creating new instances of
// ^ after this the controller designer needs to be built (possibly not released)

// reorganize exceptions and have a notification baloon functionality for EventLog

// AI able to figure out basic attacks (session ip differences)


namespace MobiController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string APPNAME = "MobiController";
        public const string CONTROLLER_FILETYPE = "rmt";

        public static string APP_DRIVE = System.Reflection.Assembly.GetExecutingAssembly().Location.Split('\\')[0];
        public static string APP_DATA_FOLDER = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\MobiController\");
        public static string CONTROLLER_DIR = APP_DATA_FOLDER + @"controllers\";
        public static string CONFIG_PATH = APP_DATA_FOLDER + ".conf";
        public static string USER_TABLE_PATH = APP_DATA_FOLDER + ".udb";
        public static string VERSION = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        public static readonly string SEED = Path.GetRandomFileName().Replace(".", "");

        public static SerializableConfig Config = new SerializableConfig();
        public static EventLogWPF Log = new EventLogWPF();

        public static frmClientView ClientView; //needs to be assigned after initialization
        public static frmLogView LogView;
        public static frmMain MainWin;

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (MainWin != null)
            {
                MainWin.StopServer(false); //kill off those pesky threads
            }

        }

        //add all serializable objects her
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            HttpEngine.PAGE_ROOT = APP_DATA_FOLDER + @"www/";
            EventLog.LOG_FILE = APP_DATA_FOLDER + "_log.txt";

            if (e.Args.Length > 0)
            {
                addController(e.Args[0]);
            }

            string procname = Process.GetCurrentProcess().ProcessName;
            int procID = Process.GetCurrentProcess().Id;
            foreach (Process p in Process.GetProcesses())
            {
                //p.main
                if (p.ProcessName.Equals(procname) && p.Id != procID)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }

            if (File.Exists(App.CONFIG_PATH))
            {
                try
                {
                    App.Config = (SerializableConfig)deserialize(App.CONFIG_PATH);
                }
                catch (InvalidCastException)
                {
                    App.Config = new SerializableConfig();
                }
                if (App.Config == null || App.Config.version == 0 || App.Config.version != SerializableConfig.VERSION)
                {
                    var tmp = new SerializableConfig();
                    if(App.Config.version != SerializableConfig.VERSION){
                        try
                        {
                            tmp.convert(App.Config);
                        }
                        catch (NullReferenceException) { }
                    }
                    App.Config = tmp;
                }
            }
            if (File.Exists(App.USER_TABLE_PATH))
            {
                try
                {
                    Bouncer.UserTable = (Dictionary<String, String>)deserialize(App.USER_TABLE_PATH);
                }
                catch (InvalidCastException)
                {
                    Bouncer.UserTable = new Dictionary<String, String>();
                }
                if (Bouncer.IsUserTableNull)
                    Bouncer.UserTable = new Dictionary<String, String>();
            }

            Directory.CreateDirectory(CONTROLLER_DIR); // won't erase
            //buildAdministrativeController();
            //buildMediaController();
            //buildFileController();
            //buildAdministrativeControllerfree();
            //buildMediaControllerfree();
            //buildRemoteDesktopController();
            //buildTouchPadControllerFree();
            //buildTouchPadController();
            //buildSlideShowController();

            HttpEngine.addPageMutation("{RSAKEY}", BitConverter.ToString(Bouncer.rcsp.ExportParameters(false).Modulus).Replace("-", ""));
            HttpEngine.addPageMutation("{RSAEX}", BitConverter.ToString(Bouncer.rcsp.ExportParameters(false).Exponent, 0).ToString().Replace("-", ""));

            // for some reason this helps speed up the initial connecting process in some envirnments
            //ClientContainer.REGEX_MOBILE1.IsMatch("blahblah");
            //ClientContainer.REGEX_MOBILE2.IsMatch("blahblah");


            ClientView = new frmClientView(); //needs to be assigned after initialization
            LogView = new frmLogView();
            frmSettings firstSettings = new frmSettings();
            MainWin = new frmMain();
            MainWin.settings = firstSettings;

            myTcpServer.Connected += ClientView.ConnectionInit;
            myTcpServer.Disconnected += ClientView.ConnectionDisconnect;
            
            if (Config.isUPnPonStart)
            {
                App.MainWin.settings.StartUPnP();
            }

            if (!App.Config.username.Equals(""))
            {
                if (Config.isStartInTray)
                {
                    MainWin.notifyIcon.Visible = true;
                    MainWin.showTrayMessage.Invoke();
                    return;
                }
            }

            MainWin.Show();
        }

        public static bool addController(string path)
        {
            if (File.Exists(path))
            {
                string cleanPath = path.Replace('/', '\\').Replace("'", "").Replace("\"", "");
                string fileName = cleanPath.Substring(cleanPath.LastIndexOf(@"\") + 1);

                if (!(cleanPath.StartsWith(CONTROLLER_DIR) || File.Exists(CONTROLLER_DIR + fileName)))
                {
                    try
                    {
                        //TODO: check with username based on filetype
                        Controller c = new Controller(new FileInfo(cleanPath));
                        File.Copy(path, CONTROLLER_DIR + fileName);
                        System.Windows.MessageBox.Show(fileName + " added to the controller list! refresh your page.");
                        return true;
                    }
                    catch (IOException)
                    {
                        System.Windows.MessageBox.Show("An error occured working with the controller file: " + path);
                    }
                    catch (ControllerException)
                    {
                        System.Windows.MessageBox.Show("This is not a valid, free, controller that can be installed: " + path);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Controller :" + path + " is already installed. You can remove the controller in the settings window.");
                }
            }
            return false;
        }

        //private static void buildSlideShowController()
        //{
        //    //touch screen only
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\SlideShowController.htm");

        //    ControllerBuilder myController = new ControllerBuilder("SlideShowController", "For slideshow presentations.", s.ReadToEnd());
        //    myController.addFunctionality("/start", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.F5 });
        //    myController.addFunctionality("/end", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Escape });

        //    myController.addFunctionality("/ns", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.N });
        //    myController.addFunctionality("/ps", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.P });

        //    myController.addFunctionality("/n", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Enter });
        //    myController.addFunctionality("/p", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Back });

        //    myController.addFunctionality("/w", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.W }); //white out
        //    myController.addFunctionality("/b", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.B }); //black out

        //    myController.addFunctionality("/1", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.NumPad1 }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Enter } } });
        //    myController.addFunctionality("/2", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.NumPad2 }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Enter } } });
        //    myController.addFunctionality("/3", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.NumPad3 }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Enter } } });
        //    myController.addFunctionality("/4", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.NumPad4 }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Enter } } });

        //    myController.save(CONTROLLER_DIR + "SlideShowController." + CONTROLLER_FILETYPE);
        //}

        //private static void buildRemoteDesktopController()
        //{
        //    //touch screen only
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\RemoteDesktopController.htm");

        //    ControllerBuilder myController = new ControllerBuilder("RemoteDesktopController", "View your screen and control your mouse and keyboard.", s.ReadToEnd());
        //    myController.addFunctionality("/ss", new ScreenServingTool());
        //    myController.addFunctionality("/s2", new ScreenServingTool() { KeyDemensions = "d", KeyCorner = "c" });
        //    myController.addFunctionality("/s", new ScreenServingTool() { FileType = ScreenServingTool.FileFormat.JPG });
        //    myController.addFunctionality("/2", new ScreenServingTool() { FileType = ScreenServingTool.FileFormat.JPG, KeyDemensions = "d", KeyCorner = "c" });
        //    myController.addFunctionality("/sr", new ScreenResQueryTool());
        //    myController.addFunctionality("/click", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTDOWN | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTUP | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/mc", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MIDDLEDOWN | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MIDDLEUP | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/rc", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_RIGHTDOWN | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_RIGHTUP | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/m", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE });
        //    myController.addFunctionality("/ma", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/ld", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTDOWN | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/lu", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTUP | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/md", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MIDDLEDOWN | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/mu", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MIDDLEUP | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/k", new KeyPressTool() { StrKey = "s", StrBackKey = "b" });
        //    myController.addFunctionality("/ke", new KeyParseTool() { StrKey = "s" });

        //    myController.save(CONTROLLER_DIR + "RemoteDesktopController." + CONTROLLER_FILETYPE);
        //}

        //private static void buildTouchPadControllerFree()
        //{
        //    //touch screen only
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\TouchPadControllerFree.htm");

        //    ControllerBuilder myController = new ControllerBuilder("TouchPadController (Free)", "Touch mousepad. Tap to click.", s.ReadToEnd());
        //    myController.addFunctionality("/click", new MouseEventTool() { Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTDOWN | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTUP | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/m", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE });

        //    myController.save(CONTROLLER_DIR + "TouchPadControllerFree." + CONTROLLER_FILETYPE);
        //}

        //private static void buildTouchPadController()
        //{
        //    //touch screen only
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\TouchPadController.htm");

        //    ControllerBuilder myController = new ControllerBuilder("TouchPadController", "Touch mousepad. Tap to click.", s.ReadToEnd());
        //    myController.addFunctionality("/click", new MouseEventTool() { Button = WinAPI.MOUSE_EVENT.cButtons.XBUTTON1, Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTDOWN | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTUP | WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE });
        //    myController.addFunctionality("/m", new MouseEventTool() { PosxKey = "x", PosyKey = "y", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_MOVE });
        //    myController.addFunctionality("/ld", new MouseEventTool() { Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTDOWN });
        //    myController.addFunctionality("/lu", new MouseEventTool() { Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_LEFTUP });
        //    myController.addFunctionality("/rd", new MouseEventTool() { Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_RIGHTDOWN });
        //    myController.addFunctionality("/ru", new MouseEventTool() { Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_RIGHTUP });
        //    myController.addFunctionality("/sc", new MouseEventTool() { WheelDeltaKey = "a", Flags = WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_WHEEL });


        //    myController.save(CONTROLLER_DIR + "TouchPadController." + CONTROLLER_FILETYPE);
        //}

        //private static void buildFileController()
        //{
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\FileController.htm");

        //    ControllerBuilder myController = new ControllerBuilder("FileController", "Open files on the remote PC. Add/Remove files and folders; View/download files from PC.", s.ReadToEnd());

        //    myController.addFunctionality("/ls", new FileListerAndFormattingTool() { PathVaraible = "path" });
        //    myController.addFunctionality("/dl", new FileServingTool() { FilePathVariable = "path", isAttatchment = true });
        //    myController.addFunctionality("/vf", new FileServingTool() { FilePathVariable = "path", isAttatchment = false });
        //    myController.addFunctionality("/ul", new FileUploadHandlerTool() { FilePathVariable = "path" });
        //    myController.addFunctionality("/exec", new FileExecTool() { FilePathVariable = "path", FileNameVariable = "file" });

        //    myController.addFunctionality("/cp", new FileCopyTool() { FilePathVariable = "?path", DestPathVariable = "path", IsMove = false });
        //    myController.addFunctionality("/ctP", new FileCopyTool() { FilePathVariable = "?path", DestPathVariable = "path", IsMove = true });

        //    myController.addFunctionality("/ud", new PathTool() { ReturnPath = PathTool.PATH.USER_PROFILE });
        //    myController.addFunctionality("/doc", new PathTool() { ReturnPath = PathTool.PATH.MY_DOCUMENTS });
        //    myController.addFunctionality("/rec", new PathTool() { ReturnPath = PathTool.PATH.RECENT });
        //    myController.addFunctionality("/fav", new PathTool() { ReturnPath = PathTool.PATH.FAVORITES });
        //    myController.addFunctionality("/desk", new PathTool() { ReturnPath = PathTool.PATH.DESKTOP });

        //    myController.save(CONTROLLER_DIR + "FileController." + CONTROLLER_FILETYPE);
        //}

        //private static void buildMediaController()
        //{
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\MediaController.htm");

        //    ControllerBuilder myController = new ControllerBuilder("Media Controller", "Play, Pause, Volume. All general media controlls.", s.ReadToEnd());

        //    myController.addFunctionality("/playpause", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.MediaPlayPause });
        //    myController.addFunctionality("/muteunmute", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.VolumeMute });
        //    myController.addFunctionality("/stop", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.MediaStop });
        //    //myController.addFunctionality("/playonly", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Pause });
        //    //myController.addFunctionality("/pauseonly", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Play });
        //    myController.addFunctionality("/playonly", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_PLAY, HwndBroadcast = true }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Play } } });
        //    myController.addFunctionality("/pauseonly", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_PAUSE, HwndBroadcast = true }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Pause } } });
        //    //myController.addFunctionality("/pauseonly", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_PAUSE, HwndBroadcast = true });
        //    myController.addFunctionality("/vold", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_VOLUME_DOWN });
        //    myController.addFunctionality("/volp", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_VOLUME_UP });
        //    myController.addFunctionality("/prev", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.MediaPreviousTrack });
        //    myController.addFunctionality("/nxt", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.MediaNextTrack });
        //    myController.addFunctionality("/micvold", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MICROPHONE_VOLUME_DOWN });
        //    myController.addFunctionality("/micvolp", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MICROPHONE_VOLUME_UP });
        //    myController.addFunctionality("/micrec", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_RECORD, Broadcast = true });
        //    myController.addFunctionality("/micswitch", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MIC_ON_OFF_TOGGLE });
        //    myController.addFunctionality("/chm", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_CHANNEL_DOWN, HwndBroadcast = true });
        //    myController.addFunctionality("/chp", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_CHANNEL_UP, HwndBroadcast = true });
        //    myController.addFunctionality("/rew", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_REWIND, HwndBroadcast = true });
        //    myController.addFunctionality("/ff", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_FAST_FORWARD, HwndBroadcast = true });

        //    myController.save(CONTROLLER_DIR + "MediaController." + CONTROLLER_FILETYPE);
        //}

        //private static void buildMediaControllerfree()
        //{
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\MediaControllerFree.htm");

        //    ControllerBuilder myController = new ControllerBuilder("Media Controller (Free)", "Send Play, Pause, and Volume Commands To Supporting Media Players.", s.ReadToEnd());
        //    //myController.addFunctionality("/playpause", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_PLAY_PAUSE, Broadcast = true });
        //    //myController.addFunctionality("/muteunmute", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_VOLUME_MUTE });
        //    myController.addFunctionality("/stop", new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.MediaStop });
        //    myController.addFunctionality("/playonly", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_PLAY, HwndBroadcast = true }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Play } } });
        //    myController.addFunctionality("/pauseonly", new ImperativeTool() { Tools = new ToolBox.ATool[2] { new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_PAUSE, HwndBroadcast = true }, new KeyPressTool() { KeyVal = (byte)System.Windows.Forms.Keys.Pause } } });
        //    myController.addFunctionality("/vold", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_VOLUME_DOWN });
        //    myController.addFunctionality("/volp", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_VOLUME_UP });
        //    //myController.addFunctionality("/prev", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_PREVIOUSTRACK, HwndBroadcast = true });
        //    //myController.addFunctionality("/nxt", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_NEXTTRACK, HwndBroadcast = true });
        //    myController.addFunctionality("/micvold", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MICROPHONE_VOLUME_DOWN });
        //    myController.addFunctionality("/micvolp", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MICROPHONE_VOLUME_UP });
        //    myController.addFunctionality("/micrec", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_RECORD, Broadcast = true });
        //    myController.addFunctionality("/micswitch", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MIC_ON_OFF_TOGGLE });
        //    myController.addFunctionality("/chm", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_CHANNEL_DOWN, HwndBroadcast = true });
        //    myController.addFunctionality("/chp", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_CHANNEL_UP, HwndBroadcast = true });
        //    myController.addFunctionality("/rew", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_REWIND, HwndBroadcast = true });
        //    myController.addFunctionality("/ff", new MediaControlTool() { Command = WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM.APPCOMMAND_MEDIA_FAST_FORWARD, HwndBroadcast = true });

        //    myController.save(CONTROLLER_DIR + "MediaControllerFree." + CONTROLLER_FILETYPE);
        //}

        //private static void buildAdministrativeController()
        //{
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\AdministrationController.htm");

        //    ControllerBuilder myController = new ControllerBuilder("Administration Controller", "Power off computer, Turn off screen, Manage Windows and Processes.", s.ReadToEnd());
        //    myController.addFunctionality("/s", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_POWEROFF, Force = false });
        //    myController.addFunctionality("/r", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_REBOOT, Force = false });
        //    myController.addFunctionality("/sl", new SuspendStateTool() { State = SuspendStateTool.STATE.SLEEP });
        //    myController.addFunctionality("/sm", new SuspendStateTool { State = SuspendStateTool.STATE.HIBERNATE });
        //    myController.addFunctionality("/l", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_LOGOFF });
        //    myController.addFunctionality("/fs", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_POWEROFF, Force = true });
        //    myController.addFunctionality("/fr", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_REBOOT, Force = true });
        //    myController.addFunctionality("/w8s", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_HYBRID_SHUTDOWN, Force = true });

        //    myController.addFunctionality("/pmon", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_MONITORPOWER, LParam = (IntPtr)2 });
        //    //myController.addFunctionality("/wmd", new ActionTool(){ thisAction = () => {
        //    //    new SendMessageTool() { Message = WinAPI.SENDMESSAGE._MSG.WM_KEYDOWN, wParam = 0x20, LParam = 0 }.Invoke(null,null);
        //    //    new SendMessageTool() { Message = WinAPI.SENDMESSAGE._MSG.WM_KEYUP, wParam = 0x20, LParam = 65539 }.Invoke(null,null);
        //    //}
        //    //});

        //    myController.addFunctionality("/lmd", new DimmerTool() { DimmAmt = 0 });
        //    //myController.addFunctionality("/lmd", new ActionTool()
        //    //{
        //    //    thisAction = delegate()
        //    //    {
        //    //        using (ManagementObjectSearcher searcher =
        //    //            new ManagementObjectSearcher(new ManagementScope("root\\WMI"), new SelectQuery("WmiMonitorBrightnessMethods")))
        //    //        {
        //    //            using (ManagementObjectCollection objectCollection = searcher.Get())
        //    //            {
        //    //                try
        //    //                {
        //    //                    foreach (ManagementObject mObj in objectCollection)
        //    //                    {
        //    //                        mObj.InvokeMethod("WmiSetBrightness",
        //    //                            new Object[] { UInt32.MaxValue, 0 });
        //    //                        break;
        //    //                    }
        //    //                }
        //    //                catch (ManagementException)
        //    //                {
        //    //                    // do nothing
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //});

        //    myController.addFunctionality("/rlmd", new DimmerTool() { DimmAmt = byte.MaxValue });
        //    //myController.addFunctionality("/rlmd", new ActionTool()
        //    //{
        //    //    thisAction = delegate()
        //    //    {
        //    //        using (ManagementObjectSearcher searcher =
        //    //            new ManagementObjectSearcher(new ManagementScope("root\\WMI"), new SelectQuery("WmiMonitorBrightnessMethods")))
        //    //        {
        //    //            using (ManagementObjectCollection objectCollection = searcher.Get())
        //    //            {
        //    //                try
        //    //                {
        //    //                    foreach (ManagementObject mObj in objectCollection)
        //    //                    {
        //    //                        mObj.InvokeMethod("WmiSetBrightness",
        //    //                            new Object[] { UInt32.MaxValue, byte.MaxValue });
        //    //                        break;
        //    //                    }
        //    //                }
        //    //                catch (ManagementException) { }
        //    //            }
        //    //        }
        //    //    }
        //    //});

        //    myController.addFunctionality("/start", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_TASKLIST });
        //    myController.addFunctionality("/ess", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_SCREENSAVE });

        //    myController.addFunctionality("/wpl", new WindowListingAndFormattingTool());
        //    myController.addFunctionality("/proc", new ProcessListingAndFormattingTool());

        //    myController.addFunctionality("/ew", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_CLOSE, IsHWndArgument = true });
        //    myController.addFunctionality("/miw", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_MINIMIZE, IsHWndArgument = true });
        //    myController.addFunctionality("/maw", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_MAXIMIZE, IsHWndArgument = true });
        //    myController.addFunctionality("/rw", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_RESTORE, IsHWndArgument = true });
        //    myController.addFunctionality("/hw", new ShowWindowTool() { NCmdShow = WinAPI.SHOW_WINDOW_NCMDSHOW.SW_HIDE });
        //    myController.addFunctionality("/sw", new ShowWindowTool() { NCmdShow = WinAPI.SHOW_WINDOW_NCMDSHOW.SW_SHOW });
        //    myController.addFunctionality("/kill", new WindowProcessKillTool());
        //    myController.addFunctionality("/pin", new ToggleWindowTopMostTool());

        //    myController.addFunctionality("/pkill", new ProcessKillTool());

        //    myController.save(CONTROLLER_DIR + "AdministrationController." + CONTROLLER_FILETYPE);
        //}

        //private static void buildAdministrativeControllerfree()
        //{
        //    StreamReader s = File.OpenText(APP_DRIVE + @"\Projects\Visual Studio\C#\MobiController\MobiController\Resources\AdministrationControllerFree.htm");

        //    ControllerBuilder myController = new ControllerBuilder("Administration Controller (Free)", "Power off computer, Turn off screen, and Manage Processes.", s.ReadToEnd());
        //    myController.addFunctionality("/s", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_POWEROFF, Force = false });
        //    myController.addFunctionality("/r", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_REBOOT, Force = false });
        //    //myController.addFunctionality("/sl", new ActionTool() { thisAction = () => WinAPI.SetSuspendState(false, true, true) });
        //    //myController.addFunctionality("/sm", new ActionTool() { thisAction = () => WinAPI.SetSuspendState(true, true, true) });
        //    myController.addFunctionality("/l", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_LOGOFF });
        //    //myController.addFunctionality("/fs", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_POWEROFF, Force = true });
        //    //myController.addFunctionality("/fr", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_REBOOT, Force = true });
        //    myController.addFunctionality("/w8s", new ExitWindowsTool() { Command = WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_HYBRID_SHUTDOWN, Force = true });

        //    myController.addFunctionality("/pmon", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_MONITORPOWER, LParam = (IntPtr)2 });
        //    //myController.addFunctionality("/wmd", new ActionTool(){ thisAction = () => {
        //    //    new SendMessageTool() { Message = WinAPI.SENDMESSAGE._MSG.WM_KEYDOWN, wParam = 0x20, LParam = 0 }.Invoke(null,null);
        //    //    new SendMessageTool() { Message = WinAPI.SENDMESSAGE._MSG.WM_KEYUP, wParam = 0x20, LParam = 65539 }.Invoke(null,null);
        //    //}
        //    //});

        //    //myController.addFunctionality("/lmd", new ActionTool()
        //    //{
        //    //    thisAction = delegate()
        //    //    {
        //    //        using (ManagementObjectSearcher searcher =
        //    //            new ManagementObjectSearcher(new ManagementScope("root\\WMI"), new SelectQuery("WmiMonitorBrightnessMethods")))
        //    //        {
        //    //            using (ManagementObjectCollection objectCollection = searcher.Get())
        //    //            {
        //    //                try
        //    //                {
        //    //                    foreach (ManagementObject mObj in objectCollection)
        //    //                    {
        //    //                        mObj.InvokeMethod("WmiSetBrightness",
        //    //                            new Object[] { UInt32.MaxValue, 0 });
        //    //                        break;
        //    //                    }
        //    //                }
        //    //                catch (ManagementException)
        //    //                {
        //    //                    // do nothing
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //});

        //    //myController.addFunctionality("/rlmd", new ActionTool()
        //    //{
        //    //    thisAction = delegate()
        //    //    {
        //    //        using (ManagementObjectSearcher searcher =
        //    //            new ManagementObjectSearcher(new ManagementScope("root\\WMI"), new SelectQuery("WmiMonitorBrightnessMethods")))
        //    //        {
        //    //            using (ManagementObjectCollection objectCollection = searcher.Get())
        //    //            {
        //    //                try
        //    //                {
        //    //                    foreach (ManagementObject mObj in objectCollection)
        //    //                    {
        //    //                        mObj.InvokeMethod("WmiSetBrightness",
        //    //                            new Object[] { UInt32.MaxValue, byte.MaxValue });
        //    //                        break;
        //    //                    }
        //    //                }
        //    //                catch (ManagementException) { }
        //    //            }
        //    //        }
        //    //    }
        //    //});

        //    //myController.addFunctionality("/start", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_TASKLIST });
        //    //myController.addFunctionality("/ess", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_SCREENSAVE });

        //    //myController.addFunctionality("/wpl", new WindowListingAndFormattingTool());

        //    //myController.addFunctionality("/ew", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_CLOSE, IsHWndArgument = true });
        //    //myController.addFunctionality("/miw", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_MINIMIZE, IsHWndArgument = true });
        //    //myController.addFunctionality("/maw", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_MAXIMIZE, IsHWndArgument = true });
        //    //myController.addFunctionality("/rw", new SysCommandTool() { Command = WinAPI.WM_SYSCOMMAND_WPARAM.SC_RESTORE, IsHWndArgument = true });
        //    //myController.addFunctionality("/hw", new ShowWindowTool() { NCmdShow = WinAPI.SHOW_WINDOW_NCMDSHOW.SW_HIDE });
        //    //myController.addFunctionality("/sw", new ShowWindowTool() { NCmdShow = WinAPI.SHOW_WINDOW_NCMDSHOW.SW_SHOW });
        //    //myController.addFunctionality("/pin", new ToggleWindowTopMostTool());

        //    myController.addFunctionality("/proc", new ProcessListingAndFormattingTool());
        //    myController.addFunctionality("/kill", new WindowProcessKillTool());
        //    myController.addFunctionality("/pkill", new ProcessKillTool());

        //    myController.save(CONTROLLER_DIR + "AdministrationControllerFree." + CONTROLLER_FILETYPE);
        //}

        public static bool serialize(object obj)
        {
            Stream outStream = null;

            if (obj is SerializableConfig)
            {
                outStream = File.Open(CONFIG_PATH, FileMode.Create);
            }
            if (obj is Dictionary<String, String>)
            {
                outStream = File.Open(USER_TABLE_PATH, FileMode.Create);
            }
            if (outStream == null)
            {
                return false;
            }
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(outStream, obj);
            outStream.Close();
            return true;
        }

        private static object deserialize(string path)
        {
            object Refe = new object();
            if (File.Exists(path))
            {
                Stream stream = File.Open(path, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    Refe = formatter.Deserialize(stream);
                }
                catch (SerializationException)
                {
                    try
                    {
                        stream.Close();
                        File.Delete(path);
                    }
                    catch (IOException)
                    {
                        //check if administrator. If admin and failed than display error
                        runas_Admin();
                    }
                    return null;
                }
                stream.Close();
            }
            return Refe;
        }

        public static void runas_Admin()
        {
            Process newProcess = new Process();
            newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            newProcess.StartInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            newProcess.StartInfo.Verb = "runas";
            newProcess.Start();
            Application.Current.Shutdown();
        }

        public static bool exec(string command, bool runasadmin = false)
        {
            Process newProcess = new Process();
            //newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            newProcess.StartInfo.FileName = command;
            if (runasadmin)
            {
                newProcess.StartInfo.Verb = "runas";
            }
            return newProcess.Start();
        }
        public static bool exec(string command, string arguemnts, bool runasadmin = false)
        {
            Process newProcess = new Process();
            //newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            newProcess.StartInfo.FileName = command;
            newProcess.StartInfo.Arguments = arguemnts;
            if (runasadmin)
            {
                newProcess.StartInfo.Verb = "runas";
            }
            return newProcess.Start();
        }
    }
}