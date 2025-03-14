using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DotsIndicator : MonoBehaviour {
    [SerializeField] GameObject dotPrefab;
    [SerializeField] bool reverse = false; // Reverse in case Child Alignment to Right
    List<Image> dots = new List<Image>();

    public void GenerateDots(int numbers, int value = 0) {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        dots.Clear();
        for (int i = 0; i < numbers; i++) {
            Image dot = Instantiate(dotPrefab, gameObject.transform).GetComponent<Image>();
            dot.color = Color.white;
            dots.Add(dot.GetComponent<Image>());
        }
        if (reverse) {
            dots.Reverse();
        }
        UpdateActiveDots(value);
    }

    public void UpdateActiveDots(int value) {
        for (int i = 0; i < dots.Count; i++) {
            if (i < value) {
                dots[i].color = Color.white;
            }
            else {
                Color temp = Color.white;
                temp.a = 0.3f;
                dots[i].color = temp;
            }
        }
    }
}
