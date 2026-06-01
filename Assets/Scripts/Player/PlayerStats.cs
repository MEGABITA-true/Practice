using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    CharacterScriptableObject characterData;
    float currentHealth;
    float currentRecovery;
    float currentMoveSpeed;
    float currentMight;
    float currentProjectileSpeed;
    float currentMagnet;

    #region Current Stats Properties
    public float CurrentHealth
    {
        get {  return currentHealth; }
        set 
        { 
            if(currentHealth != value)
            {
                currentHealth = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentHealthDisplay.text = "Здоровье: " + currentHealth;
                }
            }
        }
    }
    public float CurrentRecovery
    {
        get { return currentRecovery; }
        set
        {
            if (currentRecovery != value)
            {
                currentRecovery = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentRecoveryDisplay.text = "Восстановление: " + currentRecovery;
                }
            }
        }
    }
    public float CurrentMoveSpeed
    {
        get { return currentMoveSpeed; }
        set
        {
            if (currentMoveSpeed != value)
            {
                currentMoveSpeed = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMoveSpeedDisplay.text = "Скор. игрока: " + currentMoveSpeed;
                }
            }
        }
    }
    public float CurrentMight
    {
        get { return currentMight; }
        set
        {
            if (currentMight != value)
            {
                currentMight = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMightDisplay.text = "Мощь: " + currentMight;
                }
            }
        }
    }
    public float CurrentMagnet
    {
        get { return currentMagnet; }
        set
        {
            if (currentMagnet != value)
            {
                currentMagnet = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMagnetDisplay.text = "Магнит: " + currentMagnet;
                }
            }
        }
    }
    #endregion

    public ParticleSystem damageEffect;

    [Header("Experience/Level")]
    public int exp = 0;
    public int lvl = 1;
    public int expCap;

    [System.Serializable]
    public class LvlRange
    {
        public int startLvl;
        public int endLvl;
        public int expCapIncrease;
    }

    [Header("I-Frames")]
    public float invicibilityDuration;
    float invicibilityTimer;
    bool isInvincible;
    public List<LvlRange> lvlRanges;
    InventoryManager inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    [Header("UI")]
    public Image healthBar;
    public Image expBar;
    public TMP_Text lvlText;

    public GameObject secondWeaponTest;
    public GameObject firstPassiveItemTest, secondPassiveItemTest;

    void Awake()
    {
        characterData = CharacterSelector.GetData();
        CharacterSelector.instance.DestroySingleton();
        inventory = GetComponent<InventoryManager>();
        CurrentHealth = characterData.MaxHealth;
        CurrentRecovery = characterData.Recovery;
        CurrentMoveSpeed = characterData.Movespeed;
        CurrentMight = characterData.Might;
        CurrentMagnet = characterData.Magnet;

        StartWeapon(characterData.StartingWeapon);
        StartPassiveItem(secondPassiveItemTest);
    }

    void Start()
    {
        expCap = lvlRanges[0].expCapIncrease;
        GameManager.instance.currentHealthDisplay.text = "Здоровье: " + currentHealth;
        GameManager.instance.currentRecoveryDisplay.text = "Восстановление: " + currentRecovery;
        GameManager.instance.currentMoveSpeedDisplay.text = "Скор. игрока: " + currentMoveSpeed;
        GameManager.instance.currentMightDisplay.text = "Мощь: " + currentMight;
        GameManager.instance.currentMagnetDisplay.text = "Магнит: " + currentMagnet;

        GameManager.instance.AssignChosenCharacterUI(characterData);
        UpdateHealthBar();
        UpdateExpBar();
        UpdateLvlText();
    }

    void Update()
    {
        if (invicibilityTimer > 0)
        { 
            invicibilityTimer -= Time.deltaTime;
        }
        else if (isInvincible)
        {
            isInvincible = false;
        }
        Recover();
    }
    public void IncreaseExp(int amount)
    {
        exp += amount;
        LvlUpChecker();
        UpdateExpBar();
    }
    void LvlUpChecker()
    {
        if (exp >= expCap)
        {
            lvl++;
            exp -= expCap;

            int expCapIncrease = 0;
            foreach (LvlRange range in lvlRanges)
            {
                if (lvl >= range.startLvl && lvl <= range.endLvl)
                {
                    expCapIncrease = range.expCapIncrease;
                    break;
                }
            }
            expCap += expCapIncrease;
            UpdateLvlText();
            GameManager.instance.StartLvlUp();
        }
    }
    void UpdateExpBar()
    {
        expBar.fillAmount = (float)exp / expCap;
    }
    void UpdateLvlText()
    {
        lvlText.text = "Ур. " + lvl.ToString();
    }

    public void TakeDamage(float dmg)
    {
        if (!isInvincible)
        {
            CurrentHealth -= dmg;
            if (damageEffect) Instantiate(damageEffect, transform.position, Quaternion.identity);
            invicibilityTimer = invicibilityDuration;
            isInvincible = true;

            if (CurrentHealth <= 0)
            {
                kill();
            }
            UpdateHealthBar();
        }
    }
    public void UpdateHealthBar()
    {
        healthBar.fillAmount = currentHealth / characterData.MaxHealth;
    }
    public void kill()
    {
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLvlReachedUI(lvl);
            GameManager.instance.AssignChosenWeaponsAndPassItemsUI(inventory.weaponUISlots, inventory.passiveItemUISlots);
            GameManager.instance.GameOver();
        }
    }
    public void RestoreHealth(float amount)
    {
        if(CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += amount;
            if (CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }
    }
    void Recover()
    {
        if(CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += CurrentRecovery * Time.deltaTime;
            if(CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }
    }
    public void StartWeapon(GameObject weapon)
    {
        if(weaponIndex >= inventory.weaponSlots.Count - 1)
        {
            return;
        }
        GameObject spawnedWeapon = Instantiate(weapon, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform);
        inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>());
        weaponIndex++;
    }
    public void StartPassiveItem(GameObject passiveItem)
    {
        if (passiveItemIndex >= inventory.passiveItemSlots.Count - 1)
        {
            return;
        }
        GameObject spawnedPassiveItem = Instantiate(passiveItem, transform.position, Quaternion.identity);
        spawnedPassiveItem.transform.SetParent(transform);
        inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItem.GetComponent<PassiveItem>());
        passiveItemIndex++;
    }
}
