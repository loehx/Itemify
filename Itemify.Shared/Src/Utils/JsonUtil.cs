using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Itemify.Shared.Utils
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
        public static T SaveAsJsonToDirectory<T>(this T source, string directory, bool indent = true)
            where T : class
        {
            var fname = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss_fffffff");
            return StringifyTo<T>(source, fname, indent);
        }

        public static T StringifyTo<T>(this T source, string filepath, bool indent = true)
            where T: class
        {
            var json = JsonUtil.Stringify(source, indent);

            if (!filepath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                filepath += ".json";

            SaveFileTo(json, filepath);
            return source;
        }

        public static IEnumerable<T> SaveEachAsJsonTo<T>(this IEnumerable<T> source, string directory, bool indent = true)
        {
            var fname = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            return SaveEachAsJsonTo(source, directory, fname, indent);
        }
    
        public static IEnumerable<T> SaveEachAsJsonTo<T>(this IEnumerable<T> source, string directory, string fileName, bool indent = true)
        {
            var items = source as IReadOnlyList<T> ?? source.ToArray();
            var maxN = items.Count.ToString().Length;

            if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring(0, fileName.Length - ".json".Length);

            // (file name)-001 or (file name)-1
            Func<T, int, string> fileNameGenerator = (t, i) => fileName + "-" + (i + 1).ToString().PadLeft(maxN, '0');

            return SaveEachAsJsonTo(items, directory, fileNameGenerator);
        }

        public static IEnumerable<T> SaveEachAsJsonTo<T>(this IEnumerable<T> source, string directory, Func<T, int, string> fileNameGenerator, bool indent = true)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var items = source as IReadOnlyList<T> ?? source.ToArray();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var fname = fileNameGenerator(item, i);

                if (!fname.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    fname += ".json";

                var json = JsonUtil.Stringify(item, indent);
                var filePath = Path.Combine(directory, fname);

                SaveFileTo(json, filePath);

                yield return item;
            }
        }

        private static void SaveFileTo(string contents, string filePath)
        {
            if (contents == null) throw new ArgumentNullException(nameof(contents));
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            var fname = Path.GetFileName(filePath);
            var directory = filePath.Substring(0, filePath.Length - fname.Length);

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fname = filePath.Replace(c.ToString(), "");
            }

            filePath = Path.Combine(directory, fname);
            File.WriteAllText(filePath, contents, Encoding.Default);
        }
    }
}
