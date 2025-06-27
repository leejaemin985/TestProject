using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimController : MonoBehaviour
{
    private const string KEYNAME_MOVE_HORIZONTAL = "_Horizontal";
    private const string KEYNAME_MOVE_VERTICAL= "_Vertical";

    private Animator anim;

    private Vector2 targetMoveAnimDir = default;
    private Vector2 moveAnimDir = default;
    private float curveSpeed = 10f;

    public void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        moveAnimDir = Vector2.Lerp(moveAnimDir, targetMoveAnimDir, curveSpeed * Time.deltaTime);
        anim.SetFloat(KEYNAME_MOVE_HORIZONTAL, moveAnimDir.x);
        anim.SetFloat(KEYNAME_MOVE_VERTICAL, moveAnimDir.y);
    }

    public void SetMoveAnimDirection(Vector2 dir) => targetMoveAnimDir = new Vector2(dir.x, dir.y).normalized;
}