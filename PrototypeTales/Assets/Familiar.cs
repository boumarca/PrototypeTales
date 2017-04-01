using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Familiar : MonoBehaviour {

    [SerializeField]
    GameObject player;
    [SerializeField]
    Stats playerStats;
    [SerializeField]
    RectTransform chargeBar;

    [SerializeField]
    int StartCharge = 50;

    const int ChargeMax = 100;
    int ChargeAmount;

    void Start()
    {
        IncreaseCharge(StartCharge);
    }

    public void IncreaseCharge(int amount)
    {
        if (ChargeAmount < ChargeMax)
        {
            ChargeAmount += amount;
            chargeBar.localScale = new Vector3((float)ChargeAmount / ChargeMax, 1, 1);
        }
    }
}
