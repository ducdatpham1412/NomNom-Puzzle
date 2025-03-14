using System;

[Serializable]
public class Event {
    public interface IEvent {
        public string type { get; set; }
    }

    public class Send {
        public class Connect {
            public string type = "connect";
            public string player_id;
        }
        public class FindMatch {
            public string type = "find_match";
        }
        public class StopFindMatch {
            public string type = "stop_find_match";
        }
        public class LoadMatch {
            public string type = "load_match";
            public string match_id;
        }
        public class Disconnected {
            public string type = "disconnected";
        }
        public class ServerReady {
            public string type = "server_ready";
        }
        public class GetMoreQuestion {
            public string type = "get_more_ques";
            public int last_id;
        }
    }

    public enum Name {
        match_found,
        server_ready,
    }
}
