using System.Collections;
using UnityEngine;

public class Suggestions : MonoBehaviour {
    [SerializeField] ButtonManager BtnSuggest01;
    [SerializeField] ButtonManager BtnSuggest03;
    [SerializeField] int timeOffset = 30;

    int lastUse01 = 0;
    int lastUse03 = 0;

    void OnEnable() {
        int time01 = Mathf.CeilToInt(Time.time - lastUse01);
        if (lastUse01 == 0 || time01 > timeOffset) {
            BtnSuggest01.Title.text = Helper.GetLocalizedValue("suggestNumber", new string[] { "1" });
            BtnSuggest01.Enable();
        }
        else {
            StartCoroutine(CountDown(BtnSuggest01, Helper.GetLocalizedValue("suggestNumber", new string[] { "1" }), timeOffset - time01));
        }


        int time03 = Mathf.CeilToInt(Time.time - lastUse03);
        if (lastUse03 == 0 || time03 > timeOffset) {
            BtnSuggest03.Title.text = Helper.GetLocalizedValue("suggestNumber", new string[] { "3" });
            BtnSuggest03.Enable();
        }
        else {
            StartCoroutine(CountDown(BtnSuggest03, Helper.GetLocalizedValue("suggestNumber", new string[] { "3" }), timeOffset - time03));
        }
    }

    public void Use01() {
        lastUse01 = Mathf.FloorToInt(Time.time);

    }

    public void Use03() {
        lastUse03 = Mathf.FloorToInt(Time.time);
    }

    IEnumerator CountDown(ButtonManager Btn, string text, int countdown) {
        Btn.Disable();
        while (countdown >= 0) {
            Btn.Title.text = $"{text} ({countdown})";
            countdown--;
            yield return new WaitForSeconds(1f);
        }
        Btn.Title.text = text;
        Btn.Enable();
    }
}
