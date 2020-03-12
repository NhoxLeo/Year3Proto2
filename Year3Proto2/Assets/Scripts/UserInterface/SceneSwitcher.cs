using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneSwitcher : MonoBehaviour
{
    public GameObject fadePanel;
    public float fadeTime = 0.5f;
    private float fadeInDelay = 0.25f;
    private CanvasGroup canvas;
    private string targetScene = "";
    private string curScene = "";
    private bool isFading = false;
    private bool isSwitching = false;
    private float fadeTimeCur = 0.0f;

    public AudioClip clickSound;
    public AudioClip toolSound;

    void Awake()
    {
        canvas = fadePanel.GetComponent<CanvasGroup>();
        curScene = SceneManager.GetActiveScene().name;
        GlobalData.curScene = curScene;
        Debug.Log("Current scene: " + curScene);
        clickSound = Resources.Load("Audio/SFX/sfxUIClick2") as AudioClip;
        //toolSound = Resources.Load("Audio/SFX/sfxUIClick3") as AudioClip;
    }

    void Start()
    {
        if (curScene == "TitleScreen")
        {
            //GlobalData.LastScene = curScene;
        }

        if (fadePanel == null)
        {
            fadePanel = GameObject.Find("FadePanel(Clone)");
        }
        fadePanel.SetActive(true);

        Invoke("ExitFade", fadeInDelay);
    }

    void Update()
    {
        fadeTimeCur = Mathf.MoveTowards(fadeTimeCur, 0f, Time.unscaledDeltaTime);
        if (fadeTimeCur != 0)
        {
            isFading = true;
        }
        else
        {
            isFading = false;
        }

        if (isSwitching && !isFading)
        {
            if (targetScene == "")
            {
                ExitFade();
            }

            if (targetScene == "Quit")
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene(targetScene);
                Debug.Log("Switched from " + curScene + " to " + targetScene);
            }

            isSwitching = false;
        }
    }

    public void SceneSwitch(string scene)
    {
        StartFade();
        targetScene = scene;
    }


    private void StartFade()
    {
        if (!isSwitching && !isFading)
        {
            canvas.DOFade(1.0f, fadeTime).SetEase(Ease.InOutSine);

            isSwitching = true;
            fadeTimeCur = fadeTime;

            AudioSource source = GetComponent<AudioSource>();

            if (source != null)
            {
                source.clip = clickSound;
                source.Play();
            }

        }
    }

    private void ExitFade()
    {
        isFading = true;
        isSwitching = false;
        fadeTimeCur = fadeTime;
        canvas.DOFade(0.0f, fadeTime).SetEase(Ease.InOutSine);
    }

    public void QuitGame()
    {
        StartFade();
        targetScene = "Quit";
        Debug.Log("Silly human. You know you can't quit the game from the editor!");
    }
}