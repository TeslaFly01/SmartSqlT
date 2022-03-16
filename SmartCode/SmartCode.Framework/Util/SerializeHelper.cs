using System;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SmartCode.Framework.Util
{
    public class SerializeHelper
    {
        public static void XmlSerialize<T>(T obj, string fileName)
        {
            XmlSerialize(obj, fileName, Encoding.UTF8);
        }

        public static void XmlSerialize<T>(T obj, string fileName,Encoding encoding)
        {
            var xmlns = new XmlSerializerNamespaces();
            xmlns.Add(String.Empty, String.Empty); 
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StreamWriter writer = new StreamWriter(fileName, false, encoding);
            serializer.Serialize(writer, obj, xmlns);
            writer.Close();
        }

        public static string XmlSerialize<T>(T obj)
        {
            return XmlSerialize(obj, Encoding.UTF8);
        }

        public static string XmlSerialize<T>(T obj,Encoding encoding)
        {
            var xmlns = new XmlSerializerNamespaces();
            xmlns.Add(String.Empty, String.Empty); 
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, obj, xmlns);
            stream.Position = 0;

            StreamReader sr = new StreamReader(stream, encoding);
            string resultStr = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return resultStr;
        }

        public static T XmlDeserialize<T>(string fileName)
        {
            return XmlDeserialize<T>(fileName, Encoding.UTF8);
        }

        public static T XmlDeserialize<T>(string fileName, Encoding encoding)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StreamReader reader = new StreamReader(fileName, encoding);
            T obj = (T)serializer.Deserialize(reader);
            reader.Close();

            return obj;
        }

        public static T XmlTextDeserialize<T>(string xmlText)
        {
            return XmlTextDeserialize<T>(xmlText, Encoding.UTF8);
        }

        public static T XmlTextDeserialize<T>(string xmlText, Encoding encoding)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(encoding.GetBytes(xmlText.ToCharArray()));
            T obj = (T)serializer.Deserialize(ms);
            ms.Close();

            return obj;
        }

        public static string XmlDataContractSerialize<T>(T obj)
        {
            return XmlDataContractSerialize(obj, Encoding.UTF8);
        }

        public static string XmlDataContractSerialize<T>(T obj, Encoding encoding)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            stream.Position = 0;

            StreamReader sr = new StreamReader(stream, encoding);
            string resultStr = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return resultStr;
        }

        public static T XmlDataContractDeserialize<T>(string xmlText)
        {
            return XmlDataContractDeserialize<T>(xmlText, Encoding.UTF8);
        }

        public static T XmlDataContractDeserialize<T>(string xmlText,Encoding encoding)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(encoding.GetBytes(xmlText.ToCharArray()));
            T obj = (T)serializer.ReadObject(ms);
            ms.Close();

            return obj;
        }

        public static string JsonSerialize<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            stream.Position = 0;

            StreamReader sr = new StreamReader(stream);
            string resultStr = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return resultStr;
        }

        public static T JsonDeserialize<T>(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json.ToCharArray()));
            T obj = (T)serializer.ReadObject(ms);
            ms.Close();

            return obj;
        }
    }
}
