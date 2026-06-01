using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Stats")]
    public WeaponScriptableObject weaponData;
    float currentCooldown;
    protected Movement pm;
    protected virtual void Start()
    {
        pm = FindAnyObjectByType<Movement>();
        currentCooldown = weaponData.CooldownDuration;

    }
    protected virtual void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown <= 0f)
        {
            Attack();
        }
    }
    protected virtual void Attack()
    {
        currentCooldown = weaponData.CooldownDuration;
    }
}
