using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        //Start the coroutine we define below named ExampleCoroutine.
        StartCoroutine(ExampleCoroutine());
    }

    IEnumerator ExampleCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            Debug.Log("Load VRScene");
            SceneManager.LoadScene("VRScene", LoadSceneMode.Additive);
        }
        else
        {
            Debug.Log("Load NonVRScene");
            SceneManager.LoadScene("NonVRScene", LoadSceneMode.Additive);
        }
        SceneManager.UnloadSceneAsync("EntryScene");
        yield return null;
    }
}
