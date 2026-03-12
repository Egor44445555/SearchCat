using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager main;
    
    [SerializeField] float collectionRadius = 0.1f;
    [SerializeField] LayerMask itemLayer;
    [SerializeField] string itemTag = "Cat";
    

    [Header("Additional components")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject successMenu;
    [SerializeField] public GameObject soundIcon;
    [SerializeField] public GameObject sounIconDisabled;
    [SerializeField] public AudioSource music;
    [SerializeField] public AudioSource effect;
    [SerializeField] GameObject errorCross;
    [SerializeField] GameObject heart;
    [SerializeField] GameObject recoveryHintButton;
    [SerializeField] GameObject CountHint;
    [SerializeField] TextMeshProUGUI countHintText;
    [SerializeField] float recoveryHintTime = 5f;

    Camera cam;
    CameraShake cameraShake;
    GraphicRaycaster graphicRaycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;
    int searchCount = 0;
    int maxSearchCount = 0;
    int currentLevel = 0;
    bool gamePause = false;
    int maxLevel = 2;    
    int misses = 0;    
    int currentHint = 3;
    int successClick = 0;
    bool endLevel = false;
    PlayerData playerData;

    // Heart
    int currentLife = 5;
    float heartValue = -100f;
    float stepHeartFill = 0f;
    RectTransform heartTransform;
    float heartHeight;
    Vector2 anchoredPosition;
    float stepFill = 0f;
    float currentPositionHeartFill = 0f;
    bool spendBarHeart = false;

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

        if (spendBarHeart)
        {
            DecreaseLife();
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
        bool match = false;
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
                match = true;
                searchCount++;

                if (maxSearchCount <= searchCount)
                {
                    StartCoroutine(SuccessLevel());
                }
            }
        }

        if (!match)
        {
            playerData.misses += 1;
            misses += 1;
            currentLife -= 1;
            currentPositionHeartFill += stepHeartFill;
            spendBarHeart = true;

            if (cameraShake != null)
            {
                cameraShake.StartHitShake();
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

    public void DecreaseLife()
    {
        anchoredPosition.y -= stepHeartFill;
        heartTransform.anchoredPosition = anchoredPosition;

        if (Math.Abs(anchoredPosition.y) >= currentPositionHeartFill)
        {
            spendBarHeart = false;
        }

        if (currentLife <= 0)
        {
            endLevel = true;
        }
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
