using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputSecurity : MonoBehaviour {
    public Sprite eye;
    public Sprite eyeClose;
    public TMP_InputField inputField;

    private bool security = true;


    void Start() {
        inputField.contentType = TMP_InputField.ContentType.Password;
    }


    public void OnChangeSecurity() {
        Image imgEye = transform.Find("Eye").GetComponent<Image>();
        security = !security;
        imgEye.sprite = security ? eye : eyeClose;
        inputField.contentType = security ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        inputField.text = inputField.text;
        inputField.ForceLabelUpdate();
    }
}
