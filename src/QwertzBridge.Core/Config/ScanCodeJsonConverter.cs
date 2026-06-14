using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QwertzBridge.Core.Config;

// Reads scan codes as numbers (51) or hex strings ("0x33"); writes them as hex.
public sealed class ScanCodeJsonConverter : JsonConverter<ushort>
{
    public override ushort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetUInt16();

        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString() ?? "";
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                && ushort.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
            {
                return hex;
            }

            if (ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec))
                return dec;
        }

        throw new JsonException("Invalid scan code: expected a number or a hex string like \"0x33\".");
    }

    public override void Write(Utf8JsonWriter writer, ushort value, JsonSerializerOptions options) =>
        writer.WriteStringValue($"0x{value:X2}");
}
