using UnityEngine;
using UnityEngine.UI;

public class ItemCreature : MonoBehaviour {
    [SerializeField] Image Background;
    [SerializeField] Image Img;
    [SerializeField] Text Name;
    [SerializeField] Button Btn;
    public Creature creature;

    public void SetCreature(Creature cr, InformationController controller, string localeKey, bool enableClick = false) {
        creature = cr;
        Img.sprite = cr.sprite;
        Name.text = cr.name.Find(n => n.key == localeKey)?.value ?? "";
        Btn.onClick.RemoveAllListeners();
        if (enableClick) {
            Btn.onClick.AddListener(() => controller.OnClickCreature(this));
        }
    }

    public void SetActive(bool active) {
        Background.color = active ? new Color(1, 1, 1, 0.6f) : new Color(0, 0, 0, 0.8f);
    }
}
