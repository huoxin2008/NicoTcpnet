using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Nico.Tcpnet
{
    public static class JsonSeriHelp
    {
        public static byte[] Serialize(object obj)
        {

            DataContractJsonSerializer json = new DataContractJsonSerializer(obj.GetType());

            using (MemoryStream stream = new MemoryStream())
            {
                json.WriteObject(stream, obj);

                return stream.ToArray();

            }

        }

        //json反序列化

        public static T Deserialize<T>(byte[] data, int id = 0)
        {
            var len = data.Length - id;

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream(data, id, len))
            {
                return (T)ser.ReadObject(ms);
            }

        }

        public static object Deserialize(Type t, byte[] data, int id = 0)
        {
            var len = data.Length - id;

            DataContractJsonSerializer ser = new DataContractJsonSerializer(t);

            using (MemoryStream ms = new MemoryStream(data, id, len))
            {
                return ser.ReadObject(ms);
            }

        }

    }
}
