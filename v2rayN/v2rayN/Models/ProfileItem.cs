using SQLite;
using v2rayN.Enums;

namespace v2rayN.Models
{
    [Serializable]
    public class ProfileItem
    {
        public ProfileItem()
        {
            indexId = string.Empty;
            configType = EConfigType.VMess;
            configVersion = 2;
            address = string.Empty;
            port = 0;
            id = string.Empty;
            alterId = 0;
            security = string.Empty;
            network = string.Empty;
            remarks = string.Empty;
            headerType = string.Empty;
            requestHost = string.Empty;
            path = string.Empty;
            streamSecurity = string.Empty;
            allowInsecure = string.Empty;
            subid = string.Empty;
            flow = string.Empty;
        }

        #region function

        public string GetSummary()
        {
            string summary = string.Format("[{0}] ", (configType).ToString());
            string[] arrAddr = address.Split('.');
            string addr;
            if (arrAddr.Length > 2)
            {
                addr = $"{arrAddr[0]}***{arrAddr[arrAddr.Length - 1]}";
            }
            else if (arrAddr.Length > 1)
            {
                addr = $"***{arrAddr[arrAddr.Length - 1]}";
            }
            else
            {
                addr = address;
            }
            switch (configType)
            {
                case EConfigType.Custom:
                    summary += string.Format("[{1}]{0}", remarks, coreType.ToString());
                    break;

                default:
                    summary += string.Format("{0}({1}:{2})", remarks, addr, port);
                    break;
            }
            return summary;
        }

        public List<string> GetAlpn()
        {
            if (Utils.IsNullOrEmpty(alpn))
            {
                return null;
            }
            else
            {
                return Utils.String2List(alpn);
            }
        }

        public string GetNetwork()
        {
            if (Utils.IsNullOrEmpty(network) || !Global.Networks.Contains(network))
            {
                return Global.DefaultNetwork;
            }
            return network.TrimEx();
        }

        #endregion function

        [PrimaryKey]
        public string indexId { get; set; }

        
        
        
        public EConfigType configType { get; set; }

        
        
        
        public int configVersion { get; set; }

        
        
        
        public string address { get; set; }

        
        
        
        public int port { get; set; }

        
        
        
        public string id { get; set; }

        
        
        
        public int alterId { get; set; }

        
        
        
        public string security { get; set; }

        
        
        
        public string network { get; set; }

        
        
        
        public string remarks { get; set; }

        
        
        
        public string headerType { get; set; }

        
        
        
        public string requestHost { get; set; }

        
        
        
        public string path { get; set; }

        
        
        
        public string streamSecurity { get; set; }

        
        
        
        public string allowInsecure { get; set; }

        
        
        
        public string subid { get; set; }

        public bool isSub { get; set; } = true;

        
        
        
        public string flow { get; set; }

        
        
        
        public string sni { get; set; }

        
        
        
        public string alpn { get; set; } = string.Empty;

        public ECoreType? coreType { get; set; }

        public int preSocksPort { get; set; }

        public string fingerprint { get; set; }

        public bool displayLog { get; set; } = true;
        public string publicKey { get; set; }
        public string shortId { get; set; }
        public string spiderX { get; set; }
    }
}