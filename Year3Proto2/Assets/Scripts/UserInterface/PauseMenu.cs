using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private UIAnimator tool;
    public bool isPaused;

    private void Start()
    {
        tool = GetComponent<UIAnimator>();
        isPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        Time.timeScale = isPaused ? 0.0f : 1.0f;
    }

    public void ToggleMenu()
    {
        tool.showElement = !tool.showElement;
        isPaused = !isPaused;
        GlobalData.isPaused = tool.showElement;
    }
}