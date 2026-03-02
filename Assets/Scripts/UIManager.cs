using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager main;
    
    [SerializeField] float collectionRadius = 0.1f;
    [SerializeField] LayerMask itemLayer;
    [SerializeField] string itemTag = "Cat";
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject successMenu;
    [SerializeField] public GameObject soundIcon;
    [SerializeField] public GameObject sounIconDisabled;
    [SerializeField] public AudioSource music;
    [SerializeField] public AudioSource effect;

    Camera cam;
    int searchCount = 0;
    int maxSearchCount = 0;
    int currentLevel = 0;
    bool gamePause = false;
    int maxLevel = 2;

    void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        cam = Camera.main;
        currentLevel = PlayerPrefs.GetInt("CurrentLevel");

        if (music != null && PlayerPrefs.GetString("SoundEnable") == "0")
        {
            music.Pause();
        }
        else
        {
            music.Play();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePause();
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandleMouseClick();
        }
    }

    void TogglePause()
    {
        if (gamePause)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (gamePause) return;

        gamePause = true;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            CheckSoundIcon();
        }

        Time.timeScale = 0f;
    }

    public void UnpauseGame()
    {
        if (!gamePause) return;
        
        gamePause = false;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void HandleMouseClick()
    {
        Vector3 worldPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPosition, collectionRadius, itemLayer);
        
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(itemTag))
            {
                if (effect != null && PlayerPrefs.GetString("SoundEnable") != "0" && maxSearchCount >= searchCount)
                {
                    effect.Play();
                }
                
                hitCollider.GetComponent<Cat>().SuccessItem();
                searchCount++;

                if (maxSearchCount <= searchCount)
                {
                    StartCoroutine(SuccessLevel());
                }
            }
        }
    }

    IEnumerator SuccessLevel()
    {
        yield return new WaitForSeconds(1f);
        gamePause = true;

        if (successMenu != null)
        {
            successMenu.SetActive(true);
        }
        
        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
    }

    public void IncreaseMaxSearchCount()
    {
        maxSearchCount++;
    }

    public bool IsGamePause()
    {
        return gamePause;
    }

    public void SwitchSound()
    {
        if (PlayerPrefs.GetString("SoundEnable") == "0" || PlayerPrefs.GetString("SoundEnable") == "")
        {
            soundIcon.SetActive(true);
            sounIconDisabled.SetActive(false);
            PlayerPrefs.SetString("SoundEnable", "1");

            if (music != null)
            {
                music.Play();
            }
        }
        else
        {
            soundIcon.SetActive(false);
            sounIconDisabled.SetActive(true);
            PlayerPrefs.SetString("SoundEnable", "0");

            if (music != null)
            {
                music.Pause();
            }
        }
    }

    public void CheckSoundIcon()
    {
        if (PlayerPrefs.GetString("SoundEnable") == "0")
        {
            soundIcon.SetActive(false);
            sounIconDisabled.SetActive(true);
        }
    }

    public void StartLevel()
    {
        SceneManager.LoadSceneAsync("Level" + currentLevel);
    }

    public void StartNextLevel()
    {
        if ((currentLevel + 1) <= maxLevel)
        {
            SceneManager.LoadSceneAsync("Level" + (currentLevel + 1));
        }
        else
        {
            PlayerPrefs.SetInt("CurrentLevel", 0);
            ToMenu();
        }
    }

    public void ToMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
