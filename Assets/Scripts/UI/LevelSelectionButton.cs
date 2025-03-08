using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionButton : MonoBehaviour
{


    [SerializeField] private string levelName; // Assign this in Inspector
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogError("Button component missing on " + gameObject.name);
        }
    }

    public void OnClick()
    {
        if (HostManager.Instance == null)
        {
            Debug.LogError("HostManager.Instance is NULL!");
            return;
        }

        HostManager.Instance.gameplaySceneName = levelName;
        Debug.Log("Gameplay scene set to: " + levelName);
    }









    //[SerializeField]
    //private string levelName; // Assign the level name in the Inspector



    //public void OnClick()
    //{
    //    HostManager.Instance.gameplaySceneName = levelName;
    //    Debug.Log("HostManager.Instance.gameplaySceneName");
    //}



}