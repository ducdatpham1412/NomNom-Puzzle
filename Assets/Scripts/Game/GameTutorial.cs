using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameTutorial : MonoBehaviour {
    [SerializeField] GameObject HandController;
    [SerializeField] RectTransform InfoButton;
    [SerializeField] BlackCover BlackCover;
    [SerializeField] GameObject UIContainer;
    [SerializeField] Button SuggestBtn;

    [Header("Stats")]
    public bool isTutorial = false;
    public GameObject StepGameObject;
    public Action GameInitializedAction;
    public Action StartPanItemAction;
    public Action OnFocusAction;

    GameController Controller;
    BlackCover _BlackCover;
    GameObject _HandController;
    int delay = 200;
    Dictionary<GameObject, PingEvent> pingEvents = new();

    void Start() {
        Navigator.Instance.SceneChanged += (name) => {
            if (name == Navigator.Scene.GameScene.ToString()) {
                if (OnFocusAction != null) {
                    OnFocusAction.Invoke();
                    OnFocusAction = null;
                }
                Controller.isFocusPlayingGame = true;
            }
            else {
                Controller.isFocusPlayingGame = false;
            }
        };
        Controller = GameManager.Instance.Controller;
    }

    public void SetUpForTutorial() {
        isTutorial = true;
        GameInitializedAction = StartTutorial;
        Controller.GoToLevel(1);
    }

    public void StartTutorial() {
        Controller = GameManager.Instance.Controller;
        SuggestBtn.interactable = false;
        isTutorial = true;
        StepGameObject = null;
        Controller.InfoDialog.Open(new InfoDialog.Info {
            title = Helper.GetLocalizedValue("startTutorial"),
            fontSize = 16,
            btnTitle = "Ok",
            OnClick = () => {
                Controller.InfoDialog.Close();
                Step02();
            },
            canClose = false,
        });
        if (_BlackCover == null) {
            _BlackCover = Instantiate(BlackCover, UIContainer.transform);
            _BlackCover.GetComponent<Canvas>().worldCamera = Camera.main;
        }
    }

    void Ping(GameObject gObject) {
        if (pingEvents.ContainsKey(gObject)) {
            StopPing(gObject);
        }
        pingEvents.Add(
            gObject,
            new PingEvent {
                LT = new LTDescr(),
                originalScale = gObject.transform.localScale,
            }
        );
        GameHelper.ScalePingPong(gObject, gObject.transform.localScale * 1.2f,
            OnChange: (LTDescr lt) => {
                pingEvents[gObject].LT = lt;
            },
            delay: 3f
        );
    }

    void StopPing(GameObject gObject) {
        LeanTween.cancel(gObject);
        LeanTween.cancel(pingEvents[gObject].LT.id);
        gObject.transform.localScale = pingEvents[gObject].originalScale;
        pingEvents.Remove(gObject);
    }

    void Step02() {
        ItemController cheese = Controller.GameInit.Items.Find(item => item.square != null);
        Controller.ToolTip.Open(cheese.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("deliciousCheese"),
            OnClick = () => {
                StopPing(cheese.gameObject);
                RunStep(Step03);
            },
        });
        _BlackCover.Target(cheese.gameObject);
        Ping(cheese.gameObject);
    }

    void Step03() {
        ItemController mouse = Controller.GameInit.Items.Find(item => item.square == null);
        Controller.ToolTip.Open(mouse.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("mouseLookToTheLeft"),
            OnClick = () => RunStep(() => Step04(mouse)),
        });
        _BlackCover.Target(mouse.gameObject);
        Ping(mouse.gameObject);
    }

    void Step04(ItemController mouse) {
        Controller.ToolTip.Open(mouse.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("needToFeedTheMouse"),
            OnClick = () => {
                StopPing(mouse.gameObject);
                RunStep(() => Step05(mouse));
            },
        });
    }

    void Step05(ItemController mouse) {
        StepGameObject = mouse.gameObject;
        _BlackCover.Close();
        _HandController = Instantiate(HandController, UIContainer.transform);
        _HandController.transform.position = mouse.transform.position;
        Square emptySquare = Controller.GameInit.Squares[0][1];
        SpriteRenderer sr = _HandController.GetComponent<SpriteRenderer>();
        Color color = sr.color;
        void MoveHand() {
            if (_HandController == null) return;
            _HandController.transform.position = mouse.transform.position;
            sr.color = color;
            LeanTween.move(_HandController, emptySquare.Center, 1.5f).setEase(LeanTweenType.easeInOutSine).setOnComplete(() => {
                LeanTween.value(1, 0, 1f).setOnUpdate((float a) => {
                    if (sr != null) {
                        sr.color = new Color(color.r, color.g, color.b, a);
                    }
                }).setOnComplete(MoveHand);
            });
        }
        MoveHand();
        StartPanItemAction = Step06;
    }

    void Step06() {
        LeanTween.cancel(_HandController);
        Destroy(_HandController);
        GameInitializedAction = Step07;
    }

    void Step07() {
        StepGameObject = null;
        ItemController worm = Controller.GameInit.Items.Find(item => item.square == null);
        Controller.ToolTip.Open(worm.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("wowFatWorm"),
            OnClick = () => {
                StopPing(worm.gameObject);
                RunStep(Step08);
            },
        });
        _BlackCover.Target(worm.gameObject);
        Ping(worm.gameObject);
    }

    void Step08() {
        ItemController fish = Controller.GameInit.Items.Find(item => item.square != null);
        Controller.ToolTip.Open(fish.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("catLikeWormRight"),
            OnClick = () => {
                StopPing(fish.gameObject);
                RunStep(Step09);
            },
        });
        _BlackCover.Target(fish.gameObject);
        Ping(fish.gameObject);
    }

    void Step09() {
        ItemController worm = Controller.GameInit.Items.Find(item => item.square == null);
        Controller.ToolTip.Open(worm.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("bringWormToFish"),
            OnClick = () => {
                StopPing(worm.gameObject);
                RunStep(() => Step10(worm));
            },
        });
        _BlackCover.Target(worm.gameObject);
        Ping(worm.gameObject);
    }

    void Step10(ItemController worm) {
        StepGameObject = worm.gameObject;
        _BlackCover.Close();
        _HandController = Instantiate(HandController, UIContainer.transform);
        _HandController.transform.position = worm.transform.position;
        Square emptySquare = Controller.GameInit.Squares[1][0];
        SpriteRenderer sr = _HandController.GetComponent<SpriteRenderer>();
        Color color = sr.color;
        void MoveHand() {
            if (_HandController == null) return;
            _HandController.transform.position = worm.transform.position;
            sr.color = color;
            LeanTween.move(_HandController, emptySquare.Center, 1.5f).setEase(LeanTweenType.easeInOutSine).setOnComplete(() => {
                LeanTween.value(1, 0, 1f).setOnUpdate((float a) => {
                    if (sr != null) {
                        sr.color = new Color(color.r, color.g, color.b, a);
                    }
                }).setOnComplete(MoveHand);
            });
        }
        MoveHand();
        StartPanItemAction = Step11;
    }

    void Step11() {
        LeanTween.cancel(_HandController);
        Destroy(_HandController);
        GameInitializedAction = Step12;
    }

    void Step12() {
        StepGameObject = InfoButton.gameObject;
        Controller.ToolTip.Open(InfoButton.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("seeEachAnimalEat"),
            OnClick = () => {
                StopPing(InfoButton.gameObject);
                _BlackCover.Close();
                Navigator.Instance.NavigateTo(Navigator.Scene.InformationScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                OnFocusAction = () => RunStep(Step13);
            },
        });
        _BlackCover.Target(InfoButton.gameObject);
        Ping(InfoButton.gameObject);
    }

    void Step13() {
        Controller.InfoDialog.Open(new InfoDialog.Info {
            title = Helper.GetLocalizedValue("goCreateFoodChain"),
            fontSize = 16,
            btnTitle = Helper.GetLocalizedValue("letGo"),
            OnClick = () => {
                Controller.InfoDialog.Close();
                RunStep(Step14);
            },
            canClose = false,
        });
    }

    void Step14() {
        SuggestBtn.interactable = true;
        Controller.ToolTip.Open(SuggestBtn.gameObject, new ToolTip.Info {
            title = Helper.GetLocalizedValue("askingForHelp"),
            OnClick = () => {
                StopPing(SuggestBtn.gameObject);
                _BlackCover.Close();
                RunStep(Step15);
            },
        });
        _BlackCover.Target(SuggestBtn.gameObject);
        Ping(SuggestBtn.gameObject);
    }

    void Step15() {
        Controller.InfoDialog.Open(new InfoDialog.Info {
            title = Helper.GetLocalizedValue("lastCheckAnimalDirection"),
            fontSize = 16,
            btnTitle = Helper.GetLocalizedValue("completed"),
            OnClick = () => {
                isTutorial = false;
                StepGameObject = null;
                Controller.InfoDialog.Close();
            },
            canClose = false,
        });
    }


    async void RunStep(Action action) {
        Controller.ToolTip.Close();
        await Task.Delay(delay);
        action.Invoke();
    }

    [SerializeField]
    class PingEvent {
        public LTDescr LT = new LTDescr();
        public Vector3 originalScale;
    }
}
