using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Text;
//using System;

public class DLWebClient : MonoBehaviour
{
    [SerializeField]
    private string url = "http://127.0.0.1:5570/";
    //[SerializeField]
    //private bool force_standalone_mode = false;
    [SerializeField]
    private GameObject dlNetworkPrefab;
    [SerializeField]
    private GameObject datasetPrefab;
    [SerializeField]
    private GameObject noiseGeneratorPrefab;
    [SerializeField]
    private float networkSpacing = 25f;
    [SerializeField]
    private GameObject standaloneNetworkPrefab;
    private StandaloneNetwork standaloneNetwork;


    private List<DLNetwork> dlNetworkList;
    public delegate IEnumerator CallbackJSONDelegate(JObject jObject);
    public delegate IEnumerator CallbackDelegate();


    private IEnumerator GetJSON(string resource, CallbackJSONDelegate handleJSONDelegate)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url + resource))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(url + resource);
            }
            else
            {
                Debug.Log("Received: " + url + resource);
                JObject jObject = JObject.Parse(www.downloadHandler.text);
                // Call the callback function on the main thread.
                UnityMainThreadDispatcher.Instance().Enqueue(handleJSONDelegate(jObject));
            }
        }
    }


    private IEnumerator Upload(string resource, string dataString, CallbackDelegate handleUploadDelegate)
    {
        byte[] data = Encoding.UTF8.GetBytes(dataString);
        using (UnityWebRequest www = UnityWebRequest.Put(url + resource, data))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log("Upload Complete: " + url + resource);
                UnityMainThreadDispatcher.Instance().Enqueue(handleUploadDelegate());
            }
        }
    }


    public IEnumerator DoNothing()
    {
        yield return null;
    }


    public void RequestNetworkArchitecture(CallbackJSONDelegate handleJSONDelegate, int networkID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            StartCoroutine(handleJSONDelegate(standaloneNetwork.GetNetworkArchitectureResource()));
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("network/{0}", networkID), handleJSONDelegate));
        }
    }


    public void RequestLayerActivation(CallbackJSONDelegate handleJSONDelegate, int networkID, int layerID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            StartCoroutine(handleJSONDelegate(standaloneNetwork.GetLayerActivation(layerID)));
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("network/{0}/activation/layerid/{1}", networkID, layerID), handleJSONDelegate));
        }
    }


    public void RequestLayerFeatureVisualization(CallbackJSONDelegate handleJSONDelegate, int networkID, int layerID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            JObject jObject = standaloneNetwork.GetLayerFeatureVisualization(layerID);
            if (jObject.Count > 0)
            {
                StartCoroutine(handleJSONDelegate(jObject));
            }
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("network/{0}/featurevisualization/layerid/{1}", networkID, layerID), handleJSONDelegate));
        }
    }


    public void RequestAllFeatureVisualizations(int networkID) 
    {
        dlNetworkList[networkID].RequestAllFeatureVisualizations();
    }


    public void RequestWeightHistogram(CallbackJSONDelegate handleJSONDelegate, int networkID, int layerID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            // do something
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("network/{0}/weighthistogram/layerid/{1}", networkID, layerID), handleJSONDelegate));
        }
    }


    public void RequestActivationHistogram(CallbackJSONDelegate handleJSONDelegate, int networkID, int layerID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            // do something
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("network/{0}/activationhistogram/layerid/{1}", networkID, layerID), handleJSONDelegate));
        }
    }


    //public IEnumerator PrepareForInputDemoNetwork(HandleUploadDelegate handleUploadDelegate, Texture2D tex)
    //{
    //    yield return StartCoroutine(standaloneNetwork.PrepareForInput(tex));
    //    UnityMainThreadDispatcher.Instance().Enqueue(handleUploadDelegate());
    //}
    
    
    public void RequestPrepareForInput(CallbackDelegate handleUploadDelegate, int networkID, ActivationImage activationImage)
    {
        if (activationImage.mode == ActivationImage.Mode.Activation) return;
        if (activationImage.mode == ActivationImage.Mode.DatasetImage && activationImage.datasetID == -1) return;

        if (GlobalSettings.Instance.using_demo_network)
        {
            Texture2D tex = (Texture2D)activationImage.tex;
            // Start Coroutine to prepare the input.
            //StartCoroutine(PrepareForInputDemoNetwork(handleUploadDelegate, tex));
            standaloneNetwork.PrepareForInput(tex, handleUploadDelegate);
        }
        else
        {
            ActivationImage activationImageShallowCopy = activationImage;
            activationImageShallowCopy.tex = null;
            string output = JsonConvert.SerializeObject(activationImageShallowCopy);
            StartCoroutine(Upload(string.Format("network/{0}/prepareforinput", networkID), output, handleUploadDelegate));
        }
    }


    // server side export, currently unused
    public void RequestLayerExport(int networkID, int layerID, ActivationImage activationImage)
    {
        ActivationImage activationImageShallowCopy = activationImage;
        activationImageShallowCopy.tex = null;
        string output = JsonConvert.SerializeObject(activationImageShallowCopy);
        StartCoroutine(Upload(string.Format("network/{0}/export/layerid/{1}", networkID, layerID), output, DoNothing));
    }


    public void RequestClassificationResults(CallbackJSONDelegate handleJSONDelegate, int networkID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            StartCoroutine(handleJSONDelegate(standaloneNetwork.GetClassifiactionResults()));
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("network/{0}/classificationresult", networkID), handleJSONDelegate));
        }
    }


    public void RequestDatasetImages(CallbackJSONDelegate handleJSONDelegate, int datasetID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            StartCoroutine(handleJSONDelegate(standaloneNetwork.GetDatasetImagesResource()));
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("dataset/{0}/images", datasetID), handleJSONDelegate));
        }
    }


    public void RequestNoiseImage(CallbackJSONDelegate handleJSONDelegate, int noiseID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            StartCoroutine(handleJSONDelegate(standaloneNetwork.GetNoiseImage()));
        }
        else
        {
            StartCoroutine(GetJSON(string.Format("noiseimage/{0}", noiseID), handleJSONDelegate));
        }
    }


    public void RequestBasicInfo()
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            JObject basicInfo = new JObject();
            basicInfo["nnetworks"] = 1;
            basicInfo["ndatasets"] = 1;
            basicInfo["nnoiseGenerators"] = 1;
            StartCoroutine(AcceptBasicInfo(basicInfo));
        }
        else
        {
            StartCoroutine(GetJSON("network", AcceptBasicInfo));
        }
    }


    public void SetNetworkGenFeatVis(int networkID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            // Do nothing. Feature visualization is not supported for the demo network. The visualizations are loaded from the resources.
        }
        else
        {
            string output = "dummy";
            StartCoroutine(Upload(string.Format("network/{0}/setnetworkgenfeatvis", networkID), output, DoNothing));
        }
    }


    public void SetNetworkLoadFeatVis(int networkID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            // Do nothing. Feature visualization is not supported for the demo network. The visualizations are loaded from the resources.
        }
        else
        {
            string output = "dummy";
            StartCoroutine(Upload(string.Format("network/{0}/setnetworkloadfeatvis", networkID), output, DoNothing));
        }
    }


    public void SetNetworkDeleteFeatVis(int networkID)
    {
        if (GlobalSettings.Instance.using_demo_network)
        {
            // Do nothing. Feature visualization is not supported for the demo network. The visualizations are loaded from the resources.
        }
        else
        {
            string output = "dummy";
            StartCoroutine(Upload(string.Format("network/{0}/setnetworkdeletefeatvis", networkID), output, DoNothing));
        }
    }


    public IEnumerator AcceptBasicInfo(JObject jObject)
    {
        int Nnetworks = (int)jObject["nnetworks"];
        int Ndatasets = (int)jObject["ndatasets"];
        int NnoiseGenerators = (int)jObject["nnoiseGenerators"];
        DLNetwork dlNetwork;
        FeatureVisSettingsButtons featureVisSettingsButtons;
        Dataset dataset;
        dlNetworkList = new List<DLNetwork>();

        for (int i = 0; i < Nnetworks; i++)
        {
            Transform newInstance = Instantiate(dlNetworkPrefab).transform;
            newInstance.name = string.Format("Network{0}", i);
            newInstance.localPosition = new Vector3(networkSpacing * i, 0f, 0f);
            newInstance.localRotation = Quaternion.identity;
            newInstance.localScale = new Vector3(1f, 1f, 1f);
            newInstance.SetParent(transform);
            dlNetwork = newInstance.GetComponentInChildren<DLNetwork>();
            dlNetwork.Prepare(this, i);
            featureVisSettingsButtons = newInstance.GetComponentInChildren<FeatureVisSettingsButtons>();
            featureVisSettingsButtons.Prepare(this, i);
            dlNetworkList.Add(dlNetwork);
        }

        for (int i = 0; i < Ndatasets; i++)
        {
            Transform newInstance = Instantiate(datasetPrefab).transform;
            newInstance.name = string.Format("Dataset{0}", i);
            newInstance.localPosition = new Vector3(networkSpacing * i, 0f, -12f);
            newInstance.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
            newInstance.localScale = new Vector3(0.007f, 0.007f, 0.007f);
            newInstance.SetParent(transform);
            dataset = newInstance.GetComponent<Dataset>();
            dataset.Prepare(this, i);
            dataset.BuildDataset();
        }


        for (int i = 0; i < NnoiseGenerators; i++)
        {
            Transform newInstance = Instantiate(noiseGeneratorPrefab).transform;
            newInstance.name = string.Format("NoiseGenerator{0}", i);
            newInstance.localPosition = new Vector3(-5f, 0f, -6f + 3f * i);
            newInstance.localRotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));
            newInstance.localScale = new Vector3(0.006f, 0.006f, 0.006f);
            newInstance.SetParent(transform);
            newInstance.GetComponentInChildren<NoiseGenerateButton>().Prepare(this, i);
        }

        // Build the networks.
        foreach (DLNetwork dlNetwork_ in dlNetworkList) {
                dlNetwork_.BuildNetwork();
        }

        yield return null;
    }


    public static Texture2D StringToTex(string textureString)
    {
        byte[] b64_bytes = System.Convert.FromBase64String(textureString);
        Texture2D tex = new Texture2D(1, 1);
        if (ImageConversion.LoadImage(tex, b64_bytes))
        {
            tex.filterMode = FilterMode.Point;
            return tex;
        }
        else
        {
            Debug.Log("Texture could not be loaded");
            return null;
        }
    }


    private IEnumerator CheckWebServer()
    // Check if the web server is available.
    {
        //if(force_standalone_mode) {
        //    GlobalSettings.Instance.server_available = false;
        //    GlobalSettings.Instance.using_demo_network = true;
        //    LoadDemoNetwork();
        //    RequestBasicInfo();
        //    yield break;
        //}
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Set the timeout to 2 seconds.
            request.timeout = 2;
            yield return request.SendWebRequest();

            // Check if the web server exists and responded.
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Web server exists and responded.");
                GlobalSettings.Instance.server_available = true;
                GlobalSettings.Instance.using_demo_network = false;
            }
            else
            {
                Debug.LogWarning("Web server does not exist or did not respond. Starting standalone mode.");
                Debug.Log(request.error);
                GlobalSettings.Instance.server_available = false;
                GlobalSettings.Instance.using_demo_network = true;
                LoadDemoNetwork();
            }

            RequestBasicInfo();
        }
    }


    private void LoadDemoNetwork()
    {
        var standaloneNetwork_go = Instantiate(standaloneNetworkPrefab);
        standaloneNetwork_go.name = "StandaloneNetwork";
        standaloneNetwork_go.transform.SetParent(transform);
        standaloneNetwork = standaloneNetwork_go.GetComponent<StandaloneNetwork>();
    }


    private void Start()
    {
        StartCoroutine(CheckWebServer());
    }
}