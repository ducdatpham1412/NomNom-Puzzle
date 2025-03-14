using UnityEngine;
using UnityEngine.UI;

public class AmountEggs : MonoBehaviour {
    private Text text;


    private void UpdateProfile(AppState newState) {
        if (text != null) {
            text.text = newState.profile.eggs.ToString();
        }
    }

    void Start() {
        text = GetComponent<Text>();
        text.text = GameManager.Instance.appState.profile.eggs.ToString();
        GameManager.Instance.OnAppStateChanged += UpdateProfile;
    }


    void OnDestroy() {
        GameManager.Instance.OnAppStateChanged -= UpdateProfile;
    }
}
