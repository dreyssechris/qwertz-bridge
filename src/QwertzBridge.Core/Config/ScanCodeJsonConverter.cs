using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QwertzBridge.Core.Config;

/// <summary>
/// Reads scan codes either as JSON numbers (51) or as hex strings ("0x33");
/// writes them as hex strings for readability.
/// </summary>
public sealed class ScanCodeJsonConverter : JsonConverter<ushort>
{
    /// <inheritdoc />
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

        throw new JsonException($"Invalid scan code: expected a number or hex string like \"0x33\".");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ushort value, JsonSerializerOptions options) =>
        writer.WriteStringValue($"0x{value:X2}");
}
