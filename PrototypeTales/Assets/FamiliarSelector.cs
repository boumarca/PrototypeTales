using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamiliarSelector : MonoBehaviour {

    [SerializeField]
    Familiar[] familiars;
    [SerializeField]
    Stats player;
    [SerializeField]
    ParticleSystem fusionFX;
    [SerializeField]
    int ChargeDecreaseRate;

    Familiar selectedFamiliar;


    void Start()
    {
        familiars[0].Select(true);
        selectedFamiliar = familiars[0];
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetAxis("ArrowsVertical") == 1)
        {
            familiars[0].Select(true);
            familiars[1].Select(false);
            selectedFamiliar = familiars[0];
        }
        else if (Input.GetAxis("ArrowsVertical") == -1)
        {
            familiars[0].Select(false);
            familiars[1].Select(true);
            selectedFamiliar = familiars[1];
        }

        if (player.ChargeIsFull && selectedFamiliar.ChargeIsFull && Input.GetButtonDown("Fuse"))
        {
            StartCoroutine(DuringFusion());
        }
    }

    IEnumerator DuringFusion()
    {
        Familiar fused = selectedFamiliar;
        var main = fusionFX.main;
        main.startColor = fused.image.color;
        fusionFX.Play();
        player.IsInFusion = true;
        fused.isInFusion = true;

        if (fused == familiars[0])
            player.BuffAtk *= 2;
        else if (fused == familiars[1])
            player.BuffDef *= 2;

        while (!player.ChargeIsEmpty && !fused.ChargeIsEmpty)
        {
            player.IncreaseCharge(-ChargeDecreaseRate * Time.deltaTime);
            fused.IncreaseCharge(-ChargeDecreaseRate * Time.deltaTime);
            yield return new WaitForSeconds(0);
        }
        fusionFX.Stop();
        player.IsInFusion = false;
        fused.isInFusion = false;

        if (fused == familiars[0])
            player.BuffAtk /= 2;
        else if (fused == familiars[1])
            player.BuffDef /= 2;
    }
}
