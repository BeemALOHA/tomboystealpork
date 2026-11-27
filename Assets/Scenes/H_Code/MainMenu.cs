using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// หน้าเมนูหลัก
///   MainPanel  : START / QUIT
///   ModePanel  : STORY / ENDLESS / BACK
///   LevelPanel : ด่าน 1,2,3... / BACK
/// ใส่สคริปต์นี้ที่ GameObject "Menu" ในฉาก (ไม่ใช่ที่ไฟล์สคริปต์)
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("หน้าจอ (ลาก Panel จาก Hierarchy มาใส่)")]
    public GameObject mainPanel;
    public GameObject modePanel;
    public GameObject levelPanel;

    [Header("ฉาก (ชื่อต้องตรงกับไฟล์ .unity และอยู่ใน Build Profiles)")]
    [Tooltip("ฉากของโหมดเล่นไปเรื่อย ๆ")]
    public string endlessScene = "hee";

    void Start()
    {
        Time.timeScale = 1f;
        ShowMain();
    }

    // ---------- สลับหน้า ----------
    public void ShowMain() => Show(mainPanel);   // ปุ่ม BACK ในหน้าโหมด
    public void ShowModes() => Show(modePanel);   // ปุ่ม START, ปุ่ม BACK ในหน้าด่าน
    public void ShowLevels() => Show(levelPanel);  // ปุ่ม STORY

    void Show(GameObject panel)
    {
        if (mainPanel) mainPanel.SetActive(panel == mainPanel);
        if (modePanel) modePanel.SetActive(panel == modePanel);
        if (levelPanel) levelPanel.SetActive(panel == levelPanel);
    }

    // ---------- เข้าเกม ----------
    // ปุ่ม ENDLESS
    public void PlayEndless() => LoadLevel(endlessScene);

    // ปุ่มด่าน: ใน On Click เลือก LoadLevel (string) แล้วพิมพ์ชื่อฉาก
    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // ปุ่ม QUIT
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // กด Esc / ปุ่ม Back ของมือถือ = ย้อนกลับ 1 หน้า
    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        bool back = UnityEngine.InputSystem.Keyboard.current != null &&
                    UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        bool back = Input.GetKeyDown(KeyCode.Escape);
#endif
        if (!back) return;
        if (levelPanel && levelPanel.activeSelf) ShowModes();
        else if (modePanel && modePanel.activeSelf) ShowMain();
    }
}
