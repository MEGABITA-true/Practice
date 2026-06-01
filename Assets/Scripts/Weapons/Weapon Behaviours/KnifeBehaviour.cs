using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class KnifeBegaviour : ProjectileWeaponBegavie
{
    protected override void Start()
    {
        base.Start();
    }
    void Update()
    {
        transform.position += direction * currentSpeed * Time.deltaTime;
    }
}
