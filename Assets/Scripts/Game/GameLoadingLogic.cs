using System.Collections;
using UnityEngine;

public class GameLoadingLogic : MonoBehaviour {
    void Start() {
        StartCoroutine(GoToGameOverview());
    }

    private IEnumerator GoToGameOverview() {
        yield return new WaitForSeconds(2f);
        Navigator.Instance.NavigateTo(Navigator.Scene.GameOverview);
    }
}
