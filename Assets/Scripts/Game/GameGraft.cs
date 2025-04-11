using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameGraft : MonoBehaviour {
    [SerializeField] GameObject SquareBorder;
    SpriteRenderer SquareRenderer;
    GameController Controller;
    Coroutine openSquare;
    List<string> directions = new List<string>{
        Item.Direction.up.ToString(),
        Item.Direction.left.ToString(),
        Item.Direction.down.ToString(),
        Item.Direction.right.ToString(),
    };
    List<Square> squaresChain = new List<Square>();
    bool playMatchingSound = true;

    void Awake() {
        Controller = GetComponent<GameController>();
        SquareRenderer = SquareBorder.GetComponent<SpriteRenderer>();
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

    public bool SetSquareColor(Square square, bool playVFX = true, bool playSound = true) {
        playMatchingSound = playSound;
        if (square.ItemController == null) {
            square.SetColor(Configs.DefaultSquareColor);
            return false;
        }

        squaresChain.Clear();
        squaresChain.Add(square);
        RecursiveSquaresChain(square);

        /*
        Get color which is not the same to item around squaresChain
        */
        List<AroundColorCounting> availableColors = new List<AroundColorCounting>();
        foreach (Color c in Configs.SquareColors) {
            availableColors.Add(new AroundColorCounting {
                color = c,
                count = 0,
            });
        }
        foreach (Square s in squaresChain) {
            List<Square> aroundSquares = GetAroundSquares(s);
            foreach (Square asq in aroundSquares) {
                if (asq != null && asq.ItemController != null && !squaresChain.Contains(asq)) {
                    AroundColorCounting ac = availableColors.Find(_ac => _ac.color.Equals(asq.GetColor()));
                    if (ac != null) {
                        ac.count++;
                    }
                }
            }
        }
        AroundColorCounting uniqueColor = availableColors.OrderBy(ac => ac.count).ToList()[0];
        foreach (Square s in squaresChain) {
            s.SetColor(uniqueColor.color);
        }

        /*
        Check created a chain or not
        */
        bool hasMatched = false;
        if (squaresChain.Count >= 2) {
            if (playVFX) {
                foreach (Square s in squaresChain) {
                    PlayMatchedVFX(s);
                }
            }
            hasMatched = true;
        }

        return hasMatched;
    }

    public void CheckValidAroundSquare(Square square, bool hasMatched) {
        foreach (Square sq in GetAroundSquares(square)) {
            if (sq == null || sq.ItemController == null) continue;
            if (!CheckValidSquare(sq, sq.ItemController)) {
                PingErrorSquare(sq, !hasMatched);
            }
            else if (sq.isError) {
                sq.ResetError();
            }
        }
    }

    public void PingErrorSquare(Square square, bool shouldScale) {
        square.ItemController.PingErrorInterval(shouldScale);
        square.SetColor(Configs.ErrorSquareColor);
        square.isError = true;
    }

    public void CheckEndGame() {
        foreach (var row in Controller.GameInit.Squares) {
            foreach (Square s in row) {
                ItemController c = s.ItemController;
                if (c == null || !CheckValidSquare(square: s, controller: c, itemDirNullEnable: false)) {
                    if (playMatchingSound) {
                        if (squaresChain.Count == 1) {
                            SoundManager.Instance.PlaySF(SoundManager.SF.Marimba_01);
                        }
                        else if (squaresChain.Count == 2) {
                            SoundManager.Instance.PlaySF(SoundManager.SF.Marimba_02);
                        }
                        else if (squaresChain.Count == 3) {
                            SoundManager.Instance.PlaySF(SoundManager.SF.Marimba_03);
                        }
                        else if (squaresChain.Count > 3) {
                            SoundManager.Instance.PlaySF(SoundManager.SF.Marimba_04);
                        }
                    }
                    return;
                }
            }
        }
        Controller.EndGame();
    }

    public void SuggestItemToSquare(ItemController itemController) {
        Vector2 pos = itemController.Item.pos;
        Square trueSquare = Controller.GameInit.Squares[(int)pos.y][(int)pos.x];
        Square currentSquare = itemController.square;
        if (currentSquare != trueSquare) {
            itemController.animatingToSquare = true;
            SoundManager.Instance.PlaySF(SoundManager.SF.Pop_01);
            Controller.numberSuggestions -= 1;

            if (currentSquare) {
                currentSquare.RemoveItem();
                if (currentSquare.isError) currentSquare.ResetError();
            }

            Vector3 targetScale = itemController.transform.localScale * 3.5f;
            LeanTween.move(itemController.gameObject, Vector3.zero, 0.2f).setEase(LeanTweenType.easeOutQuad);
            LeanTween.scale(itemController.gameObject, targetScale, 0.2f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
                ItemAttachSquare(item: itemController, square: trueSquare, isRoot: true, duration: 0.6f);
            });
        }
        else {
            LeanTween.scale(itemController.gameObject, itemController.transform.localScale * 2f, 1f).setEase(LeanTweenType.punch);
        }
    }

    public void ItemAttachSquare(ItemController item, Square square, bool isRoot = false, float duration = 0.1f) {
        ItemController currentItem = square.ItemController;
        if (currentItem && currentItem != item) {
            currentItem.square = null;
            currentItem.BackToOriginal();
        }
        SquareAttachItem(
                square: square,
                item: item,
                attachParams: new SquareAttachItemParams {
                    animatedTo = true,
                    checkEndGame = true,
                    checkValidAroundSquares = true,
                    duration = duration,
                    isRoot = isRoot
                }
            );
        item.CheckValidAtOriginalSquare();
    }

    public void SquareAttachItem(
      ItemController item,
      Square square,
      SquareAttachItemParams attachParams
  ) {
        void CheckAndSet() {
            if (!square.material) square.material = square.GetComponent<SpriteRenderer>().material;

            bool hasMatched = false;

            if (attachParams.isRoot) {
                square.material.SetColor("_Color01", Configs.RootSquareColor);
                foreach (var col in square.GetComponents<Collider2D>()) {
                    col.enabled = false;
                }
                item.GetComponent<Collider2D>().enabled = false;
            }

            if (attachParams.color != null) {
                square.SetColor((Color)attachParams.color);
            }
            else if (attachParams.setSquareColor) {
                hasMatched = SetSquareColor(square, attachParams.playVFX, attachParams.playSound);
            }

            if (attachParams.checkValidAroundSquares) {
                CheckValidAroundSquare(square, hasMatched);
            }
        }

        square.ItemController = item;
        square.ItemController.square = square;
        if (attachParams.animatedTo) {
            item.animatingToSquare = true;
            LeanTween.scale(item.gameObject, item.originalScale, attachParams.duration).setEase(LeanTweenType.easeOutQuad);
            LeanTween.move(item.gameObject, square.Center, attachParams.duration).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
                CheckAndSet();
                item.animatingToSquare = false;
                item.transform.position = square.Center;
                Controller.GameGraft.CheckEndGame();
            });
        }
        else {
            square.ItemController.gameObject.transform.position = square.Center;
            if (attachParams.checkEndGame) {
                CheckEndGame();
            }
            CheckAndSet();
        }
    }

    void RecursiveSquaresChain(Square square) {
        ItemController itemController = square.ItemController;
        if (!itemController) return;

        Creature creature = Controller.GameInit.CreaturesObject.Creatures.Find(c => c.id == itemController.Item.creature_id);
        if (creature == null) return;

        List<Square> nextSquares = new List<Square>();

        if (creature.type == Creature.Type.animal) {
            string dir = GetAvailableDir(itemController.Item.direction, creature);
            Square sqDir = GetSquare(square, dir);

            void CheckSqDir() {
                if (sqDir == null) return;
                if (sqDir.ItemController == null) return;
                if (squaresChain.Contains(sqDir)) return;
                Item.Direction? temp = Helper.StringToEnum<Item.Direction>(dir);
                if (temp == null) return;
                if (!CanBeEaten(square, sqDir.ItemController, (Item.Direction)temp)) return;
                squaresChain.Add(sqDir);
                nextSquares.Add(sqDir);
            }

            CheckSqDir();
        }

        if (Controller.GameInit.Relationship[itemController.Item.creature_id].eaten.Count == 0) {
            foreach (Square s in nextSquares) {
                RecursiveSquaresChain(s);
            }
            return;
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

        foreach (Square s in squaresEat) {
            if (squaresChain.Contains(s)) continue;
            squaresChain.Add(s);
            nextSquares.Add(s);
        }

        foreach (Square s in nextSquares) {
            RecursiveSquaresChain(s);
        }
    }

    public bool CheckValidSquare(
        Square square,
        ItemController controller,
        bool? eat = null,
        bool itemDirNullEnable = true
    ) {
        Creature creature = Controller.GameInit.CreaturesObject.Creatures.Find(c => c.id == controller.Item.creature_id);
        if (creature == null) return false;

        if (eat == null) {
            if (creature.type == Creature.Type.animal) {
                return CheckValidSquare(square, controller, eat: false) || CheckValidSquare(square, controller, eat: true);
            }
            return CheckValidSquare(square, controller, eat: false);
        }

        Square s;

        // Case 1: Eat other (Animal)
        if (eat == true) {
            string dir = GetAvailableDir(controller.Item.direction, creature);
            s = GetSquare(square, dir);
            if (s == null) return false;
            if (s.ItemController == null) return itemDirNullEnable;
            if (Controller.GameInit.Relationship[controller.Item.creature_id].eat.Contains(s.ItemController.Item.creature_id)) {
                return true;
            }
            return false;
        }

        // Case 2: Be eaten by other (Can be any of Animal, Food or Plant)
        if (Controller.GameInit.Relationship[controller.Item.creature_id].eaten.Count == 0) return false;

        bool isSquareValid(Square sq, bool beEaten, string dir) {
            if (sq == null) return true;
            ItemController ct = sq.ItemController;
            if (ct == null) return itemDirNullEnable;
            Creature cr = Controller.GameInit.CreaturesObject.Creatures.Find(c => c.id == ct.Item.creature_id);
            if (cr == null) return false;
            if (cr.type != Creature.Type.animal) return true;
            if (ct.Item.direction != dir) return true;
            if (DirCanBeEmpty(ct.Item.direction, cr)) return true;
            return beEaten;
        }

        bool canInteract(Square sq, bool beEaten) {
            if (beEaten) return true;
            if (sq == null) return false;
            if (sq.ItemController == null) return itemDirNullEnable;
            return false;
        }

        Square sUp = GetSquare(square, Item.Direction.up.ToString());
        bool eatenUp = CanBeEaten(sUp, controller, Item.Direction.down);
        if (!isSquareValid(sUp, eatenUp, Item.Direction.down.ToString())) return false;

        Square sLeft = GetSquare(square, Item.Direction.left.ToString());
        bool eatenLeft = CanBeEaten(sLeft, controller, Item.Direction.right);
        if (!isSquareValid(sLeft, eatenLeft, Item.Direction.right.ToString())) return false;

        Square sDown = GetSquare(square, Item.Direction.down.ToString());
        bool eatenDown = CanBeEaten(sDown, controller, Item.Direction.up);
        if (!isSquareValid(sDown, eatenDown, Item.Direction.up.ToString())) return false;

        Square sRight = GetSquare(square, Item.Direction.right.ToString());
        bool eatenRight = CanBeEaten(sRight, controller, Item.Direction.left);
        if (!isSquareValid(sRight, eatenRight, Item.Direction.left.ToString())) return false;

        /*
        All squares around this square are valid, not mean final result is valid for it.
        We need at least one of around squares can eat (interact to) this square.
        */
        if (!(
            canInteract(sUp, eatenUp) ||
            canInteract(sLeft, eatenLeft) ||
            canInteract(sDown, eatenDown) ||
            canInteract(sRight, eatenRight)
        )) {
            return false;
        }

        return true;
    }

    List<Square> GetAroundSquares(Square square) {
        List<Square> res = new List<Square>();
        foreach (string dir in directions) {
            Square sq = GetSquare(square, dir);
            res.Add(sq);
        }
        return res;
    }

    string GetAvailableDir(string dir, Creature creature) {
        /*
           With animal having direction == "", we can set it having direction dynamic,
           For example: Worm with direction == "left" is the same as Worm with direction == "" 
       */
        if (dir != "") return dir;
        if (creature.rotationOffset == 90f) {
            dir = Item.Direction.left.ToString();
        }
        else if (creature.rotationOffset == 0f) {
            dir = Item.Direction.down.ToString();
        }
        return dir;
    }

    bool DirCanBeEmpty(string dir, Creature creature) {
        if (dir == "") return true;
        if (creature.rotationOffset == 90f && dir == Item.Direction.left.ToString()) {
            return true;
        }
        if (creature.rotationOffset == 0f && dir == Item.Direction.down.ToString()) {
            return true;
        }
        return false;
    }

    bool CanBeEaten(Square sq, ItemController controller, Item.Direction dir) {
        if (sq == null) return false;
        ItemController ct = sq.ItemController;
        if (ct == null) return false;
        Creature cr = Controller.GameInit.CreaturesObject.Creatures.Find(c => c.id == ct.Item.creature_id);
        if (cr == null) return false;
        string availableDir = GetAvailableDir(ct.Item.direction, cr);
        if (availableDir != dir.ToString()) return false;
        return Controller.GameInit.Relationship[controller.Item.creature_id].eaten.Contains(ct.Item.creature_id);
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

    void PlayMatchedVFX(Square square) {
        StartCoroutine(square.ItemController.ScaleAndShake(shakeSpeed: 45f, scale: 1.5f));
        Controller.PlayVFXLeaf(square.Center);
    }


    [SerializeField]
    class AroundColorCounting {
        public Color color;
        public int count;
    }

    [SerializeField]
    public class SquareAttachItemParams {
        public bool animatedTo = false;
        public bool checkEndGame = false;
        public bool isRoot = false;
        public Color? color = null;
        public bool setSquareColor = true;
        public bool checkValidAroundSquares = false;
        public bool playVFX = true;
        public bool playSound = true;
        public float duration = 0.1f;
    }
}
