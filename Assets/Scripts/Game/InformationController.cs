using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class InformationController : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] RectTransform Body;
    [SerializeField] RectTransform EatContent;
    [SerializeField] Transform EatContainer;
    [SerializeField] Transform EatenContainer;

    [Header("ScrollView")]
    [SerializeField] ScrollRect ScrollRect;
    [SerializeField] GridLayoutGroup ScrollContent;

    [Header("Prefabs")]
    [SerializeField] GameObject ItemCreature;

    [Header("Data")]
    [SerializeField] CreaturesObject CreaturesObject;

    Dictionary<string, Relationship> Relationship = new Dictionary<string, Relationship>();
    List<ItemCreature> CreaturesContent = new List<ItemCreature>();
    string localeKey;
    ItemCreature focusingCreature;

    void Awake() {
        SoundManager.Instance.PlaySF(SoundManager.SF.Whoosh_Transition);
    }

    void Start() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/relationship");
        string json = jsonFile.text;
        Relationship = JsonConvert.DeserializeObject<Dictionary<string, Relationship>>(json);

        localeKey = GetLocaleKey();

        foreach (var (cr, index) in CreaturesObject.Creatures.Select((v, i) => (v, i))) {
            ItemCreature newCr = Instantiate(ItemCreature, ScrollContent.transform).GetComponent<ItemCreature>();
            newCr.SetCreature(cr, this, localeKey);
            CreaturesContent.Add(newCr);
            if (index == 0) {
                OnClickCreature(cr.id, false);
            }
        }

        FadeInUp();
    }

    public void OnClickCreature(string creatureID, bool playSF = true) {
        if (focusingCreature) {
            if (focusingCreature.creatureID == creatureID) return;
            focusingCreature.SetActive(false);
        }
        if (playSF) SoundManager.Instance.PlaySF(SoundManager.SF.Pop_01);
        int _index = CreaturesContent.FindIndex
        (c => c.creatureID == creatureID);
        var findCr = CreaturesContent[_index];
        focusingCreature = findCr;
        focusingCreature.SetActive(true);
        Relationship relationship = Relationship[creatureID];

        void FillCreature(List<string> listCrId, Transform content) {
            foreach (var (crId, index) in listCrId.Select((v, i) => (v, i))) {
                Creature findCr = CreaturesObject.Creatures.Find(c => c.id == crId);
                if (index < content.childCount) {
                    content.GetChild(index).gameObject.SetActive(true);
                    content.GetChild(index).GetComponent<ItemCreature>().SetCreature(findCr, this, localeKey);
                }
                else {
                    ItemCreature newCr = Instantiate(ItemCreature, content).GetComponent<ItemCreature>();
                    newCr.SetCreature(findCr, this, localeKey);
                }
            }

            for (int i = listCrId.Count; i < content.childCount; i++) {
                content.GetChild(i).gameObject.SetActive(false);
            }
        }

        FillCreature(relationship.eat, EatContainer);
        FillCreature(relationship.eaten, EatenContainer);

        LayoutRebuilder.ForceRebuildLayoutImmediate(EatContent);

        if (playSF) {
            ScrollToIndex(_index);
        }
    }

    public string GetLocaleKey() {
        string key = Helper.GetLocaleKey();
        if (key == "en-US") return "en";
        if (key == "ja") return "jp";
        if (key == "ko") return "ko";
        if (key == "vi") return "vi";
        return "";
    }

    public void GoBackGameScene() {
        Navigator.Instance.UnloadSceneAsync(Navigator.Scene.InformationScene);
    }

    public void GoToTutorial() {
        var tutorial = GameManager.Instance.Controller.GameTutorial;
        if (!tutorial.isTutorial) {
            tutorial.SetUpForTutorial();
        }
        GoBackGameScene();
    }

    void FadeInUp() {
        CanvasGroup canvasGroup = Body.GetComponent<CanvasGroup>();
        float moveY = 150f;
        float duration = 0.7f;

        canvasGroup.alpha = 0f;
        Body.anchoredPosition -= new Vector2(0, moveY);

        LeanTween.value(gameObject, 0f, 1f, duration)
            .setOnUpdate((float val) => canvasGroup.alpha = val);

        LeanTween.moveY(Body, Body.anchoredPosition.y + moveY, duration)
            .setEase(LeanTweenType.easeOutCubic);
    }

    void ScrollToIndex(int index) {
        int columns = Mathf.CeilToInt((float)ScrollContent.transform.childCount / ScrollContent.constraintCount);
        int scrollToIndex = index / ScrollContent.constraintCount;
        scrollToIndex = Mathf.Max(0, scrollToIndex - 1);
        float colWidth = ScrollContent.cellSize.x + ScrollContent.spacing.x;
        float totalContentWidth = colWidth * columns - ScrollContent.spacing.x + ScrollContent.padding.left + ScrollContent.padding.right;
        float viewportWidth = ScrollRect.viewport.rect.width;
        float targetX = colWidth * scrollToIndex;
        targetX = Mathf.Clamp(targetX, 0, Mathf.Max(0, totalContentWidth - viewportWidth));

        RectTransform rect = ScrollContent.GetComponent<RectTransform>();
        Vector2 currentPos = rect.localPosition;
        Vector2 targetPos = new Vector3(-targetX, currentPos.y);
        StopAllCoroutines();
        StartCoroutine(ScrollCoroutine(rect, currentPos, targetPos));
    }

    IEnumerator ScrollCoroutine(RectTransform rect, Vector2 from, Vector2 to) {
        float duration = 0.2f;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            Vector2 pos = Vector2.Lerp(from, to, elapsedTime / duration);
            rect.localPosition = pos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rect.localPosition = to;
    }
}
