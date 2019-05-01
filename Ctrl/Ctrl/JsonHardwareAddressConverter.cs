using System;
using Newtonsoft.Json;

public class JsonHardwareAddressConverter : JsonConverter<HardwareAddress>
{
    public JsonHardwareAddressConverter()
    {

    }

    public override void WriteJson(JsonWriter writer, HardwareAddress value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override HardwareAddress ReadJson(JsonReader reader, Type objectType, HardwareAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new HardwareAddress((string)reader.Value);
    }

    public override bool CanRead => true;

    public override bool CanWrite => false;

}