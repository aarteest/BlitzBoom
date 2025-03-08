using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_MM : MonoBehaviour
{
    [SerializeField] private string sceneName; // Scene to load, assign in the Inspector
    private Button button;


    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => LoadScene(sceneName));
        }
    }

	private void Start()
	{
		AudioManager.Instance?.PlayBackgroundMusic();

	}
	public void LoadScene(string scene)
    {
        if (!string.IsNullOrEmpty(scene))
        {
            SceneManager.LoadScene(scene);
        }
        else
        {
            Debug.LogError("Scene name is not assigned!");
        }
    }
}