using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InMatchUI : MonoBehaviour
{
    Slider healthBar;
    Slider chargeBar;

    [SerializeField] float maxHealth;
    public float currentHealth;

    [SerializeField] float maxCharge;
    public float currentCharge;

    [SerializeField] GameObject ghostBars;
    Canvas ui;

    float chargeDelay;
    [SerializeField] float maxChargeDelay;

    // set by player that owns the UI
    public float chargeRecoverySpeed = 4;


    void Start()
    {
        currentHealth = maxHealth;
        currentCharge = maxCharge;

        ui = FindAnyObjectByType<Canvas>();

        Vector3 flip = new Vector3 (1, 1, 1);
        if (GameObject.Find("GhostBars(Clone)") != null) // this is obviously a hack
        {
            flip = new Vector3(-1, 1, 1);
        }

        GameObject newBars = Instantiate(ghostBars, ui.transform);

        newBars.transform.localScale = flip; // this is also part of the hack

        healthBar = newBars.transform.GetChild(0).GetComponent<Slider>();
        chargeBar = newBars.transform.GetChild(1).GetComponent<Slider>();

        healthBar.maxValue = maxHealth;
        chargeBar.maxValue = maxCharge;
    }


    public void ChangeHealth(float toChangeBy)
    {
        currentHealth += toChangeBy;
    }

    public void ChangeCharge(float toChangeBy)
    {
        currentCharge += toChangeBy;
        chargeDelay = maxChargeDelay;
    }

    private void FixedUpdate()
    {
        if (chargeDelay > 0)
        {
            chargeDelay -= Time.fixedDeltaTime;
        }
        else
        {
            if (currentCharge < maxCharge)
            {
                currentCharge += Time.fixedDeltaTime * chargeRecoverySpeed;
            }
        }

        if (healthBar.value != currentHealth)
        {
            healthBar.value -= (healthBar.value - currentHealth) / 10;

            if (healthBar.value - currentHealth < 0.5f)
            {
                healthBar.value = currentHealth;
            }
        }

        if (chargeBar.value != currentCharge)
        {
            chargeBar.value -= (chargeBar.value - currentCharge) / 10;

            if (chargeBar.value - currentCharge < 0.5f)
            {
                chargeBar.value = currentCharge;
            }
        }
    }
}
