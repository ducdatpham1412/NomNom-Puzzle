
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;


[Serializable]
public class RoomThrowEgg : BaseRoom, INetworkSerializable, IEquatable<RoomThrowEgg> {
    public List<Player> players;
    public List<Egg> eggs;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        if (serializer.IsWriter) {
            int playersCount = players.Count;
            int eggsCount = eggs.Count;
            serializer.SerializeValue(ref playersCount);
            serializer.SerializeValue(ref eggsCount);
            for (int i = 0; i < playersCount; i++) {
                players[i].NetworkSerialize(serializer);
            }
            for (int i = 0; i < eggsCount; i++) {
                eggs[i].NetworkSerialize(serializer);
            }
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(type);
            writer.WriteValueSafe(status);
        }
        else {
            int playersCount = 0;
            int eggsCount = 0;
            serializer.SerializeValue(ref playersCount);
            serializer.SerializeValue(ref eggsCount);
            players = new List<Player>();
            for (int i = 0; i < playersCount; i++) {
                Player p = new Player();
                p.NetworkSerialize(serializer);
                players.Add(p);
            }
            eggs = new List<Egg>();
            for (int i = 0; i < eggsCount; i++) {
                Egg e = new Egg();
                e.NetworkSerialize(serializer);
                eggs.Add(e);
            }
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out id);
            reader.ReadValueSafe(out type);
            reader.ReadValueSafe(out status);
        }
    }


    public bool Equals(RoomThrowEgg other) {
        return players.Equals(other.players) && eggs.Equals(other.eggs) && status == other.status;
    }


    [Serializable]
    public class Player : INetworkSerializable, IEquatable<Player> {
        public string id;
        public float move_speed;
        public float shot_speed;
        public int point;

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 init_pos;


        public void CopyProperties(Player other) {
            move_speed = other.move_speed;
            shot_speed = other.shot_speed;
            point = other.point;
            init_pos = other.init_pos;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(id);
                writer.WriteValueSafe(move_speed);
                writer.WriteValueSafe(shot_speed);
                writer.WriteValueSafe(point);
                writer.WriteValueSafe(init_pos);
            }
            else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out move_speed);
                reader.ReadValueSafe(out shot_speed);
                reader.ReadValueSafe(out point);
                reader.ReadValueSafe(out init_pos);
            }
        }

        public bool Equals(Player other) {
            return id == other.id && move_speed == other.move_speed && shot_speed == other.shot_speed && point == other.point && init_pos.Equals(other.init_pos);
        }
    }

    [Serializable]
    public class Egg : INetworkSerializable, IEquatable<Egg> {
        public int id;
        public float move_speed;
        public float shot_speed;
        public float created;
        public string creator;
        public string status; // active, hit, missed

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(id);
                writer.WriteValueSafe(move_speed);
                writer.WriteValueSafe(shot_speed);
                writer.WriteValueSafe(created);
                writer.WriteValueSafe(creator);
                writer.WriteValueSafe(status);
            }
            else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out move_speed);
                reader.ReadValueSafe(out shot_speed);
                reader.ReadValueSafe(out created);
                reader.ReadValueSafe(out creator);
                reader.ReadValueSafe(out status);
            }
        }

        public bool Equals(Egg other) {
            return move_speed == other.move_speed && shot_speed == other.shot_speed && status == other.status;
        }
    }
}
