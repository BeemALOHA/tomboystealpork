using UnityEngine;
using UnityEngine.InputSystem;
public class LaneSwapController : MonoBehaviour
{
    [Header("ตัวละคร 2 ฝั่ง")]
    public RunnerController runnerA;
    public RunnerController runnerB;

    [Header("จุดอ้างอิง (Create Empty)")]
    [Tooltip("จุดอ้างอิงจุดที่ 1 (ลาก Create Empty มาใส่)")]
    public Transform point1;
    [Tooltip("จุดอ้างอิงจุดที่ 2 (ลาก Create Empty มาใส่)")]
    public Transform point2;

    [Header("การวาร์ป")]
    [Tooltip("หน่วงเวลาก่อนกดวาร์ปซ้ำได้อีก (วินาที)")]
    public float swapCooldown = 0.15f;
    [Tooltip("รีเซ็ตความเร็วแกน Y เป็น 0 ตอนวาร์ป เพื่อไม่ให้พุ่งต่อแรงเกินไป")]
    public bool resetVerticalVelocityOnWarp = true;
    [Tooltip("หากตัวละครมีระบบกลับทิศแรงโน้มถ่วง ให้สลับทิศตามจุดปลายทางด้วยไหม")]
    public bool syncGravityToTarget = false;

    [Header("เอฟเฟกต์ตอนวาร์ป (ไม่บังคับ)")]
    public GameObject warpEffectPrefab;

    private float cooldownTimer;
    private bool aAtPoint1 = true;

    private void Start()
    {
        SyncInitialState();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && SwapPressedThisFrame())
        {
            TriggerWarpSwap();
        }
    }

    private void SyncInitialState()
    {
        if (runnerA == null || point1 == null || point2 == null) return;

        float distToP1 = Mathf.Abs(GetCurrentY(runnerA) - point1.position.y);
        float distToP2 = Mathf.Abs(GetCurrentY(runnerA) - point2.position.y);
        aAtPoint1 = distToP1 <= distToP2;
    }

    public void TriggerWarpSwap()
    {
        if (point1 == null || point2 == null) return;
        if (runnerA == null && runnerB == null) return;
        if ((runnerA != null && runnerA.IsDead) || (runnerB != null && runnerB.IsDead)) return;

        float targetAY = aAtPoint1 ? point2.position.y : point1.position.y;
        float targetBY = aAtPoint1 ? point1.position.y : point2.position.y;

        if (runnerA != null)
        {
            WarpRunnerToY(runnerA, targetAY);
            if (syncGravityToTarget)
                SetRunnerGravitySign(runnerA, targetAY >= targetBY ? 1 : -1);
        }

        if (runnerB != null)
        {
            WarpRunnerToY(runnerB, targetBY);
            if (syncGravityToTarget)
                SetRunnerGravitySign(runnerB, targetBY >= targetAY ? 1 : -1);
        }

        aAtPoint1 = !aAtPoint1;
        cooldownTimer = swapCooldown;
    }

    private float GetCurrentY(RunnerController runner)
    {
        Rigidbody2D rb = runner.GetComponent<Rigidbody2D>();
        return rb != null ? rb.position.y : runner.transform.position.y;
    }

    private void WarpRunnerToY(RunnerController runner, float desiredY)
    {
        if (runner == null || runner.IsDead) return;

        if (warpEffectPrefab != null)
        {
            Instantiate(warpEffectPrefab, runner.transform.position, Quaternion.identity);
        }

        Rigidbody2D rb = runner.GetComponent<Rigidbody2D>();
        float currentX = rb != null ? rb.position.x : runner.transform.position.x;
        float currentZ = runner.transform.position.z;

        if (rb != null)
        {
            rb.position = new Vector2(currentX, desiredY);
            if (resetVerticalVelocityOnWarp)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector2.zero;
#else
                rb.velocity = Vector2.zero;
#endif
            }
        }

        runner.transform.position = new Vector3(currentX, desiredY, currentZ);

        if (warpEffectPrefab != null)
        {
            Instantiate(warpEffectPrefab, runner.transform.position, Quaternion.identity);
        }
    }

    private void SetRunnerGravitySign(RunnerController runner, int sign)
    {
        Rigidbody2D rb = runner.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        float mag = Mathf.Abs(rb.gravityScale);
        rb.gravityScale = mag * -sign;
    }

    private static bool SwapPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.qKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Q);
#endif
    }

    private void OnDrawGizmos()
    {
        if (point1 != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(point1.position, 0.25f);
        }

        if (point2 != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point2.position, 0.25f);
        }

        if (point1 != null && point2 != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(point1.position, point2.position);
        }
    }
}