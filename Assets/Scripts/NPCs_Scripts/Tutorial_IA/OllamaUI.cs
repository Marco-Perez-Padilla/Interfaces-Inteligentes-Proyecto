using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/**
 * @file: OllamaUI.cs
 * @brief: Gestiona la interacción con el LLM (Ollama) y la integración con Whisper para NPCs interactivos.
 *
 * Notas:
 * - Convierte las transcripciones de Whisper en prompts seguros para el LLM.
 * - Muestra la respuesta en un FloatingText3D con efecto máquina de escribir.
 * - Dispara eventos cuando se detectan palabras clave en la respuesta ("palanca").
 */
public class OllamaUI : MonoBehaviour
{
    // Auxiliar para serializar el texto
    [System.Serializable]
    private class SafeString
    {
        public string value;
    }

    [SerializeField] private string texto = "Dame la bienvenida y paso al tunel del terror. Indica que me monte en la vagoneta para empezar";

    [Header("Whisper UI Reference")]
    public WhisperUI whisperUI;        // Referencia al sistema Whisper

    [Header("LLM Settings")]
    private string littleLlmUrl = "http://gpu1.esit.ull.es:4000/v1/chat/completions";

    [Header("Output")]
    public FloatingText3D floatingText;  // Texto 3D para mostrar la respuesta

    public delegate void LLMEvent();
    public event LLMEvent OnPalancaDetected;   // Evento disparado si se menciona la palabra "palanca"

    [Header("Typewriter Settings")]
    public float charDelay = 0.05f;    // Velocidad de escritura del texto

    void OnEnable()
    {
        // Suscribirse a eventos de Whisper
        if (whisperUI != null) {
            whisperUI.OnWhisperTranscription += OnWhisperTranscribed;
            whisperUI.OnWhisperError += OnWhisperFailed;
        }
    }

    void OnDisable()
    {
        // Desuscribirse de eventos de Whisper
        if (whisperUI != null) {
            whisperUI.OnWhisperTranscription -= OnWhisperTranscribed;
            whisperUI.OnWhisperError -= OnWhisperFailed;
        }
    }

    // Eventos de Whisper
    private void OnWhisperFailed()
    {
        Debug.Log("Whisper falló, usando el texto por defecto.");
        StartCoroutine(DelayedSendFromWhisper(0.1f, texto));  // Enviar texto por defecto al LLM
    }

    private void OnWhisperTranscribed()
    {
        string prompt = whisperUI.outputText;
        StartCoroutine(DelayedSendFromWhisper(1.5f, prompt)); // Enviar transcripción al LLM tras pequeño delay
    }

    private IEnumerator DelayedSendFromWhisper(float delay, string prompt)
    {
        yield return new WaitForSeconds(delay);

        SafeString safe = new SafeString { value = prompt };
        string json = JsonUtility.ToJson(safe);
        string safePrompt = json.Substring(10, json.Length - 12);  // Extraer valor limpio del JSON

        string npcContext = "Eres un NPC de tutorial en un juego de aventura. "
                            + "Mantén siempre tu rol de NPC interactivo. "
                            + "Si el jugador dice 'vale', 'ok' o 'sí', explícale que debe tirar de la palanca de la vagoneta para empezar el recorrido. "
                            + "Si dice otra cosa, da la bienvenida al túnel del terror y asegúrate de incluir la palabra 'vagoneta'. "
                            + "No salgas nunca de tu rol y haz que tu respuesta sea coherente con la acción del jugador.";

        // Escapar caracteres problemáticos
        string escapedNpc = npcContext.Replace("\"", "\\\"").Replace("\n", "\\n");
        string escapedUser = prompt.Replace("\"", "\\\"").Replace("\n", "\\n");

        // Construir JSON seguro para el LLM
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

    // Corrutina que llama al LLM
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

            // Extraer contenido de la respuesta del LLM
            string marker = "\"content\":\"";
            int start = jsonResponse.IndexOf(marker);
            if (start != -1) {
                start += marker.Length;
                int end = jsonResponse.IndexOf("\"", start);
                string content = jsonResponse.Substring(start, end - start);

                content = content.Replace("\\n", "\n").Replace("\\\"", "\"");

                // Mostrar respuesta con efecto máquina de escribir
                if (floatingText != null)
                {
                    StartCoroutine(TypewriterEffect(content));
                }

                // Detectar palabra clave "palanca" y disparar evento
                if (content.ToLower().Contains("palanca"))
                {
                    Debug.Log("Evento LLM: 'palanca' detectada!");
                    OnPalancaDetected?.Invoke();
                }
            }
        }
    }

    // Efecto máquina de escribir
    private IEnumerator TypewriterEffect(string fullText)
    {
        floatingText.SetText("");  // Limpiar texto previo
        foreach (char c in fullText)
        {
            floatingText.SetText(floatingText.GetComponent<TextMeshPro>().text + c);
            yield return new WaitForSeconds(charDelay);
        }
    }
}
