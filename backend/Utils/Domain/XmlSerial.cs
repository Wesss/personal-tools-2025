using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using System.Reflection;
using System.Text;
using System.Xml;

namespace DomainUtils
{
    /// <summary>
    /// Represents and object that can be serialized into xml 
    /// </summary>
    public interface IXmlSerial
    {
        /// <summary>
        /// Writes the serialized version of this file to the given stream
        /// </summary>
        public void ToXml(Stream stream);
    }

    /// <summary>
    /// static class with implementation of IXmlFields
    /// </summary>
    public abstract class XmlSerial : IXmlSerial
    {
        private static readonly IExtendedXmlSerializer ser;

        private static readonly XmlWriterSettings writeSettings = new() { Indent = true };

        static XmlSerial()
        {
            ser = new ConfigurationContainer()
                .UseAutoFormatting()
                .UseOptimizedNamespaces()
                // Issues with getting content deserialized with this feature enabled; its some null ref exception in the library
                // without good way to trace down issue.
                //.EnableParameterizedContent()
                // Remove system namespace from common C# classes/structures
                .EnableImplicitTyping(typeof(string))
                .Create();
        }

        /// <summary>
        /// Serializes the instance and writes it to the given stream
        /// </summary>
        public void ToXml(Stream stream)
        {
            ToXml(this, stream);
        }

        /// <summary>
        /// Serializes the instance and writes it to the given stream
        /// </summary>
        public static void ToXml(IXmlSerial instance, Stream stream)
        {
            // The overload XmlSerial.Serialize(stream, instance) requires stream to be readable as
            // well for some reason, which is not always the case here.
            var serialized = ser.Serialize(writeSettings, instance);
            var writer = new StreamWriter(stream);
            writer.Write(serialized);
            writer.Flush();
        }

        /// <summary>
        /// Serializes the given array to the given stream
        /// </summary>
        public static void ToXmlArray<T>(T[] instance, Stream stream) where T : IXmlSerial
        {
            var wrapper = new IXmlSerialArrayWrapper<T>()
            {
                items = instance
            };
            wrapper.ToXml(stream);
        }

        /// <summary>
        /// Serializes the given array to the given stream
        /// </summary>
        public static void ToXmlArrayFile<T>(T[] instance, string path) where T : IXmlSerial
        {
            var wrapper = new IXmlSerialArrayWrapper<T>()
            {
                items = instance
            };
            wrapper.ToXmlFile(path);
        }

        /// <summary>
        /// Serializes the given array to the given stream
        /// </summary>
        public static string ToXmlArrayString<T>(T[] instance) where T : IXmlSerial
        {
            var wrapper = new IXmlSerialArrayWrapper<T>()
            {
                items = instance
            };
            return wrapper.ToXmlString();
        }

        /// <summary>
        /// Deserializes an instance from the given source stream
        /// </summary>
        public static T FromXml<T>(Stream stream) where T : IXmlSerial
        {
            return ser.Deserialize<T>(stream);
        }

        /// <summary>
        /// Deserializes an instance from the given string
        /// </summary>
        public static T FromXml<T>(string content) where T : IXmlSerial
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return FromXml<T>(stream);
        }

        /// <summary>
        /// Deserializes an instance from a file at the given path.
        /// </summary>
        public static T FromXmlFile<T>(string path) where T : IXmlSerial
        {
            using var stream = File.OpenRead(path);
            return FromXml<T>(stream);
        }

        /// <summary>
        /// Deserializes an instance from the given source stream
        /// </summary>
        public static T[] FromXmlArray<T>(Stream stream) where T : IXmlSerial
        {
            var wrapper = FromXml<IXmlSerialArrayWrapper<T>>(stream);
            return wrapper.items;
        }

        /// <summary>
        /// Deserializes an instance from the given string
        /// </summary>
        public static T[] FromXmlArray<T>(string content) where T : IXmlSerial
        {
            var wrapper = FromXml<IXmlSerialArrayWrapper<T>>(content);
            return wrapper.items;
        }

        /// <summary>
        /// Deserializes an instance from a file at the given path.
        /// </summary>
        public static T[] FromXmlArrayFile<T>(string path) where T : IXmlSerial
        {
            var wrapper = FromXmlFile<IXmlSerialArrayWrapper<T>>(path);
            return wrapper.items;
        }
    }

    public static class IXmlSerialExtensions
    {
        /// <summary>
        /// Returns a string containing this object serialized.
        /// </summary>
        public static string ToXmlString(this IXmlSerial serial)
        {
            using var stream = new MemoryStream();
            serial.ToXml(stream);
            return new StreamReader(stream).ReadToEnd();
        }

        /// <summary>
        /// Serailizes and writes this object to the given file
        /// </summary>
        /// <remarks>
        /// A new file will be created if none exist, and will be overwritten if exists.
        /// </remarks>
        public static void ToXmlFile(this IXmlSerial serial, string filepath)
        {
            var dir = Path.GetDirectoryName(filepath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using var stream = File.Open(filepath, File.Exists(filepath) ? FileMode.Truncate : FileMode.OpenOrCreate);
            serial.ToXml(stream);
        }
    }

    internal class IXmlSerialArrayWrapper<T> : IXmlSerial where T : IXmlSerial 
    {
        public T[] items = Array.Empty<T>();

        public IXmlSerialArrayWrapper() { }

        public void ToXml(Stream stream)
        {
            XmlSerial.ToXml(this, stream);
        }
    }
}