#region Header

/**
 * JsonMapper.cs
 *   JSON to .Net object and object to JSON conversions.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


namespace _3rdBy.Plugins.LitJson
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Object = System.Object;

    internal struct PropertyMetadata
    {
        public MemberInfo Info;
        public bool IsField;
        public Type Type;
    }

    internal struct ArrayMetadata
    {
        private Type element_type;
        private bool is_array;
        private bool is_list;


        public Type ElementType
        {
            get
            {
                if (element_type == null)
                    return typeof(JsonData);

                return element_type;
            }

            set => element_type = value;
        }

        public bool IsArray
        {
            get => is_array;
            set => is_array = value;
        }

        public bool IsList
        {
            get => is_list;
            set => is_list = value;
        }
    }

    internal struct ObjectMetadata
    {
        private Type element_type;
        private bool is_dictionary;

        private IDictionary<string, PropertyMetadata> properties;


        public Type ElementType
        {
            get
            {
                if (element_type == null)
                    return typeof(JsonData);

                return element_type;
            }

            set => element_type = value;
        }

        public bool IsDictionary
        {
            get => is_dictionary;
            set => is_dictionary = value;
        }

        public IDictionary<string, PropertyMetadata> Properties
        {
            get => properties;
            set => properties = value;
        }
    }

    internal delegate void ExporterFunc(object obj, JsonWriter writer);

    public delegate void ExporterFunc<T>(T obj, JsonWriter writer);

    internal delegate object ImporterFunc(object input);

    public delegate TValue ImporterFunc<TJson, TValue>(TJson input);

    public delegate IJsonWrapper WrapperFactory();
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LitJsonIgnoreSerialize : Attribute
    {
    }

    public class JsonMapper
    {
        #region Fields

        private static readonly int maxNestingDepth;

        private static readonly IFormatProvider datetimeFormat;

        private static readonly IDictionary<Type, ExporterFunc> baseExportersTable;
        private static readonly IDictionary<Type, ExporterFunc> customExportersTable;

        private static readonly IDictionary<Type,
            IDictionary<Type, ImporterFunc>> baseImportersTable;

        private static readonly IDictionary<Type,
            IDictionary<Type, ImporterFunc>> customImportersTable;

        private static readonly IDictionary<Type, ArrayMetadata> arrayMetadata;
        private static readonly Object arrayMetadataLock = new Object();

        private static readonly IDictionary<Type,
            IDictionary<Type, MethodInfo>> convOps;

        private static readonly Object convOpsLock = new Object();

        private static readonly IDictionary<Type, ObjectMetadata> objectMetadata;
        private static readonly Object objectMetadataLock = new Object();

        private static readonly IDictionary<Type,
            IList<PropertyMetadata>> typeProperties;

        private static readonly Object typePropertiesLock = new Object();

        private static readonly JsonWriter staticWriter;
        private static readonly Object staticWriterLock = new Object();

        private static Type ignoreSerializableAttributeType = typeof(LitJsonIgnoreSerialize);

        #endregion


        #region Constructors

        static JsonMapper()
        {
            maxNestingDepth = 100;

            arrayMetadata = new Dictionary<Type, ArrayMetadata>();
            convOps = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
            objectMetadata = new Dictionary<Type, ObjectMetadata>();
            typeProperties = new Dictionary<Type,
                IList<PropertyMetadata>>();

            staticWriter = new JsonWriter();

            datetimeFormat = DateTimeFormatInfo.InvariantInfo;

            baseExportersTable = new Dictionary<Type, ExporterFunc>();
            customExportersTable = new Dictionary<Type, ExporterFunc>();

            baseImportersTable = new Dictionary<Type,
                IDictionary<Type, ImporterFunc>>();
            customImportersTable = new Dictionary<Type,
                IDictionary<Type, ImporterFunc>>();

            RegisterBaseExporters();
            RegisterBaseImporters();
        }

        #endregion


        #region Private Methods

        private static void AddArrayMetadata(Type type)
        {
            if (arrayMetadata.ContainsKey(type))
                return;

            var data = new ArrayMetadata
            {
                IsArray = type.IsArray
            };

            if (type.GetInterface("System.Collections.IList") != null)
                data.IsList = true;

            foreach (var pInfo in type.GetProperties())
            {
                if (pInfo.Name != "Item")
                    continue;

                var parameters = pInfo.GetIndexParameters();

                if (parameters.Length != 1)
                    continue;

                if (parameters[0].ParameterType == typeof(int))
                    data.ElementType = pInfo.PropertyType;
            }

            lock (arrayMetadataLock)
            {
                try
                {
                    arrayMetadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddObjectMetadata(Type type)
        {
            if (objectMetadata.ContainsKey(type))
                return;

            var data = new ObjectMetadata();

            if (type.GetInterface("System.Collections.IDictionary") != null)
                data.IsDictionary = true;

            data.Properties = new Dictionary<string, PropertyMetadata>();

            foreach (var pInfo in type.GetProperties())
            {
                if (pInfo.Name == "Item")
                {
                    var parameters = pInfo.GetIndexParameters();

                    if (parameters.Length != 1)
                        continue;

                    if (parameters[0].ParameterType == typeof(string))
                        data.ElementType = pInfo.PropertyType;

                    continue;
                }

                var pData = new PropertyMetadata
                {
                    Info = pInfo,
                    Type = pInfo.PropertyType
                };

                data.Properties.Add(pInfo.Name, pData);
            }

            foreach (var fInfo in type.GetFields())
            {
                var pData = new PropertyMetadata
                {
                    Info = fInfo,
                    IsField = true,
                    Type = fInfo.FieldType
                };

                data.Properties.Add(fInfo.Name, pData);
            }

            lock (objectMetadataLock)
            {
                try
                {
                    objectMetadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddTypeProperties(Type type)
        {
            if (typeProperties.ContainsKey(type))
                return;

            IList<PropertyMetadata> props = new List<PropertyMetadata>();

            foreach (var pInfo in type.GetProperties())
            {
                if (pInfo.Name == "Item")
                    continue;

                var pData = new PropertyMetadata
                {
                    Info = pInfo,
                    IsField = false
                };
                props.Add(pData);
            }

            foreach (var fInfo in type.GetFields())
            {
                var pData = new PropertyMetadata
                {
                    Info = fInfo,
                    IsField = true
                };

                props.Add(pData);
            }

            lock (typePropertiesLock)
            {
                try
                {
                    typeProperties.Add(type, props);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static MethodInfo GetConvOp(Type t1, Type t2)
        {
            lock (convOpsLock)
            {
                if (!convOps.ContainsKey(t1))
                    convOps.Add(t1, new Dictionary<Type, MethodInfo>());
            }

            lock (convOpsLock)
            {
                if (convOps[t1].ContainsKey(t2))
                    return convOps[t1][t2];
            }

            var op = t1.GetMethod(
                "op_Implicit", new Type[] { t2 });

            lock (convOpsLock)
            {
                try
                {
                    convOps[t1].Add(t2, op);
                }
                catch (ArgumentException)
                {
                    return convOps[t1][t2];
                }
            }

            return op;
        }

        private static Object ReadValue(Type instType, JsonReader reader, string fileName = "")
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd)
                return null;

            var underlyingType = Nullable.GetUnderlyingType(instType);
            var valueType = underlyingType ?? instType;

            switch (reader.Token)
            {
                case JsonToken.Null when instType.IsClass || underlyingType != null:
                    return null;
                case JsonToken.Null:
                    throw new JsonException($"{fileName}无法将null分配给类型的实例 {instType}");
                case JsonToken.Double:
                case JsonToken.Int:
                case JsonToken.Long:
                case JsonToken.String:
                case JsonToken.Boolean:
                {
                    var jsonType = reader.Value.GetType();

                    if (valueType.IsAssignableFrom(jsonType))
                        return reader.Value;

                    // If there's a custom importer that fits, use it
                    if (customImportersTable.ContainsKey(jsonType) &&
                        customImportersTable[jsonType].ContainsKey(
                            valueType))
                    {
                        var importer =
                            customImportersTable[jsonType][valueType];

                        return importer(reader.Value);
                    }

                    // Maybe there's a base importer that works
                    if (baseImportersTable.ContainsKey(jsonType) &&
                        baseImportersTable[jsonType].ContainsKey(
                            valueType))
                    {
                        var importer =
                            baseImportersTable[jsonType][valueType];

                        return importer(reader.Value);
                    }

                    // Maybe it's an enum
#if NETSTANDARD1_5
                if (value_type.IsEnum())
                    return Enum.ToObject (value_type, reader.Value);
#else
                    if (valueType.IsEnum)
                        return Enum.ToObject(valueType, reader.Value);
#endif
                    // Try using an implicit conversion operator
                    var convOp = GetConvOp(valueType, jsonType);

                    if (convOp != null)
                        return convOp.Invoke(null,
                            new Object[] { reader.Value });

                    // No luck
                    throw new JsonException(string.Format(
                        "Can't assign value '{0}' (type {1}) to type {2}",
                        reader.Value, jsonType, instType));
                }
            }

            Object instance = null;

            if (reader.Token == JsonToken.ArrayStart)
            {
                AddArrayMetadata(instType);
                var tData = arrayMetadata[instType];

                if (!tData.IsArray && !tData.IsList)
                    throw new JsonException(string.Format(
                        "Type {0} can't act as an array",
                        instType));

                IList list;
                Type elemType;

                if (!tData.IsArray)
                {
                    list = (IList)Activator.CreateInstance(instType);
                    elemType = tData.ElementType;
                }
                else
                {
                    list = new ArrayList();
                    elemType = instType.GetElementType();
                }

                list.Clear();

                while (true)
                {
                    var item = ReadValue(elemType, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    list.Add(item);
                }

                if (tData.IsArray)
                {
                    var n = list.Count;
                    instance = Array.CreateInstance(elemType, n);

                    for (var i = 0; i < n; i++)
                        ((Array)instance).SetValue(list[i], i);
                }
                else
                    instance = list;
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                AddObjectMetadata(valueType);
                var tData = objectMetadata[valueType];

                instance = Activator.CreateInstance(valueType);

                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    var property = (string)reader.Value;

                    if (tData.Properties.ContainsKey(property))
                    {
                        var propData =
                            tData.Properties[property];

                        if (propData.IsField)
                        {
                            ((FieldInfo)propData.Info).SetValue(
                                instance, ReadValue(propData.Type, reader));
                        }
                        else
                        {
                            var pInfo =
                                (PropertyInfo)propData.Info;

                            if (pInfo.CanWrite)
                                pInfo.SetValue(
                                    instance,
                                    ReadValue(propData.Type, reader),
                                    null);
                            else
                                ReadValue(propData.Type, reader);
                        }
                    }
                    else
                    {
                        if (!tData.IsDictionary)
                        {
                            if (!reader.SkipNonMembers)
                            {
                                throw new JsonException(string.Format(
                                    "The type {0} doesn't have the " +
                                    "property '{1}'",
                                    instType, property));
                            }
                            else
                            {
                                ReadSkip(reader);
                                continue;
                            }
                        }

                        ((IDictionary)instance).Add(
                            property, ReadValue(
                                tData.ElementType, reader));
                    }
                }
            }

            return instance;
        }

        private static IJsonWrapper ReadValue(WrapperFactory factory,
            JsonReader reader)
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd ||
                reader.Token == JsonToken.Null)
                return null;

            var instance = factory();

            if (reader.Token == JsonToken.String)
            {
                instance.SetString((string)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Double)
            {
                instance.SetDouble((double)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Int)
            {
                instance.SetInt((int)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Long)
            {
                instance.SetLong((long)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Boolean)
            {
                instance.SetBoolean((bool)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.ArrayStart)
            {
                instance.SetJsonType(JsonType.Array);

                while (true)
                {
                    var item = ReadValue(factory, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    ((IList)instance).Add(item);
                }
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                instance.SetJsonType(JsonType.Object);

                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    var property = (string)reader.Value;

                    ((IDictionary)instance)[property] = ReadValue(
                        factory, reader);
                }
            }

            return instance;
        }

        private static void ReadSkip(JsonReader reader)
        {
            ToWrapper(
                delegate { return new JsonMockWrapper(); }, reader);
        }

        private static void RegisterBaseExporters()
        {
            baseExportersTable[typeof(byte)] =
                delegate(Object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((byte)obj)); };

            baseExportersTable[typeof(char)] =
                delegate(Object obj, JsonWriter writer) { writer.Write(Convert.ToString((char)obj)); };

            baseExportersTable[typeof(DateTime)] =
                delegate(Object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToString((DateTime)obj,
                        datetimeFormat));
                };

            baseExportersTable[typeof(decimal)] =
                delegate(Object obj, JsonWriter writer) { writer.Write((decimal)obj); };

            baseExportersTable[typeof(sbyte)] =
                delegate(Object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((sbyte)obj)); };

            baseExportersTable[typeof(short)] =
                delegate(Object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((short)obj)); };

            baseExportersTable[typeof(ushort)] =
                delegate(Object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((ushort)obj)); };

            baseExportersTable[typeof(uint)] =
                delegate(Object obj, JsonWriter writer) { writer.Write(Convert.ToUInt64((uint)obj)); };

            baseExportersTable[typeof(ulong)] =
                delegate(Object obj, JsonWriter writer) { writer.Write((ulong)obj); };

            baseExportersTable[typeof(DateTimeOffset)] =
                delegate(Object obj, JsonWriter writer)
                {
                    writer.Write(((DateTimeOffset)obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", datetimeFormat));
                };
        }

        private static void RegisterBaseImporters()
        {
            ImporterFunc importer;

            importer = delegate(Object input) { return Convert.ToByte((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(byte), importer);

            importer = delegate(Object input) { return Convert.ToUInt64((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(ulong), importer);

            importer = delegate(Object input) { return Convert.ToInt64((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(long), importer);

            importer = delegate(Object input) { return Convert.ToSByte((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(sbyte), importer);

            importer = delegate(Object input) { return Convert.ToInt16((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(short), importer);

            importer = delegate(Object input) { return Convert.ToUInt16((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(ushort), importer);

            importer = delegate(Object input) { return Convert.ToUInt32((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(uint), importer);

            importer = delegate(Object input) { return Convert.ToSingle((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(float), importer);

            importer = delegate(Object input) { return Convert.ToDouble((int)input); };
            RegisterImporter(baseImportersTable, typeof(int),
                typeof(double), importer);

            importer = delegate(Object input) { return Convert.ToDecimal((double)input); };
            RegisterImporter(baseImportersTable, typeof(double),
                typeof(decimal), importer);

            importer = delegate(Object input) { return Convert.ToSingle((double)input); };
            RegisterImporter(baseImportersTable, typeof(double),
                typeof(float), importer);

            importer = delegate(Object input) { return Convert.ToUInt32((long)input); };
            RegisterImporter(baseImportersTable, typeof(long),
                typeof(uint), importer);

            importer = delegate(Object input) { return Convert.ToChar((string)input); };
            RegisterImporter(baseImportersTable, typeof(string),
                typeof(char), importer);

            importer = delegate(Object input) { return Convert.ToDateTime((string)input, datetimeFormat); };
            RegisterImporter(baseImportersTable, typeof(string),
                typeof(DateTime), importer);

            importer = delegate(Object input) { return DateTimeOffset.Parse((string)input, datetimeFormat); };
            RegisterImporter(baseImportersTable, typeof(string),
                typeof(DateTimeOffset), importer);
        }

        private static void RegisterImporter(
            IDictionary<Type, IDictionary<Type, ImporterFunc>> table,
            Type jsonType, Type valueType, ImporterFunc importer)
        {
            if (!table.ContainsKey(jsonType))
                table.Add(jsonType, new Dictionary<Type, ImporterFunc>());

            table[jsonType][valueType] = importer;
        }

        private static void WriteValue(Object obj, JsonWriter writer,
            bool writerIsPrivate,
            int depth)
        {
            if (depth > maxNestingDepth)
                throw new JsonException(
                    string.Format("Max allowed object depth reached while " +
                                  "trying to export from type {0}",
                        obj.GetType()));

            switch (obj)
            {
                case null:
                    writer.Write(null);
                    return;
                case IJsonWrapper wrapper:
                {
                    if (writerIsPrivate)
                        writer.TextWriter.Write(wrapper.ToJson());
                    else
                        wrapper.ToJson(writer);

                    return;
                }
                case string s:
                    writer.Write(s);
                    return;
                case double d:
                    writer.Write(d);
                    return;
                case float f:
                    writer.Write(f);
                    return;
                case int i:
                    writer.Write(i);
                    return;
                case bool b:
                    writer.Write(b);
                    return;
                case long l:
                    writer.Write(l);
                    return;
                case Array array:
                {
                    writer.WriteArrayStart();

                    foreach (var elem in array)
                        WriteValue(elem, writer, writerIsPrivate, depth + 1);

                    writer.WriteArrayEnd();

                    return;
                }
                case IList list:
                {
                    writer.WriteArrayStart();
                    foreach (var elem in list)
                        WriteValue(elem, writer, writerIsPrivate, depth + 1);
                    writer.WriteArrayEnd();

                    return;
                }
                case IDictionary dictionary:
                {
                    writer.WriteObjectStart();
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        var propertyName = entry.Key is string key
                            ? key
                            : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                        writer.WritePropertyName(propertyName);
                        WriteValue(entry.Value, writer, writerIsPrivate,
                            depth + 1);
                    }

                    writer.WriteObjectEnd();

                    return;
                }
            }

            var objType = obj.GetType();

            // See if there's a custom exporter for the object
            if (customExportersTable.ContainsKey(objType))
            {
                var exporter = customExportersTable[objType];
                exporter(obj, writer);

                return;
            }

            // If not, maybe there's a base exporter
            if (baseExportersTable.ContainsKey(objType))
            {
                var exporter = baseExportersTable[objType];
                exporter(obj, writer);

                return;
            }

            // Last option, let's see if it's an enum
            if (obj is Enum)
            {
                var eType = Enum.GetUnderlyingType(objType);

                if (eType == typeof(long))
                    writer.Write((long)obj);
                else if (eType == typeof(uint))
                    writer.Write((uint)obj);
                else if (eType == typeof(ulong))
                    writer.Write((ulong)obj);
                else if (eType == typeof(ushort))
                    writer.Write((ushort)obj);
                else if (eType == typeof(short))
                    writer.Write((short)obj);
                else if (eType == typeof(byte))
                    writer.Write((byte)obj);
                else if (eType == typeof(sbyte))
                    writer.Write((sbyte)obj);
                else
                    writer.Write((int)obj);

                return;
            }

            // Okay, so it looks like the input should be exported as an
            // object
            AddTypeProperties(objType);
            var props = typeProperties[objType];

            writer.WriteObjectStart();
            foreach (var pData in props)
            {
                if (Attribute.IsDefined(pData.Info, ignoreSerializableAttributeType))
                {
                    continue;
                }
                
                if (pData.IsField)
                {
                    writer.WritePropertyName(pData.Info.Name);
                    WriteValue(((FieldInfo)pData.Info).GetValue(obj),
                        writer, writerIsPrivate, depth + 1);
                }
                else
                {
                    var pInfo = (PropertyInfo)pData.Info;

                    if (pInfo.CanRead)
                    {
                        writer.WritePropertyName(pData.Info.Name);
                        WriteValue(pInfo.GetValue(obj, null),
                            writer, writerIsPrivate, depth + 1);
                    }
                }
            }

            writer.WriteObjectEnd();
        }

        #endregion

        public static void ResetIgnoreAttribute(Type type)
        {
            if (type == null)
            {
                return;
            }

            ignoreSerializableAttributeType = type;
        }


        public static string ToJson(Object obj)
        {
            lock (staticWriterLock)
            {
                staticWriter.Reset();

                WriteValue(obj, staticWriter, true, 0);

                return staticWriter.ToString();
            }
        }

        public static void ToJson(Object obj, JsonWriter writer)
        {
            WriteValue(obj, writer, false, 0);
        }

        public static JsonData ToObject(JsonReader reader)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, reader);
        }

        public static JsonData ToObject(TextReader reader)
        {
            var jsonReader = new JsonReader(reader);

            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, jsonReader);
        }

        public static JsonData ToObject(string json)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, json);
        }

        public static T ToObject<T>(JsonReader reader)
        {
            return (T)ReadValue(typeof(T), reader);
        }

        public static T ToObject<T>(TextReader reader)
        {
            var jsonReader = new JsonReader(reader);

            return (T)ReadValue(typeof(T), jsonReader);
        }

        public static T ToObject<T>(string json)
        {
            var reader = new JsonReader(json);

            return (T)ReadValue(typeof(T), reader);
        }

        public static Object ToObject(string json, Type convertType)
        {
            var reader = new JsonReader(json);

            return ReadValue(convertType, reader);
        }

        public static Object ToObject(string json, Type convertType, string filePath)
        {
            var lastIndex = filePath.LastIndexOf('/');
            if (lastIndex == -1)
            {
                throw new JsonException("路径无效，找不到文件名。");
            }

            var filename = filePath[(lastIndex + 1)..];


            var reader = new JsonReader(json);

            return ReadValue(convertType, reader, filename);
        }

        public static IJsonWrapper ToWrapper(WrapperFactory factory,
            JsonReader reader)
        {
            return ReadValue(factory, reader);
        }

        public static IJsonWrapper ToWrapper(WrapperFactory factory,
            string json)
        {
            var reader = new JsonReader(json);

            return ReadValue(factory, reader);
        }

        public static void RegisterExporter<T>(ExporterFunc<T> exporter)
        {
            ExporterFunc exporterWrapper =
                delegate(Object obj, JsonWriter writer) { exporter((T)obj, writer); };

            customExportersTable[typeof(T)] = exporterWrapper;
        }

        public static void RegisterImporter<TJson, TValue>(
            ImporterFunc<TJson, TValue> importer)
        {
            ImporterFunc importerWrapper =
                delegate(Object input) { return importer((TJson)input); };

            RegisterImporter(customImportersTable, typeof(TJson),
                typeof(TValue), importerWrapper);
        }

        public static void UnregisterExporters()
        {
            customExportersTable.Clear();
        }

        public static void UnregisterImporters()
        {
            customImportersTable.Clear();
        }
    }
}