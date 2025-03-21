using Newtonsoft.Json;
using UnityEngine;
using System;

public class Vector2Converter : JsonConverter<Vector2> {
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
        float[] values = serializer.Deserialize<float[]>(reader);
        return new Vector2(values[0], values[1]);
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
        serializer.Serialize(writer, new float[] { value.x, value.y });
    }
}
