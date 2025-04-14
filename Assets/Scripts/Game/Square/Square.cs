using UnityEngine;

public class Square : MonoBehaviour {
    public GameController Controller;
    public Vector2 Pos;
    public Vector3 Center { get; private set; }
    public ItemController ItemController;
    public Collider2D PanCollider { get; private set; }
    [HideInInspector] public Material material;
    [HideInInspector] public bool isError = false;
    Color Color;

    void Awake() {
        Color = Configs.DefaultSquareColor;
    }

    void Start() {
        material = GetComponent<SpriteRenderer>().material;
        PanCollider = GetComponents<Collider2D>()[0];
    }

    void Update() {
        HandleClick();
    }

    public void SetCenter(float squareSize) {
        Center = transform.position + new Vector3(squareSize / 2, -squareSize / 2, 0f);
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

    public void ResetError() {
        isError = false;
        Controller.GameGraft.SetSquareColor(this, playSound: false);
        if (ItemController) ItemController.ResetAllActions();
    }

    void HandleClick() {
        if (Controller.numberSuggestions <= 0) return;
        if (ItemController != null) return;
        if (!Controller.ShouldHandlePan()) return;
        if (GameHelper.TouchBegin() && GameHelper.TouchHitGameObject(GameHelper.TouchPosition(), gameObject)) {
            ItemController trueItem = Controller.GameInit.Items.Find(item => item.Item.pos.Equals(Pos));
            Controller.GameGraft.SuggestItemToSquare(trueItem);
        }
    }
}
