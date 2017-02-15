using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Itemify.Shared.Utils;
using Itemify.Shared.Interfaces;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Itemify.Shared.Utils
{
    public enum DateTimeUnit
    {
        Ticks,
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Week,
        IsoWeek,
        Month,
        Quarter,
        Year
    }

    public static class ObjectExtensions
    {

        public static Guid ToGuid(this int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static Guid ToGuid(this long value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static int ToInt(this Guid value)
        {
            var b = value.ToByteArray();
            var bint = BitConverter.ToInt32(b, 0);
            return bint;
        }

        public static long ToLong(this Guid value)
        {
            var b = value.ToByteArray();
            var blong = BitConverter.ToInt64(b, 0);
            return blong;
        }

        public static int GetIsoDayOfWeek(this DateTimeOffset source)
        {
            var day = (int)source.DayOfWeek;
            return day == 0 ? 7 : day;
        }

        public static int GetIsoDayOfWeek(this DateTime source)
        {
            var day = (int)source.DayOfWeek;
            return day == 0 ? 7 : day;
        }

        // TO BE TESTED
        public static string ToReadableString(this double d, int fraction = 2)
        {
            var format = "0." + new string('#', fraction);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (0 == d)
                return "0";

            if (d > 1)
            {
                if (d > 1000000)
                {
                    if (d == double.MaxValue)
                        return "max";

                    d = Math.Round(d / 1000000, fraction);
                    return d.ToString(format, CultureInfo.InvariantCulture) + "m";
                }

                if (d > 1000)
                {
                    d = Math.Round(d / 1000, fraction);
                    return d.ToString(format, CultureInfo.InvariantCulture) + "k";
                }


                return Math.Round(d, fraction).ToString(format, CultureInfo.InvariantCulture);
            }

            // d < 1
            {
                var x = d / Math.Pow(10, fraction);
                if (d < x)
                {
                    if (d == double.MinValue)
                        return "min";

                    return "<" + x.ToString(format, CultureInfo.InvariantCulture);
                }

                d = Math.Round(d, fraction);
                return d.ToString(format, CultureInfo.InvariantCulture);
            }
        }

        public static int PercentOf(this double n, double total)
        {
            return (int)Math.Round(n / total * 100);
        }

        public static int PercentOf(this decimal n, decimal total)
        {
            return (int)Math.Round(n / total * 100);
        }

        public static int PercentOf(this int n, int total)
        {
            return (int)Math.Round((double)n / total * 100);
        }

        public static int PercentOf(this long n, long total)
        {
            return (int)Math.Round((double)n / total * 100);
        }

        public static string ToJson(this object obj, bool indent = false)
        {
            return JsonUtil.Stringify(obj, indent);
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable<T> value)
        {
            return value;
        }

        public static IEnumerable<T> AsEnumerable<T>(this IQueryable<T> value)
        {
            return value;
        }

        public static IEnumerable<T> AsEnumerable<T>(this T value)
        {
            yield return value;
        }

        public static bool IsEmpty(this Guid guid)
        {
            return guid == Guid.Empty;
        }

        public static T ToStringOrDefault<T>(this T obj) where T : class
        {
            return obj ?? default(T);
        }

        public static T ConvertTo<T>(this object value)
        {
            if (null == value) return default(T);

            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type);

            if (underlyingType != null)
                type = underlyingType;

            var o = value as JObject;
            if (o != null)
                return o.ToObject<T>();

            if (type.GetTypeInfo().IsEnum)
                return (T)Enum.Parse(type, value.ToString(), true);

            if (type == typeof(Guid))
                return (T)(object)Guid.Parse((string)value);

            if (type == typeof(TimeSpan))
                return (T)(object)TimeSpan.Parse(value as string);

            if (type == typeof(DateTime))
                return (T)(object)DateTime.Parse(value as string);

            if (type == typeof(DateTimeOffset))
                return (T)(object)DateTimeOffset.Parse(value as string);

            return (T)Convert.ChangeType(value, type);
        }

        public static T SafeConvertTo<T>(this object value)
        {
            try
            {
                return value.ConvertTo<T>();
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Clone the object, and returning a reference to a cloned object.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object MemberwiseClone(this object source)
        {
            if (source == null) return null;
            System.Reflection.MethodInfo inst = source.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (inst != null)
                return inst.Invoke(source, null);
            else
                return null;
        }

        public static T ExtendByExcluding<T>(this T obj, T source, params Expression<Func<T, object>>[] propertyLambdas)
        {
            var skipProperties = propertyLambdas.Select(k => k.Body).OfType<MemberExpression>().Select(k => k.Member.Name).ToArray();

            return obj.ExtendByExcluding(source, skipProperties);
        }

        public static T ExtendByExcluding<T>(this T obj, T source, params string[] skipProperties)
        {
            var used = new bool[skipProperties.Length];

            try
            {
                return ExtendBy(obj, source, p =>
                {
                    var index = Array.IndexOf(skipProperties, p.Name);
                    var ok = index >= 0;
                    if (ok)
                        used[index] = true;
                    return !ok;
                });
            }
            finally
            {
                // Inform developer if some properties not exist in T.

                var missing = used.Select((ok, i) => new KeyValuePair<string, bool>(skipProperties[i], ok)).Where(k => !k.Value).Select(k => k.Key).Join(", ");

                if (!string.IsNullOrEmpty(missing))
                    throw new Exception("The following properties could not be found: " + missing);
            }
        }


        public static T ExtendBy<T>(this T obj, T source, Func<PropertyInfo, bool> predicate = null)
        {
            var t = typeof(T);
            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                if (predicate != null && !predicate(prop)) continue;

                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(obj, value, null);
            }

            return obj;
        }

        public static PropertyInfo[] GetProperties<T>(this T obj)
        {
            return typeof(T).GetProperties();
        }

        /// <summary>
        /// Clone the object, and returning a reference to a cloned object.
        /// </summary>
        /// <returns>
        /// Reference to the new cloned object.
        /// </returns>
        public static object DeepClone(this object source)
        {
            //First we create an instance of this specific Type.
            var newObject = Activator.CreateInstance(source.GetType());

            //We get the array of fields for the new Type instance.
            var fields = newObject.GetType().GetFields();

            var i = 0;

            foreach (var fi in source.GetType().GetFields())
            {
                //We query if the fiels support the ICloneable interface.
                var cloneType = fi.FieldType.GetTypeInfo().GetInterface("ICloneable", true);

                if (cloneType != null)
                {
                    //Getting the ICloneable interface from the object.
                    var IClone = (ICloneable)fi.GetValue(source);

                    //We use the clone method to set the new value to the field.
                    fields[i].SetValue(newObject, IClone.Clone());
                }
                else
                {
                    // If the field doesn't support the ICloneable 
                    // interface then just set it.
                    fields[i].SetValue(newObject, fi.GetValue(source));
                }

                //Now we check if the object support the 
                //IEnumerable interface, so if it does
                //we need to enumerate all its items and check if 
                //they support the ICloneable interface.
                var enumerableType = fi.FieldType.GetTypeInfo().GetInterface("IEnumerable", true);
                if (enumerableType != null)
                {
                    //Get the IEnumerable interface from the field.
                    var IEnum = (IEnumerable)fi.GetValue(source);

                    //This version support the IList and the 
                    //IDictionary interfaces to iterate on collections.
                    var listType = fields[i].FieldType.GetTypeInfo().GetInterface("IList", true);
                    var dicType = fields[i].FieldType.GetTypeInfo().GetInterface("IDictionary", true);

                    var j = 0;
                    if (listType != null)
                    {
                        //Getting the IList interface.
                        var list = (IList)fields[i].GetValue(newObject);

                        foreach (var obj in IEnum)
                        {
                            //Checking to see if the current item 
                            //support the ICloneable interface.
                            cloneType = obj.GetType().GetTypeInfo().GetInterface("ICloneable", true);

                            if (cloneType != null)
                            {
                                //If it does support the ICloneable interface, 
                                //we use it to set the clone of
                                //the object in the list.
                                var clone = (ICloneable)obj;

                                list[j] = clone.Clone();
                            }

                            //NOTE: If the item in the list is not 
                            //support the ICloneable interface then in the 
                            //cloned list this item will be the same 
                            //item as in the original list
                            //(as long as this Type is a reference Type).

                            j++;
                        }
                    }
                    else if (dicType != null)
                    {
                        //Getting the dictionary interface.
                        var dic = (IDictionary)fields[i].GetValue(newObject);
                        j = 0;

                        foreach (DictionaryEntry de in IEnum)
                        {
                            //Checking to see if the item 
                            //support the ICloneable interface.
                            cloneType = de.Value.GetType().GetTypeInfo().GetInterface("ICloneable", true);

                            if (cloneType != null)
                            {
                                var clone = (ICloneable)de.Value;

                                dic[de.Key] = clone.Clone();
                            }
                            j++;
                        }
                    }
                }
                i++;
            }
            return newObject;
        }

        public static Dictionary<string, string> GetDictionary<T>(this T obj, bool includeNullValues = false)
        {
            var dict = new Dictionary<string, string>();
            var props = obj.GetProperties();
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(string))
                    dict.Add(prop.Name, (string)prop.GetValue(obj));
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(decimal))
                    dict.Add(prop.Name, prop.GetValue(obj).ToString());
                else if (prop.PropertyType == typeof(DateTime))
                    dict.Add(prop.Name, (prop.GetValue(obj) is DateTime ? (DateTime)prop.GetValue(obj) : new DateTime()).ToString("yyyy-MM-ddTHH:mm:ss"));
                else if (prop.PropertyType == typeof(int?))
                    dict.Add(prop.Name, prop.GetValue(obj) == null ? "" : prop.GetValue(obj).ToString());
                else
                {
                    var value = prop.GetValue(obj);
                    if (value != null || includeNullValues)
                        dict.Add(prop.Name, value?.ToString());
                }
            }

            return dict;
        }

        public static string ReplacePlaceholders(this string obj, Dictionary<string, string> dict)
        {
            return dict.Aggregate(obj, (current, keyValue) => current.Replace("{" + keyValue.Key.ToCamelCase() + "}", keyValue.Value));
        }

        public static IEnumerable<string> ReplacePlaceholders(this IEnumerable<string> objs, Dictionary<string, string> dict)
        {
            return objs.Select(obj => obj.ReplacePlaceholders(dict));
        }

        public static IEnumerable<string> Replace(this IEnumerable<string> objs, string oldString, string newString)
        {
            return objs.Select(obj => obj.Replace(oldString, newString));
        }
    }
}