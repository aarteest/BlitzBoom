using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class VritraMovement : NetworkBehaviour
{
    public Transform[] waypoints;
    private NetworkVariable<int> currentWaypointIndex = new NetworkVariable<int>(0);

    public float speed = 200f;
    public float proximityThreshold = 2f;
    public bool loopPath = true;

    public Transform[] players;
    public float cycloneActivationRange = 200f;
    public float debrisActivationRange = 300f;

    public GameObject cyclonePrefab;
    public Transform cycloneSpawnPoint;

    private Transform followingPlayer = null;
    private float followTime = 0f;
    public float speedIncreaseAmount = 50f;
    private bool speedBoosted = false;

    public int hitsRemainingToDefeat = 3;
    private NetworkVariable<int> hitsLeft = new NetworkVariable<int>(4);
    public TextMeshProUGUI bossHitsText;

    private GameObject spawnedCyclone;

    private DebrisBossAbility debrisSpawner;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }

        bossHitsText.text = "Hits Left: " + hitsLeft.Value;
    }

    void Update()
    {
        if (!IsServer) return;

        MoveTowardsWaypoint();
        CheckCycloneActivation();
        CheckDebrisActivation();
        TrackPlayerFollowing();
    }

    void MoveTowardsWaypoint()
    {
        if (waypoints.Length == 0) return;

        Vector3 direction = (waypoints[currentWaypointIndex.Value].position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex.Value].position) < proximityThreshold)
        {
            GetNextWaypoint();
        }
    }

    void GetNextWaypoint()
    {
        if (!IsServer) return;

        currentWaypointIndex.Value++;

        if (currentWaypointIndex.Value >= waypoints.Length)
        {
            currentWaypointIndex.Value = loopPath ? 0 : waypoints.Length - 1;
        }
    }

    void CheckCycloneActivation()
    {
        foreach (var player in players)
        {
            if (Vector3.Distance(player.position, transform.position) <= cycloneActivationRange)
            {
                ActivateCycloneServerRpc();
                return;
            }
        }
    }

    [ServerRpc]
    void ActivateCycloneServerRpc()
    {
        if (spawnedCyclone == null && cyclonePrefab != null && cycloneSpawnPoint != null)
        {
            spawnedCyclone = Instantiate(cyclonePrefab, cycloneSpawnPoint.position, Quaternion.identity);
            spawnedCyclone.GetComponent<NetworkObject>().Spawn();
        }
    }

    void CheckDebrisActivation()
    {
        foreach (var player in players)
        {
            if (Vector3.Distance(player.position, transform.position) <= debrisActivationRange)
            {
                SpawnDebrisServerRpc();
                return;
            }
        }
    }

    [ServerRpc]
    void SpawnDebrisServerRpc()
    {
        if (debrisSpawner != null)
        {
            debrisSpawner.SpawnDebris(transform.position, transform.forward);
        }
    }

    void TrackPlayerFollowing()
    {
        foreach (var player in players)
        {
            Vector3 toPlayer = player.position - transform.position;
            if (toPlayer.magnitude < 100f && Vector3.Dot(transform.forward, toPlayer.normalized) < -0.5f)
            {
                if (followingPlayer == player)
                {
                    followTime += Time.deltaTime;
                }
                else
                {
                    followingPlayer = player;
                    followTime = 0f;
                }

                if (followTime >= 5f && !speedBoosted)
                {
                    IncreaseSpeedServerRpc();
                    speedBoosted = true;
                }

                return;
            }
        }

        followingPlayer = null;
        followTime = 0f;
    }

    [ServerRpc]
    void IncreaseSpeedServerRpc()
    {
        speed += speedIncreaseAmount;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc()
    {
        if (!IsServer) return;

        hitsLeft.Value--;
        UpdateHitsClientRpc(hitsLeft.Value);

        if (hitsLeft.Value <= 0)
        {
            Destroy(gameObject);
            GetComponent<NetworkObject>().Despawn();
        }
    }

    [ClientRpc]
    void UpdateHitsClientRpc(int newHitsLeft)
    {
        bossHitsText.text = "Hits Left: " + newHitsLeft;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.collider.CompareTag("Player"))
        {
            TakeDamageServerRpc();
        }
    }
}











//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using TMPro;
//using Unity.Netcode;

//public class VritraMovement : NetworkBehaviour
//{
//	// Reference to the DebrisSpawner script
//	public DebrisBossAbility debrisSpawner; // Reference to the DebrisSpawner

//	// Reference to the GameSceneManager
//	//private GameSceneManager gameSceneManager;


