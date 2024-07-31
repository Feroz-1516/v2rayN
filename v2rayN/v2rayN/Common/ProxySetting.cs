using System.Runtime.InteropServices;
using static v2rayN.Common.ProxySetting.InternetConnectionOption;

namespace v2rayN.Common
{
    internal class ProxySetting
    {
        private const string _regPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";

        private static bool SetProxyFallback(string? strProxy, string? exceptions, int type)
        {
            if (type == 1)
            {
                Utils.RegWriteValue(_regPath, "ProxyEnable", 0);
                Utils.RegWriteValue(_regPath, "ProxyServer", string.Empty);
                Utils.RegWriteValue(_regPath, "ProxyOverride", string.Empty);
                Utils.RegWriteValue(_regPath, "AutoConfigURL", string.Empty);
            }
            if (type == 2)
            {
                Utils.RegWriteValue(_regPath, "ProxyEnable", 1);
                Utils.RegWriteValue(_regPath, "ProxyServer", strProxy ?? string.Empty);
                Utils.RegWriteValue(_regPath, "ProxyOverride", exceptions ?? string.Empty);
                Utils.RegWriteValue(_regPath, "AutoConfigURL", string.Empty);
            }
            else if (type == 4)
            {
                Utils.RegWriteValue(_regPath, "ProxyEnable", 0);
                Utils.RegWriteValue(_regPath, "ProxyServer", string.Empty);
                Utils.RegWriteValue(_regPath, "ProxyOverride", string.Empty);
                Utils.RegWriteValue(_regPath, "AutoConfigURL", strProxy ?? string.Empty);
            }
            return true;
        }

      
        public static bool UnsetProxy()
        {
            return SetProxy(null, null, 1);
        }

      
        public static bool SetProxy(string? strProxy, string? exceptions, int type)
        {
            try
            {
             
                bool result = SetConnectionProxy(null, strProxy, exceptions, type);
            
                var connections = EnumerateRasEntries();
                foreach (var connection in connections)
                {
                    result |= SetConnectionProxy(connection, strProxy, exceptions, type);
                }
                return result;
            }
            catch (Exception ex)
            {
                SetProxyFallback(strProxy, exceptions, type);
                Logging.SaveLog(ex.Message, ex);
                return false;
            }
        }

        private static bool SetConnectionProxy(string? connectionName, string? strProxy, string? exceptions, int type)
        {
            InternetPerConnOptionList list = new();

            int optionCount = 1;
            if (type == 1) 
            {
                optionCount = 1;
            }
            else if (type is 2 or 4) 
            {
                optionCount = Utils.IsNullOrEmpty(exceptions) ? 2 : 3;
            }

            int m_Int = (int)PerConnFlags.PROXY_TYPE_DIRECT;
            PerConnOption m_Option = PerConnOption.INTERNET_PER_CONN_FLAGS;
            if (type == 2) 
            {
                m_Int = (int)(PerConnFlags.PROXY_TYPE_DIRECT | PerConnFlags.PROXY_TYPE_PROXY);
                m_Option = PerConnOption.INTERNET_PER_CONN_PROXY_SERVER;
            }
            else if (type == 4) 
            {
                m_Int = (int)(PerConnFlags.PROXY_TYPE_DIRECT | PerConnFlags.PROXY_TYPE_AUTO_PROXY_URL);
                m_Option = PerConnOption.INTERNET_PER_CONN_AUTOCONFIG_URL;
            }

         
            InternetConnectionOption[] options = new InternetConnectionOption[optionCount];
            options[0].m_Option = PerConnOption.INTERNET_PER_CONN_FLAGS;
            options[0].m_Value.m_Int = m_Int;
            if (optionCount > 1)
            {
                options[1].m_Option = m_Option;
                options[1].m_Value.m_StringPtr = Marshal.StringToHGlobalAuto(strProxy);
                if (optionCount > 2)
                {
                    options[2].m_Option = PerConnOption.INTERNET_PER_CONN_PROXY_BYPASS;
                    options[2].m_Value.m_StringPtr = Marshal.StringToHGlobalAuto(exceptions); 
                }
            }

            // default stuff
            list.dwSize = Marshal.SizeOf(list);
            if (connectionName != null)
            {
                list.szConnection = Marshal.StringToHGlobalAuto(connectionName);
            }
            else
            {
                list.szConnection = nint.Zero;
            }
            list.dwOptionCount = options.Length;
            list.dwOptionError = 0;

            int optSize = Marshal.SizeOf(typeof(InternetConnectionOption));
            
            nint optionsPtr = Marshal.AllocCoTaskMem(optSize * options.Length);
            
            for (int i = 0; i < options.Length; ++i)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    nint opt = new(optionsPtr.ToInt64() + i * optSize);
                    Marshal.StructureToPtr(options[i], opt, false);
                }
                else
                {
                    nint opt = new(optionsPtr.ToInt32() + i * optSize);
                    Marshal.StructureToPtr(options[i], opt, false);
                }
            }

            list.options = optionsPtr;

