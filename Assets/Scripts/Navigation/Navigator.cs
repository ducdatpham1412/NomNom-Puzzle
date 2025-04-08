using System;
using UnityEngine.SceneManagement;

public class Navigator : Singleton<Navigator> {
    public enum Scene {
        GameScene,
        InformationScene,
    }
    public Action<string> SceneChanged;

    public void NavigateTo(Scene scene, LoadSceneMode mode = LoadSceneMode.Single) {
        string name = scene.ToString();
        SceneManager.LoadScene(name, mode);
        SceneChanged?.Invoke(name);
    }

    public void UnloadSceneAsync(Scene scene) {
        SceneManager.UnloadSceneAsync(scene.ToString());
        SceneChanged?.Invoke(SceneManager.GetActiveScene().name);
    }
}
