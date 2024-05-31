using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Barracuda;

public class StandaloneNetwork : MonoBehaviour
{
    private JObject dataImagesResource;
    private JObject networkArchitectureResource;

    [SerializeField]
    public NNModel modelAsset;
    private Model m_RuntimeModel;
    private List<Tensor> activations;

    // Default values for the Demo ResNet.
    private int N_CLASSES = 101;
    private int nLayers = -1;
    private int INPUTWIDTH = 175;
    private int INPUTHEIGHT = 150;
    private Vector3 NORM_MEAN = new Vector3(0.5487017f, 0.5312975f, 0.50504637f);
    private Vector3 NORM_STD = new Vector3(0.1878664f, 0.18194826f, 0.19830684f);

    private readonly BurstCPUOps m_Ops = new BurstCPUOps(); // for CPU
    // private readonly PrecompiledComputeOps m_Ops = new PrecompiledComputeOps(); // if you want GPU ops

    private RenderTexture renderTexture;
    

    private void Awake()
    {
        dataImagesResource = ReadDataFromJson("data_images_resource.json");
        networkArchitectureResource = ReadDataFromJson("network_architecture_resource.json");

        m_RuntimeModel = ModelLoader.Load(modelAsset);
        activations = new List<Tensor>();
        
        renderTexture = new RenderTexture(INPUTWIDTH, INPUTHEIGHT, 24);
        RenderTexture.active = renderTexture;

        InitializeUsingBlackImage();

        Debug.Log("StandaloneNetwork initialized.");

    }

    public JObject GetNetworkArchitectureResource()
    {
        return networkArchitectureResource;
    }

    public JObject GetDatasetImagesResource()
    {
        return dataImagesResource;
    }

