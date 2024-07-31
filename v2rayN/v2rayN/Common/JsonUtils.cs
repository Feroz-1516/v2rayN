using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace v2rayN
{
    internal class JsonUtils
    {
    
        public static T DeepCopy<T>(T obj)
        {
            return Deserialize<T>(Serialize(obj, false))!;
        }

      
        public static T? Deserialize<T>(string? strJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strJson))
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(strJson);
            }
            catch
            {
                return default;
            }
        }

       
        public static JsonNode? ParseJson(string strJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strJson))
                {
                    return null;
                }
                return JsonNode.Parse(strJson);
            }
            catch
            {
              
                return null;
            }
        }

       
        public static string Serialize(object? obj, bool indented = true)
        {
            string result = string.Empty;
            try
            {
                if (obj == null)
                {
                    return result;
                }
                var options = new JsonSerializerOptions
                {
                    WriteIndented = indented ? true : false,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                result = JsonSerializer.Serialize(obj, options);
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
            }
            return result;
        }

       
        public static int ToFile(object? obj, string? filePath, bool nullValue = true)
        {
            if (filePath is null)
            {
                return -1;
            }
            try
            {
                using FileStream file = File.Create(filePath);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = nullValue ? JsonIgnoreCondition.Never : JsonIgnoreCondition.WhenWritingNull
                };

                JsonSerializer.Serialize(file, obj, options);
                return 0;
            }
            catch (Exception ex)
            {
                Logging.SaveLog(ex.Message, ex);
                return -1;
            }
        }
    }
}