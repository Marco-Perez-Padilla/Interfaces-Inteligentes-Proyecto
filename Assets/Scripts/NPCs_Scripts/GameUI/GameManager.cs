using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * @file: GameManager.cs
 * @brief: Gestor principal del juego implementado como Singleton. Controla el flujo del juego,
 * gestiona los paneles de UI (Game Over y Victoria), y proporciona funciones para reiniciar
 * nivel, cargar siguiente nivel o salir del juego.
 *
 * Notas: El tiempo del juego se pausa (Time.timeScale = 0) cuando se muestra Game Over o Victoria.
 * Solo puede haber una instancia de GameManager en la escena.
 */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject gameOverPanel;    // Panel de derrota
    public GameObject victoryPanel;     // Panel de victoria

    private bool gameEnded = false;

    // Implementación del patrón Singleton
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Inicialización: ocultar paneles y resetear tiempo
    void Start()
    {
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Activar pantalla de Game Over
    public void GameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Activar pantalla de Victoria
    public void Victory()
    {
        if (gameEnded) return;

        gameEnded = true;
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Reiniciar el nivel actual
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Cargar el siguiente nivel por nombre
    public void LoadNextLevel(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // Salir del juego
    public void QuitGame()
    {
        Application.Quit();
    }
}