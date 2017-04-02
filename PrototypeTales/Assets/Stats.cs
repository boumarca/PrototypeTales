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
    float ChargeAmount;

    [SerializeField]
    RectTransform healthBar;
    [SerializeField]
    RectTransform chargeBar;
    [SerializeField]
    int StartCharge;
    [SerializeField]
    int MaxHitsBeforeGuardBreak;

    float blockBonus = 1f;
    int currentBlockHits;

    public bool IsInFusion;

    public bool ChargeIsFull { get { return ChargeAmount >= ChargeMax; } }
    public bool ChargeIsEmpty { get { return ChargeAmount == 0; } }

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

    public bool BreakGuard(int force)
    {
        currentBlockHits += force;

        if (currentBlockHits >= MaxHitsBeforeGuardBreak)
        {
            currentBlockHits = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Defend(bool defend)
    {
        blockBonus = defend ? 1.5f : 1;
    }

    public void IncreaseCharge(float amount)
    {
        if (chargeBar != null)
        {
            ChargeAmount += amount;
            ChargeAmount = Mathf.Clamp(ChargeAmount, 0, ChargeMax);
            chargeBar.localScale = new Vector3(ChargeAmount / ChargeMax, 1, 1);
        }
    }

    public void Heal(int heal)
    {
        CurrentHP += heal;
        healthBar.localScale = new Vector3(Mathf.Clamp((float)CurrentHP / MaxHP, 0f, 1f), 1, 1);

        if (CurrentHP > MaxHP)
            CurrentHP = MaxHP;
    }
}
