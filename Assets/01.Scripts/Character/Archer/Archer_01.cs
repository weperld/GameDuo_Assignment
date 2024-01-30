using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer_01 : Archer
{
    protected override ActiveSkill _ActiveSkill
    {
        get
        {
            if (activeSkill == null)
                activeSkill = new ActiveSkill_Cooldown_Reset(this);
            return activeSkill;
        }
    }
    protected override PassiveSkill _PassiveSkill
    {
        get
        {
            if (passiveSkill == null)
                passiveSkill = new PassiveSkill_Dmg_Boost(this);
            return passiveSkill;
        }
    }
}
