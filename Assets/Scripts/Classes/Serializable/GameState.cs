using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Unity.Netcode;


[Serializable]
public class FindingMatchState {
    public int secondsElapsed;
}


[Serializable]
public class BaseRoom {
    public enum Type {
        throw_egg,
        math,
        flappy_egg,
    }
    public enum Status {
        active,
        ended,
    }

    public string id;
    public string type;
    public string status;
}


[Serializable]
public class MatchState : INetworkSerializable, IEquatable<MatchState> {
    public string id;
    public Configs configs;
    public List<Player> players;
    public List<JObject> rooms;
    public float created;
    public float ended;
    public string status; // active, ended, disconnected

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        if (serializer.IsWriter) {
            int playersCount = players.Count;
            int roomsCount = rooms.Count;
            serializer.SerializeValue(ref playersCount);
            serializer.SerializeValue(ref roomsCount);

            serializer.SerializeValue(ref configs);

            for (int i = 0; i < playersCount; i++) {
                players[i].NetworkSerialize(serializer);
            }
            for (int i = 0; i < roomsCount; i++) {
                string jsonString = rooms[i].ToString();
                serializer.SerializeValue(ref jsonString);
            }

            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(created);
            writer.WriteValueSafe(ended);
            writer.WriteValueSafe(status);
        }
        else {
            int playersCount = 0;
            int roomsCount = 0;
            serializer.SerializeValue(ref playersCount);
            serializer.SerializeValue(ref roomsCount);

            if (configs == null) {
                configs = new Configs();
            }
            serializer.SerializeValue(ref configs);

            players = new List<Player>();
            for (int i = 0; i < playersCount; i++) {
                Player p = new Player();
                p.NetworkSerialize(serializer);
                players.Add(p);
            }
            rooms = new List<JObject>();
            for (int i = 0; i < roomsCount; i++) {
                string jsonString = string.Empty;
                serializer.SerializeValue(ref jsonString);
                JObject jObject = JObject.Parse(jsonString);
                rooms.Add(jObject);
            }

            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out id);
            reader.ReadValueSafe(out created);
            reader.ReadValueSafe(out ended);
            reader.ReadValueSafe(out status);
        }
    }

    public bool Equals(MatchState other) {
        if (other.players.Count != players.Count) {
            return false;
        }

        if (other.rooms.Count != rooms.Count) {
            return false;
        }

        if (!configs.Equals(other.configs) || !players.Equals(other.players) || !rooms.Equals(other.rooms)) {
            return false;
        }

        return id == other.id && created == other.created && ended == other.ended && status == other.status;
    }


    public class Configs : INetworkSerializable, IEquatable<Configs> {
        public float maxX;
        public float maxY;
        public string ip;
        public int port;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(maxX);
                writer.WriteValueSafe(maxY);
                writer.WriteValueSafe(ip);
                writer.WriteValueSafe(port);
            }
            else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out maxX);
                reader.ReadValueSafe(out maxY);
                reader.ReadValueSafe(out ip);
                reader.ReadValueSafe(out port);
            }
        }

        public bool Equals(Configs other) {
            return maxX == other.maxX && maxY == other.maxY && ip == other.ip && port == other.port;
        }

    }

    public class Player : INetworkSerializable, IEquatable<Player> {
        public string id;
        public string name;
        public string avatar;
        public int clientID; // = -1 if client has not connected or disconnected
        public string status;

        public enum Status {
            active,
            ready,
            disconnected,
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(id);
                writer.WriteValueSafe(name);
                writer.WriteValueSafe(avatar);
                writer.WriteValueSafe(status);
            }
            else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out name);
                reader.ReadValueSafe(out avatar);
                reader.ReadValueSafe(out status);
            }
        }

        public bool Equals(Player other) {
            return id == other.id && name == other.name && avatar == other.avatar;
        }
    }

    public enum Status {
        active,
        ended,
        disconnected,
    }
}


[Serializable]
public class GameState {
    public Status status;
    public FindingMatchState data;
    public MatchState matchState;
    public int my_team;
    public enum Status {
        active,
        findingMatch,
        inGame,
    }
}


[Serializable]
public class World {
    public float maxX;
    public float maxY;
}


[Serializable]
public class GameHistory {
    public string id;
    public Room[] rooms;
    public float created;
    public float ended;
    public string status; // MathState.Status

    [Serializable]
    public class Room {
        public string id;
        public Player[] players;
        public string type;
        public string status;

        [Serializable]
        public class Player {
            public string id;
            public int point;
        }
    }
}