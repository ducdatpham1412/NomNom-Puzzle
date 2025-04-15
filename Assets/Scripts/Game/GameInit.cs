using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Matching = GameState.PlayingLevel.Matching;

public class GameInit : MonoBehaviour {
    [Header("Prefabs")]
    [SerializeField] GameObject Square;
    [SerializeField] GameObject Item;

    [Header("GameObjects")]
    [SerializeField] Transform SquaresBoard;
    [SerializeField] Transform ChoicesBoard;
    [SerializeField] Text TextLevel;

    [Header("Resources")]
    public CreaturesObject CreaturesObject;
    public Dictionary<string, Relationship> Relationship = new Dictionary<string, Relationship>();
    public string SquareTag = Helper.Tag.Square.ToString();
    public string ChoicesBoardTag = Helper.Tag.ChoicesBoard.ToString();

    [Header("Stats")]
    public Square[][] Squares;
    public List<ItemController> Items;
    public Level level;
    [HideInInspector] public bool isInitializing;
    float squareSize;

    GameController Controller;

    void Start() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/relationship");
        string json = jsonFile.text;
        Relationship = JsonConvert.DeserializeObject<Dictionary<string, Relationship>>(json);
    }

    public void InitGame(Level _level, int currentLevel) {
        isInitializing = true;
        if (!Controller) Controller = GetComponent<GameController>();
        level = _level;
        TextLevel.text = $"Lv. {currentLevel}";
        Controller.numberSuggestions = 0;

        InitSquaresBoard();
        StartCoroutine(AnimateSquares());

        if (Controller.GameTutorial.isTutorial) {
            InitChoicesBoard(currentLevel);
            return;
        }

        if (currentLevel < Controller.levelStorage) {
            var playingLevel = Controller.GetPlayingLevel(currentLevel);
            if (playingLevel != null) {
                InitChoicesBoard(currentLevel);
            }
            else {
                Controller.ended = true;
                FillAllSquares();
            }
            return;
        }

        InitChoicesBoard(currentLevel);
    }

    void InitSquaresBoard() {
        foreach (Transform child in SquaresBoard) {
            Destroy(child.gameObject);
        }

        Vector2 size = level.size;
        Squares = new Square[(int)size.y][];
        float boardWidth = SquaresBoard.GetComponent<SpriteRenderer>().bounds.size.x;

        Vector2 boardSize;
        if (size.x >= size.y) {
            boardSize = new Vector2(boardWidth, boardWidth * (size.y / size.x));
        }
        else {
            boardSize = new Vector2(boardWidth * (size.x / size.y), boardWidth);
        }

        float t = (size.x - 2f) / (7f - 2f);
        float gap = boardSize.x * Mathf.Lerp(0.02f, 0.008f, t);
        squareSize = (boardSize.x - (gap * (size.x - 1))) / size.x;

        Vector2 startPos = new Vector2(SquaresBoard.transform.position.x - boardSize.x / 2, SquaresBoard.transform.position.y + boardSize.y / 2);

        var sr = Square.GetComponent<SpriteRenderer>();
        float sc = squareSize / sr.bounds.size.x;
        Vector3 localScale = new Vector3(sc, sc, 1f);

        Controller.GameGraft.SetSquareBorderScale(localScale);

        for (int row = 0; row < size.y; row++) {
            Squares[row] = new Square[(int)size.x];
            for (int col = 0; col < size.x; col++) {
                float x = startPos.x + col * squareSize + col * gap;
                float y = startPos.y - row * squareSize - row * gap;
                GameObject NewSquare = Instantiate(Square, parent: SquaresBoard, position: new Vector3(x, y, 0), rotation: Quaternion.identity);
                NewSquare.transform.localScale = localScale;
                Square cpn = NewSquare.GetComponent<Square>();
                cpn.Pos = new Vector2 { x = col, y = row };
                cpn.Controller = Controller;
                cpn.SetCenter(squareSize);
                Squares[row][col] = cpn;
            }
        }
    }

    IEnumerator AnimateSquares() {
        Vector2 size = level.size;
        Vector3 originalScale = Squares[0][0].transform.localScale;
        float deltaTime = Mathf.Min(1.1f / (size.x * size.y), 0.06f);
        float duration = 0.4f;

        string method = Helper.GetRandomInArr(new string[] { "move", "rotate" });

        List<AudioSource> audios = new List<AudioSource>();

        if (method == "move") {
            for (int row = 0; row < size.y; row++) {
                for (int col = 0; col < size.x; col++) {
                    Squares[row][col].transform.localScale = Vector3.zero;
                }
            }

            for (int col = (int)size.x - 1; col >= 0; col--) {
                for (int row = 0; row < size.y; row++) {
                    Square sq = Squares[row][col];
                    Vector3 originalPos = sq.transform.position;
                    sq.transform.position += new Vector3(0f, 1.3f, 0f);
                    LeanTween.move(sq.gameObject, originalPos, duration).setEase(LeanTweenType.easeOutQuad);
                    LeanTween.scale(sq.gameObject, originalScale, duration).setEase(LeanTweenType.easeOutQuad);
                    AudioSource audio = SoundManager.Instance.PlaySF(SoundManager.SF.Bubble);
                    if (audio != null) audios.Add(audio);
                    yield return new WaitForSeconds(deltaTime);
                }
            }
        }

        else if (method == "rotate") {
            for (int row = 0; row < size.y; row++) {
                for (int col = 0; col < size.x; col++) {
                    Square sq = Squares[row][col];
                    sq.transform.rotation = Quaternion.Euler(0, 0, 60);
                    sq.transform.localScale = Vector3.zero;
                }
            }

            for (int col = 0; col < size.x; col++) {
                for (int row = 0; row < size.y; row++) {
                    Square sq = Squares[row][col];
                    LeanTween.rotate(sq.gameObject, Vector3.zero, duration).setEase(LeanTweenType.easeOutQuad);
                    LeanTween.scale(sq.gameObject, originalScale, duration).setEase(LeanTweenType.easeOutQuad);
                    AudioSource audio = SoundManager.Instance.PlaySF(SoundManager.SF.Bubble);
                    if (audio != null) audios.Add(audio);
                    yield return new WaitForSeconds(deltaTime);
                }
            }
        }

        yield return new WaitForSeconds(duration); // Wait for the last tween finished to confirm last square has finished

        foreach (AudioSource audio in audios) {
            SoundManager.Instance.RemoveAudioSource(audio);
        }

        isInitializing = false;
    }

    void InitChoicesBoard(int currentLevel) {
        Controller.PlayAgainButton.gameObject.SetActive(false);
        foreach (Transform child in ChoicesBoard) {
            Destroy(child.gameObject);
        }
        Items = new List<ItemController>();
        GameState.PlayingLevel playingLevel = Controller.GetPlayingLevel(currentLevel);

        List<Item> itemsInChoicesBoard = new List<Item>();
        List<Item> itemsRecommended = new List<Item>();
        List<MatchStore> itemsStorage = new List<MatchStore>();

        foreach (Item[] row in level.data) {
            foreach (Item item in row) {
                int index = Array.FindIndex(level.init_pos, p => p.Equals(item.pos));
                if (index >= 0) {
                    itemsRecommended.Add(item);
                    continue;
                }
                if (playingLevel != null) {
                    Matching matching = playingLevel.matchings.Find(m => m.itemPos.Equals(item.pos));
                    if (matching != null) {
                        itemsStorage.Add(new MatchStore {
                            item = item,
                            matching = matching,
                        });
                        continue;
                    }
                }
                itemsInChoicesBoard.Add(item);
            }
        }


        ChoiceBoardSize boardSize = GetChoiceBoardSize(itemsInChoicesBoard.Count + itemsStorage.Count);

        ItemController InitItem(Item item) {
            GameObject NewItem = Instantiate(Item, ChoicesBoard);
            ItemController controller = NewItem.GetComponent<ItemController>();
            controller.Controller = Controller;
            controller.SetItem(item);
            NewItem.transform.localScale = boardSize.scale;
            Items.Add(controller);
            return controller;
        }

        foreach (Item initItem in itemsRecommended) {
            ItemController ct = InitItem(initItem);
            Square sq = Squares[(int)initItem.pos.y][(int)initItem.pos.x];
            Controller.GameGraft.SquareAttachItem(
                square: sq,
                item: ct,
                attachParams: new GameGraft.SquareAttachItemParams {
                    isRoot = true,
                    playSound = false,
                    setSquareColor = false
                }
            );
        }

        Helper.Shuffle(itemsInChoicesBoard);

        int i = 0;

        Vector3 GetPos(int _i) {
            int row = _i / boardSize.cols;
            int col = _i % boardSize.cols;
            float xPos = -boardSize.width / 2 + col * boardSize.gapX;
            float yPos = boardSize.height / 2 - row * boardSize.gapY;
            return new Vector3(xPos, yPos, 0f);
        }

        for (i = 0; i < itemsInChoicesBoard.Count; i++) {
            ItemController ct = InitItem(itemsInChoicesBoard[i]);
            ct.transform.localPosition = GetPos(i);
            ct.SetOriginalPos(ct.transform.position);
        }

        foreach (MatchStore matchStore in itemsStorage) {
            ItemController ct = InitItem(matchStore.item);
            ct.transform.localPosition = GetPos(i);
            ct.SetOriginalPos(ct.transform.position);

            Square sq = Squares[(int)matchStore.matching.squarePos.y][(int)matchStore.matching.squarePos.x];
            Controller.GameGraft.SquareAttachItem(
                square: sq,
                item: ct,
                attachParams: new GameGraft.SquareAttachItemParams {
                    playVFX = false,
                    playSound = false,
                }
            );
            i++;
        }

        for (int row = 0; row < level.size.y; row++) {
            for (int col = 0; col < level.size.x; col++) {
                Square square = Squares[row][col];
                ItemController item = square.ItemController;
                if (item != null && !Controller.GameGraft.CheckValidSquare(square, item)) {
                    Controller.GameGraft.PingErrorSquare(square, shouldScale: false);
                }
            }
        }

        if (Controller.GameTutorial.GameInitializedAction != null) {
            Controller.GameTutorial.GameInitializedAction?.Invoke();
            Controller.GameTutorial.GameInitializedAction = null;
        }
    }

    void FillAllSquares() {
        Controller.PlayAgainButton.gameObject.SetActive(true);
        Items = new List<ItemController>();
        foreach (Transform child in ChoicesBoard) {
            Destroy(child.gameObject);
        }
        ChoiceBoardSize boardSize = GetChoiceBoardSize(0);
        foreach (Item[] row in level.data) {
            foreach (Item item in row) {
                GameObject NewItem = Instantiate(Item, ChoicesBoard);
                ItemController controller = NewItem.GetComponent<ItemController>();
                controller.Controller = Controller;
                controller.SetItem(item);
                Square sq = Squares[(int)item.pos.y][(int)item.pos.x];
                NewItem.transform.localScale = boardSize.scale;
                Controller.GameGraft.SquareAttachItem(
                    square: sq,
                    item: controller,
                    attachParams: new GameGraft.SquareAttachItemParams {
                        playVFX = false,
                        playSound = false
                    }
                );
            }
        }
    }

    ChoiceBoardSize GetChoiceBoardSize(int totalItems, float gapYRatio = 1f) {
        SpriteRenderer sr = ChoicesBoard.GetComponent<SpriteRenderer>();
        float width = sr.bounds.size.x - 0.2f;
        float height = sr.bounds.size.y - 0.2f;

        float gapX = squareSize * 0.9f;
        float gapY = squareSize * gapYRatio;

        int cols = Mathf.FloorToInt(width / gapX);
        int rows = Mathf.CeilToInt((float)totalItems / cols);

        if (gapY * rows > height) {
            return GetChoiceBoardSize(totalItems, gapYRatio - 0.05f);
        }

        float scaleWidth = squareSize * 0.65f;
        float trueWidth = Item.GetComponent<SpriteRenderer>().bounds.size.x;
        float sc = scaleWidth / trueWidth;
        Vector3 scale = new Vector3(sc, sc, 1f);

        return new ChoiceBoardSize {
            width = gapX * (cols - 1),
            height = gapY * (rows - 1),
            gapX = gapX,
            gapY = gapY,
            cols = cols,
            scale = scale,
        };
    }

    [Serializable]
    class MatchStore {
        public Item item;
        public Matching matching;
    }

    [Serializable]
    class ChoiceBoardSize {
        public float width;
        public float height;
        public float gapX;
        public float gapY;
        public int cols;
        public Vector3 scale;
    }
}
