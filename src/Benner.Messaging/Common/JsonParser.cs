using Newtonsoft.Json;
using System;

namespace Benner.Messaging.Common
{
    public static class JsonParser
    {
        public static T Deserialize<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Error parsing the object.", e);
            }
        }

        public static string Serialize(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Error parsing the object.", e);
            }
        }
    }
}
