using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class CreateAccountLogic : MonoBehaviour {
    private string username = "";
    private string password = "";
    public ErrorBanner errorBanner;
    public ButtonManager buttonManager;
    public WelcomeLogic welcomeLogic;


    public void onChangeUsername(string value) {
        username = value;
        if (errorBanner != null && errorBanner.gameObject.activeInHierarchy) {
            errorBanner.gameObject.SetActive(false);
        }
    }

    public void onChangePassword(string value) {
        password = value;
    }

    public void onChangePlayerName(string value) {
        onChangeUsername(value);
    }

    IEnumerator PerformCreateAccount() {
        yield return new WaitForSeconds(3f);
        buttonManager.StopLoading();
        welcomeLogic.ShowCanvas(WelcomeLogic.CanvasName.EnterName.ToString());
    }

    public void CreateAccount() {
        if (username.Length < 8) {
            errorBanner.Show(Helper.GetLocalizedValue(LocalizationManager.Table.Welcome, "usernameHasBeenTaken"));
            return;
        }
        buttonManager.StartLoading();
        StartCoroutine(PerformCreateAccount());
    }

    public async void ChangePlayerName() {
        if (username.Length < 5) {
            errorBanner.Show(Helper.GetLocalizedValue(LocalizationManager.Table.Error, "name_min_letter", new object[] { 5 }));
            return;
        }

        try {
            var res = await ApiManager.POST<JObject>(
                "/auth/login",
                data: new Dictionary<string, object> {
                    {"device_id", Helper.DeviceID},
                    {"name", username},
                    {"language", LocalizationManager.Instance.GetLocale()}
                },
                parameters: new Dictionary<string, string> {
                    {"type", "device_id"},
                }
            );
            ApiManager.SetAccount(resLogin: res);
            GameManager.Instance.appState.resource = await ApiManager.GET<Resource>("/common/resource");
            var passport = await ApiManager.GET<Passport>("/common/passport");
            GameManager.Instance.appState.profile = passport.profile;
            Storage.SetProfile(passport.profile);
            Navigator.Instance.NavigateTo(Navigator.Scene.Home);
        }
        catch (Exception e) {
            errorBanner.Show(e.Message);
        }
        finally {
            buttonManager.StopLoading();
        }
    }
}
