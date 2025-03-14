using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour {
    [Header("GameObjects")]
    [SerializeField] Transform GunTransform;
    public Transform GunHead;
    public BulletPool Pool;
    public GunStats[] Guns = new GunStats[2];
    [SerializeField] GunStats[] RoundGuns = new GunStats[2];

    [Header("Skills")]
    public BaseSkill FirstSkill;
    public BaseSkill SecondSkill;
    public BaseSkill Ulti; // TODO: Develop later

    [Header("Stats")]
    public Vector2 direction = Vector2.right;
    public int firstSkillNumber = 0;
    public int secondSkillNumber = 0;

    [Header("Runtime Value")]
    public BaseSkill CurrentSkill;
    public GunStats CurrentGun;
    public PlayerManager PlayerManager;

    bool isBursting = false;
    SpriteRenderer GunRenderer;
    AudioSource GunAudio;
    PlayerGamepad Gamepad;
    PlayerShopping Shopping;
    Vector2 baseVector = Vector2.right;
    Vector2 lastDirection;
    Coroutine reloadCoroutine;
    float originalCameraSize = 6f;

    void Start() {
        PlayerManager = GetComponent<PlayerManager>();
        GunRenderer = GunTransform.GetComponent<SpriteRenderer>();
        GunAudio = GunHead.GetComponent<AudioSource>();
        if (PlayerManager.IsOwner) {
            Gamepad = PlayerManager.Manager.GetComponent<PlayerGamepad>();
            Gamepad.SetPlayerSkill(this);
            Shopping = PlayerManager.Manager.GetComponent<PlayerShopping>();
            Shopping.SetPlayerSkill(this);
        }
        ResetGuns();
        originalCameraSize = GameHelper.world.maxY;
    }

    // TODO: Assign PlayerGamepad in OnNetworkSpawn

    void Update() {
        HandleGunDirection();
    }

    public void ChangeGun() {
        int index = Array.IndexOf(RoundGuns, CurrentGun);
        index = (index + 1) % RoundGuns.Length;
        if (RoundGuns[index] != null) {
            CheckToStopReload();
            EquipGun(RoundGuns[index]);
        }
    }

    public void PlayHit(bool forceOneShot) {
        if (!CurrentSkill) {
            StartCoroutine(Burst(forceOneShot));
        }
        else {
            PlaySkill();
        }
    }

    public void PressSkill(int index) {
        bool played = false;

        Gamepad.ResetCurrentActions();

        if (index == 0) {
            played = CheckSkill(FirstSkill);
        }

        if (index == 1) {
            played = CheckSkill(SecondSkill);
        }

        if (index == 2) {
            played = CheckSkill(Ulti);
        }

        if (!played) {
            CheckToStopReload();
        }
    }

    public Vector3 GetSkillSpawnPos(bool isLocal = true) {
        Vector3 size = gameObject.GetComponent<SpriteRenderer>().bounds.size;
        if (isLocal) return new Vector3(0f, size.y * 1 / 4.5f, 0f);
        return new Vector3(transform.position.x, transform.position.y + size.y * 1 / 4.5f, 0f);
    }

    public void OpenCloseAiming(bool isAiming) {
        Light2D Aiming = PlayerManager.AimingLight;
        GunAudio.PlayOneShot(PlayerManager.Manager.OutOfAmmo);

        if (isAiming) {
            PlayerManager.FOVLight.enabled = false;
            PlayerManager.BodyLight.gameObject.SetActive(true);

            Aiming.gameObject.SetActive(true);
            Aiming.pointLightInnerRadius = 0f;
            Aiming.pointLightOuterRadius = CurrentGun.aimingRadius;
            Aiming.pointLightInnerAngle = CurrentGun.aimingAngle;
            Aiming.pointLightOuterAngle = CurrentGun.aimingAngle;
            StartCoroutine(ChangeCameraSize(CurrentGun.aimingRadius / 2 + 0.5f));
            return;
        }

        PlayerManager.FOVLight.enabled = true;
        PlayerManager.BodyLight.gameObject.SetActive(false);
        Aiming.gameObject.SetActive(false);
        StartCoroutine(ChangeCameraSize(originalCameraSize));
    }

    /*
    Guns
    */
    void ResetGuns(bool playSound = true) {
        for (int i = 0; i < Guns.Length; i++) {
            if (Guns[i] != null) {
                RoundGuns[i] = Instantiate(Guns[i]);
            }
            else {
                RoundGuns[i] = null;
            }
        }
        if (CurrentGun != null) {
            GunStats FindGun = Array.Find(RoundGuns, g => g != null && g.id == CurrentGun.id);
            EquipGun(FindGun ?? RoundGuns[0], playSound);
        }
        else {
            EquipGun(RoundGuns[0], playSound);
        }
    }

    public void BuySellGun(GunStats gun) {
        GunStats FindGun = Array.Find(RoundGuns, g => g != null && g.id == gun.id);

        // Sell gun
        if (FindGun != null) {
            SoundManager.Instance.PlaySF(SoundManager.SF.Sell);
            int index = Array.IndexOf(RoundGuns, FindGun);
            RoundGuns[index] = null;
            PlayerManager.eggs += FindGun.sellPrice;
            CurrentGun = null;
            ResetGuns(playSound: false);
            return;
        }

        // Buy gun
        int eggsRemaining = PlayerManager.eggs - gun.price;
        if (eggsRemaining >= 0) {
            if (RoundGuns[1] != null) {
                // TODO: Throw this gun for other player collect
            }

            Guns[1] = gun;
            RoundGuns[1] = gun;
            CurrentGun = gun;
            PlayerManager.eggs -= gun.price;
            ResetGuns();
        }
    }

    public void ReloadGun() {
        if (reloadCoroutine != null || (CurrentGun.numberBullets <= 0 && CurrentGun.isLimitBullets)) return;

        void Reload() {
            if (CurrentGun.isLimitBullets) {
                int bulletsNeedMore = CurrentGun.magazineSize - CurrentGun.currentBullets;
                int bulletsActuallyAvailable = Math.Min(bulletsNeedMore, CurrentGun.numberBullets);
                CurrentGun.currentBullets += bulletsActuallyAvailable;
                CurrentGun.numberBullets -= bulletsActuallyAvailable;
                Gamepad.TextRemainingBullets.text = CurrentGun.numberBullets.ToString();
            }
            else {
                CurrentGun.currentBullets = CurrentGun.magazineSize;
            }
            Gamepad.TextCurrentBullets.text = CurrentGun.currentBullets.ToString();
            reloadCoroutine = null;
        }

        if (CurrentGun.ReloadSound != null) {
            GunAudio.PlayOneShot(CurrentGun.ReloadSound);
        }
        reloadCoroutine = StartCoroutine(Gamepad.Countdown(Gamepad.ReloadCountdown, CurrentGun.reloadDelay, Reload));
    }

    public void PlaySoundLightSwitch() {
        GunAudio.PlayOneShot(PlayerManager.Manager.LightSwitch);
    }

    /*
    Skills
    */
    public void BuySkill(BaseSkill skill) {
        if (PlayerManager.eggs < skill.price || CheckSkillFull(skill)) return;
        SoundManager.Instance.PlaySF(SoundManager.SF.Buy);
        PlayerManager.eggs -= skill.price;
        if (skill == FirstSkill) {
            firstSkillNumber += 1;
        }
        else if (skill == SecondSkill) {
            secondSkillNumber += 1;
        }
    }

    public bool CheckSkillFull(BaseSkill skill) {
        if (skill == FirstSkill) {
            return firstSkillNumber >= skill.maxNumber;
        }
        if (skill == SecondSkill) {
            return secondSkillNumber >= skill.maxNumber;
        }
        throw new Exception("Can not check skill is full or not");
    }


    /*
    Coroutines
    */
    public IEnumerator DrawSkillTrajectoryUntilHolding() {
        while (!Gamepad.isHolding && CurrentSkill != null) {
            Gamepad.Trajectory.DrawStraight(CurrentSkill.transform.position, GetDirection(), radius: CurrentGun.aimingRadius);
            yield return null;
        }
    }

    public IEnumerator DrawBulletTrajectoryUntilHolding() {
        while (!Gamepad.isHolding && Gamepad.isTraject) {
            Gamepad.Trajectory.DrawStraight(GunHead.transform.position, GetDirection(), radius: CurrentGun.aimingRadius);
            yield return null;
        }
    }

    IEnumerator Burst(bool forceOneShot = false) {
        if (isBursting) yield break;

        if (CurrentGun.currentBullets <= 0) {
            if (!GunAudio.isPlaying) {
                GunAudio.PlayOneShot(PlayerManager.Manager.OutOfAmmo);
            }
            yield break;
        }

        isBursting = true;

        CheckToStopReload();

        if (forceOneShot || !CurrentGun.holdToBurst) {
            yield return StartCoroutine(Fire());
        }
        else {
            while (Gamepad.isHolding && CurrentGun.currentBullets > 0) {
                yield return StartCoroutine(Fire());
            }
        }
        isBursting = false;
    }

    IEnumerator Fire() {
        int burstAmount = 1;
        if (CurrentGun.burstAmount == 1) {
            GunAudio.Play();
            Bullet bullet = Pool.GetBullet();
            bullet.transform.position = GunHead.position;
            bullet.transform.rotation = GunTransform.rotation;
        }
        else if (CurrentGun.burstAmount > 1) {
            burstAmount = Math.Min(CurrentGun.burstAmount, CurrentGun.currentBullets);
            GunAudio.Play();
            Bullet[] bullets = Pool.GetBullets(burstAmount);
            foreach (var b in bullets) {
                b.gameObject.SetActive(true);
                b.transform.position = GunHead.position;
                b.transform.rotation = GunTransform.rotation;
                yield return new WaitForSeconds(CurrentGun.fireDelay);
            }
        }
        CurrentGun.currentBullets -= burstAmount;
        Gamepad.TextCurrentBullets.text = CurrentGun.currentBullets.ToString();
        if (CurrentGun.currentBullets <= 0) {
            ReloadGun();
            yield break;
        }
        if (CurrentGun.burstDelay >= 0.5f) {
            StartCoroutine(Gamepad.Countdown(Gamepad.ShotCountdown, CurrentGun.burstDelay));
        }
        yield return new WaitForSeconds(CurrentGun.burstDelay);
    }

    IEnumerator ChangeCameraSize(float targetSize) {
        float duration = 0.35f;
        float currentSize = PlayerManager.MainCamera.orthographicSize;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            PlayerManager.MainCamera.orthographicSize = Mathf.Lerp(currentSize, targetSize, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        PlayerManager.MainCamera.orthographicSize = targetSize;
    }

    void EquipGun(GunStats gun, bool playSound = true) {
        CurrentGun = gun;

        if (CurrentGun.EquipSound && playSound) {
            GunAudio.PlayOneShot(CurrentGun.EquipSound);
        }

        GunAudio.clip = CurrentGun.ShotSound;
        GunRenderer.sprite = CurrentGun.Gun;
        Pool.FillPool(CurrentGun);

        Vector3 size = GunRenderer.sprite.bounds.size;
        float pivotX = GunRenderer.sprite.pivot.x / GunRenderer.sprite.rect.width;
        float maxX = size.x * (1 - pivotX);
        GunHead.localPosition = new Vector2(maxX, 0f);

        if (Gamepad) {
            Gamepad.CurrentGunUI.sprite = CurrentGun.GunUI != null ? CurrentGun.GunUI : CurrentGun.Gun;
            Gamepad.BulletUI.GetComponent<Image>().sprite = Gamepad.BulletSprite;
            Gamepad.TextCurrentBullets.text = CurrentGun.currentBullets.ToString();
            Gamepad.TextRemainingBullets.text = CurrentGun.isLimitBullets ? CurrentGun.numberBullets.ToString() : "";
            Gamepad.ResetCurrentActions();
        }
    }

    void BackToGunFromSkill(bool playSound) {
        if (Gamepad) {
            Gamepad.Trajectory.RemoveLine();
            Gamepad.BulletUI.GetComponent<Image>().sprite = Gamepad.BulletSprite;
        }
        CurrentSkill = null;
        GunTransform.gameObject.SetActive(true);
        if (playSound) {
            GunAudio.PlayOneShot(CurrentGun.EquipSound);
        }
    }

    void HandleGunDirection() {
        if (!PlayerManager.IsOwner) return;

        Vector2 temp = GetDirection();
        if (Equals(temp, lastDirection)) return;
        lastDirection = temp;

        // Check rotation
        float degree = Vector2.SignedAngle(baseVector, temp);
        Quaternion newRotation = Quaternion.Euler(0, 0, degree);
        GunTransform.rotation = newRotation;
        if (Gamepad) {
            Gamepad.BulletUI.rotation = newRotation;
        }
        if (CurrentSkill != null) {
            CurrentSkill.transform.rotation = newRotation;
        }

        // Check flipY
        degree = Math.Abs(degree);
        GunRenderer.flipY = degree > 90;

        // Check scale
        if (degree > 90) {
            degree = degree - (degree - 90) * 2;
        }
        float scaleX = Mathf.Lerp(1, 0.75f, degree / 90);
        float scaleY = Mathf.Lerp(1, 0.6f, degree / 90);
        GunTransform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    bool CheckSkill(BaseSkill Skill) {
        if (Skill == null) {
            return true;
        }

        if (Skill == FirstSkill && firstSkillNumber <= 0) return true;
        if (Skill == SecondSkill && secondSkillNumber <= 0) return true;

        if (Gamepad && Gamepad.isHolding) Gamepad.isHolding = false;

        if (CurrentSkill == null || CurrentSkill.id != Skill.id) {
            GunTransform.gameObject.SetActive(false);
            if (CurrentSkill != null) {
                Destroy(CurrentSkill.gameObject);
            }
            CurrentSkill = Instantiate(Skill.gameObject, GetSkillSpawnPos(isLocal: false), Quaternion.identity).GetComponent<BaseSkill>();
            CurrentSkill.SetCreator(PlayerManager);
            if (!CurrentSkill.canReady) {
                PlaySkill();
                return true;
            }
            if (CurrentSkill.SkillSprite != null && Gamepad != null) {
                Gamepad.BulletUI.GetComponent<Image>().sprite = CurrentSkill.SkillSprite;
            }
            CurrentSkill.Ready(direction);
            StartCoroutine(DrawSkillTrajectoryUntilHolding());
            return false;
        }

        // If CurrentSkill == Skill => Turn off skill and switch back to Shotting mode
        Destroy(CurrentSkill.gameObject);
        BackToGunFromSkill(true);
        return true;
    }

    void PlaySkill() {
        if (CurrentSkill.id == FirstSkill.id) {
            firstSkillNumber -= 1;
        }
        else if (CurrentSkill.id == SecondSkill.id) {
            secondSkillNumber -= 1;
        }
        CurrentSkill.Play(GetDirection());
        BackToGunFromSkill(false);
        foreach (var dot in Shopping.FirstSkillDots) {
            dot.UpdateActiveDots(firstSkillNumber);
        }
        foreach (var dot in Shopping.SecondSkillDots) {
            dot.UpdateActiveDots(secondSkillNumber);
        }
    }

    Vector2 GetDirection() {
        return direction == Vector2.zero ? (PlayerManager.Movement.moveDirection == Vector2.zero ? PlayerManager.Movement.lastDirection : PlayerManager.Movement.moveDirection) : direction;
    }

    void CheckToStopReload() {
        if (reloadCoroutine != null) {
            StopCoroutine(reloadCoroutine);
            Gamepad.ReloadCountdown.fillAmount = 0;
            reloadCoroutine = null;
        }
    }
}

