using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FighterSelectionManager))]
public class FighterSelectorMenu : MonoBehaviour
{
    public static bool shouldDisableOnStart = false;

    [SerializeField] public GameObject menuCanvas;
    [SerializeField] public List<GameObject> otherMenus = new List<GameObject>();
    [SerializeField] public FighterSelectionManager selectionManager;
    [SerializeField] public bool isolated = false;

    private void Awake()
    {
        if (!selectionManager)
            selectionManager = GetComponent<FighterSelectionManager>();

        foreach (Object menu in Resources.FindObjectsOfTypeAll<SettingsMenu>())
        {
            Debug.Log("Found menu, adding: " + menu);
            otherMenus.Add(((SettingsMenu)menu).menu);
        }
        foreach (MainMenu menu in Resources.FindObjectsOfTypeAll<MainMenu>())
        {
            Debug.Log("Found menu, adding: " + menu);
            otherMenus.Add(menu.canvas);
        }

        if (shouldDisableOnStart)
            menuCanvas?.SetActive(false);
    }

    private void Update()
    {
        if (!isolated && !shouldDisableOnStart)
        { 
            ActivateAlone();
            selectionManager?.CollectSessionPlayers();
            shouldDisableOnStart = true;
            isolated = true;
        }
    }

    public void ActivateAlone()
    {
        foreach (GameObject menu in otherMenus)
        {
            menu.SetActive(false);
        }
        menuCanvas?.SetActive(true);
    }
}
