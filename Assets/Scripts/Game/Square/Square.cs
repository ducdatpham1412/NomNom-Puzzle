using System.Collections;
using UnityEngine;

public class Square : MonoBehaviour {
    public GameController Controller;
    public Vector2 Pos;
    public Vector3 Center;
    Color Color = Configs.DefaultSquareColor;

    ItemController ItemController;
    Material material;
    Coroutine animatedCenter;

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
            animatedCenter = StartCoroutine(AnimateToCenter(checkEndGame));
        }
        else {
            ItemController.gameObject.transform.position = Center;
            if (checkEndGame) {
                Controller.GameGraft.CheckEndGame();
            }
        }

        if (!material) material = GetComponent<SpriteRenderer>().material;

        bool hasMatched = false;

        if (isRoot) {
            material.SetColor("_Color01", Configs.RootSquareColor);
        }
        else if (color != null) {
            SetColor((Color)color);
        }
        else {
            hasMatched = Controller.GameGraft.SetSquareColor(this);
        }

        if (checkValidAroundSquares) {
            Controller.GameGraft.CheckValidAroundSquare(this, hasMatched);
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

    public void PlayMatchedVFX() {
        // scaleCoroutine = StartCoroutine(ScaleUpAndDown());
        StartCoroutine(ItemController.ScaleUpAndDownCoroutine(shakeSpeed: 45f, scale: 1.5f));
        Controller.PlayVFXLeaf(Center);
    }

    public void PingError(bool shouldScale) {
        StartCoroutine(ItemController.PingErrorInterval(shouldScale));
        SetColor(Configs.ErrorSquareColor);
    }

    public void StopCoroutines() {
        StopAllCoroutines();
        animatedCenter = null;
    }

    public bool HasAnyCoroutines() {
        return animatedCenter != null;
    }

    IEnumerator AnimateToCenter(bool checkEndGame) {
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
        animatedCenter = null;
    }
}
