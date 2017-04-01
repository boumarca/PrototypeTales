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

    float blockBonus = 1f;

    void Start()
    {
        CurrentHP = MaxHP;
    }

    public int Attack(Stats other)
    {
        return Atk - other.Def;
    }

    public void Damage(int damage)
    {
        CurrentHP -= damage;
        if (CurrentHP <= 0)
            Die();
    }

    public void Defend(bool defend)
    {
        blockBonus = defend ? 1.5f : 1;
    }
}
