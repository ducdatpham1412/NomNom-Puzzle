using UnityEngine;
using UnityEngine.UI;

public class GunShop : MonoBehaviour {
    [Header("UI Element")]
    [SerializeField] Image Background;
    [SerializeField] Image GunImage;
    [SerializeField] Text GunName;
    [SerializeField] Text BuyText;
    [SerializeField] Text SellText;
    [SerializeField] GameObject Indicator;

    [Header("Stats")]
    public PlayerShopping Manager;
    public GunStats GunStats;

    void Start() {
        GunImage.sprite = GunStats.GunUI != null ? GunStats.GunUI : GunStats.Gun;
        var gun = GunStats.gunNames.Find(g => g.key == Helper.GetLocaleKey());
        GunName.text = gun != null ? gun.name : "";
        BuyText.text = $"{Helper.GetLocalizedValue(LocalizationManager.Table.Game, "buy")}: {GunStats.price}";
        SellText.text = $"{Helper.GetLocalizedValue(LocalizationManager.Table.Game, "sell")}: {GunStats.sellPrice}";
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Selected() {
        Color c = Helper.ColorFromHex("#E02B0C");
        c.a = 0.4f;
        Background.color = c;
    }

    public void Deselected() {
        Color c = Helper.ColorFromHex("#525252");
        c.a = 0.4f;
        Background.color = c;
    }

    public void UnFocus() {
        Indicator.SetActive(false);
    }

    void OnClick() {
        Indicator.SetActive(true);
        Manager.SelectGun(this);
        // TODO: Add sound effect
    }
}