    public JObject GetLayerActivation(int layerID)
    // Returns the activations of the layer with the given layerID.
    {
        var layerIdToOutputIndex = networkArchitectureResource["layer_id_to_output_index"];
        int outputIndex = (int) layerIdToOutputIndex[layerID.ToString()];
        Tensor tensor = activations[outputIndex];

        // Transform the tensor to uint8.
        float vMax = m_Ops.ReduceMax(tensor.Flatten(), 7)[0,0,0,0];
        float vMin = m_Ops.ReduceMin(tensor.Flatten(), 7)[0,0,0,0];
        float zeroNormalized = NormalizePixel(0f, vMin, vMax);

        // Convert tensor data to byte arrays and then to Base64 strings.
        JArray base64Strings = new JArray();
        int height = tensor.height;
        int width = tensor.width;
        for (int i = 0; i < tensor.channels; i++)
        {
            Texture2D texture2D = new Texture2D(width, height);
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    float value = tensor[0, j, k, i];
                    value = NormalizePixel(value, vMin, vMax);
                    Color color = new Color(value, value, value);
                    texture2D.SetPixel(k, height - j, color);
                }
            }
            base64Strings.Add(EncodeToPNG(texture2D));
        }

        // Create the output.
        JObject output = new JObject();
        output["tensors"] = base64Strings;
        output["layerID"] = layerID;
        output["zeroValue"] = (int)zeroNormalized;
        output["mode"] = "Activation";

        return output;
    }


    private static string EncodeToPNG(Texture2D texture2D)
    {
        byte[] byteArray = ImageConversion.EncodeToPNG(texture2D);
        string base64String = Convert.ToBase64String(byteArray);
        return base64String;
    }


    public JObject GetLayerFeatureVisualization(int layerID) {
    // Loads prepared JSON files and returns them.
        return ReadDataFromJson("layer_" + layerID.ToString() + ".json");
    }


    public void PrepareForInput(Texture2D tex, DLWebClient.CallbackDelegate callbackDelegate = null, bool blocking = false)
    {
        // Resize the image.
        var resizedTexture = Resize(tex, INPUTWIDTH, INPUTHEIGHT);
        // Create tensor from image texture.
        // We enforce 3 color channels.
        using var tensor = new Tensor(resizedTexture, channels:3);

        // Normalize image. Pixel values are automaticaly transformed to the range [0,1]
        var tensor_normalized = new Tensor(tensor.shape);
        for (int i = 0; i < INPUTHEIGHT; i++)
        {
            for (int j = 0; j < INPUTWIDTH; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    tensor_normalized[0, i, j, k] = (tensor[0, i, j, k] - NORM_MEAN[k]) / NORM_STD[k];
                }
            }
        }

        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, m_RuntimeModel);
        worker.Execute(tensor_normalized);
        tensor_normalized.Dispose();

        if (blocking){
            GetModelResults(worker, callbackDelegate);
        } else {
            StartCoroutine(GetModelResultsDeferred(worker, callbackDelegate));
        }
    }


    public IEnumerator GetModelResultsDeferred(IWorker worker, DLWebClient.CallbackDelegate callbackDelegate)
    {
        yield return new WaitForSeconds(0.5f);
        UnityMainThreadDispatcher.Instance().Enqueue(GetModelResultsDeferred_(worker, callbackDelegate));
    }


    public IEnumerator GetModelResultsDeferred_(IWorker worker, DLWebClient.CallbackDelegate callbackDelegate)
    {
        GetModelResults(worker, callbackDelegate);
        yield return null;
    }


    public void GetModelResults(IWorker worker, DLWebClient.CallbackDelegate callbackDelegate)
    {
        var outputNames = m_RuntimeModel.outputs;
        foreach (var activation in activations)
        {
            activation.Dispose();
        }
        activations.Clear();
        nLayers = outputNames.Count;
        for (int i = 0; i < nLayers; i++)
        {
            activations.Add(worker.CopyOutput(outputNames[i]));
        }
        
        worker.Dispose();
        
        if (callbackDelegate != null)
        {
            StartCoroutine(callbackDelegate());
        }
        
    }

    public JObject GetClassifiactionResults()
    {
        // Determine TOP 10 class names and probabilities.
        var tensor = activations[activations.Count - 1];
        var activation = new JArray();
        for (int i = 0; i < N_CLASSES; i++)
        {
            activation.Add((float)tensor[0, 0, 0, i] * 100f);
        }
        JArray indices = JArgSort(activation);
        indices = new JArray(indices.Reverse());
        indices = new JArray(indices.Take(10));
        // Get the activations of the last layer.
        JArray confidenceValues = new JArray(indices.Select(index => activation[(int)index]));
        
        // Round every element to 2 decimal places.
        for (int i = 0; i < confidenceValues.Count; i++)
        {
            confidenceValues[i] = Math.Round((float)confidenceValues[i], 2);
        }

        // Get the class names.
        JArray classNames = new JArray(indices.Select(index => dataImagesResource["class_names"][(int)index]));
        // Create the output.
        JObject output = new JObject();
        output["class_names"] = classNames;
        output["confidence_values"] = confidenceValues;
        return output;
    }

    public JObject GetNoiseImage()
    {
        Texture2D texture2D = new Texture2D(INPUTWIDTH, INPUTHEIGHT);
        for (int i = 0; i < INPUTHEIGHT; i++)
        {
            for (int j = 0; j < INPUTWIDTH; j++)
            {
                float value_r = UnityEngine.Random.Range(0f, 1f);
                float value_g = UnityEngine.Random.Range(0f, 1f);
                float value_b = UnityEngine.Random.Range(0f, 1f);
                Color color = new Color(value_r, value_g, value_b);
                texture2D.SetPixel(j, i, color);
            }
        }
        string encodedImage = EncodeToPNG(texture2D);

        // Create the output.
        JObject output = new JObject();
        output["tensor"] = encodedImage;
        return output;
    }

    public static JArray JArgSort(JArray items)
    // Sorts the indices of the items in descending order.
    {
        var sortedIndices = items
            .Select((item, index) => new { item, index })
            .OrderBy(x => (float)x.item)
            .Select(x => x.index);

        return new JArray(sortedIndices);
    }

    public float NormalizePixel(float x, float vMin, float vMax)
    {
        float normalized = (x - vMin) / (vMax - vMin + 1e-6f);
        //normalized = normalized * 255f;
        //normalized = Clamp(normalized, 0f, 255f);

        return normalized;
    }

    public float Clamp(float value, float min, float max)
    {
        return Math.Min(Math.Max(value, min), max);
    }


    Texture2D Resize(Texture2D texture2D,int targetX,int targetY)
    {
        Graphics.Blit(texture2D, renderTexture);
        Texture2D result = new Texture2D(targetX,targetY);
        result.ReadPixels(new Rect(0, 0 ,targetX, targetY),0,0);
        result.Apply();
        return result;
    }

    public void InitializeUsingBlackImage()
    {
        // Create a black texture.
        Texture2D blackTexture = new Texture2D(128, 128);
        // Fill the texture with black color.
        for (int i = 0; i < blackTexture.width; i++)
        {
            for (int j = 0; j < blackTexture.height; j++)
            {
                blackTexture.SetPixel(i, j, Color.black);
            }
        }
        PrepareForInput(blackTexture, blocking: true);
    }

    public static JObject ReadDataFromJson(String jsonFileName) {
        var jsonFilePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        string dataString;
        #if UNITY_ANDROID
            WWW reader = new WWW(jsonFilePath);
            while (!reader.isDone) { } // Do nothing
            dataString = reader.text;
        #else
            dataString = File.ReadAllText(jsonFilePath);
        #endif
        return JObject.Parse(dataString);
    }


}