            nint ipcoListPtr = Marshal.AllocCoTaskMem(list.dwSize); 
            Marshal.StructureToPtr(list, ipcoListPtr, false);

            
            bool isSuccess = NativeMethods.InternetSetOption(nint.Zero,
               InternetOption.INTERNET_OPTION_PER_CONNECTION_OPTION,
               ipcoListPtr, list.dwSize);
            int returnvalue = 0;
            if (!isSuccess)
            { 
                returnvalue = Marshal.GetLastPInvokeError();
            }
            else
            {
               
                NativeMethods.InternetSetOption(nint.Zero, InternetOption.INTERNET_OPTION_SETTINGS_CHANGED, nint.Zero, 0);
                NativeMethods.InternetSetOption(nint.Zero, InternetOption.INTERNET_OPTION_REFRESH, nint.Zero, 0);
            }

            if (list.szConnection != nint.Zero) Marshal.FreeHGlobal(list.szConnection); 
            if (optionCount > 1)
            {
                Marshal.FreeHGlobal(options[1].m_Value.m_StringPtr); 
                if (optionCount > 2)
                {
                    Marshal.FreeHGlobal(options[2].m_Value.m_StringPtr); 
                }
            }
            Marshal.FreeCoTaskMem(optionsPtr); 
            Marshal.FreeCoTaskMem(ipcoListPtr); 
            if (returnvalue != 0)
            {
              
                throw new ApplicationException($"Set Internet Proxy failed with error code: {Marshal.GetLastWin32Error()}");
            }

            return true;
        }

     
        private static IEnumerable<string> EnumerateRasEntries()
        {
            int entries = 0;
            RASENTRYNAME[] rasEntryNames = new RASENTRYNAME[1];
            int bufferSize = Marshal.SizeOf(typeof(RASENTRYNAME));
            rasEntryNames[0].dwSize = Marshal.SizeOf(typeof(RASENTRYNAME));

            uint result = NativeMethods.RasEnumEntries(null, null, rasEntryNames, ref bufferSize, ref entries);
          
            if (result == (uint)ErrorCode.ERROR_BUFFER_TOO_SMALL)
            {
                rasEntryNames = new RASENTRYNAME[bufferSize / Marshal.SizeOf(typeof(RASENTRYNAME))];
                for (int i = 0; i < rasEntryNames.Length; i++)
                {
                    rasEntryNames[i].dwSize = Marshal.SizeOf(typeof(RASENTRYNAME));
                }

                result = NativeMethods.RasEnumEntries(null, null, rasEntryNames, ref bufferSize, ref entries);
            }
            if (result == 0)
            {
                var entryNames = new List<string>();
                for (int i = 0; i < entries; i++)
                {
                    entryNames.Add(rasEntryNames[i].szEntryName);
                }

                return entryNames;
            }
            throw new ApplicationException($"RasEnumEntries failed with error code: {result}");
        }

        #region WinInet structures

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct InternetPerConnOptionList
        {
            public int dwSize;               
            public nint szConnection;        
            public int dwOptionCount;        
             public int dwOptionError;
            public nint options;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct InternetConnectionOption
        {
            private static readonly int Size;
            public PerConnOption m_Option;
            public InternetConnectionOptionValue m_Value;

            static InternetConnectionOption()
            {
                Size = Marshal.SizeOf(typeof(InternetConnectionOption));
            }

          
            [StructLayout(LayoutKind.Explicit)]
            public struct InternetConnectionOptionValue
            {
      
                [FieldOffset(0)]
                public System.Runtime.InteropServices.ComTypes.FILETIME m_FileTime;

                [FieldOffset(0)]
                public int m_Int;

                [FieldOffset(0)]
                public nint m_StringPtr;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct RASENTRYNAME
            {
                public int dwSize;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName + 1)]
                public string szEntryName;

                public int dwFlags;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH + 1)]
                public string szPhonebookPath;
            }


            public const int RAS_MaxEntryName = 256;

            public const int MAX_PATH = 260; 
        }

        #endregion WinInet structures

        #region WinInet enums

        public enum InternetOption : uint
        {
            INTERNET_OPTION_PER_CONNECTION_OPTION = 75,
            INTERNET_OPTION_REFRESH = 37,
            INTERNET_OPTION_SETTINGS_CHANGED = 39
        }


        public enum PerConnOption
        {
            INTERNET_PER_CONN_FLAGS = 1,
            INTERNET_PER_CONN_PROXY_SERVER = 2, 
            INTERNET_PER_CONN_PROXY_BYPASS = 3, 
            INTERNET_PER_CONN_AUTOCONFIG_URL = 4
        }


        [Flags]
        public enum PerConnFlags
        {
            PROXY_TYPE_DIRECT = 0x00000001,  
            PROXY_TYPE_PROXY = 0x00000002,
            PROXY_TYPE_AUTO_PROXY_URL = 0x00000004,
            PROXY_TYPE_AUTO_DETECT = 0x00000008  
        }

        public enum ErrorCode : uint
        {
            ERROR_BUFFER_TOO_SMALL = 603,
            ERROR_INVALID_SIZE = 632
        }

        #endregion WinInet enums

        internal static class NativeMethods
        {
            [DllImport("WinInet.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool InternetSetOption(nint hInternet, InternetOption dwOption, nint lpBuffer, int dwBufferLength);

            [DllImport("Rasapi32.dll", CharSet = CharSet.Auto)]
            public static extern uint RasEnumEntries(
                string? reserved,          
                string? lpszPhonebook,     
                [In, Out] RASENTRYNAME[]? lprasentryname,
                ref int lpcb,          
                ref int lpcEntries       
            );
        }
    }
}