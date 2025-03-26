using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameGraft : MonoBehaviour {
    [SerializeField] GameObject SquareBorder;
    SpriteRenderer SquareRenderer;
    GameController Controller;
    Color nextSquareColor;
    Coroutine openSquare;

    void Start() {
        Controller = GetComponent<GameController>();
        SquareRenderer = SquareBorder.GetComponent<SpriteRenderer>();
        nextSquareColor = Configs.SquareColors[0];
    }

    public bool OpenSquareBorder(Square square, ItemController item) {
        bool isValid = CheckValidSquare(square, item);
        SquareBorder.transform.position = square.transform.position;
        SquareRenderer.material.SetColor("_Color", isValid ? Helper.ColorFromHex(Configs.Color.green01) : Color.red);
        SquareBorder.SetActive(true);
        if (openSquare != null) {
            StopCoroutine(openSquare);
            openSquare = null;
        }
        openSquare = StartCoroutine(OpenSquareCoroutine(from: 0, to: 1));
        return isValid;
    }

    public void HideSquareBorder() {
        openSquare = StartCoroutine(OpenSquareCoroutine(from: 1, to: 0, () => {
            SquareBorder.SetActive(false);
        }));
    }

    public void SetSquareBorderScale(Vector3 value) {
        SquareBorder.transform.localScale = value;
    }

    public void SetSquareColor(Square square) {
        List<Square> squaresChain = new List<Square> { square };
        RecursiveSetSquareColor(square, squaresChain);

        if (squaresChain.Count <= 2) {
            // TODO: Player sound Impressive = 1
        }
        else if (squaresChain.Count == 3) {
            // TODO: Player sound Impressive = 2
        }
        else if (squaresChain.Count > 3) {
            // TODO: Player sound Impressive = 3
        }

        foreach (Square s in squaresChain) {
            s.PlayVFX();
        }
    }

    void RecursiveSetSquareColor(Square square, List<Square> squaresChain) {
        ItemController itemController = square.GetItemController();
        if (!itemController) return;

        Color syncColor = Color.white;

        if (itemController.Item.direction != "") {
            Square sqDir = GetSquare(square, itemController.Item.direction);
            if (sqDir.GetItemController()) {
                syncColor = sqDir.GetColor();
                if (syncColor == Color.white) {
                    // Apply for InitSquare, which doesn't have _Color when attached
                    syncColor = GetSquareColor();
                    sqDir.SetColor(syncColor);
                    square.SetColor(syncColor);
                }
                else {
                    square.SetColor(syncColor);
                }
                squaresChain.Add(sqDir);
            }
        }

        List<Square> squaresEat = new List<Square>();

        Square sUp = GetSquare(square, Item.Direction.up.ToString());
        if (CanBeEaten(sUp, itemController, Item.Direction.down)) {
            squaresEat.Add(sUp);
        }

        Square sLeft = GetSquare(square, Item.Direction.left.ToString());
        if (CanBeEaten(sLeft, itemController, Item.Direction.right)) {
            squaresEat.Add(sLeft);
        }

        Square sDown = GetSquare(square, Item.Direction.down.ToString());
        if (CanBeEaten(sDown, itemController, Item.Direction.up)) {
            squaresEat.Add(sDown);
        }

        Square sRight = GetSquare(square, Item.Direction.right.ToString());
        if (CanBeEaten(sRight, itemController, Item.Direction.left)) {
            squaresEat.Add(sRight);
        }

        bool hadSetThisSquare = syncColor != Color.white;

        if (squaresEat.Count == 0) {
            if (!hadSetThisSquare) {
                square.SetColor(GetSquareColor());
            }
            return;
        }

        if (!hadSetThisSquare) {
            Square sqTakeColor = squaresEat.Find(s => s.GetColor() != Color.white);
            syncColor = sqTakeColor != null ? sqTakeColor.GetColor() : GetSquareColor();
            if (syncColor == Color.white) {
                syncColor = GetSquareColor();
            }
            square.SetColor(syncColor);
        }

        foreach (Square s in squaresEat) {
            squaresChain.Add(s);
            RecursiveSetSquareColor(s, squaresChain);
        }
    }

    Color GetSquareColor() {
        Color c = nextSquareColor;
        int index = Configs.SquareColors.FindIndex(_c => _c == c);
        nextSquareColor = Configs.SquareColors[(index + 1) % Configs.SquareColors.Count];
        return c;
    }

    public void CheckEndGame() {
        foreach (var row in Controller.GameInit.Squares) {
            foreach (Square s in row) {
                if (s.GetItemController() == null) {
                    return;
                }
            }
        }

        Level level = Controller.GameInit.level;

        bool[][] check = new bool[(int)level.size.y][];
        for (int row = 0; row < level.size.y; row++) {
            check[row] = new bool[(int)level.size.x];
            for (int col = 0; col < level.size.x; col++) {
                check[row][col] = false;
            }
        }

        bool CheckItem(Item item, bool isFirst) {
            if (item.direction == "") {
                if (!isFirst) check[(int)item.pos.y][(int)item.pos.x] = true;
                return true;
            }

            Item itemDir = GetItem(pivot: item, dir: item.direction);
            if (itemDir == null) return false;

            bool canEat = Controller.GameInit.Relationship[item.creature_id].eat.Contains(itemDir.creature_id);
            if (!canEat) return false;
            check[(int)item.pos.y][(int)item.pos.x] = true;
            return CheckItem(itemDir, isFirst: false);
        }

        foreach (Item[] row in level.data) {
            foreach (Item item in row) {
                bool hasChecked = check[(int)item.pos.y][(int)item.pos.x];
                if (!hasChecked) {
                    bool isValid = CheckItem(item, isFirst: true);
                    if (!isValid) return;
                }
            }
        }

        foreach (Item[] row in level.data) {
            foreach (Item item in row) {
                bool hasChecked = check[(int)item.pos.y][(int)item.pos.x];
                if (!hasChecked) return;
            }
        }

        EndGame();
    }

    async void EndGame() {
        Debug.Log("End game end game hehe");
        // TODO: Add VFX winner here
        await Task.Delay(2000);
        Controller.NextLevel();
    }



    bool CheckValidSquare(Square square, ItemController controller, bool? eat = null) {
        if (eat == null) {
            if (controller.Item.direction == "") {
                return CheckValidSquare(square, controller, eat: false);
            }
            return CheckValidSquare(square, controller, eat: true);
        }

        Square s;

        // Case 1: Eat other
        if (eat == true) {
            s = GetSquare(pivot: square, controller.Item.direction);
            if (s == null) return false;
            if (s.GetItemController() == null) return true;
            if (Controller.GameInit.Relationship[controller.Item.creature_id].eat.Contains(s.GetItemController().Item.creature_id)) return true;
            return false;
        }

        Square sUp = GetSquare(square, Item.Direction.up.ToString());
        if (CanBeEaten(sUp, controller, Item.Direction.down)) return true;

        Square sLeft = GetSquare(square, Item.Direction.left.ToString());
        if (CanBeEaten(sLeft, controller, Item.Direction.right)) return true;

        Square sDown = GetSquare(square, Item.Direction.down.ToString());
        if (CanBeEaten(sDown, controller, Item.Direction.up)) return true;

        Square sRight = GetSquare(square, Item.Direction.right.ToString());
        if (CanBeEaten(sRight, controller, Item.Direction.left)) return true;

        foreach (var sq in new List<Square> { sUp, sLeft, sDown, sRight }) {
            // Only one of four direction squares is empty => Still be valid
            if (sq != null && sq.GetItemController() == null) return true;
        }

        return false;
    }


    bool CanBeEaten(Square sq, ItemController controller, Item.Direction dir) {
        return sq != null
                && sq.GetItemController() != null
                && sq.GetItemController().Item.direction == dir.ToString()
                && Controller.GameInit.Relationship[controller.Item.creature_id].eaten.Contains(sq.GetItemController().Item.creature_id);
    }

    Square GetSquare(Square pivot, string dir) {
        if (Controller.GameInit.Squares.Length > 0) {
            int maxRow = Controller.GameInit.Squares.Length;
            int maxCol = Controller.GameInit.Squares[0].Length;
            int pivotX = (int)pivot.Pos.x;
            int pivotY = (int)pivot.Pos.y;

            if (dir == Item.Direction.up.ToString()) {
                if (pivotY - 1 >= 0) {
                    return Controller.GameInit.Squares[pivotY - 1][pivotX];
                }
                return null;
            }

            if (dir == Item.Direction.left.ToString()) {
                if (pivotX - 1 >= 0) {
                    return Controller.GameInit.Squares[pivotY][pivotX - 1];
                }
                return null;
            }

            if (dir == Item.Direction.down.ToString()) {
                if (pivotY + 1 < maxRow) {
                    return Controller.GameInit.Squares[pivotY + 1][pivotX];
                }
                return null;
            }

            if (dir == Item.Direction.right.ToString()) {
                if (pivotX + 1 < maxCol) {
                    return Controller.GameInit.Squares[pivotY][pivotX + 1];
                }
                return null;
            }
        }
        return null;
    }

    Item GetItem(Item pivot, string dir) {
        int maxRow = Controller.GameInit.level.data.Length;
        int maxCol = Controller.GameInit.level.data[0].Length;
        int pivotX = (int)pivot.pos.x;
        int pivotY = (int)pivot.pos.y;

        if (dir == Item.Direction.up.ToString()) {
            if (pivotY - 1 >= 0) {
                return Controller.GameInit.level.data[pivotY - 1][pivotX];
            }
            return null;
        }

        if (dir == Item.Direction.left.ToString()) {
            if (pivotX - 1 >= 0) {
                return Controller.GameInit.level.data[pivotY][pivotX - 1];
            }
            return null;
        }

        if (dir == Item.Direction.down.ToString()) {
            if (pivotY + 1 < maxRow) {
                return Controller.GameInit.level.data[pivotY + 1][pivotX];
            }
            return null;
        }

        if (dir == Item.Direction.right.ToString()) {
            if (pivotX + 1 < maxCol) {
                return Controller.GameInit.level.data[pivotY][pivotX + 1];
            }
            return null;
        }

        return null;
    }

    IEnumerator OpenSquareCoroutine(float from, float to, Action callback = null) {
        float duration = 0.3f;
        float elapsedTime = 0f;
        Color c = SquareRenderer.material.GetColor("_Color");
        SquareRenderer.material.SetColor("_Color", new Color(c.r, c.g, c.b, from));
        while (elapsedTime < duration) {
            float a = Mathf.Lerp(from, to, elapsedTime / duration);
            SquareRenderer.material.SetColor("_Color", new Color(c.r, c.g, c.b, a));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SquareRenderer.material.SetColor("_Color", new Color(c.r, c.g, c.b, to));
        callback?.Invoke();
    }
}
