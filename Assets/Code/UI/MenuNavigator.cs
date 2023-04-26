using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigator : MonoBehaviour
{
    public static MenuNavigator Menu { get { return FindObjectOfType<MenuNavigator>(); } }

    const int MAIN = 0;
    const int SETTINGS = 1;
    const int CHARACTER = 2;

    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject selectMenu;

    int currentMenu = 0;

    public void Forward()
    {
        UpdateCurrentMenu();
        if (currentMenu == MAIN)
        {
            Activate(CHARACTER);
        }
    }

    public void ForwardAlternate()
    {
        UpdateCurrentMenu();
        if (currentMenu != SETTINGS)
        {
            Activate(SETTINGS);
        }
    }

    public void Back()
    {
        UpdateCurrentMenu();
        if (currentMenu == CHARACTER || currentMenu == SETTINGS)
        {
            Activate(MAIN);
        }
    }

    public void UpdateCurrentMenu()
    {
        if (this == null)
            return;
        if (mainMenu.activeSelf)
        {
            currentMenu = 0;
            return;
        }
        if (settingsMenu.activeSelf)
        {
            currentMenu = 1;
            return;
        }
        if (selectMenu.activeSelf)
        {
            currentMenu = 2;
            return;
        }
    }

    public void Activate(int menu)
    {
        switch (menu)
        {
            case MAIN:
                mainMenu.SetActive(true);
                settingsMenu.SetActive(false);
                selectMenu.SetActive(false);
                break;
            case SETTINGS:
                mainMenu.SetActive(false);
                settingsMenu.SetActive(true);
                selectMenu.SetActive(false);
                break;
            case CHARACTER:
                mainMenu.SetActive(false);
                settingsMenu.SetActive(false);
                selectMenu.SetActive(true);
                break;
        }
    }
}
