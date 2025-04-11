using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InfoDialog : MonoBehaviour {
    [SerializeField] RectTransform Content;
    [SerializeField] Image Image;
    [SerializeField] Text Title;
    [SerializeField] Text BtnTitle;
    [SerializeField] GameObject CloseButton;
    [SerializeField] SoundManager.SF soundEffect = SoundManager.SF.Pop_01;
    Action OnClick;

    public void ClickButton() {
        SoundManager.Instance.PlaySF(soundEffect);
        OnClick?.Invoke();
    }

    public void Open(Info info) {
        Title.text = info.title;
        Title.fontSize = info.fontSize;
        BtnTitle.text = info.btnTitle;
        OnClick = info.OnClick;
        CloseButton.SetActive(info.canClose != false);
        soundEffect = info.sfx;

        gameObject.SetActive(true);
        StartCoroutine(RebuildAfterOneFrame());
        return;
    }

    public void Close() {
        gameObject.SetActive(false);
    }

    IEnumerator RebuildAfterOneFrame() {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
    }

    [SerializeField]
    public class Info {
        public string title;
        public int fontSize = 24;
        public string btnTitle;
        public bool canClose = true;
        public Action OnClick;
        public SoundManager.SF sfx = SoundManager.SF.Pop_01;
    }
}
