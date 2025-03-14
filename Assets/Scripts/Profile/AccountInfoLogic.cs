using TMPro;
using UnityEngine;

public class AccountInfoLogic : MonoBehaviour {
    public TMP_InputField username;
    public TMP_InputField password;


    void Start() {
        Account account = GameManager.Instance.appState.account;
        username.text = account.username;
        password.contentType = TMP_InputField.ContentType.Password;
        password.text = account.password;
        password.ForceLabelUpdate();
    }
}
