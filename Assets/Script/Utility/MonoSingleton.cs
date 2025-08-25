using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    GameObject newInstance = new GameObject(typeof(T).Name);
                    instance = newInstance.AddComponent<T>();
                }

                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            if (instance != this as T)
            {
                Destroy(this);
            }
        }

        DontDestroyOnLoad(instance);
    }

    protected virtual void OnAwake() { }    
}
