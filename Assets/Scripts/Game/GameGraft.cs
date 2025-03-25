using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGraft : MonoBehaviour {
    [SerializeField] GameObject SquareBorder;
    SpriteRenderer SquareRenderer;

    GameController Controller;

    void Start() {
        Controller = GetComponent<GameController>();
        SquareRenderer = SquareBorder.GetComponent<SpriteRenderer>();
    }

    public bool OpenSquareBorder(Square square, ItemController item) {
        bool isValid = CheckValidSquare(square, item);
        SquareBorder.transform.position = square.transform.position;
        SquareRenderer.material.SetColor("_Color", isValid ? Helper.ColorFromHex(Configs.Color.green01) : Color.red);
        SquareBorder.SetActive(true);
        StartCoroutine(OpenSquareCoroutine(from: 0, to: 1));
        return isValid;
    }

    public void HideSquareBorder() {
        StartCoroutine(OpenSquareCoroutine(from: 1, to: 0, () => {
            SquareBorder.SetActive(false);
        }));
    }

    public void SetSquareBorderScale(Vector3 value) {
        SquareBorder.transform.localScale = value;
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

    void EndGame() {
        Debug.Log("End game end game hehe");
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

        // Case 2: Be eaten by others
        bool CanBeEaten(Square sq, Item.Direction dir) {
            return sq != null
                    && sq.GetItemController() != null
                    && sq.GetItemController().Item.direction == dir.ToString()
                    && Controller.GameInit.Relationship[controller.Item.creature_id].eaten.Contains(sq.GetItemController().Item.creature_id);
        }

        Square sUp = GetSquare(square, Item.Direction.up.ToString());
        if (CanBeEaten(sUp, Item.Direction.down)) return true;

        Square sLeft = GetSquare(square, Item.Direction.left.ToString());
        if (CanBeEaten(sLeft, Item.Direction.right)) return true;

        Square sDown = GetSquare(square, Item.Direction.down.ToString());
        if (CanBeEaten(sDown, Item.Direction.up)) return true;

        Square sRight = GetSquare(square, Item.Direction.right.ToString());
        if (CanBeEaten(sRight, Item.Direction.left)) return true;

        foreach (var sq in new List<Square> { sUp, sLeft, sDown, sRight }) {
            // Only one of four direction squares is empty => Still be valid
            if (sq != null && sq.GetItemController() == null) return true;
        }

        return false;
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
