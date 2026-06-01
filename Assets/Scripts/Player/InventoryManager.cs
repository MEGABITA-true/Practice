using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public List<WeaponController> weaponSlots = new List<WeaponController>(6);
    public int[] weaponLevels = new int[6];
    public List<Image> weaponUISlots = new List<Image>(6);
    public List<PassiveItem> passiveItemSlots = new List<PassiveItem>(6);
    public int[] passiveItemLevels = new int[6];
    public List<Image> passiveItemUISlots = new List<Image>(6);

    [System.Serializable]
    public class WeaponUpgrade
    {
        public int weaponUpgradeIndex;
        public GameObject initialWeapon;
        public WeaponScriptableObject weaponData;
    }
    [System.Serializable]
    public class PassItemUpgrade
    {
        public int passItemUpgradeIndex;
        public GameObject initialPassItem;
        public PassiveItemScriptableObject passItemData;
    }
    [System.Serializable]
    public class UpgradeUI
    {
        public TMP_Text upgradeNameDisplay;
        public TMP_Text upgradeDescriptionDisplay;
        public Image upgradeIcon;
        public Button upgradeButton;
    }
    public List<WeaponUpgrade> weaponUpgradeOptions = new List<WeaponUpgrade>();
    public List<PassItemUpgrade> passItemUpgradeOptions = new List<PassItemUpgrade>();
    public List<UpgradeUI> upgradeUIOptions = new List<UpgradeUI>();

    PlayerStats player;
    void Start()
    {
        player = GetComponent<PlayerStats>();
    }

    public void AddWeapon(int slotIndex, WeaponController weapon)
    {
        weaponSlots[slotIndex] = weapon;
        weaponLevels[slotIndex] = weapon.weaponData.Level;
        weaponUISlots[slotIndex].enabled = true;
        weaponUISlots[slotIndex].sprite = weapon.weaponData.Icon;

        if(GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLvlUp();
        }
    }
    public void AddPassiveItem(int slotIndex, PassiveItem passiveItem)
    {
        passiveItemSlots[slotIndex] = passiveItem;
        passiveItemLevels[slotIndex] = passiveItem.passiveItemData.Level;
        passiveItemUISlots[slotIndex].enabled = true;
        passiveItemUISlots[slotIndex].sprite = passiveItem.passiveItemData.Icon;

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLvlUp();
        }
    }
    public void LevelUpWeapon(int slotIndex, int upgradeIndex)
    {
        if(weaponSlots.Count > slotIndex)
        {
            WeaponController weapon = weaponSlots[slotIndex];
            if (!weapon.weaponData.NextLevelPrefab)
            {
                Debug.LogError("NO NEXT LEVEL FOR " + weapon.name);
                return;
            }
            GameObject upgradeWeapon = Instantiate(weapon.weaponData.NextLevelPrefab, transform.position, Quaternion.identity);
            upgradeWeapon.transform.SetParent(transform);
            AddWeapon(slotIndex, upgradeWeapon.GetComponent<WeaponController>());
            Destroy(weapon.gameObject);
            weaponLevels[slotIndex] = upgradeWeapon.GetComponent<WeaponController>().weaponData.Level;
            weaponUpgradeOptions[upgradeIndex].weaponData = upgradeWeapon.GetComponent<WeaponController>().weaponData;

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLvlUp();
            }
        }
    }
    public void LevelUpPassiveItem(int slotIndex, int upgradeIndex)
    {
        if (passiveItemSlots.Count > slotIndex)
        {
            PassiveItem passiveItem = passiveItemSlots[slotIndex];
            if (!passiveItem.passiveItemData.NextLevelPrefab)
            {
                Debug.LogError("NO NEXT LEVEL FOR " + passiveItem.name);
                return;
            }
            GameObject upgradePassiveItem = Instantiate(passiveItem.passiveItemData.NextLevelPrefab, transform.position, Quaternion.identity);
            upgradePassiveItem.transform.SetParent(transform);
            AddPassiveItem(slotIndex, upgradePassiveItem.GetComponent<PassiveItem>());
            Destroy(passiveItem.gameObject);
            passiveItemLevels[slotIndex] = upgradePassiveItem.GetComponent<PassiveItem>().passiveItemData.Level;
            passItemUpgradeOptions[upgradeIndex].passItemData = upgradePassiveItem.GetComponent <PassiveItem>().passiveItemData;

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            {
                GameManager.instance.EndLvlUp();
            }
        }
    }
    void ApplyUpgradeOptions()
    {
        List<WeaponUpgrade> availableWeaponUpgrades = new List<WeaponUpgrade>(weaponUpgradeOptions);
        List<PassItemUpgrade> availablePassItemUpgrades = new List<PassItemUpgrade>(passItemUpgradeOptions);

        foreach (var upgradeOption in upgradeUIOptions)
        {
            if (availableWeaponUpgrades.Count == 0 && availablePassItemUpgrades.Count == 0)
                return;

            int upgradeType;
            if (availableWeaponUpgrades.Count == 0)
                upgradeType = 2;
            else if (availablePassItemUpgrades.Count == 0)
                upgradeType = 1;
            else
                upgradeType = Random.Range(1, 3);

            if (upgradeType == 1)
            {
                WeaponUpgrade chosenWeaponUpgrade = availableWeaponUpgrades[Random.Range(0, availableWeaponUpgrades.Count)];
                availableWeaponUpgrades.Remove(chosenWeaponUpgrade);

                if (chosenWeaponUpgrade != null)
                {
                    int existingSlotIndex = -1;
                    for (int i = 0; i < weaponSlots.Count; i++)
                    {
                        if (weaponSlots[i] != null && weaponSlots[i].weaponData == chosenWeaponUpgrade.weaponData)
                        {
                            existingSlotIndex = i;
                            break;
                        }
                    }
                    if (existingSlotIndex != -1)
                    {
                        if (!chosenWeaponUpgrade.weaponData.NextLevelPrefab)
                        {
                            DisableUpgradeUI(upgradeOption);
                            continue;
                        }

                        EnableUpgradeUI(upgradeOption);

                        int slotIndex = existingSlotIndex;
                        int upgradeIdx = chosenWeaponUpgrade.weaponUpgradeIndex;

                        upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpWeapon(slotIndex, upgradeIdx));

                        WeaponController nextWeapon = chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>();
                        upgradeOption.upgradeDescriptionDisplay.text = nextWeapon.weaponData.Description;
                        upgradeOption.upgradeNameDisplay.text = nextWeapon.weaponData.Name;
                    }
                    else
                    {
                        EnableUpgradeUI(upgradeOption);

                        GameObject initialWeapon = chosenWeaponUpgrade.initialWeapon;
                        upgradeOption.upgradeButton.onClick.AddListener(() => player.StartWeapon(initialWeapon));

                        upgradeOption.upgradeDescriptionDisplay.text = chosenWeaponUpgrade.weaponData.Description;
                        upgradeOption.upgradeNameDisplay.text = chosenWeaponUpgrade.weaponData.Name;
                    }

                    upgradeOption.upgradeIcon.sprite = chosenWeaponUpgrade.weaponData.Icon;
                }
            }
            else if (upgradeType == 2)
            {
                PassItemUpgrade chosenPassItemUpgrade = availablePassItemUpgrades[Random.Range(0, availablePassItemUpgrades.Count)];
                availablePassItemUpgrades.Remove(chosenPassItemUpgrade);

                if (chosenPassItemUpgrade != null)
                {
                    int existingSlotIndex = -1;
                    for (int i = 0; i < passiveItemSlots.Count; i++)
                    {
                        if (passiveItemSlots[i] != null && passiveItemSlots[i].passiveItemData == chosenPassItemUpgrade.passItemData)
                        {
                            existingSlotIndex = i;
                            break;
                        }
                    }

                    if (existingSlotIndex != -1)
                    {
                        if (!chosenPassItemUpgrade.passItemData.NextLevelPrefab)
                        {
                            DisableUpgradeUI(upgradeOption);
                            continue;
                        }

                        EnableUpgradeUI(upgradeOption);

                        int slotIndex = existingSlotIndex;
                        int upgradeIdx = chosenPassItemUpgrade.passItemUpgradeIndex;

                        upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpPassiveItem(slotIndex, upgradeIdx));

                        PassiveItem nextPassive = chosenPassItemUpgrade.passItemData.NextLevelPrefab.GetComponent<PassiveItem>();
                        upgradeOption.upgradeDescriptionDisplay.text = nextPassive.passiveItemData.Description;
                        upgradeOption.upgradeNameDisplay.text = nextPassive.passiveItemData.Name;
                    }
                    else
                    {
                        EnableUpgradeUI(upgradeOption);

                        GameObject initialPassItem = chosenPassItemUpgrade.initialPassItem;
                        upgradeOption.upgradeButton.onClick.AddListener(() => player.StartPassiveItem(initialPassItem));

                        upgradeOption.upgradeDescriptionDisplay.text = chosenPassItemUpgrade.passItemData.Description;
                        upgradeOption.upgradeNameDisplay.text = chosenPassItemUpgrade.passItemData.Name;
                    }
                    upgradeOption.upgradeIcon.sprite = chosenPassItemUpgrade.passItemData.Icon;
                }
            }
        }
    }

    void RemoveUpgradeOptions()
    {
        foreach (var upgradeOption in upgradeUIOptions)
        {
            upgradeOption.upgradeButton.onClick.RemoveAllListeners();
            DisableUpgradeUI(upgradeOption);
        }
    }
    public void RemoveAndApplyUpgrades()
    {
        RemoveUpgradeOptions();
        ApplyUpgradeOptions();
    }
    void DisableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);
    }
    void EnableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
    }
}
