using UnityEngine;
using UnityEngine.UI;

public class GamePoints : MonoBehaviour {
    public Text TextValue;
    public GameObject Points;

    public void UpdatePoint(int point) {
        GameObject[] children = Points.GetAllChildren();

        for (int i = 0; i < children.Length; i++) {
            bool isGreen = i + 1 <= point;
            Image img = children[i].GetComponent<Image>();
            img.color = isGreen ? Helper.ColorFromHex(Configs.Color.green01) : new Color(1f, 1f, 1f, 0.8f);
        }
    }
}
