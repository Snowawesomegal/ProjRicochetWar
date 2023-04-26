using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InMatchUI : MonoBehaviour
{
    GameObject owner;

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

        List<GameObject> bars = new List<GameObject>();

        GameObject newBars = Instantiate(ghostBars, ui.transform);

        foreach (GameObject fooObj in GameObject.FindGameObjectsWithTag("HealthBar"))
        {
            bars.Add(fooObj);
        }

        if (bars.Count == 2) // this is player 2's bar
        {
            newBars.transform.localScale = new Vector3(-1, 1, 1);
        }
        if (bars.Count == 3) // this is player 3's bar
        {
            newBars.transform.localPosition = new Vector3(1, -1, 1);
        }
        if (bars.Count == 4) // this is player 4's bar
        {
            newBars.transform.localScale = new Vector3(-1, 1, 1);
            newBars.transform.localPosition = new Vector3(1, -1, 1);
        }

        healthBar = newBars.transform.GetChild(0).GetComponent<Slider>();
        chargeBar = newBars.transform.GetChild(1).GetComponent<Slider>();

        healthBar.maxValue = maxHealth;
        chargeBar.maxValue = maxCharge;
    }


    public void ChangeHealth(float toChangeBy)
    {
        currentHealth += toChangeBy;

        if (currentHealth <= 0)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            gameObject.SetActive(false);

            if (players.Length == 2)
            {
                Debug.Log("start switching");

                if (players[0].activeSelf)
                    players[0].GetComponent<InMatchUI>().StartCoroutine("SwitchToMenu");
                else
                    players[1].GetComponent<InMatchUI>().StartCoroutine("SwitchToMenu");
            }
        }
    }

    IEnumerator SwitchToMenu()
    {
        yield return new WaitForSeconds(3);

        Debug.Log("switch");

        GameManager.Instance.LoadSelectScene();
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
