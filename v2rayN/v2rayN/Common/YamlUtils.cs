using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace v2rayN.Common
{
    internal class YamlUtils
    {
        #region YAML

        
        
        
        
        
        
        public static T FromYaml<T>(string str)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            try
            {
                T obj = deserializer.Deserialize<T>(str);
                return obj;
            }
            catch (Exception ex)
            {
                Logging.SaveLog("FromYaml", ex);
                return deserializer.Deserialize<T>("");
            }
        }

        
        
        
        
        
        public static string ToYaml(Object obj)
        {
            var serializer = new SerializerBuilder()
                    .WithNamingConvention(HyphenatedNamingConvention.Instance)
                    .Build();

            string result = string.Empty;
            try
            {
                result = serializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return result;
        }

        #endregion YAML
    }
}