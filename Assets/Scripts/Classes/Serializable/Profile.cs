using System;



[Serializable]
public class Authorized {
    public string username;
    public string password;
    public string token;
    public string refresh_token;
}

[Serializable]
public class Profile {
    [Serializable]
    public class Setting {
        public string language;
    }

    public string id;
    public string account_type;
    public string email;
    public string phone;
    public string name;
    public string description;
    public string avatar;
    public int eggs;
    public int ranking;
    public Setting setting;
}


[Serializable]
public class Passport {
    public Profile profile;
}
