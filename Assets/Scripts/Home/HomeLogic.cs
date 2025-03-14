using UnityEngine;

public class HomeLogic : MonoBehaviour {
    public void PauseUnPauseMusicBackground() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }

    public void GoToMatchMaking() {
        Navigator.Instance.NavigateTo(Navigator.Scene.MatchMaking);
    }
}
