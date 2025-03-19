using UnityEngine;

public class GameInit : MonoBehaviour {
    [Header("Prefabs")]
    [SerializeField] GameObject Square;
    [SerializeField] GameObject Item;

    [Header("GameObjects")]
    [SerializeField] Transform ChessBoard;
    [SerializeField] Transform ChoicesBoard;

    GameController Controller;

    public void InitGame(int size) {
        InitChessBoard(size);
        InitChoicesBoard(size);
    }

    void InitChessBoard(int size) {
        foreach (Transform child in ChessBoard) {
            Destroy(child.gameObject);
        }
        float screenWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect;
        float boardSize = screenWidth * 0.8f;
        float t = (size - 2f) / (7f - 2f);
        float gap = boardSize * Mathf.Lerp(0.02f, 0.008f, t);
        float squareSize = (boardSize - (gap * (size - 1))) / size;

        Vector2 startPos = new Vector2(ChessBoard.transform.position.x - boardSize / 2, ChessBoard.transform.position.y + boardSize / 2);

        SpriteRenderer sr = Square.GetComponent<SpriteRenderer>();
        float s = squareSize / sr.bounds.size.x;
        Vector3 localScale = new Vector3(s, s, 1f);

        for (int row = 0; row < size; row++) {
            for (int col = 0; col < size; col++) {
                GameObject square = Instantiate(Square, ChessBoard);
                float x = startPos.x + col * squareSize + col * gap;
                float y = startPos.y - row * squareSize - row * gap;
                square.transform.position = new Vector3(x, y, 0);
                square.transform.localScale = localScale;
                // sr = square.GetComponent<SpriteRenderer>();
                // if (sr != null) {
                //     sr.color = (row + col) % 2 == 0 ? Color.white : Color.black;
                // }
            }
        }
    }

    void InitChoicesBoard(int size) {
        if (!Controller) Controller = GetComponent<GameController>();
        foreach (Transform child in ChoicesBoard) {
            Destroy(child.gameObject);
        }
        SpriteRenderer sr = ChoicesBoard.GetComponent<SpriteRenderer>();
        float width = sr.bounds.size.x - 0.2f;
        float height = sr.bounds.size.y - 0.2f;

        float t = (size - 3f) / (7f - 3f);
        float s = Mathf.Lerp(0.8f, 0.35f, t);
        Vector3 scale = new Vector3(s, s, 1f);

        int cols = size + 1;
        float gap = width / cols;
        int currentIndex = 0;
        int total = size * size;

        // TODO: Take radome 1 to make init Square to ChessBoard

        while (currentIndex < total - 1) {
            int row = currentIndex / cols;
            int col = currentIndex % cols;
            float xPos = -width / 2 + col * gap + gap / 2;
            float yPos = height / 2 - row * gap - gap / 2;

            GameObject choice = Instantiate(Item, ChoicesBoard);
            choice.transform.localPosition = new Vector3(xPos, yPos, 0f);
            choice.transform.localScale = scale;
            Controller.ChoicesPos.Add(new GameController.ChoicePos {
                localPos = choice.transform.localPosition,
                isEmpty = false,
            });
            currentIndex++;
        }
    }
}
