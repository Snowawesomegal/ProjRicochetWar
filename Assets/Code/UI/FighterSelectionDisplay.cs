using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FighterSelectionDisplay : MonoBehaviour
{
    private float squareOffset = 40;
    [SerializeField] private SessionPlayer player;
    public SessionPlayer SessionPlayer { get { return player; } }
    public bool HasPlayer { get { return player != null; } }
    public bool Unassigned { get { return player == null; } }

    [SerializeField] Image playerDisplay;
    [SerializeField] RectTransform selectionIndicator;
    [SerializeField] FighterSelectionManager manager;

    [SerializeField] PlayerUIBrowser uiBrowser;

    private bool isReady = false;
    public bool Ready { get { return isReady; } }

    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite notReadySprite;
    [SerializeField] private Image readyImage;

    public void AssignPlayer(SessionPlayer player, Sprite defaultSelectionSprite, FighterSelectionManager manager)
    {
        this.player = player;
        this.manager = manager;
        uiBrowser = player.Input.GetComponent<PlayerUIBrowser>();
        uiBrowser.currentButton = 0;
        uiBrowser.playerIndex = player.PlayerIndex;
        PlaceIndicator(uiBrowser.currentButton);
        Debug.Log("Subscribing navigate");
        Debug.Log("Devices: " + player.Input.devices);
        uiBrowser.NavigateBehavior += NavigateIndicator;
        uiBrowser.SubmitBehavior += SelectFighter;
        uiBrowser.ReadyBehavior += ReadyUp;
        if (playerDisplay && defaultSelectionSprite)
        {
            playerDisplay.sprite = defaultSelectionSprite;
        }
        UpdateReadyImage();
    }

    public void RemovePlayer()
    {
        uiBrowser.NavigateBehavior -= NavigateIndicator;
        uiBrowser.SubmitBehavior -= SelectFighter;
        uiBrowser = null;
        manager = null;
        player = null;
        isReady = false;
        if (playerDisplay)
            playerDisplay.sprite = null;
    }

    private Vector2 InidicatorOffset(int playerIndex)
    {
        switch (playerIndex)
        {
            default:
            case 0: return new Vector2(-squareOffset, squareOffset);
            case 1: return new Vector2(squareOffset, squareOffset);
            case 2: return new Vector2(squareOffset, -squareOffset);
            case 3: return new Vector2(-squareOffset, -squareOffset);
        }
    }

    public void NavigateIndicator(Vector2 direction)
    {
        if (direction.x < 0)
        { // left
            uiBrowser.currentButton--;
            if (uiBrowser.currentButton < 0)
                uiBrowser.currentButton = 0;
            PlaceIndicator(uiBrowser.currentButton);
        } else if (direction.x > 0)
        { // right
            uiBrowser.currentButton++;
            if (uiBrowser.currentButton >= manager.possibleButtons.Count)
                uiBrowser.currentButton = manager.possibleButtons.Count - 1;
            PlaceIndicator(uiBrowser.currentButton);
        }
    }

    public void PlaceIndicator(int index)
    {
        if (manager == null)
            return;

        RectTransform rect = manager.possibleButtons[index].rect;
        Vector2 position = rect.position;
        position += InidicatorOffset(player.PlayerIndex);
        selectionIndicator.position = position;
    }

    public void SelectFighter()
    {
        player.SelectedFighter = manager.possibleButtons[uiBrowser.currentButton].fighter;
        if (playerDisplay)
            playerDisplay.sprite = player.SelectedFighter.showcaseSprite;
    }

    public void ReadyUp()
    {
        if (manager != null && player.SelectedFighter != null) {
            isReady = !isReady;
            Debug.Log("Player " + player.PlayerIndex + " is ready!");
            manager.QueryReady();
        }
        UpdateReadyImage();
    }

    public void UpdateReadyImage()
    {
        if (readyImage && readySprite && notReadySprite)
            readyImage.sprite = isReady ? readySprite : notReadySprite;
    }
}
