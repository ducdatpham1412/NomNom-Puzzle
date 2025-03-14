using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour {
    public Texture IconSound;
    public Texture IconSoundMute;
    private RawImage iconSoundImg;

    void Start() {
        Transform iconSound = transform.Find("IconSound");
        if (iconSound != null) {
            iconSoundImg = iconSound.GetComponent<RawImage>();
        }
        SetPlaying(SoundManager.Instance.Music.isPlaying);
        SoundManager.Instance.OnMusicPlaying += SetPlaying;
    }

    void OnDestroy() {
        SoundManager.Instance.OnMusicPlaying -= SetPlaying;
    }

    public void SetPlaying(bool value) {
        iconSoundImg.texture = value ? IconSound : IconSoundMute;
    }

}
