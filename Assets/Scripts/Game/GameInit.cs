using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GameInit : MonoBehaviour {
    [Header("Prefabs")]
    [SerializeField] GameObject Square;
    [SerializeField] GameObject Item;

    [Header("GameObjects")]
    [SerializeField] Transform SquaresBoard;
    [SerializeField] Transform ChoicesBoard;

    [Header("Resources")]
    public List<Creature> Creatures = new List<Creature>();
    public Dictionary<string, Relationship> Relationship = new Dictionary<string, Relationship>();
    public string SquareTag = Helper.Tag.Square.ToString();
    public string ChoicesBoardTag = Helper.Tag.ChoicesBoard.ToString();

    [Header("Stats")]
    public Square[][] Squares;
    public Level level;

    GameController Controller;

    void Start() {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/relationship");
        string json = jsonFile.text;
        Relationship = JsonConvert.DeserializeObject<Dictionary<string, Relationship>>(json);
    }

    public void InitGame(Level _level) {
        if (!Controller) Controller = GetComponent<GameController>();
        level = _level;
        InitSquaresBoard();
        InitChoicesBoard();
    }

    void InitSquaresBoard() {
        Vector2 size = level.size;
        Squares = new Square[(int)size.y][];
        foreach (Transform child in SquaresBoard) {
            Destroy(child.gameObject);
        }
        float screenWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect;
        Vector2 boardSize;
        if (size.x >= size.y) {
            boardSize = new Vector2(screenWidth * 0.8f, screenWidth * 0.8f * (size.y / size.x));
        }
        else {
            boardSize = new Vector2(screenWidth * 0.8f * (size.x / size.y), screenWidth * 0.8f);
        }

        float t = (size.x - 2f) / (7f - 2f);
        float gap = boardSize.x * Mathf.Lerp(0.02f, 0.008f, t);
        float squareSize = (boardSize.x - (gap * (size.x - 1))) / size.x;

        Vector2 startPos = new Vector2(SquaresBoard.transform.position.x - boardSize.x / 2, SquaresBoard.transform.position.y + boardSize.y / 2);

        SpriteRenderer sr = Square.GetComponent<SpriteRenderer>();
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
                // sr = square.GetComponent<SpriteRenderer>();
                // if (sr != null) {
                //     sr.color = (row + col) % 2 == 0 ? Color.white : Color.black;
                // }
            }
        }
    }

    void InitChoicesBoard() {
        Vector2 size = level.size;
        foreach (Transform child in ChoicesBoard) {
            Destroy(child.gameObject);
        }
        SpriteRenderer sr = ChoicesBoard.GetComponent<SpriteRenderer>();
        float width = sr.bounds.size.x - 0.2f;
        float height = sr.bounds.size.y - 0.2f;

        int s = (int)Mathf.Sqrt(size.x * size.y) + 1;

        int cols = s + 1;
        float gap = width / cols;

        float scaleWidth = gap * 0.8f;
        float trueWidth = Item.GetComponent<SpriteRenderer>().bounds.size.x;
        float sc = scaleWidth / trueWidth;
        Vector3 scale = new Vector3(sc, sc, 1f);

        int currentIndex = 0;
        int total = (int)(size.x * size.y);

        System.Random random = new System.Random();
        int initIndex = random.Next(0, total);

        while (currentIndex < total) {
            GameObject NewItem = Instantiate(Item, ChoicesBoard);
            ItemController controller = NewItem.GetComponent<ItemController>();
            int row = currentIndex / (int)size.x;
            int col = currentIndex % (int)size.x;
            controller.Controller = Controller;
            controller.SetItem(level.data[row][col]);
            NewItem.transform.localScale = scale;

            if (currentIndex == initIndex) {
                Square sq = Squares[row][col];
                sq.AttachItem(controller);
                // Disable panning or interact with other panning item
                controller.GetComponent<CapsuleCollider2D>().enabled = false;
                sq.GetComponent<BoxCollider2D>().enabled = false;
                // TODO: Make Square to display this is init square
            }
            else {
                row = currentIndex / cols;
                col = currentIndex % cols;
                float xPos = -width / 2 + col * gap + gap / 2;
                float yPos = height / 2 - row * gap - gap / 2;
                NewItem.transform.localPosition = new Vector3(xPos, yPos, 0f);
            }

            currentIndex++;
        }
    }

    [Serializable]
    public class ChoicePos {
        public Vector3 localPos;
        public ItemController item;
    }
}
