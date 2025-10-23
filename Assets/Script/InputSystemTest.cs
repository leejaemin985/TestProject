using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTest : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputAsset;

    InputAction move;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        move = inputAsset.FindActionMap("PlayerActions", true).FindAction("Move", true);
        inputAsset.Enable();

        PrintAllBindings();
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            RebindCompositeKey("up");
            //move.ApplyBindingOverride()
        }

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            PrintAllBindings();
        }

    }

    private void PrintAllBindings()
    {
        Debug.Log($"== Move All Bindings");
        foreach (var bind in move.bindings)
        {
            string effetivePath = bind.effectivePath;
            string overrideInfo = bind.overridePath;

            Debug.Log($"[{bind.name}] - Path (origin: {bind.path}, override: '{effetivePath}'{overrideInfo})");
        }
    }

    private void RebindCompositeKey(string compositePart)
    {
        move.Disable();

        int bindingIndex = move.bindings.IndexOf(x => x.isPartOfComposite && x.name == compositePart);
        if (bindingIndex == -1)
        {
            Debug.LogError($"{compositePart}를 찾을 수 없습니다.");
            move.Enable();
            return;
        }

        move.ApplyBindingOverride(bindingIndex, "<keyboard>/o");
        move.Enable();

        return;
        Debug.Log($"{compositePart} 키를 변경합니다. 새 키를 눌러주세요...");
        move.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(operation =>
            {
                Debug.Log($"{compositePart} 리바인딩 완료!");
                operation.Dispose();
                move.Enable();
            })
            .OnCancel(operation =>
            {
                Debug.Log($"리바인딩 취소");
                operation.Dispose();
                move.Enable();
            })
            .Start();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input == null) return;
        Debug.Log($"Test - input: {input} (context)");
    }

}
