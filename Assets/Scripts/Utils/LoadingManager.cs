using System.Collections;
using LottiePlugin.UI;
using UnityEngine;

public class LoadingManager : MonoBehaviour {
    private AnimatedImage animatedImage;
    private CanvasGroup canvasGroup;

    void Awake() {
        animatedImage = GetComponent<AnimatedImage>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private IEnumerator WaitAndPlay() {
        yield return new WaitUntil(() => animatedImage.isActiveAndEnabled);
        animatedImage.Play();
    }

    public void StartLoading() {
        // We have to set this because if we call loadingManager.StartLoading() in Start function of this gameobject's container, the Start function of this has not been run => See function GetLeaderBoard() in LeaderBoardLogic
        if (canvasGroup == null || animatedImage == null) {
            animatedImage = GetComponent<AnimatedImage>();
            canvasGroup = GetComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(WaitAndPlay());
    }


    public void StopLoading() {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        animatedImage.Stop();
    }
}
