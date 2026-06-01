using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class ProjectileWeaponBegavie : MonoBehaviour
{
    public WeaponScriptableObject weaponData;
    protected Vector3 direction;
    public float destroyAfterSeconds;
    protected float currentDamage;
    protected float currentSpeed;
    protected float currentCooldownDuration;
    protected int currentPierce;

    public void Awake()
    {
        currentDamage = weaponData.Damage;
        currentSpeed = weaponData.Speed;
        currentCooldownDuration = weaponData.CooldownDuration;
        currentPierce = weaponData.Pierce;
    }
    public float GetCurrentDamage()
    {
        return currentDamage * FindAnyObjectByType<PlayerStats>().CurrentMight;
    }
    protected virtual void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }
    public void DirectionChecker(Vector3 dir)
    {
        direction = dir;

        float dirx = direction.x;
        float diry = direction.y;

        Vector3 scale = transform.localScale;
        Vector3 rotation = transform.rotation.eulerAngles;

        if (dirx < 0 && diry == 0) //left
        {
            scale.x = scale.x * -1;
            scale.y = scale.y * -1;
        }
        else if (dirx == 0 && diry < 0) //up
        {
            rotation.z = 180f;
        }
        else if (dirx == 0 && diry > 0) //down
        {
            rotation.z = 0f;
        }
        else if (dirx < 0 && diry > 0) //left up
        { 
            rotation.z = 45f;
        }
        else if (dirx < 0 && diry < 0) //left down
        {
            rotation.z = 135f;
        }
        else if (dirx > 0 && diry > 0) //right up
        {
            rotation.z = -45f;
        }
        else if (dirx > 0 && diry < 0) //right down
        {
            rotation.z = -135f;
        }

        transform.localScale = scale;
        transform.rotation = Quaternion.Euler(rotation);
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            EnemyStats enemy = col.GetComponent<EnemyStats>();
            enemy.TakeDamage(GetCurrentDamage(), transform.position);
            ReducePierce();
        }
        else if (col.CompareTag("Prop"))
        {
            if(col.gameObject.TryGetComponent(out BreakableProps breakable))
            {
                breakable.TakeDamage(GetCurrentDamage());
                ReducePierce();
            }
        }
    }
    void ReducePierce()
    {
        currentPierce--;
        if(currentPierce <= 0)
        {
            Destroy(gameObject);
        }
    }
}
