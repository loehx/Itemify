using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Itemify.Core.Utils
{
    public static class JsonUtil
    {
        private static JsonSerializerSettings settings = null;
        private static JsonSerializerSettings Settings
        {
            get
            {
                if (settings != null)
                    return settings;

                var s = new JsonSerializerSettings
                {
                    Culture = CultureInfo.InvariantCulture,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto
                };

                s.Converters.Add(new StringEnumConverter()
                {
                    CamelCaseText = true,
                    AllowIntegerValues = true
                });

                return settings = s;
            }
        }

        public static string Stringify(object obj, bool indent = false)
        {
            return JsonConvert.SerializeObject(obj, indent ? Formatting.Indented : Formatting.None, Settings);
        }

        public static T Parse<T>(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            if (json == string.Empty)
                throw new JsonSerializationException($"Empty string could not be parsed.");

            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public static T TryParse<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, Settings);
            }
            catch (Exception err)
            {
                return default(T);
            }
        }
    }
}
