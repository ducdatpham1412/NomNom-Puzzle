using UnityEngine;

public static class GameObjectExtensions {
    public static GameObject[] GetAllChildren(this GameObject parent) {
        int childCount = parent.transform.childCount;
        GameObject[] children = new GameObject[childCount];

        for (int i = 0; i < childCount; i++) {
            children[i] = parent.transform.GetChild(i).gameObject;
        }

        return children;
    }
}
