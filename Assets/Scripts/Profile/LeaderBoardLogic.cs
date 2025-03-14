using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Components;

public class LeaderBoardLogic : MonoBehaviour {
    public GameObject ItemLeaderBoardPrefab;
    public LoadingManager loadingManager;
    public GameObject ScrollView;
    public LocalizeStringEvent YourRank;

    private Transform Content;


    void Start() {
        YourRank.StringReference.Arguments = new object[] { $"{GameManager.Instance.appState.profile.ranking}" };
        Content = Helper.FindChildRecursive(ScrollView.transform, "Content");
        GetLeaderBoard();
    }


    public async void GetLeaderBoard() {
        loadingManager.StartLoading();
        await Task.Delay(8000);
        ScrollView.SetActive(true);
        loadingManager.StopLoading();
        for (int i = 0; i <= 3; i++) {
            Instantiate(ItemLeaderBoardPrefab, Vector3.zero, Quaternion.identity, Content);
        }
    }

    public void GoBack() {
        if (Navigator.Instance.CanGoBack()) {
            Navigator.Instance.GoBack();
        }
        else {
            Navigator.Instance.NavigateTo(Navigator.Scene.Home);
        }
    }
}