//	// Waypoints
//	public Transform[] waypoints;
//	private int currentWaypointIndex = 0;
//	// Collider behind the boss for triggering debris
//	[SerializeField]
//	private Collider debrisTrigger;

//	// Movement Variables
//	public float speed = 200f; // Base speed
//	public float proximityThreshold = 2f; // Distance to switch to next waypoint

//	// Behavior Variables
//	public bool loopPath = true; // Whether to loop the path
//	public Transform[] players; // Reference to players for dynamic behavior
//	public float cycloneActivationRange = 200f; // Range for cyclone activation

//	public float debrisActivationRange = 300f; // Range for cyclone activation

//	// Cyclone Ability
//	public GameObject cyclonePrefab; // Cyclone prefab
//	public Transform cycloneSpawnPoint; // Optional: Where the cyclone spawns

//	// Tracking player behind boss
//	private Transform followingPlayer = null;
//	private float followTime = 0f;
//	public float speedIncreaseAmount = 50f; // Increase in speed when followed for 5s
//	private bool speedBoosted = false; // To prevent multiple boosts

//	// Boss Health
//	public int hitsRemainingToDefeat = 3; // Number of hits required to defeat the boss

//	// Add a cooldown timer to prevent rapid scene changes
//	private bool isProcessingHit = false;
//	private float hitCooldown = 0.5f; // Adjust the cooldown time as needed

//	// Reference to the UI Text
//	public TextMeshProUGUI bossHitsText;
//	private int hitsLeft = 4;
//	// Cyclone Spawning
//	private GameObject spawnedCyclone;


//	void Start()
//	{
//		// Start at the first waypoint
//		if (waypoints.Length > 0)
//		{
//			transform.position = waypoints[0].position;
//		}

//		// Set up the collider for the debris trigger (child of the boss)
//		debrisTrigger = transform.Find("DebrisTrigger")?.GetComponent<Collider>();
//		if (debrisTrigger != null)
//		{
//			debrisTrigger.isTrigger = true;  // Ensure it's set to trigger
//		}

//        bossHitsText.text = "Hits Left :" + hitsLeft;

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        // COMMENTED THIS FOR NO ERROR


//        // Get reference to the GameSceneManager
//        //gameSceneManager = FindObjectOfType<GameSceneManager>();
//        //if (gameSceneManager == null)
//        //{
//        //	Debug.LogError("GameSceneManager not found in the scene!");
//        //}
//    }

//    void Update()
//	{
//		if (waypoints.Length == 0) return;

//		// Move towards the current waypoint
//		MoveTowardsWaypoint();

//		// Check if close enough to the current waypoint to switch to the next
//		if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < proximityThreshold)
//		{
//			GetNextWaypoint();
//		}

//		// Check if a player is following behind for 5 seconds
//		TrackPlayerFollowing();

//		// Check conditions for activating the cyclone ability
//		CheckCycloneActivation();

//		CheckDebrisActivation();

//		// Check if the debris trigger should activate
//		//CheckDebrisTrigger();
//	}

//	void MoveTowardsWaypoint()
//	{
//		// Get direction to the current waypoint
//		Vector3 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;

//		// Move in the direction of the waypoint
//		transform.position += direction * speed * Time.deltaTime;

//		// Rotate towards the waypoint (optional, for visual alignment)
//		Quaternion targetRotation = Quaternion.LookRotation(direction);
//		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
//	}

//	void GetNextWaypoint()
//	{
//		// Advance to the next waypoint
//		currentWaypointIndex++;

//		// Loop back to the start if at the end of the path
//		if (currentWaypointIndex >= waypoints.Length)
//		{
//			if (loopPath)
//			{
//				currentWaypointIndex = 0;
//			}
//			else
//			{
//				currentWaypointIndex--; // Stop at the last waypoint
//			}
//		}
//	}

//	// Check if any player is in range to activate the cyclone
//	void CheckCycloneActivation()
//	{
//		foreach (var player in players)
//		{
//			float distance = Vector3.Distance(player.position, transform.position);

//			// Primary Condition: Player within range
//			if (distance <= cycloneActivationRange)
//			{
//				ActivateCyclone();
//				return;
//			}
//		}
//	}

//	void ActivateCyclone()
//	{
//		// Log when the ability is called
//		//Debug.Log("Cyclone ability called!");

//		// Check if cyclonePrefab and cycloneSpawnPoint are assigned correctly
//		if (cyclonePrefab == null)
//		{
//			Debug.LogWarning("Cyclone prefab is missing!");
//		}

//		if (cycloneSpawnPoint == null)
//		{
//			Debug.LogWarning("Cyclone spawn point is missing!");
//		}

