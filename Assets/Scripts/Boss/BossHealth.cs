using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class BossHealth : NetworkBehaviour
{
    public NetworkVariable<int> BossHP = new NetworkVariable<int>(4, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private TextMeshProUGUI hpText;

    [SerializeField]
    private string sceneName; // Scene to load, assign in the Inspector
    public bool lastHitToBoss;
    [SerializeField]
    private GameObject winScreen;
    private void Start()
    {
        if (IsServer)
        {
            BossHP.Value = 4; // Initialize on server
        }

        // Find TMP text with the specific tag
        hpText = GameObject.FindGameObjectWithTag("BossHP")?.GetComponent<TextMeshProUGUI>();

        if (hpText != null)
        {
            hpText.text = "Boss HP: " + BossHP.Value;
        }

        
    }

    private void Update()
    {
        // Sync UI on all clients when HP changes
        BossHP.OnValueChanged += (oldValue, newValue) =>
        {
            if (hpText != null)
            {
                hpText.text = "Boss HP: " + newValue;
            }
        };

        if (lastHitToBoss == true)
        {
            //RaceManager.Singleton.EndRace();

            LoadScene(sceneName);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            //Debug.Log("Boss Collision");
            //hpText.text = "Boss HP: " + (BossHP.Value - 1);

            TakeDamage(1);
        }
    }
    private void TakeDamage(int amount)
    {
        if (BossHP.Value > 0)
        {
            BossHP.Value -= amount;

            if (BossHP.Value <= 0)
            {
                lastHitToBoss = true;
                Die();
                //winScreen.SetActive(true);
            }
        }
    }

    private void Die()
    {
        Debug.Log("Boss has died!");
        NetworkObject.Despawn(true);

        LoadScene(sceneName);
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