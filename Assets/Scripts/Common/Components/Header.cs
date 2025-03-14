using UnityEngine;
using UnityEngine.UI;

public class Header : MonoBehaviour {
    [Header("GameObjects")]
    public Text EggsAmount;

    void Start() {
        EggsAmount.text = GameManager.Instance.appState.profile.eggs.ToString();
    }

    public void GoToProfile() {
        Navigator.Instance.NavigateTo(Navigator.Scene.Profile);
    }
}
