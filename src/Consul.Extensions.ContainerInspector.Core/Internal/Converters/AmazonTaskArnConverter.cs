using Consul.Extensions.ContainerInspector.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Consul.Extensions.ContainerInspector.Core.Internal.Converters
{
    internal class AmazonTaskArnConverter : JsonConverter<AmazonTaskArn>
    {
        public override AmazonTaskArn? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return AmazonTaskArn.ParseTaskArn(reader);
        }

        public override void Write(Utf8JsonWriter writer, AmazonTaskArn value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.EncodedArn);
        }
    }
}
