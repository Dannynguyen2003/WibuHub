using System.Text.Json;

namespace WibuHub.MVC.Customer.ExtensionsMethod
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
            => session.SetString(key, JsonSerializer.Serialize(value));

        public static T? GetObject<T>(this ISession session, string key)
            => session.GetString(key) == null
               ? default
               : JsonSerializer.Deserialize<T>(session.GetString(key)!);
    }
}
