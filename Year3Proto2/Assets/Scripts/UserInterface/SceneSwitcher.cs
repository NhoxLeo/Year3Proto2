using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

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

    public float loadTime = 2.0f;
    private CanvasGroup loadingCanvas;
    public GameObject loadingScreen;
    private GameObject loadingIcon;
    private TMP_Text loadingHint;

    public AudioClip clickSound;
    public AudioClip toolSound;

    void Awake()
    {
        canvas = fadePanel.GetComponent<CanvasGroup>();
        loadingCanvas = loadingScreen.GetComponent<CanvasGroup>();
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
        fadePanel.SetActive(!GlobalData.isLoadingIn);
        if (fadePanel.activeSelf) { Invoke("ExitFade", fadeInDelay); }

        loadingIcon = loadingScreen.transform.Find("LoadingIcon").gameObject;
        loadingIcon.SetActive(GlobalData.isLoadingIn);
        loadingHint = loadingScreen.transform.Find("HintText").GetComponent<TMP_Text>();
        loadingHint.text = GlobalData.currentLoadingHint;
        loadingScreen.SetActive(GlobalData.isLoadingIn);
        if (loadingScreen.activeSelf) 
        { 
            Invoke("EndLoad", loadTime);
            loadingScreen.GetComponent<UIAnimator>().SetVisibility(true);
        }
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
                if (GlobalData.isLoadingIn)
                {
                    EndLoad();
                }
                else
                {
                    ExitFade();
                }
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

        loadingIcon.transform.Rotate(0.0f, 0.0f, -270.0f * Time.smoothDeltaTime, Space.Self);
    }

    public void SceneSwitch(string scene)
    {
        StartFade();
        targetScene = scene;
        GlobalData.isLoadingIn = false;
    }


    public void SceneSwitchLoad(string scene)
    {
        GlobalData.isLoadingIn = true;
        BeginLoad();
        targetScene = scene;
    }


    private void StartFade()
    {
        if (!isSwitching && !isFading)
        {
            canvas.alpha = 0.0f;
            fadePanel.SetActive(!GlobalData.isLoadingIn);
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

    private void BeginLoad()
    {
        if (!isSwitching && !isFading)
        {
            loadingCanvas.alpha = 0.0f;
            loadingScreen.SetActive(GlobalData.isLoadingIn);
            loadingScreen.GetComponent<UIAnimator>().SetVisibility(true);
            //loadingCanvas.interactable = true;
            //loadingCanvas.blocksRaycasts = true;
            loadingIcon.SetActive(false);
            isSwitching = true;
            fadeTimeCur = 0.5f;

            loadingHint.text = SelectLoadingHint();
        }

        AudioSource source = GetComponent<AudioSource>();

        if (source != null)
        {
            source.clip = clickSound;
            source.Play();
        }
    }

    private void EndLoad()
    {
        isFading = true;
        isSwitching = false;
        fadeTimeCur = fadeTime;
        loadingScreen.GetComponent<UIAnimator>().SetVisibility(false);
        //loadingCanvas.DOFade(0.0f, 0.45f).SetEase(Ease.InOutSine);
        //loadingCanvas.interactable = false;
        //loadingCanvas.blocksRaycasts = false;
        GlobalData.isLoadingIn = false;
    }

    private string SelectLoadingHint()
    {
        string hint = GlobalData.currentLoadingHint;
        while (hint == GlobalData.currentLoadingHint)
        {
            hint = GlobalData.loadingHint[Random.Range(0, GlobalData.loadingHint.Count)];
        }
        GlobalData.currentLoadingHint = hint;
        return hint;
    }

    public void QuitGame()
    {
        StartFade();
        targetScene = "Quit";
        Debug.Log("Silly human. You know you can't quit the game from the editor!");
    }
}