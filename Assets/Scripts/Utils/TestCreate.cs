using UnityEngine;

public class TestCreate : MonoBehaviour {
    public string input_01 = "";
    public string input_02 = "";
    public string input_03 = "";

    public void OnChangeInput01(string value) {
        input_01 = value;
    }

    public void OnChangeInput02(string value) {
        input_02 = value;
    }

    public void OnChangeInput03(string value) {
        input_03 = value;
    }
}
