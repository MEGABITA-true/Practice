using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpGem : PickUp
{
    public int expGive;
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
        player.IncreaseExp(expGive);
    }
}
