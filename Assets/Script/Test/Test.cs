using UnityEngine;
using Addressable;
using Utility.EffectObject;
using System.Security.Principal;

public class Test : MonoBehaviour
{
    public EffectObject roarEffect;
    public EffectObjectPool pool;

    // Start is called before the first frame update
    private async void Start()
    {
        var ob = await AddressableManager.LoadAsset<GameObject>(AddressableKey.PK_UserStateEffectGroup);
        roarEffect = ob.GetComponent<AddressableAsset_UserStateEffect>().roarStateEffect;
        pool = EffectObjectPool.CreatePoolInstance<RoarStateEffect>((RoarStateEffect)roarEffect, new() { count = 5, effectRoot = null });
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            pool.OnPlayEffect(default, default);
        }
    }
}
