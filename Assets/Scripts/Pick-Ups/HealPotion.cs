using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPotion : PickUp
{
    public int heal;
    public override void Collect()
    {
        if (hasBeenCollected)
        {
            return;
        }
        else
        {
            base.Collect();
        }
        PlayerStats player = FindAnyObjectByType<PlayerStats>();
        player.RestoreHealth(heal);
        player.UpdateHealthBar();   
    }

}
