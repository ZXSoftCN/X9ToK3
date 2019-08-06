using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace K3ToX9BillTransfer
{
    public static class XmlSerializerHelper
    {
        private static Dictionary<Type, XmlSerializer> _cache;
        private static XmlSerializerNamespaces _defaultNamespace;

        static XmlSerializerHelper()
        {
            _defaultNamespace = new XmlSerializerNamespaces();
            //_defaultNamespace.Add(string.Empty, string.Empty);
            //*轩辕天恩 序列化命名空间
            _defaultNamespace.Add("xsi", @"http://www.w3.org/2001/XMLSchema-instance");
            _defaultNamespace.Add("xsd", @"http://www.w3.org/2001/XMLSchema");

            _cache = new Dictionary<Type, XmlSerializer>();
        }

        private static void XmlSerializeInternal(Stream stream, object o, Encoding encoding)
        {
            if (o == null)
                throw new ArgumentNullException("o");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            XmlSerializer serializer = new XmlSerializer(o.GetType());
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            settings.Encoding = encoding;
            settings.IndentChars = "    ";

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, o);
                writer.Close();
            }
        }

        private static XmlSerializer GetSerializer<T>()
        {
            var type = typeof(T);
            if (_cache.ContainsKey(type))
            {
                return _cache[type];
            }
            var serializer = XmlSerializer.FromTypes(new[] { typeof(T) }).FirstOrDefault();
            _cache.Add(type, serializer);

            return serializer;
        }


        public static string XmlSerialize<T>(this T obj, Encoding encoding)
        {
            using (var memoryStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineChars = "\r\n";
                settings.Encoding = encoding;
                settings.IndentChars = "";

                using (XmlWriter writer = XmlWriter.Create(memoryStream, settings))
                {
                    GetSerializer<T>().Serialize(writer, obj, _defaultNamespace);
                    writer.Close();
                }

                //GetSerializer<T>().Serialize(memoryStream, obj,_defaultNamespace);

                memoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(memoryStream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 从XML字符串中反序列化对象.采用缓存提供性能，同时解决了.net2.0到4.0。
        /// 会因为Net XmlSerializer抛出FileNotFound的内部异常。
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="xml">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserialize<T>(this string xml, Encoding encoding)
        {
            using (var memoryStream = new MemoryStream(encoding.GetBytes(xml)))
            {
                var obj = GetSerializer<T>().Deserialize(memoryStream);
                return obj == null ? default(T) : (T)obj;
            }
        }
    }
}
