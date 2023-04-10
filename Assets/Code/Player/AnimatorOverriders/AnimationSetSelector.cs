using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSetSelector : MonoBehaviour
{
    [SerializeField] private AnimationSetCollection animationSetCollection;
    public PlayerAnimationSetter playerAnimationSetter;

    public Transform contentHolder;

    public GameObject setSelectionPrefab; // should be the button selecting a skin, it calls Select(skin, skinIndex)

    public GameObject setScrollView;

    public void SetPlayer(PlayerAnimationSetter _playerSpriteSetter)
    {
        Debug.Log("Local bobby found");
        if (playerAnimationSetter != null)
            return;

        playerAnimationSetter = _playerSpriteSetter;
        animationSetCollection = playerAnimationSetter.animationSetCollection;
        
        AnimationSetSelectionCreator newSkinSelect; // create instance of SkinSelectionCreator

        for (int i = 0; i < animationSetCollection.animationSets.Count; i++)
        {
            newSkinSelect = Instantiate(setSelectionPrefab, contentHolder).GetComponent<AnimationSetSelectionCreator>();
            newSkinSelect.Setup(animationSetCollection.animationSets[i], playerAnimationSetter, i);
        }
    }

    public void ToggleSkinView()
    {
        if (setScrollView.activeInHierarchy)
            setScrollView.SetActive(false);
        else
            setScrollView.SetActive(true);
    }
}
