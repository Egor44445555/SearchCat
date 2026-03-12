using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager main;
    
    [SerializeField] float collectionRadius = 0.1f;
    [SerializeField] LayerMask itemLayer;
    [SerializeField] string itemTag = "Cat";
    
    [Header("Hint")]
    [SerializeField] GameObject CountHint;
    [SerializeField] GameObject recoveryHintButton;
    [SerializeField] GameObject HintEffect;
    [SerializeField] TextMeshProUGUI countHintText;
    [SerializeField] float recoveryHintTime = 5f;
    bool recoveryHint = false;
    float recoveryHintTimer = 0f;
    Image recoveryHintButtonImage;

    [Header("Heart")]
    [SerializeField] GameObject heart;
    [SerializeField] GameObject innetLifeCount;
    [SerializeField] TextMeshProUGUI outerCountLifeText;
    [SerializeField] TextMeshProUGUI innerCountLifeText;
    RectTransform heartTransform;
    Vector2 anchoredPosition;
    RectTransform heartCountTransform;
    Vector2 anchoredCountPosition;
    int currentLife = 5;
    float heartValue = -100f;
    float stepHeartFill = 0f;    
    float heartHeight;    
    float stepFill = 0f;
    float currentPositionHeartFill = 0f;
    bool spendBarHeart = false;

    [Header("Menu")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject successMenu;
    [SerializeField] GameObject failMenu;
    [SerializeField] GameObject hintMenu;

    [Header("Additional components")]
    [SerializeField] TextMeshProUGUI allCatsCount;
    [SerializeField] TextMeshProUGUI currentCatsCount;
    [SerializeField] GameObject sounIconDisabled;
    [SerializeField] AudioSource music;
    [SerializeField] AudioSource effect;
    [SerializeField] GameObject errorCross;
    
    Camera cam;
    CameraShake cameraShake;
    GraphicRaycaster graphicRaycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;
    Vector2 startPoint = new Vector2();
    int searchCount = 0;
    int maxSearchCount = 0;
    int currentLevel = 0;
    bool gamePause = false;
    int maxLevel = 2;    
    int misses = 0;    
    int currentHint = 3;    
    bool allowClick = false;
    int successClick = 0;
    bool endLevel = false;
    float timer = 0f;
    PlayerData playerData;
    Cat[] cats;


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
        if (JsonSave.main != null)
        {
            playerData = JsonSave.LoadData<PlayerData>("playerData");
            currentLevel = playerData.currentLevel;
        }

        Time.timeScale = 1f;
        eventSystem = EventSystem.current;
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        cam = Camera.main;
        cameraShake = cam.GetComponent<CameraShake>();

        if (recoveryHintButton != null)
        {
            recoveryHintButtonImage = recoveryHintButton.GetComponent<Image>();
        }        

        if (heart != null)
        {
            heartTransform = heart.transform.GetComponent<RectTransform>();
            anchoredPosition = heartTransform.anchoredPosition;
            heartHeight = heartTransform.sizeDelta.y * heartTransform.localScale.y;
            stepHeartFill = heartHeight * (100 / currentLife) / 100f;
            heartValue = anchoredPosition.y;
            heartCountTransform = innetLifeCount.transform.GetComponent<RectTransform>();;
            anchoredCountPosition = heartTransform.anchoredPosition;
        }
        
        if (countHintText != null)
        {
            countHintText.text = currentHint.ToString();
        }

        if (music != null)
        {
            if (!IsSoundsActive())
            {
                music.Pause();
            }
            else
            {
                music.Play();
            }
        }
        
        cats = FindObjectsOfType<Cat>();

        if (allCatsCount != null)
        {
            allCatsCount.text = cats.Length.ToString();
        }        
    }

    void Update()
    {
        if (endLevel)
        {
            timer += Time.deltaTime;
        }

        if (timer > 1f && endLevel)
        {
            if (currentLife > 0)
            {
                StartCoroutine(SuccessLevel());
            }
            else
            {
                StartCoroutine(FailLevel());
            }
                       
            endLevel = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePause();
        }

        if (Input.GetMouseButtonDown(0))
        {
            startPoint = Input.mousePosition;
            allowClick = !gamePause ? true : false;
        }

        if (Input.GetMouseButtonUp(0) && Vector2.Distance(startPoint, Input.mousePosition) < 10f && !gamePause && !IsTouchOverUI(Input.mousePosition) && allowClick)
        {
            HandleMouseClick();
        }

        if (spendBarHeart)
        {
            DecreaseLife();
        }

        if (recoveryHint && recoveryHintButtonImage != null)
        {
            recoveryHintTimer += Time.deltaTime;

            recoveryHintButtonImage.fillAmount -= 1f / recoveryHintTime * Time.deltaTime;

            if (recoveryHintTimer >= recoveryHintTime)
            {
                RecoverHint();
            }
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

        if (GameObject.FindGameObjectWithTag("Popup"))
        {
            GameObject.FindGameObjectWithTag("Popup").SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void HandleMouseClick()
    {
        bool match = false;
        Vector3 worldPosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.nearClipPlane));        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPosition, collectionRadius, itemLayer);

        foreach (var item in GameObject.FindGameObjectsWithTag("Effect"))
        {
            Destroy(item.gameObject);
        }
        
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(itemTag))
            {
                match = true;
                hitCollider.GetComponent<Cat>().SuccessItem();
                successClick += 1;
                currentCatsCount.text = successClick.ToString();
                hitCollider.tag = "Untagged";
                
                if (JsonSave.main != null)
                {
                    playerData.points += 350;
                    playerData.differences += 1;
                }

                if (effect != null && IsSoundsActive())
                {
                    effect.Play();
                }
            }
        }

        if (!match && errorCross != null)
        {
            GameObject crossObject = Instantiate(errorCross, worldPosition, Quaternion.identity);

            if (!IsSoundsActive())
            {
                crossObject.GetComponent<AudioSource>().Stop();
            }
            

            playerData.misses += 1;
            misses += 1;
            currentLife -= 1;
            outerCountLifeText.text = currentLife.ToString();
            innerCountLifeText.text = currentLife.ToString();
            currentPositionHeartFill += stepHeartFill;
            spendBarHeart = true;

            if (cameraShake != null)
            {
                cameraShake.StartHitShake();
            }
        }

        if (cats.Length <= successClick)
        {
            endLevel = true;
        }
        
        if (JsonSave.main != null)
        {
            JsonSave.SaveData(playerData, "playerData");
        }
    }

    public void DecreaseLife()
    {
        anchoredPosition.y -= stepHeartFill;
        heartTransform.anchoredPosition = anchoredPosition;
        anchoredCountPosition.y += stepHeartFill;
        heartCountTransform.anchoredPosition = anchoredCountPosition;

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
        if (!IsSoundsActive())
        {
            sounIconDisabled.SetActive(false);
            PlayerPrefs.SetString("SoundEnable", "1");

            if (music != null)
            {
                music.Play();
            }
        }
        else
        {
            sounIconDisabled.SetActive(true);
            PlayerPrefs.SetString("SoundEnable", "0");

            if (music != null)
            {
                music.Pause();
            }
        }

        CheckSoundIcon();
    }

    public void CheckSoundIcon()
    {
        if (!IsSoundsActive())
        {
            sounIconDisabled.SetActive(true);
        }
    }

    bool IsTouchOverUI(Vector2 _position)
    {
        pointerEventData = new PointerEventData(eventSystem) { position = _position };
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);
        return results.Count > 0;
    }

    public void StartLevel()
    {
        SceneManager.LoadSceneAsync("Level" + currentLevel);
    }

    public void StartNextLevel()
    {
        if ((currentLevel + 1) <= maxLevel)
        {
            if (JsonSave.main != null)
            {
                playerData = JsonSave.LoadData<PlayerData>("playerData");
                playerData.currentLevel += 1;
                JsonSave.SaveData(playerData, "playerData");
            }

            SceneManager.LoadSceneAsync("Level" + (currentLevel + 1));
        }
        else
        {
            if (JsonSave.main != null)
            {
                playerData = JsonSave.LoadData<PlayerData>("playerData");
                playerData.currentLevel = 0;
                JsonSave.SaveData(playerData, "playerData");
            }
            
            ToMenu();
        }
    }

    public bool IsSoundsActive()
    {
        return PlayerPrefs.GetString("SoundEnable") == "1";
    }

    IEnumerator SuccessLevel()
    {
        yield return new WaitForSeconds(0.5f);
        gamePause = true;

        if (successMenu != null)
        {
            successMenu.SetActive(true);
        }
    }

    IEnumerator FailLevel()
    {
        yield return new WaitForSeconds(1f);
        gamePause = true;

        if (failMenu != null)
        {
            failMenu.SetActive(true);
        }        
    }

    public void GetHint()
    {
        if (currentHint > 0)
        {
            foreach (var item in FindObjectsOfType<DestroyAfterParticles>())
            {
                Destroy(item.gameObject);
            }

            foreach (var item in cats)
            {
                if (!item.IsActive())
                {
                    if (JsonSave.main != null)
                    {
                        playerData = JsonSave.LoadData<PlayerData>("playerData");
                        playerData.tips += 1;
                        JsonSave.SaveData(playerData, "playerData");
                    }

                    if (HintEffect != null)
                    {
                        Instantiate(HintEffect, item.transform.position, Quaternion.identity);
                    }
                    
                    break;
                }
            }
        }

        currentHint -= 1;

        if (countHintText != null)
        {
            countHintText.text = currentHint.ToString();
        }

        if (currentHint == 0)
        {
            recoveryHintButton.SetActive(true);
            recoveryHint = true;
            CountHint.SetActive(false);
        }
    }

    public void OpenHintPopup()
    {
        hintMenu.SetActive(true);
        gamePause = true;
        Time.timeScale = 0f;
    }

    public void GetHintForAd()
    {
        RecoverHint();
    }

    void RecoverHint()
    {
        currentHint += 1;
        recoveryHintTimer = 0f;
        recoveryHintButtonImage.fillAmount = 1f;
        recoveryHintButton.SetActive(false);
        CountHint.SetActive(true);
        countHintText.text = currentHint.ToString();
        recoveryHint = false;
        hintMenu.SetActive(false);
        gamePause = false;
        Time.timeScale = 1f;
    }

    public void ToMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
