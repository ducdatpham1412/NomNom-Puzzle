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
    [SerializeField] Transform ScrollContent;

    [Header("Prefabs")]
    [SerializeField] GameObject ItemCreature;

    [Header("Data")]
    [SerializeField] CreaturesObject CreaturesObject;

    Dictionary<string, Relationship> Relationship = new Dictionary<string, Relationship>();
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
            ItemCreature newCr = Instantiate(ItemCreature, ScrollContent).GetComponent<ItemCreature>();
            newCr.SetCreature(cr, this, localeKey, enableClick: true);
            if (index == 0) {
                OnClickCreature(newCr);
            }
        }

        FadeInUp();
    }

    public void OnClickCreature(ItemCreature creature) {
        if (focusingCreature == creature) return;

        if (focusingCreature) {
            focusingCreature.SetActive(false);
        }
        focusingCreature = creature;
        focusingCreature.SetActive(true);

        Relationship relationship = Relationship[creature.creature.id];

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
}
