using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class Profile {
    public string device_id;
    public string language;
    public int current_level;
}


[Serializable]
public class Creature {
    public string id;
    public Type type;
    public List<Name> name;
    public Sprite sprite;
    public float rotationOffset = 0f;

    // public void OnValidate() {
    //     if (name == null) {
    //         name = new List<Name>();
    //     }

    //     string[] defaultKeys = { "en", "jp", "ko", "vi" };

    //     // Add missing keys
    //     foreach (string key in defaultKeys) {
    //         if (!name.Exists(n => n.key == key)) {
    //             name.Add(new Name { key = key, value = "" });
    //         }
    //     }
    // }

    public enum Type {
        animal,
        food,
        plant,
    }

    [Serializable]
    public class Name {
        public string key;
        public string value;
    }
}


[Serializable]
public class Relationship {
    public List<string> eat;
    public List<string> eaten;
}


[Serializable]
public class Item {
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 pos;
    public string creature_id;
    public string direction;

    public enum Direction {
        up,
        left,
        down,
        right,
    }
}



[Serializable]
public class Level {
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 size;
    public Item[][] data;
    public string status;
}
