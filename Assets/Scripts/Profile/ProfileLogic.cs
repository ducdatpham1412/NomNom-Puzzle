using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileLogic : MonoBehaviour {
    [Header("GameObjects")]
    public GameObject SettingDialog;
    public Transform Avatar;
    public Image EnglishButton;
    public Image VietnameseButton;
    public Text Name;
    public Text EggsAmount;
    public RectTransform ContentHistoryScrollView;
    public GameObject ContentHistory;

    [Header("Prefabs")]
    public GameObject ItemHistory;

    Color yellow;


    void Start() {
        Profile profile = GameManager.Instance.appState.profile;
        Helper.SetImageUrl(Avatar, profile.avatar);
        Name.text = profile.name;
        EggsAmount.text = profile.eggs.ToString();

        yellow = Helper.ColorFromHex(Configs.Color.yellow);
        EnglishButton.color = profile.setting.language == Configs.LocaleID.En ? yellow : Color.white;
        VietnameseButton.color = profile.setting.language == Configs.LocaleID.Vi ? yellow : Color.white;

        GetHistory();
    }


    async void GetHistory() {
        try {
            var histories = await ApiManager.GET<GameHistory[]>(
                "/game/history",
                parameters: new Dictionary<string, string>{
                    {"user_id", GameManager.Instance.appState.profile.id}
                }
            );
            foreach (var his in histories) {
                GameObject newHistory = Instantiate(ItemHistory, parent: ContentHistory.transform, worldPositionStays: false);
                newHistory.GetComponent<ItemHistory>().Initialize(his);
            }

            // We have to force Content rebuild layout, because GetHistory is async, so Instantiate occur after frame setting in Start function
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentHistoryScrollView);
        }
        catch (Exception ex) {
            Debug.Log($"Error: {ex.Message}");
        }
    }

    public void GoBack() {
        Navigator.Instance.NavigateTo(Navigator.Scene.Home);
    }

    public void ShowHideSetting() {
        if (SettingDialog != null) {
            SettingDialog.SetActive(!SettingDialog.activeInHierarchy);
        }
    }


    public void EditProfile() {
        Navigator.Instance.Push(Navigator.Scene.ProfileEdit);
    }

    public void AccountInfo() {
        Navigator.Instance.Push(Navigator.Scene.AccountInfo);
    }

    public void LogOut() {
        Navigator.Instance.Push(Navigator.Scene.Welcome);
    }

    public void SetLocale(int localeID) {
        LocalizationManager.Instance.SetLocale(localeID);
        AppState appState = GameManager.Instance.UpdateAppState(appState => {
            appState.profile.setting.language = localeID == 0 ? Configs.LocaleID.En : Configs.LocaleID.Vi;
            return appState;
        });

        EnglishButton.color = appState.profile.setting.language == Configs.LocaleID.En ? yellow : Color.white;
        VietnameseButton.color = appState.profile.setting.language == Configs.LocaleID.Vi ? yellow : Color.white;
    }
}
