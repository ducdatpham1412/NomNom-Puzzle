using UnityEngine;
using UnityEngine.UI;

public class ItemLevel : MonoBehaviour {
    [SerializeField] Text text;

    public void SetLevel(int level) {
        text.text = $"{Helper.GetLocalizedValue("level")} {level}";
    }
}
