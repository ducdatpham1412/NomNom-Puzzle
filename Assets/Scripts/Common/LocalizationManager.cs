using System;
using System.Collections;
using UnityEngine.Localization.Settings;



public class LocalizationManager : Singleton<LocalizationManager> {
    protected LocalizationManager() { }

    public enum Table {
        Game,
    }

    private bool active = false;

    public void SetLocale(int localeID, Action callback = null) {
        if (active) return;
        IEnumerator _SetLocale(int localeID) {
            active = true;
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
            active = false;
            callback?.Invoke();
        }
        StartCoroutine(_SetLocale(localeID));
    }

    public string GetLocale() {
        int index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        return index == 0 ? "en" : "vi";
    }
}
