using System.Collections;
using UnityEngine;

public class LoginLogic : MonoBehaviour {
    private string username = "";
    private string password = "";
    public ButtonManager buttonManager;

    public void onChangeUsername(string value) {
        username = value;
    }

    public void onChangePassword(string value) {
        password = value;
    }

    private IEnumerator PerformLogin() {
        Debug.Log($"Start login\nUsername: {username}\nPassword: {password}");
        yield return new WaitForSeconds(2f);
        buttonManager.StopLoading();
        Navigator.Instance.NavigateTo(Navigator.Scene.Home);
    }

    public void Login() {
        buttonManager.StartLoading();
        StartCoroutine(PerformLogin());
    }
}
