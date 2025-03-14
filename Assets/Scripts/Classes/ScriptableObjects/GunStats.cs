using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunStats", menuName = "Scriptable Objects/GunStats")]
public class GunStats : ScriptableObject {
    [Serializable]
    public class GunName {
        public string key;
        public string name;
    }
    [Serializable]
    public class BulletStats {
        public float damage;
        public float speed;
    }

    public string id;

    [Header("Resources")]
    public Sprite Gun;
    public Sprite GunUI; // Display on UI in GamePad
    public Sprite Bullet;
    public AudioClip ShotSound;
    public AudioClip ReloadSound;
    public AudioClip EquipSound;

    [Header("Languages")]
    public List<GunName> gunNames = new List<GunName> {
        new GunName { key = "vi", name = "Súng" },
        new GunName { key = "en", name = "Gun" }
    };

    [Header("Prices")]
    public int price;
    public int sellPrice;

    [Header("Stats")]
    public int burstAmount = 1;
    public bool holdToBurst = true;
    public float fireDelay = 0.05f; // Delay between 2 fires in a burst
    public float burstDelay = 0.1f; // Delay between 2 bursts
    public float reloadDelay = 1f;
    public bool isLimitBullets = true;
    public int numberBullets; // If isLimitBullet = false => Not need to using this
    public int magazineSize;
    public int currentBullets;
    public BulletStats bulletStats;

    [Header("Aiming stats")]
    public float aimingRadius;
    public float aimingAngle;
}
