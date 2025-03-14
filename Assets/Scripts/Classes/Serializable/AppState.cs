using System;

[Serializable]
public class Resource {
    public string[] avatars;
}


[Serializable]
public class Account {
    public string username;
    public string password;
    public string token;
    public string refresh_token;
}

[Serializable]
public class ClientValue {
    public string player_id;
    public string match_ip;
    public int? match_port;
    public float last_seen;
    public string status; // active, finding_match, in_game, in_game_dis
}


[Serializable]
public class AppState {
    public Resource resource;
    public Profile profile;
    public Account account;
    public ClientValue client;
}
