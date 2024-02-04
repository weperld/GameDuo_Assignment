using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer_02 : Archer
{
    public override ActiveSkill _ActiveSkill
    {
        get
        {
            if (activeSkill == null)
                activeSkill = new ActiveSkill_Atk_Spd_Boost(this);
            return activeSkill;
        }
    }
    public override PassiveSkill _PassiveSkill
    {
        get
        {
            if (passiveSkill == null)
                passiveSkill = new PassiveSkill_Skip_Wave(this);
            return passiveSkill;
        }
    }
}
