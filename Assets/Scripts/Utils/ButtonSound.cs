using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour {
    [SerializeField] Texture IconSound;
    [SerializeField] Texture IconSoundMute;
    [SerializeField] RawImage iconSoundImg;

    public void SetPlaying(bool value) {
        iconSoundImg.texture = value ? IconSound : IconSoundMute;
    }

}
