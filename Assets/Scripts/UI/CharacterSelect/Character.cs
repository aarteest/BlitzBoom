using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class Character : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private string carInfo = "Nice Big Car..Hmmm";
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject introPrefab;
    [SerializeField] private NetworkObject gameplayPrefab;
    [SerializeField] private GameObject victoryPrefab;
    public int playerScore;

    public int Id => id;
    public string DisplayName => displayName;

    public string CarInfo => carInfo;
    public Sprite Icon => icon;
    public GameObject IntroPrefab => introPrefab;
    public NetworkObject GameplayPrefab => gameplayPrefab;

    public GameObject VictoryPrefab => victoryPrefab;

    public void SetPlayerScore(int score)
    {
        playerScore = score;
    }

}
