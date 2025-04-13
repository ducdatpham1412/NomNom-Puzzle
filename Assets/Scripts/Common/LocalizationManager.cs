using System;
using UnityEngine.Localization.Settings;



public class LocalizationManager : Singleton<LocalizationManager> {
    public void SetLocale(int localeID, Action callback = null) {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
        callback?.Invoke();
    }

    public string GetLocale() {
        int index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        return index == 0 ? "en" : "vi";
    }
}
