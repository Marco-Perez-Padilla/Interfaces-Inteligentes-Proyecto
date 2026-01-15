using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

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

    [Header("Output")]
    public FloatingText3D floatingText;

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

        string npcContext = "Eres un NPC de tutorial en un juego de aventura. "
                            + "Mantén siempre tu rol de NPC interactivo. "
                            + "Si el jugador dice 'vale', 'ok' o 'sí', explícale que debe tirar de la palanca de la vagoneta para empezar el recorrido. "
                            + "Si dice otra cosa, da la bienvenida al túnel del terror y asegúrate de incluir la palabra 'vagoneta'. "
                            + "No salgas nunca de tu rol y haz que tu respuesta sea coherente con la acción del jugador.";

        // Escapar el contenido
        string escapedNpc = npcContext.Replace("\"", "\\\"").Replace("\n", "\\n");
        string escapedUser = prompt.Replace("\"", "\\\"").Replace("\n", "\\n");

        // Construir JSON de forma segura
        string jsonPayload = "{"
            + "\"model\": \"ollama/llama3.1:8b\","
            + "\"messages\": ["
            + "{\"role\": \"system\", \"content\": \"" + escapedNpc + "\"},"
            + "{\"role\": \"user\", \"content\": \"" + escapedUser + "\"}"
            + "]"
            + "}";

        // Enviar al LLM
        StartCoroutine(SendMessageToChatbot(jsonPayload, true));

    }

    // -------------------------------
    // Corrutina que llama al LLM
    // -------------------------------
    public IEnumerator SendMessageToChatbot(string jsonPayload, bool isRawPayload = false)
    {
        Debug.Log("Entra en OllamaUI con mensaje: " + jsonPayload);

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

            
                if (floatingText != null)
                {
                    StartCoroutine(TypewriterEffect(content));
                }
            }
        }
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        floatingText.SetText("");
        foreach (char c in fullText)
        {
            floatingText.SetText(floatingText.GetComponent<TextMeshPro>().text + c);
            yield return new WaitForSeconds(charDelay);
        }
    }
}
