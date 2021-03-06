using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableController : MonoBehaviour
{

    // Singleton Pattern
    private static InteractableController _instance;
    public static InteractableController instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<InteractableController>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private InputActionProperty switchToolAction;
    [SerializeField]
    private XRBaseInteractor leftInteractor;
    [SerializeField]
    private XRBaseInteractor rightInteractor;
    public XRBaseInteractor RightInteractor
    {
        get => rightInteractor;
    }
    [SerializeField]
    private XRInteractionManager xRInteractionManager;
    [SerializeField]
    private Transform imageSelector;
    [SerializeField]
    private Transform featureVisualizer;
    private Transform[] interactables = new Transform[2];
    private int activeInteractableIdx = 2 - 1;

    [SerializeField]
    private Transform attachmentPointOculusTouch;
    [SerializeField]
    private Transform attachmentPointVive;

    [SerializeField]
    private Transform rightInteractorTransformOculusTouch;
    [SerializeField]
    private Transform rightInteractorTransformVive;

    [SerializeField]
    private Transform leftInteractorTransformOculusTouch;
    [SerializeField]
    private Transform leftInteractorTransformVive;
    [SerializeField]
    private bool VRMode = true;


    private void Awake()
    {
        // decativated because the second tool has no unique function yet
        //switchToolAction.action.performed += _ => SwitchTool();
    }

    private void Start()
    {
        // register interactables
        interactables[0] = imageSelector;
        interactables[1] = featureVisualizer;

        foreach (Transform tmpTransform in interactables)
        {
            tmpTransform.gameObject.SetActive(false);
        }

        SwitchTool();

        if (VRMode) InvokeRepeating("ScanRightHandDevices", 1f, 1f);  //1s delay, repeat every 1s
    }


    public void ScanRightHandDevices()
    {
        // choose attachoffset depending on device
        var rightHandedControllers = new List<UnityEngine.XR.InputDevice>();
        var desiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, rightHandedControllers);

        bool found = false;
        foreach (var device in rightHandedControllers)
        {
            if (device.name == "Oculus Touch Controller OpenXR")
            {
                rightInteractor.attachTransform = attachmentPointOculusTouch;
                rightInteractor.transform.SetParent(rightInteractorTransformOculusTouch);
                rightInteractor.transform.localPosition = Vector3.zero;
                rightInteractor.transform.localRotation = Quaternion.identity;
                leftInteractor.transform.SetParent(leftInteractorTransformOculusTouch);
                leftInteractor.transform.localPosition = Vector3.zero;
                leftInteractor.transform.localRotation = Quaternion.identity;
                found = true;
            }
            else if (device.name == "HTC Vive Controller OpenXR")
            {
                rightInteractor.attachTransform = attachmentPointVive;
                rightInteractor.transform.SetParent(rightInteractorTransformVive);
                rightInteractor.transform.localPosition = Vector3.zero;
                rightInteractor.transform.localRotation = Quaternion.identity;
                leftInteractor.transform.SetParent(leftInteractorTransformVive);
                leftInteractor.transform.localPosition = Vector3.zero;
                leftInteractor.transform.localRotation = Quaternion.identity;
                found = true;
            }
        }

        if (found)
        {
            CancelInvoke("ScanRightHandDevices");
            Debug.Log("Found controller type and adjusted attechment point.");
        }
        else
        {
            Debug.Log("Found the following devices:");
            foreach (var device in rightHandedControllers)
            {
                Debug.Log(device.name);
            }
        }
    }

    private void SwitchTool()
    {
        int newActiveInteractableIdx = activeInteractableIdx + 1;
        if (newActiveInteractableIdx >= interactables.Length) newActiveInteractableIdx = 0;
        Equip(interactables[newActiveInteractableIdx], interactables[activeInteractableIdx]);
        activeInteractableIdx = newActiveInteractableIdx;
    }


    private void Equip(Transform interactableTransform, Transform oldInteractableTransform)
    {
        oldInteractableTransform.gameObject.SetActive(false);
        interactableTransform.gameObject.SetActive(true);
        xRInteractionManager.ForceSelect(rightInteractor, interactableTransform.GetComponent<XRBaseInteractable>());

        if (InputManagerFPSInteraction.instance != null)
        {
            ImageSelector imageSelector = interactableTransform.GetComponent<ImageSelector>();
            if (imageSelector != null)
            {
                InputManagerFPSInteraction.instance.RegisterInteractionGun(imageSelector);
            }
            else
            {
                imageSelector = oldInteractableTransform.GetComponent<ImageSelector>();
                if (imageSelector != null)
                {
                    InputManagerFPSInteraction.instance.UnRegisterInteractionGun(imageSelector);
                }
            }
        }
    }

    /*public InputActionProperty SwitchToolAction
    {
        get => switchToolAction;
        set => SetInputActionProperty(ref switchToolAction, value);
    }


    void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
    {
        if (Application.isPlaying)
            property.DisableDirectAction();

        property = value;

        if (Application.isPlaying && isActiveAndEnabled)
            property.EnableDirectAction();
    }
    */



}
