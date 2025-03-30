using System.Collections;
using UnityEngine;

public class Square : MonoBehaviour {
    public GameController Controller;
    public Vector2 Pos;
    public Vector3 Center;
    public bool havingAnyCoroutines = false;
    Color Color = Configs.DefaultSquareColor;

    ItemController ItemController;
    Material material;

    public void AttachItem(
        ItemController item,
        Color? color = null,
        bool animatedTo = false,
        bool checkEndGame = false,
        bool isRoot = false,
        bool checkValidAroundSquares = false
    ) {
        ItemController = item;
        ItemController.square = this;
        if (animatedTo) {
            StartCoroutine(AnimateToCenter(checkEndGame));
        }
        else {
            ItemController.gameObject.transform.position = Center;
            if (checkEndGame) {
                Controller.GameGraft.CheckEndGame();
            }
        }

        if (!material) material = GetComponent<SpriteRenderer>().material;

        if (isRoot) {
            material.SetColor("_Color01", Configs.RootSquareColor);
        }
        else if (color != null) {
            SetColor((Color)color);
        }
        else {
            Controller.GameGraft.SetSquareColor(this);
        }

        if (checkValidAroundSquares) {
            Controller.GameGraft.CheckValidAroundSquare(this);
        }
    }

    public void SetCenter(float squareSize) {
        Center = transform.position + new Vector3(squareSize / 2, -squareSize / 2, 0f);
    }

    public ItemController GetItemController() {
        return ItemController;
    }

    public void TemporarySetItemToNull() {
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

    public void PlayVFX() {
        // TODO: Add VFX bloom make fresh feel
    }

    public void PingError() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        StartCoroutine(ItemController.ScaleUpAndDownCoroutine());
        SetColor(Configs.ErrorSquareColor);
    }

    IEnumerator AnimateToCenter(bool checkEndGame) {
        havingAnyCoroutines = true;
        float duration = 0.1f;
        float elapsedTime = 0f;
        Vector3 currentPos = ItemController.transform.position;
        while (elapsedTime < duration) {
            ItemController.gameObject.transform.position = Vector3.Lerp(currentPos, Center, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ItemController.gameObject.transform.position = Center;
        if (checkEndGame) Controller.GameGraft.CheckEndGame();
        havingAnyCoroutines = false;
    }
}
