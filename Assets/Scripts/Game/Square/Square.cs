using UnityEngine;

public class Square : MonoBehaviour {
    public GameController Controller;
    public Vector2 Pos;
    public Vector3 Center { get; private set; }
    [HideInInspector] public bool isError { get; private set; } = false;
    Color Color = Configs.DefaultSquareColor;

    ItemController ItemController;
    Material material;

    void Start() {
        material = material = GetComponent<SpriteRenderer>().material;
    }

    public void AttachItem(
        ItemController item,
        Color? color = null,
        bool animatedTo = false,
        bool checkEndGame = false,
        bool isRoot = false,
        bool checkValidAroundSquares = false,
        bool playVFX = true,
        bool playSound = true
    ) {
        void CheckAndSet() {
            if (!material) material = GetComponent<SpriteRenderer>().material;

            bool hasMatched = false;

            if (isRoot) {
                material.SetColor("_Color01", Configs.RootSquareColor);
            }
            else if (color != null) {
                SetColor((Color)color);
            }
            else {
                hasMatched = Controller.GameGraft.SetSquareColor(this, playVFX, playSound);
            }

            if (checkValidAroundSquares) {
                Controller.GameGraft.CheckValidAroundSquare(this, hasMatched);
            }
        }

        ItemController = item;
        ItemController.square = this;
        if (animatedTo) {
            ItemController.AnimateToSquare(CheckAndSet);
        }
        else {
            ItemController.gameObject.transform.position = Center;
            if (checkEndGame) {
                Controller.GameGraft.CheckEndGame();
            }
            CheckAndSet();
        }
    }

    public void SetCenter(float squareSize) {
        Center = transform.position + new Vector3(squareSize / 2, -squareSize / 2, 0f);
    }

    public ItemController GetItemController() {
        return ItemController;
    }

    public void RemoveItem() {
        ItemController = null;
        SetColor(Configs.DefaultSquareColor);
    }

    public Color GetColor() {
        return Color;
    }

    public void SetColor(Color color) {
        Color = color;
        material.SetColor("_Color", color);
    }

    public void PlayMatchedVFX() {
        StartCoroutine(ItemController.ScaleAndShake(shakeSpeed: 45f, scale: 1.5f));
        Controller.PlayVFXLeaf(Center);
    }

    public void PingError(bool shouldScale) {
        ItemController.PingErrorInterval(shouldScale);
        SetColor(Configs.ErrorSquareColor);
        isError = true;
    }

    public void ResetError() {
        isError = false;
        Controller.GameGraft.SetSquareColor(this);
        if (ItemController) ItemController.ResetCoroutines();
    }

}
