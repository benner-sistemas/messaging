using Newtonsoft.Json;
using System;

namespace Benner.Messaging.Configuration
{
    public static class JsonParser
    {
        public static T Deserialize<T>(string content, JsonSerializerSettings settings = null)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content, settings ?? new JsonSerializerSettings
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

        public static string Serialize(object obj, JsonSerializerSettings settings = null)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, settings ?? new JsonSerializerSettings
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
