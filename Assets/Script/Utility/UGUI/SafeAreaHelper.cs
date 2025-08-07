using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeAreaHelper : MonoSingleton<SafeAreaHelper>
{
    private void Awake()
    {
        SceneManager.sceneLoaded -= RefreshSceneForSafeArea;
        SceneManager.sceneLoaded += RefreshSceneForSafeArea;
    }

    private void RefreshSceneForSafeArea(Scene scene, LoadSceneMode _)
    {
        var canvasArr = FindObjectsOfType<Canvas>(true);

        foreach (var canvas in canvasArr)
        {
            var child = canvas.transform.Find("SafeArea");
            if (child != null) continue;

            GameObject go = new GameObject("SafeArea", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            go.AddComponent<SafeArea>();

            var rect = (RectTransform)go.transform;
            // Set Stretch All
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;


            List<Transform> moveTransform = new();
            
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                var trans = canvas.transform.GetChild(i);
                if (trans.CompareTag("NonSafeAreaBehind")) continue;
                if (trans.CompareTag("NonSafeAreaFront"))
                {
                    trans.SetAsLastSibling();
                    continue;
                }
                if (trans.name.Equals("SafeArea")) continue;

                moveTransform.Add(trans);
            }
            foreach (var trans in moveTransform)
            {
                trans.SetParent(go.transform, true);
                trans.localScale = Vector3.one;
            }
        }
    }
}
