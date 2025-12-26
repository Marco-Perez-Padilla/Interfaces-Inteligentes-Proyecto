using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;


// Main class that manages messages sent to Ollama
public class OllamaUI : MonoBehaviour
{
    // Auxiliar class to wrap the prompt sent by Whisper into a serializable string
    [System.Serializable]
    private class SafeString
    {
        public string value;
    }

    [SerializeField] private string texto = "Dame la bienvenida y paso al tunel del terror. Indica que me monte en la vagoneta para empezar";

    [Header("Whisper UI Reference")]
    public WhisperUI whisperUI; 

    [Header("LLM Settings")]
    private string littleLlmUrl = "http://gpu1.esit.ull.es:4000/v1/chat/completions";

    [Header("Output UI")]
    public Text llmOutputText;

    private Keyboard keyboard;

    [Header("Typewriter Settings")]
    public float charDelay = 0.05f; 

    private void OnEnable()
    {
        if (whisperUI != null)
            whisperUI.OnWhisperTranscription += OnWhisperTranscribed;
    }

    private void OnDisable()
    {
        if (whisperUI != null)
            whisperUI.OnWhisperTranscription -= OnWhisperTranscribed;
    }

    void Start()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard == null) return;

        if (keyboard.xKey.wasPressedThisFrame)
        {
            StartCoroutine(SendMessageToChatbot(texto));
        }
    }

    // -------------------------------
    // Evento de Whisper
    // -------------------------------
    private void OnWhisperTranscribed()
    {
        string prompt = whisperUI.outputText.text;
        StartCoroutine(DelayedSendFromWhisper(1.5f, prompt));
    }

    private IEnumerator DelayedSendFromWhisper(float delay, string prompt)
    {
        yield return new WaitForSeconds(delay);

        SafeString safe = new SafeString { value = prompt };
        string json = JsonUtility.ToJson(safe);
        string safePrompt = json.Substring(10, json.Length - 12);

        StartCoroutine(SendMessageToChatbot(safePrompt));
    }

    // -------------------------------
    // Corrutina que llama al LLM
    // -------------------------------
    public IEnumerator SendMessageToChatbot(string message)
    {
        Debug.Log("Entra en OllamaUI con mensaje: " + message);

        string jsonPayload = "{"
            + "\"model\": \"ollama/llama3.1:8b\","
            + "\"messages\": [{\"role\": \"user\", \"content\": \"" + message + "\"}]"
            + "}";

        UnityWebRequest request = new UnityWebRequest(littleLlmUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer sk-1234");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al conectar con el chatbot: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta de la IA: " + jsonResponse);

            string marker = "\"content\":\"";
            int start = jsonResponse.IndexOf(marker);
            if (start != -1) {
                start += marker.Length;
                int end = jsonResponse.IndexOf("\"", start);
                string content = jsonResponse.Substring(start, end - start);

                content = content.Replace("\\n", "\n").Replace("\\\"", "\"");

            
                if (llmOutputText != null)
                {
                    StartCoroutine(TypewriterEffect(content));
                }
            }
        }
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        llmOutputText.text = "";
        foreach (char c in fullText)
        {
            llmOutputText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }
}
