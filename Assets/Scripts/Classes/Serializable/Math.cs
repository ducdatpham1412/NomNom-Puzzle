
using System;
using System.Collections.Generic;
using Unity.Netcode;


[Serializable]
public class RoomMath : BaseRoom, INetworkSerializable, IEquatable<RoomMath> {

    public List<Player> players;
    public List<Question> questions; // questions should only saved in server, not serializer to network

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        if (serializer.IsWriter) {
            int playersCount = players.Count;
            // int questionsCount = questions.Count;
            serializer.SerializeValue(ref playersCount);
            // serializer.SerializeValue(ref questionsCount);
            for (int i = 0; i < playersCount; i++) {
                players[i].NetworkSerialize(serializer);
            }
            // for (int i = 0; i < questionsCount; i++) {
            //     questions[i].NetworkSerialize(serializer);
            // }
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(id);
            writer.WriteValueSafe(type);
            writer.WriteValueSafe(status);
        }
        else {
            int playersCount = 0;
            // int questionsCount = 0;
            serializer.SerializeValue(ref playersCount);
            // serializer.SerializeValue(ref questionsCount);
            players = new List<Player>();
            for (int i = 0; i < playersCount; i++) {
                Player p = new Player();
                p.NetworkSerialize(serializer);
                players.Add(p);
            }
            // questions = new List<Question>();
            // for (int i = 0; i < questionsCount; i++) {
            //     Question e = new Question();
            //     e.NetworkSerialize(serializer);
            //     questions.Add(e);
            // }
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out id);
            reader.ReadValueSafe(out type);
            reader.ReadValueSafe(out status);
        }
    }

    public bool Equals(RoomMath other) {
        return players.Equals(other.players) && questions.Equals(other.questions) && status == other.status;
    }

    [Serializable]
    public class Player : INetworkSerializable, IEquatable<Player> {
        public string id;
        public int point;

        public void CopyProperties(Player other) {
            point = other.point;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(id);
                writer.WriteValueSafe(point);
            }
            else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out point);
            }
        }

        public bool Equals(Player other) {
            return id == other.id && point == other.point;
        }
    }

    [Serializable]
    public class Question : INetworkSerializable, IEquatable<Question> {
        public int id;
        public string ques;
        public float result;
        public List<float> selections;
        public float created;
        public List<Answer> answers;
        public string winner;
        public string status; // active, ended

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsWriter) {
                int selectionsCount = selections.Count;
                int answersCount = answers.Count;
                serializer.SerializeValue(ref selectionsCount);
                serializer.SerializeValue(ref answersCount);
                for (int i = 0; i < answersCount; i++) {
                    answers[i].NetworkSerialize(serializer);
                }
                var writer = serializer.GetFastBufferWriter();
                for (int i = 0; i < selectionsCount; i++) {
                    writer.WriteValueSafe(selections[i]);
                }
                writer.WriteValueSafe(id);
                writer.WriteValueSafe(ques);
                writer.WriteValueSafe(result);
                writer.WriteValueSafe(created);
                writer.WriteValueSafe(winner);
                writer.WriteValueSafe(status);
            }
            else {
                int selectionsCount = 0;
                int answersCount = 0;
                serializer.SerializeValue(ref selectionsCount);
                serializer.SerializeValue(ref answersCount);
                answers = new List<Answer>();
                for (int i = 0; i < answersCount; i++) {
                    Answer a = new Answer();
                    a.NetworkSerialize(serializer);
                    answers.Add(a);
                }
                var reader = serializer.GetFastBufferReader();
                selections = new List<float>();
                for (int i = 0; i < selectionsCount; i++) {
                    float v;
                    reader.ReadValueSafe(out v);
                    selections.Add(v);
                }
                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out ques);
                reader.ReadValueSafe(out result);
                reader.ReadValueSafe(out created);
                reader.ReadValueSafe(out winner);
                reader.ReadValueSafe(out status);
            }
        }

        public bool Equals(Question other) {
            return id == other.id && ques == other.ques && result == other.result && selections.Equals(other.selections) && created == other.created && answers.Equals(other.answers) && winner == other.winner && status == other.status;
        }

        [Serializable]
        public class Answer : INetworkSerializable, IEquatable<Answer> {
            public float select;
            public string creator;
            public long created;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
                if (serializer.IsWriter) {
                    var writer = serializer.GetFastBufferWriter();
                    writer.WriteValueSafe(select);
                    writer.WriteValueSafe(created);
                    writer.WriteValueSafe(creator);
                }
                else {
                    var reader = serializer.GetFastBufferReader();
                    reader.ReadValueSafe(out select);
                    reader.ReadValueSafe(out created);
                    reader.ReadValueSafe(out creator);
                }
            }

            public bool Equals(Answer other) {
                return select == other.select && creator == other.creator && created == other.created;
            }
        }

        public enum Status {
            active,
            ended,
        }
    }
}
