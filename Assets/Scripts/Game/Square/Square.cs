using System.Collections;
using UnityEngine;

public class Square : MonoBehaviour {
    public GameController Controller;
    public Vector2 Pos;
    public Vector3 Center;
    Color Color = Color.white;

    ItemController ItemController;
    Material material;

    public void AttachItem(ItemController item, bool animatedTo = false, bool checkEndGame = false, bool isRoot = false, Color? color = null) {
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
            material.SetColor("_Color", (Color)color);
        }
        else {
            Controller.GameGraft.SetSquareColor(this);
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
        SetColor(Color.white);
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
    }
}
