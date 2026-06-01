using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "EnemyScriptableObject", menuName = "ScriptableObjects/Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    [SerializeField]
    public float moveSpeed;
    public float MoveSpeed { get => moveSpeed; private set => moveSpeed = value; }

    [SerializeField]
    public float maxHealth;
    public float MaxHealth { get => maxHealth; private set => maxHealth = value; }

    [SerializeField]
    public float damage;
    public float Damage { get => damage; private set => damage = value; }
}
