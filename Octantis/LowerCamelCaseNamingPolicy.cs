using System.Text.Json;

namespace Octantis
{
    internal class LowerCamelCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            if (name.Length > 1)
            {
                name = CamelCase.ConvertName(name);
                return char.ToLowerInvariant(name[0]) + name[1..];
            }
            return name.ToLowerInvariant();
        }
    }
}