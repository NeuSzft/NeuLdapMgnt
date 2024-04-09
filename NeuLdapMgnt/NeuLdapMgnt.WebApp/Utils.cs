using System.Text.Json;

namespace NeuLdapMgnt.WebApp
{
    public class Utils
    {
        public int Zero { get; set; } = 0;
        public static T? GetClone<T>(T obj)
        {
            if (obj is null) return default;

            string objJson = JsonSerializer.Serialize<T>(obj);
            return JsonSerializer.Deserialize<T>(objJson);
        }
    }
}
