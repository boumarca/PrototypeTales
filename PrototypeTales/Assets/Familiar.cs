using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Familiar : MonoBehaviour {

    [SerializeField]
    GameObject player;
    [SerializeField]
    Stats playerStats;
    [SerializeField]
    RectTransform chargeBar;
    [SerializeField]
    Image arrow;
    [SerializeField]
    int StartCharge = 50;

    bool isSelected;
    const int ChargeMax = 100;
    float ChargeAmount;

    public bool isInFusion;
    public Image image;

    public bool ChargeIsFull { get { return ChargeAmount >= ChargeMax; } }
    public bool ChargeIsEmpty { get { return ChargeAmount == 0; } }

    void Start()
    {
        IncreaseCharge(StartCharge);
    }

    public void IncreaseCharge(float amount)
    {
        ChargeAmount += amount;
        ChargeAmount = Mathf.Clamp(ChargeAmount, 0, ChargeMax);
        chargeBar.localScale = new Vector3(ChargeAmount / ChargeMax, 1, 1);        
    }

    public void Select(bool select)
    {
        isSelected = select;
        arrow.enabled = select;
    }    
}
