namespace SolarWinds.DLLChecker.Backend.Helpers
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public static class SerializeHelper
    {
        public static T Deserialize<T>(string serializedObject)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(serializedObject))
            {
                return (T) serializer.Deserialize(reader);
            }
        }

        public static string Serialize<T>(T objectGraph)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize((TextWriter) writer, objectGraph);
                return writer.ToString();
            }
        }
    }
}

