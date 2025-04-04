using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class GameState {
    public List<PlayingLevel> playingLevels;

    [Serializable]
    public class PlayingLevel {
        public int level;
        public List<Matching> matchings;

        [Serializable]
        public class Matching {
            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 itemPos;

            [JsonConverter(typeof(Vector2Converter))]
            public Vector2 squarePos;
        }
    }
}


[Serializable]
public class World {
    public float maxX;
    public float maxY;
}

