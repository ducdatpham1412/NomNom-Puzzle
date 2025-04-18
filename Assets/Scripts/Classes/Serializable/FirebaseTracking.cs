
public class FirebaseTracking {
    static bool isReady = false;

    public enum Event {
        custom_app_open,
        start_level,
        finish_level,
        open_level,
        setting_language,
    }

    public static void Initialize() {

    }

    static void OpenApp() {
        if (!isReady) return;
        // Params: DeviceID, App Version, Helper.Timestamp
    }

    public static void StartLevel(int level) {
        if (!isReady) return;
        // Params: DeviceID, Level, Helper.Timestamp
    }

    public static void FinishLevel(int level) {
        if (!isReady) return;
        // Params: DeviceID, Level, Helper.Timestamp
    }

    public static void OpenLevel(int level) {
        if (!isReady) return;
        // Params: DeviceID, Level, Helper.Timestamp
    }

    public static void SetLanguage(string lan) {
        if (!isReady) return;
        // Params: DeviceID, lan, Helper.Timestamp
    }
}