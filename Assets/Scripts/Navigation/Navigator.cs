using UnityEngine.SceneManagement;

public class Navigator : Singleton<Navigator> {
    public enum Scene {
        GameScene,
        InformationScene,
    }

    public void NavigateTo(Scene scene, LoadSceneMode mode = LoadSceneMode.Single) {
        string temp = scene.ToString();
        SceneManager.LoadScene(temp, mode);
    }

    public void UnloadSceneAsync(Scene scene) {
        SceneManager.UnloadSceneAsync(scene.ToString());
    }
}
