using System.Collections;
using UnityEngine;

public class Square : MonoBehaviour {
    public GameController Controller;
    public Vector2 Pos;
    public Vector3 Center;

    ItemController ItemController;

    public void AttachItem(ItemController item, bool animatedTo = false, bool checkEndGame = false) {
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
    }

    public void SetCenter(float squareSize) {
        Center = transform.position + new Vector3(squareSize / 2, -squareSize / 2, 0f);
    }

    public ItemController GetItemController() {
        return ItemController;
    }

    public void RemoveItemController() {
        ItemController = null;
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
