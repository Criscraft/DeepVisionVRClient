using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class LocomotionSettings : MonoBehaviour
{
    [SerializeField]
    private Image teleportationButtonImage;
    [SerializeField]
    private Image freeMovementButtonImage;
    [SerializeField]
    private Color colorActive;
    [SerializeField]
    private Color colorInactive;

    [SerializeField]
    private ActionBasedContinuousMoveProvider continuousMoveProvider;
    [SerializeField]
    private TeleportationProvider teleportationProvider;
    [SerializeField]
    private XRInteractorLineVisual teleportationVisualLine;
    [SerializeField]
    private XRRayInteractor interactor;
    [SerializeField]
    private Transform recticleTransform;


    void Start()
    {
        OnTeleportationButtonClick();
    }


    public void OnTeleportationButtonClick()
    {
        freeMovementButtonImage.color = colorInactive;
        teleportationButtonImage.color = colorActive;
        teleportationProvider.enabled = true;
        continuousMoveProvider.enabled = false;
        teleportationVisualLine.enabled = true;
        recticleTransform.gameObject.SetActive(true);
        interactor.enabled = true;
    }


    public void OnFreeMovementButtonClick()
    {
        teleportationButtonImage.color = colorInactive;
        freeMovementButtonImage.color = colorActive;
        teleportationProvider.enabled = false;
        continuousMoveProvider.enabled = true;
        teleportationVisualLine.enabled = false;
        recticleTransform.gameObject.SetActive(false);
        interactor.enabled = false;
    }
}