//		// Check if the ability can be activated
//		if (cyclonePrefab != null && cycloneSpawnPoint != null)
//		{
//			if (spawnedCyclone == null)
//			{
//				// Debug log moved inside the condition
//				//Debug.Log("Cyclone prefab exists. Spawning cyclone at: " + cycloneSpawnPoint.position);
//				spawnedCyclone = Instantiate(cyclonePrefab, cycloneSpawnPoint.position, Quaternion.identity);
//			}
//			else
//			{
//				//Debug.Log("Cyclone Already Spawned");
//			}
//		}
//		else
//		{
//			Debug.Log("Cyclone not activated. Check conditions.");
//		}
//	}

//	void CheckDebrisActivation()
//	{
//		foreach (var player in players)
//		{
//			float distance = Vector3.Distance(player.position, transform.position);
			
//			// Primary Condition: Player within range
//			if (distance <= debrisActivationRange)
//			{
//				debrisSpawner.SpawnDebris(transform.position, transform.forward);
//				return;
//			}
//		}
//	}
	

//	void TrackPlayerFollowing()
//	{
//		foreach (var player in players)
//		{
//			Vector3 toPlayer = player.position - transform.position;
//			float distance = toPlayer.magnitude;

//			// Check if player is behind the boss within a certain range
//			if (distance < 100f && Vector3.Dot(transform.forward, toPlayer.normalized) < -0.5f)
//			{
//				if (followingPlayer == player)
//				{
//					followTime += Time.deltaTime;
//				}
//				else
//				{
//					followingPlayer = player;
//					followTime = 0f; // Reset timer for a new player
//				}

//				if (followTime >= 5f && !speedBoosted)
//				{
//					speed += speedIncreaseAmount;
//					speedBoosted = true;
//					Debug.Log("Boss speed increased because player followed for 5s!");
//				}

//				return;
//			}
//		}

//		// Reset tracking if no player is behind
//		followingPlayer = null;
//		followTime = 0f;
//	}

//	//Chaos Ability Trigger
//	public void TriggerChaosAbility(GameObject targetCar)
//	{
//		if (targetCar.TryGetComponent(out ChaosAbility chaosAbility))
//		{
//			chaosAbility.ActivateChaos();
//		}
//	}


//    private void OnCollisionEnter(Collision collision)
//    {
//        if(collision.collider.CompareTag("Player"))
//		{

//			hitsLeft = hitsLeft - 1;
//			Debug.Log(hitsLeft); 
//            bossHitsText.text = "Hits Left :" + hitsLeft;
//            if (hitsLeft == 0)
//			{
//				Destroy(gameObject);
//			}
//		}
//    }





    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // COMMENTED THIS FOR NO ERROR


    // Handle collisions
    //private void OnCollisionEnter(Collision collision)
    //{
    //	// Check if the collision is with a multiplayer or singleplayer car
    //	ActualFloatingCar multiplayerCar = collision.gameObject.GetComponent<ActualFloatingCar>();
    //	SinglePlayerCarController singleplayerCar = collision.gameObject.GetComponent<SinglePlayerCarController>();

    //	// Process the hit only if a valid car hits the boss and no cooldown is active
    //	if (!isProcessingHit && (multiplayerCar != null || singleplayerCar != null))
    //	{
    //		StartCoroutine(HandleHit());
    //	}



    //}

    //private IEnumerator HandleHit()
    //{
    //	isProcessingHit = true; // Set the cooldown flag
    //	//Debug.Log("Player dashed into the boss!");

    //	// Only decrement and update if there are hits remaining
    //	if (hitsRemainingToDefeat > 0)
    //	{
    //		// Reduce hits remaining
    //		hitsRemainingToDefeat--;

    //		// Update the UI text
    //		UpdateBossHitsText();

    //		// Check if boss is defeated
    //		if (hitsRemainingToDefeat == 0)
    //		{
    //			//Debug.Log("Boss defeated!");

    //			// Use GameSceneManager to load the next scene
    //			if (gameSceneManager != null)
    //			{
    //				gameSceneManager.LoadSpecificScene(1); // Replace 1 with the index of the desired scene
    //			}
    //			else
    //			{
    //				Debug.LogError("GameSceneManager not assigned!");
    //			}
    //		}
    //	}

    //	// Wait for the cooldown time before allowing another hit
    //	yield return new WaitForSeconds(hitCooldown);
    //	isProcessingHit = false; // Reset the cooldown flag
    //							 // Updates the UI text component with the current hits remaining
    //	void UpdateBossHitsText()
    //	{
    //		if (bossHitsText != null)
    //		{
    //			
	//			bossHitsText.text = "Boss Hits left to change Scene: " + hitsRemainingToDefeat;

    //		}
    //	}
    //}



//}
