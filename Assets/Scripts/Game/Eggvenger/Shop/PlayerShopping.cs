using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShopping : MonoBehaviour {
    [SerializeField] CanvasGroup ShoppingDialog;
    [SerializeField] PlayerSkill PlayerSkill;
    [SerializeField] List<GunShop> GunShops = new List<GunShop>();
    [SerializeField] SkillShop[] SkillShops = new SkillShop[2];
    [SerializeField] Image BackgroundButton;
    [SerializeField] Text ButtonTitle;
    [SerializeField] Text EggsText;
    public List<DotsIndicator> FirstSkillDots = new List<DotsIndicator>();
    public List<DotsIndicator> SecondSkillDots = new List<DotsIndicator>();
    GunShop currentGunShop;
    SkillShop currentSkillShop;
    Button Button;

    void Start() {
        Button = BackgroundButton.GetComponent<Button>();
        foreach (var gun in GunShops) {
            gun.Manager = this;
        }
        foreach (var skill in SkillShops) {
            if (skill == null) {
                Debug.LogError("Skill shops length has to be 2");
            }
            skill.Manager = this;
        }
    }

    public void Open() {
        ShoppingDialog.alpha = 1;
        ShoppingDialog.interactable = true;
        ShoppingDialog.blocksRaycasts = true;
        UpdateButton();
        SoundManager.Instance.PlaySF(SoundManager.SF.SwitchItem);
    }

    public void Close() {
        ShoppingDialog.alpha = 0;
        ShoppingDialog.interactable = false;
        ShoppingDialog.blocksRaycasts = false;
        SoundManager.Instance.PlaySF(SoundManager.SF.KnockWood);
    }

    public void BuySell() {
        if (currentGunShop) {
            PlayerSkill.BuySellGun(currentGunShop.GunStats);
            EggsText.text = PlayerSkill.PlayerManager.eggs.ToString();
            UpdateGunSelected();
            UpdateButton();
        }
        else if (currentSkillShop) {
            PlayerSkill.BuySkill(currentSkillShop.Skill);
            EggsText.text = PlayerSkill.PlayerManager.eggs.ToString();
            UpdateSkills();
            UpdateButton();
        }
    }

    public void SelectGun(GunShop gun) {
        if (currentGunShop == gun) return;
        SoundManager.Instance.PlaySF(SoundManager.SF.SwitchItem);
        if (currentSkillShop) {
            currentSkillShop.UnFocus();
            currentSkillShop = null;
        }
        if (currentGunShop) {
            currentGunShop.UnFocus();
        }
        currentGunShop = gun;
        UpdateButton();
    }

    public void SelectSkill(SkillShop skill) {
        if (currentSkillShop == skill) return;
        SoundManager.Instance.PlaySF(SoundManager.SF.SwitchItem);
        if (currentGunShop) {
            currentGunShop.UnFocus();
            currentGunShop = null;
        }
        if (currentSkillShop) {
            currentSkillShop.UnFocus();
        }
        currentSkillShop = skill;
        UpdateButton();
    }

    public void SetPlayerSkill(PlayerSkill skill) {
        PlayerSkill = skill;
        foreach (var gun in GunShops) {
            gun.Manager = this;
        }
        if (skill.FirstSkill) {
            SkillShops[0].SetSkill(skill.FirstSkill);
            foreach (var dot in FirstSkillDots) {
                dot.GenerateDots(skill.FirstSkill.maxNumber, PlayerSkill.firstSkillNumber);
            }
        }
        if (skill.SecondSkill) {
            SkillShops[1].SetSkill(skill.SecondSkill);
            foreach (var dot in SecondSkillDots) {
                dot.GenerateDots(skill.SecondSkill.maxNumber, PlayerSkill.secondSkillNumber);
            }
        }
        EggsText.text = PlayerSkill.PlayerManager.eggs.ToString();
        UpdateGunSelected();
    }

    void UpdateGunSelected() {
        foreach (var gun in GunShops) {
            var FindGun = Array.Find(PlayerSkill.Guns, g => g != null && g.id == gun.GunStats.id);
            if (FindGun != null) {
                gun.Selected();
            }
            else {
                gun.Deselected();
            }
        }
    }

    void UpdateSkills() {
        if (currentSkillShop.Skill == PlayerSkill.FirstSkill) {
            foreach (var dot in FirstSkillDots) {
                dot.UpdateActiveDots(PlayerSkill.firstSkillNumber);
            }
        }
        else if (currentSkillShop.Skill == PlayerSkill.SecondSkill) {
            foreach (var dot in SecondSkillDots) {
                dot.UpdateActiveDots(PlayerSkill.secondSkillNumber);
            }
        }
    }

    void UpdateButton() {
        if (currentGunShop == null && currentSkillShop == null) {
            Button.interactable = false;
            return;
        }

        if (currentGunShop) {
            BackgroundButton.gameObject.SetActive(true);
            var FindGun = Array.Find(PlayerSkill.Guns, g => g != null && g.id == currentGunShop.GunStats.id);
            if (FindGun != null) {
                BackgroundButton.color = Helper.ColorFromHex(Configs.Color.red);
                ButtonTitle.text = Helper.GetLocalizedValue(LocalizationManager.Table.Game, "sell");
                Button.interactable = true;
            }
            else {
                BackgroundButton.color = Helper.ColorFromHex(Configs.Color.green);
                ButtonTitle.text = Helper.GetLocalizedValue(LocalizationManager.Table.Game, "buy");
                Button.interactable = PlayerSkill.PlayerManager.eggs >= currentGunShop.GunStats.price;
            }
        }
        else if (currentSkillShop) {
            BackgroundButton.color = Helper.ColorFromHex(Configs.Color.green);
            ButtonTitle.text = Helper.GetLocalizedValue(LocalizationManager.Table.Game, "buy");

            bool check = PlayerSkill.CheckSkillFull(currentSkillShop.Skill);
            if (check) {
                Button.interactable = false;
                return;
            }

            Button.interactable = PlayerSkill.PlayerManager.eggs >= currentSkillShop.Skill.price;
        }
    }
}
