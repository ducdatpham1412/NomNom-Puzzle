using Newtonsoft.Json;
using UnityEngine;
using System;
using System.Collections.Generic;

public class Vector2Converter : JsonConverter<Vector2> {
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
        float[] values = serializer.Deserialize<float[]>(reader);
        return new Vector2(values[0], values[1]);
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
        serializer.Serialize(writer, new float[] { value.x, value.y });
    }
}


public class Vector2ArrayConverter : JsonConverter<Vector2[]> {
    public override Vector2[] ReadJson(JsonReader reader, Type objectType, Vector2[] existingValue, bool hasExistingValue, JsonSerializer serializer) {
        float[][] values = serializer.Deserialize<float[][]>(reader);
        List<Vector2> res = new List<Vector2>();
        foreach (float[] v in values) {
            res.Add(new Vector2 { x = v[0], y = v[1] });
        }
        return res.ToArray();
    }

    public override void WriteJson(JsonWriter writer, Vector2[] value, JsonSerializer serializer) {
        List<float[]> res = new List<float[]>();
        foreach (Vector2 v in value) {
            res.Add(new float[] { v.x, v.y });
        }
        serializer.Serialize(writer, res.ToArray());
    }
}
