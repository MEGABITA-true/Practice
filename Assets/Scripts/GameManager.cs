using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public enum Gamestate
    {
        Gameplay,
        Paused,
        GameOver,
        LvlUp
    }
    public Gamestate currentState;
    public Gamestate previousState;

    [Header("Damage Text Settings")]
    public Canvas damageTextCanvas;
    public float textFontSize = 20;
    public TMP_FontAsset textFont;
    public Camera referenceCamera;

    [Header("Screens")]
    public GameObject pauseScreen;
    public GameObject resultsScreen;
    public GameObject LvlUpScreen;

    [Header("Current Stats")]
    public TMP_Text currentHealthDisplay;
    public TMP_Text currentRecoveryDisplay;
    public TMP_Text currentMoveSpeedDisplay;
    public TMP_Text currentMightDisplay;
    public TMP_Text currentMagnetDisplay;

    [Header("Result screen display")]
    public Image chosenCharacterSprite;
    public TMP_Text chosenCharacterName;
    public TMP_Text levelReachedDisplay;
    public TMP_Text timeSurvivedDisplay;
    public List<Image> chosenWeaponsUI = new List<Image>(6);
    public List<Image> chosenPassiveItemsUI = new List<Image>(6);

    [Header("Stopwatch")]
    public float timeLimit;
    float stopwatchTime;
    public TMP_Text stopwatchDisplay;

    public bool isGameOver = false;
    public bool choosingUpgrade = false;

    public GameObject playerObject;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("EXTRA " + this + " DELETED");
            Destroy(gameObject);
        }
        DisableScreens();
    }
    void Update()
    {
        switch (currentState)
        {
            case Gamestate.Gameplay:
                CheckForPauseAndResume();
                UpdateStopwatch(); 
                break;
            case Gamestate.Paused:
                CheckForPauseAndResume();
                break;
            case Gamestate.GameOver:
                if (!isGameOver)
                {
                    isGameOver = true;
                    Time.timeScale = 0;
                    DisplayResults();
                }
                break;
            case Gamestate.LvlUp:
                if (!choosingUpgrade)
                {
                    choosingUpgrade = true;
                    Time.timeScale = 0;
                    LvlUpScreen.SetActive(true);
                }
                break;
            default:
                break;
        }
    }
    IEnumerator GenerateFloatingTextCouroutine(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        GameObject textObj = new GameObject("Damage Floating Text");
        RectTransform rect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI tmPro = textObj.AddComponent<TextMeshProUGUI>();
        tmPro.text = text;
        tmPro.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmPro.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmPro.fontSize = textFontSize;
        if (textFont) tmPro.font = textFont;
        rect.position = referenceCamera.WorldToScreenPoint(target.position);

        Destroy(textObj, duration);

        textObj.transform.SetParent(instance.damageTextCanvas.transform);
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0;
        float yOffset = 0;
        while(t < duration)
        {
            yield return w;
            t += Time.deltaTime;
            tmPro.color = new Color(tmPro.color.r, tmPro.color.g, tmPro.color.b, 1 - t / duration);
            yOffset += speed * Time.deltaTime;
            rect.position = referenceCamera.WorldToScreenPoint(target.position + new Vector3(0, yOffset));
        }
    }
    public static void GenerateFloatingText(string text, Transform target, float duration = 1f, float speed = 1f)
    {
        if(!instance.damageTextCanvas) return;
        if(!instance.referenceCamera) instance.referenceCamera = Camera.main;
        instance.StartCoroutine(instance.GenerateFloatingTextCouroutine(
            text, target, duration, speed
        ));
    }
    public void ChangeState(Gamestate newState)
    {
        currentState = newState;
    }
    public void PauseGame()
    {
        if(currentState != Gamestate.Paused)
        {
            previousState = currentState;
            ChangeState(Gamestate.Paused);
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            Debug.Log("Game is paused");
        }
    }
    public void ResumeGame()
    {
        if(currentState == Gamestate.Paused)
        {
            ChangeState(Gamestate.Gameplay);
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
            Debug.Log("Game is resumed");
        }
    }
    void CheckForPauseAndResume()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (currentState == Gamestate.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    void DisableScreens()
    {
        pauseScreen.SetActive(false);
        resultsScreen.SetActive(false);
        LvlUpScreen.SetActive(false);
    }
    public void GameOver()
    {
        timeSurvivedDisplay.text = stopwatchDisplay.text;
        ChangeState(Gamestate.GameOver);
    }
    void DisplayResults()
    {
        resultsScreen.SetActive(true);
    }
    public void AssignChosenCharacterUI(CharacterScriptableObject chosenCharacterData)
    {
        chosenCharacterSprite.sprite = chosenCharacterData.Icon;
        chosenCharacterName.text = chosenCharacterData.Name;
    } 
    public void AssignLvlReachedUI(int lvlReachedData)
    {
        levelReachedDisplay.text = lvlReachedData.ToString();
    }
    public void AssignChosenWeaponsAndPassItemsUI(List<Image> chosenWeaponData, List<Image> chosenPassItemData)
    {
        if(chosenWeaponData.Count != chosenWeaponsUI.Count || chosenPassItemData.Count != chosenPassiveItemsUI.Count)
        {
            Debug.Log("Different lenghts");
            return;
        }

        for (int i = 0; i < chosenWeaponsUI.Count; i++)
        {
            if (chosenWeaponData[i].sprite)
            {
                chosenWeaponsUI[i].enabled = true;
                chosenWeaponsUI[i].sprite = chosenWeaponData[i].sprite;
            }
            else
            {
                chosenWeaponsUI[i].enabled = false;
            }
        }
        for (int i = 0; i < chosenPassiveItemsUI.Count; i++)
        {
            if (chosenPassItemData[i].sprite)
            {
                chosenPassiveItemsUI[i].enabled = true;
                chosenPassiveItemsUI[i].sprite = chosenPassItemData[i].sprite;
            }
            else
            {
                chosenPassiveItemsUI[i].enabled = false;
            } 
        }
    }
    void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime;
        UpdateStopwatchDisplay();
        if(stopwatchTime >= timeLimit)
        {
            playerObject.SendMessage("Kill");
        }
    }

    void UpdateStopwatchDisplay()
    {
        int minutes = Mathf.FloorToInt(stopwatchTime / 60);
        int seconds = Mathf.FloorToInt(stopwatchTime % 60); ;
        stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void StartLvlUp()
    {
        ChangeState(Gamestate.LvlUp);
        playerObject.SendMessage("RemoveAndApplyUpgrades");
    }
    public void EndLvlUp()
    {
        choosingUpgrade = false;
        Time.timeScale = 1;
        LvlUpScreen.SetActive(false);
        ChangeState(Gamestate.Gameplay);
    }
}
