using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour {

    public event Action Die = delegate { };

    public int RawAtk;
    public int RawDef;    
    public float BuffAtk;
    public float BuffDef;

    public int MaxHP;
    public int CurrentHP;
    
    public int Atk { get { return (int)(RawAtk * BuffAtk); } }
    public int Def { get { return (int)(RawDef * BuffDef * blockBonus); } }

    const int ChargeMax = 100;
    int ChargeAmount;

    [SerializeField]
    RectTransform healthBar;
    [SerializeField]
    RectTransform chargeBar;
    [SerializeField]
    int StartCharge;

    float blockBonus = 1f;

    void Start()
    {
        CurrentHP = MaxHP;
        if(chargeBar != null)
            IncreaseCharge(StartCharge);
    }

    public int Attack(Stats other)
    {
        return Atk - other.Def;
    }

    public void Damage(int damage)
    {
        CurrentHP -= damage;
        healthBar.localScale = new Vector3(Mathf.Clamp((float)CurrentHP / MaxHP, 0f, 1f), 1, 1);
        if (CurrentHP <= 0)
            Die();
    }

    public void Defend(bool defend)
    {
        blockBonus = defend ? 1.5f : 1;
    }

    public void IncreaseCharge(int amount)
    {
        if (chargeBar != null && ChargeAmount < ChargeMax)
        {
            ChargeAmount += amount;
            chargeBar.localScale = new Vector3((float)ChargeAmount / ChargeMax, 1, 1);
        }
    }
}
