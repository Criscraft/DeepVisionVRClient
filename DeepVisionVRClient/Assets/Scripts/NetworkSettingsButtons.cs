using UnityEngine;
using UnityEngine.UIElements;

public class NetworkSettingsButtons : MonoBehaviour
{
    [SerializeField]
    private Transform network;

    private bool networkUIVisible = true;


    public void OnNetworkScaleSliderChanged(float value)
    {
        network.localScale = new Vector3(value, value, value);
    }


    public void OnUIToggleButtonClick()
    {
        networkUIVisible = !networkUIVisible;
        foreach (GameObject uiElement in GameObject.FindGameObjectsWithTag("ToggableNetworkUI"))
        {
            uiElement.SetActive(networkUIVisible);
        }
    }

}
