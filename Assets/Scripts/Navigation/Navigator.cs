using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class Navigator : Singleton<Navigator> {
    public enum Scene {
        Welcome,
        Home,
        MatchMaking,
        GameOverview,
        GameThrowEgg,
        GameMath,
        Profile,
        ProfileEdit,
        AccountInfo,
        MatchScene,
    }
    private List<string> histories = new List<string>();

    public void NavigateTo(Scene scene) {
        string temp = scene.ToString();
        int index = histories.LastIndexOf(temp);
        if (index >= 0) {
            histories.RemoveAt(index);
            histories.Add(temp);
        }
        else {
            histories.Add(temp);
        }
        SceneManager.LoadScene(temp);
    }

    public void NetworkLoad(Scene scene) {
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
        }
    }

    public void GoBack() {
        if (CanGoBack()) {
            histories.RemoveAt(histories.Count - 1);
            SceneManager.LoadScene(histories[histories.Count - 1]);
        }
    }

    public void Push(Scene scene) {
        string temp = scene.ToString();
        histories.Add(temp);
        SceneManager.LoadScene(temp);
    }

    public bool CanGoBack() {
        return histories.Count >= 2;
    }
}
