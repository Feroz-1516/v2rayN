using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace v2rayN
{
    internal class Utils
    {
        #region 资源Json操作

        public static string GetEmbedText(string res)
        {
            string result = string.Empty;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using Stream? stream = assembly.GetManifestResourceStream(res);
                ArgumentNullException.ThrowIfNull(stream);
                using StreamReader reader = new(stream);
                result = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return result;
        }

        public static string? LoadResource(string? res)
        {
            try
            {
                if (!File.Exists(res))
                {
                    return null;
                }
                return File.ReadAllText(res);
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return null;
        }

        #endregion 资源Json操作

        #region 转换函数


        public static string List2String(List<string>? lst, bool wrap = false)
        {
            try
            {
                if (lst == null)
                {
                    return string.Empty;
                }
                if (wrap)
                {
                    return string.Join("," + Environment.NewLine, lst);
                }
                else
                {
                    return string.Join(",", lst);
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
                return string.Empty;
            }
        }

   
        public static List<string> String2List(string str)
        {
            try
            {
                str = str.Replace(Environment.NewLine, "");
                return new List<string>(str.Split(',', StringSplitOptions.RemoveEmptyEntries));
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
                return new List<string>();
            }
        }

        public static List<string> String2ListSorted(string str)
        {
            try
            {
                str = str.Replace(Environment.NewLine, "");
                List<string> list = new(str.Split(',', StringSplitOptions.RemoveEmptyEntries));
                list.Sort();
                return list;
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
                return new List<string>();
            }
        }

        
        
        
        
        
        public static string Base64Encode(string plainText)
        {
            try
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                return Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception ex)
            {
                Logging.SaveLog("Base64Encode", ex);
                return string.Empty;
            }
        }

        
        
        
        
        
        public static string Base64Decode(string plainText)
        {
            try
            {
                plainText = plainText.Trim()
                  .Replace(Environment.NewLine, "")
                  .Replace("\n", "")
                  .Replace("\r", "")
                  .Replace('_', '/')
                  .Replace('-', '+')
                  .Replace(" ", "");

                if (plainText.Length % 4 > 0)
                {
                    plainText = plainText.PadRight(plainText.Length + 4 - plainText.Length % 4, '=');
                }

                byte[] data = Convert.FromBase64String(plainText);
                return Encoding.UTF8.GetString(data);
            }
            catch (Exception ex)
            {
                Logging.SaveLog("Base64Decode", ex);
                return string.Empty;
            }
        }

        
        
        
        
        
        public static int ToInt(object? obj)
        {
            try
            {
                return Convert.ToInt32(obj ?? string.Empty);
            }
            catch 
            {
                
                return 0;
            }
        }

        public static bool ToBool(object obj)
        {
            try
            {
                return Convert.ToBoolean(obj);
            }
            catch 
            {
                
                return false;
            }
        }

        public static string ToString(object obj)
        {
            try
            {
                return obj?.ToString() ?? string.Empty;
            }
            catch
            {
                
                return string.Empty;
            }
        }

        
        
        
        
        
        
        
        public static void ToHumanReadable(long amount, out double result, out string unit)
        {
            uint factor = 1024u;
            
            long KBs = amount;
            if (KBs > 0)
            {
                
                long MBs = KBs / factor;
                if (MBs > 0)
                {
                    
                    long GBs = MBs / factor;
                    if (GBs > 0)
                    {
                        
                        long TBs = GBs / factor;
                        if (TBs > 0)
                        {
                            result = TBs + ((GBs % factor) / (factor + 0.0));
                            unit = "TB";
                            return;
                        }
                        result = GBs + ((MBs % factor) / (factor + 0.0));
                        unit = "GB";
                        return;
                    }
                    result = MBs + ((KBs % factor) / (factor + 0.0));
                    unit = "MB";
                    return;
                }
                result = KBs + ((amount % factor) / (factor + 0.0));
                unit = "KB";
                return;
            }
            else
            {
                result = amount;
                unit = "B";
            }
        }

        public static string HumanFy(long amount)
        {
            ToHumanReadable(amount, out double result, out string unit);
            return $"{string.Format("{0:f1}", result)} {unit}";
        }

        public static string UrlEncode(string url)
        {
            return Uri.EscapeDataString(url);
            
        }

        public static string UrlDecode(string url)
        {
            return Uri.UnescapeDataString(url);
            
        }

        public static NameValueCollection ParseQueryString(string query)
        {
            var result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            if (IsNullOrEmpty(query))
            {
                return result;
            }

            var parts = query[1..].Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var keyValue = part.Split(['=']);
                if (keyValue.Length != 2)
                {
                    continue;
                }
                var key = Uri.UnescapeDataString(keyValue[0]);
                var val = Uri.UnescapeDataString(keyValue[1]);

                if (result[key] is null)
                {
                    result.Add(key, val);
                }
            }

            return result;
        }

        public static string GetMD5(string str)
        {
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            byte[] byteNew = MD5.HashData(byteOld);
            StringBuilder sb = new(32);
            foreach (byte b in byteNew)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static ImageSource IconToImageSource(Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                new System.Windows.Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
        }

        
        
        
        
        
        public static string GetPunycode(string url)
        {
            if (Utils.IsNullOrEmpty(url))
            {
                return url;
            }
            try
            {
                Uri uri = new(url);
                if (uri.Host == uri.IdnHost || uri.Host == $"[{uri.IdnHost}]")
                {
                    return url;
                }
                else
                {
                    return url.Replace(uri.Host, uri.IdnHost);
                }
            }
            catch
            {
                return url;
            }
        }

        public static bool IsBase64String(string plainText)
        {
            var buffer = new Span<byte>(new byte[plainText.Length]);
            return Convert.TryFromBase64String(plainText, buffer, out int _);
        }

        public static string Convert2Comma(string text)
        {
            if (Utils.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Replace("，", ",").Replace(Environment.NewLine, ",");
        }

        #endregion 转换函数

        #region 数据检查

        
        
        
        
        
        public static bool IsNumeric(string oText)
        {
            try
            {
                int var1 = ToInt(oText);
                return true;
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
                return false;
            }
        }

        
        
        
        
        
        public static bool IsNullOrEmpty(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }
            if (text == "null")
            {
                return true;
            }
            return false;
        }

        
        
        
        
        public static bool IsIP(string ip)
        {
            
            if (IsNullOrEmpty(ip))
            {
                return false;
            }

            
            
            
            if (ip.IndexOf(@"/") > 0)
            {
                string[] cidr = ip.Split('/');
                if (cidr.Length == 2)
                {
                    if (!IsNumeric(cidr[0]))
                    {
                        return false;
                    }
                    ip = cidr[0];
                }
            }

            
            string pattern = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";

            
            return IsMatch(ip, pattern);
        }

        
        
        
        
        public static bool IsDomain(string? domain)
        {
            
            if (IsNullOrEmpty(domain))
            {
                return false;
            }

            return Uri.CheckHostName(domain) == UriHostNameType.Dns;
        }

        
        
        
        
        
        public static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        public static bool IsIpv6(string ip)
        {
            if (IPAddress.TryParse(ip, out IPAddress? address))
            {
                return address.AddressFamily switch
                {
                    AddressFamily.InterNetwork => false,
                    AddressFamily.InterNetworkV6 => true,
                    _ => false,
                };
            }
            return false;
        }

        #endregion 数据检查

        #region 测速

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        

        public static void SetSecurityProtocol(bool enableSecurityProtocolTls13)
        {
            if (enableSecurityProtocolTls13)
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            }
            else
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            ServicePointManager.DefaultConnectionLimit = 256;
        }

        public static bool PortInUse(int port)
        {
            bool inUse = false;
            try
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

                var lstIpEndPoints = new List<IPEndPoint>(IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners());

                foreach (IPEndPoint endPoint in ipEndPoints)
                {
                    if (endPoint.Port == port)
                    {
                        inUse = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return inUse;
        }

        public static int GetFreePort(int defaultPort = 9090)
        {
            try
            {
                if (!Utils.PortInUse(defaultPort))
                {
                    return defaultPort;
                }

                TcpListener l = new(IPAddress.Loopback, 0);
                l.Start();
                int port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return port;
            }
            catch
            {
            }
            return 59090;
        }

        #endregion 测速

        #region 杂项

        
        
        
        
        public static string GetVersion(bool blFull = true)
        {
            try
            {
                string location = GetExePath();
                if (blFull)
                {
                    return string.Format("v2rayN - V{0} - {1}",
                            FileVersionInfo.GetVersionInfo(location).FileVersion?.ToString(),
                            File.GetLastWriteTime(location).ToString("yyyy/MM/dd"));
                }
                else
                {
                    return string.Format("v2rayN/{0}",
                        FileVersionInfo.GetVersionInfo(location).FileVersion?.ToString());
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
                return string.Empty;
            }
        }

        
        
        
        
        public static string? GetClipboardData()
        {
            string? strData = string.Empty;
            try
            {
                IDataObject data = Clipboard.GetDataObject();
                if (data.GetDataPresent(DataFormats.UnicodeText))
                {
                    strData = data.GetData(DataFormats.UnicodeText)?.ToString();
                }
                return strData;
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return strData;
        }

        
        
        
        
        public static void SetClipboardData(string strData)
        {
            try
            {
                Clipboard.SetText(strData);
            }
            catch
            {
            }
        }

        
        
        
        
        public static string GetGUID(bool full = true)
        {
            try
            {
                if (full)
                {
                    return Guid.NewGuid().ToString("D");
                }
                else
                {
                    return BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0).ToString();
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return string.Empty;
        }

        
        
        
        
        public static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity current = WindowsIdentity.GetCurrent();
                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
                
                return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
                return false;
            }
        }

        public static string GetDownloadFileName(string url)
        {
            var fileName = Path.GetFileName(url);
            fileName += "_temp";

            return fileName;
        }

        public static IPAddress? GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                
                
                .FirstOrDefault();
        }

        public static bool IsGuidByParse(string strSrc)
        {
            return Guid.TryParse(strSrc, out Guid g);
        }

        public static void ProcessStart(string fileName, string arguments = "")
        {
            try
            {
                Process.Start(new ProcessStartInfo(fileName, arguments) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
        }

        public static void SetDarkBorder(System.Windows.Window window, bool dark)
        {
            
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(window).EnsureHandle();
            int attribute = dark ? 1 : 0;
            uint attributeSize = (uint)Marshal.SizeOf(attribute);
            DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref attribute, attributeSize);
            DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref attribute, attributeSize);
        }

        public static bool IsLightTheme()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i > 0;
        }

        
        
        
        
        public static Dictionary<string, string> GetSystemHosts()
        {
            var systemHosts = new Dictionary<string, string>();
            var hostfile = @"C:\Windows\System32\drivers\etc\hosts";
            try
            {
                if (File.Exists(hostfile))
                {
                    var hosts = File.ReadAllText(hostfile).Replace("\r", "");
                    var hostsList = hosts.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var host in hostsList)
                    {
                        if (host.StartsWith("#")) continue;
                        var hostItem = host.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (hostItem.Length < 2) continue;
                        systemHosts.Add(hostItem[1], hostItem[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return systemHosts;
        }

        #endregion 杂项

        #region TempPath

        
        
        
        
        public static string GetPath(string fileName)
        {
            string startupPath = StartupPath();
            if (IsNullOrEmpty(fileName))
            {
                return startupPath;
            }
            return Path.Combine(startupPath, fileName);
        }

        
        
        
        
        public static string GetExePath()
        {
            return Environment.ProcessPath ?? string.Empty;
        }

        public static string StartupPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetTempPath(string filename = "")
        {
            string _tempPath = Path.Combine(StartupPath(), "guiTemps");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if (Utils.IsNullOrEmpty(filename))
            {
                return _tempPath;
            }
            else
            {
                return Path.Combine(_tempPath, filename);
            }
        }

        public static string UnGzip(byte[] buf)
        {
            using MemoryStream sb = new();
            using GZipStream input = new(new MemoryStream(buf), CompressionMode.Decompress, false);
            input.CopyTo(sb);
            sb.Position = 0;
            return new StreamReader(sb, Encoding.UTF8).ReadToEnd();
        }

        public static string GetBackupPath(string filename)
        {
            string _tempPath = Path.Combine(StartupPath(), "guiBackups");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            return Path.Combine(_tempPath, filename);
        }

        public static string GetConfigPath(string filename = "")
        {
            string _tempPath = Path.Combine(StartupPath(), "guiConfigs");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if (Utils.IsNullOrEmpty(filename))
            {
                return _tempPath;
            }
            else
            {
                return Path.Combine(_tempPath, filename);
            }
        }

        public static string GetBinPath(string filename, string? coreType = null)
        {
            string _tempPath = Path.Combine(StartupPath(), "bin");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if (coreType != null)
            {
                _tempPath = Path.Combine(_tempPath, coreType.ToString()!);
                if (!Directory.Exists(_tempPath))
                {
                    Directory.CreateDirectory(_tempPath);
                }
            }
            if (Utils.IsNullOrEmpty(filename))
            {
                return _tempPath;
            }
            else
            {
                return Path.Combine(_tempPath, filename);
            }
        }

        public static string GetLogPath(string filename = "")
        {
            string _tempPath = Path.Combine(StartupPath(), "guiLogs");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if (Utils.IsNullOrEmpty(filename))
            {
                return _tempPath;
            }
            else
            {
                return Path.Combine(_tempPath, filename);
            }
        }

        public static string GetFontsPath(string filename = "")
        {
            string _tempPath = Path.Combine(StartupPath(), "guiFonts");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if (Utils.IsNullOrEmpty(filename))
            {
                return _tempPath;
            }
            else
            {
                return Path.Combine(_tempPath, filename);
            }
        }

        #endregion TempPath

        #region 开机自动启动等

        
        
        
        
        
        public static void SetAutoRun(string AutoRunRegPath, string AutoRunName, bool run)
        {
            try
            {
                var autoRunName = $"{AutoRunName}_{GetMD5(StartupPath())}";

                
                RegWriteValue(AutoRunRegPath, autoRunName, "");
                if (IsAdministrator())
                {
                    AutoStart(autoRunName, "", "");
                }

                if (run)
                {
                    string exePath = GetExePath();
                    if (IsAdministrator())
                    {
                        AutoStart(autoRunName, exePath, "");
                    }
                    else
                    {
                        RegWriteValue(AutoRunRegPath, autoRunName, exePath.AppendQuotes());
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
        }

        public static string? RegReadValue(string path, string name, string def)
        {
            RegistryKey? regKey = null;
            try
            {
                regKey = Registry.CurrentUser.OpenSubKey(path, false);
                string? value = regKey?.GetValue(name) as string;
                if (IsNullOrEmpty(value))
                {
                    return def;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            finally
            {
                regKey?.Close();
            }
            return def;
        }

        public static void RegWriteValue(string path, string name, object value)
        {
            RegistryKey? regKey = null;
            try
            {
                regKey = Registry.CurrentUser.CreateSubKey(path);
                if (IsNullOrEmpty(value.ToString()))
                {
                    regKey?.DeleteValue(name, false);
                }
                else
                {
                    regKey?.SetValue(name, value);
                }
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            finally
            {
                regKey?.Close();
            }
        }

        
        
        
        
        
        
        
        public static void AutoStart(string taskName, string fileName, string description)
        {
            if (Utils.IsNullOrEmpty(taskName))
            {
                return;
            }
            string TaskName = taskName;
            var logonUser = WindowsIdentity.GetCurrent().Name;
            string taskDescription = description;
            string deamonFileName = fileName;

            using var taskService = new TaskService();
            var tasks = taskService.RootFolder.GetTasks(new Regex(TaskName));
            foreach (var t in tasks)
            {
                taskService.RootFolder.DeleteTask(t.Name);
            }
            if (Utils.IsNullOrEmpty(fileName))
            {
                return;
            }

            var task = taskService.NewTask();
            task.RegistrationInfo.Description = taskDescription;
            task.Settings.DisallowStartIfOnBatteries = false;
            task.Settings.StopIfGoingOnBatteries = false;
            task.Settings.RunOnlyIfIdle = false;
            task.Settings.IdleSettings.StopOnIdleEnd = false;
            task.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            task.Triggers.Add(new LogonTrigger { UserId = logonUser, Delay = TimeSpan.FromSeconds(10) });
            task.Principal.RunLevel = TaskRunLevel.Highest;
            task.Actions.Add(new ExecAction(deamonFileName.AppendQuotes(), null, Path.GetDirectoryName(deamonFileName)));

            taskService.RootFolder.RegisterTaskDefinition(TaskName, task);
        }

        public static void RemoveTunDevice()
        {
            try
            {
                var sum = MD5.HashData(Encoding.UTF8.GetBytes("wintunsingbox_tun"));
                var guid = new Guid(sum);
                string pnputilPath = @"C:\Windows\System32\pnputil.exe";
                string arg = $$""" /remove-device  "SWD\Wintun\{{{guid}}}" """;

                
                Process proc = new()
                {
                    StartInfo = new()
                    {
                        FileName = pnputilPath,
                        Arguments = arg,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                proc.Start();
                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
            }
            catch
            {
            }
        }

        #endregion 开机自动启动等

        #region Windows API

        [Flags]
        public enum DWMWINDOWATTRIBUTE : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19,
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, ref int attributeValue, uint attributeSize);

        #endregion Windows API
    }
}