using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections.Generic;

public class BossCarController : NetworkBehaviour
{
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public float detectionDistance = 5f;
    public float reactionDelay = 0.5f; // Delay before reacting to obstacles
    public float turnErrorMargin = 5f; // Adds randomness to turning accuracy
    public float speedIncrease = 2f;
    public float speedDecrease = 2f;
    public float abilityCooldown = 5f;

    private int currentWaypoint = 0;
    private Transform nearestPlayer;
    private float defaultSpeed;
    private float abilityTimer = 0f;
    private float obstacleReactionTimer = 0f;
    private bool reactingToObstacle = false;

    [SerializeField] private LayerMask obstacleDetectionLayer;
    [SerializeField] private GameObject obstaclePrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            agent.enabled = false; // Disable NavMeshAgent on clients
            return;
        }

        defaultSpeed = BossSpeedModifier.Instance != null ? BossSpeedModifier.Instance.GetBossSpeed() : agent.speed;
        agent.speed = defaultSpeed;
        agent.SetDestination(GetImperfectTarget(waypoints[currentWaypoint].position));



        //if (!IsServer) 
        //{
        //    agent.enabled = false; // Disable NavMeshAgent on clients
        //    return;
        //}

        //defaultSpeed = agent.speed;
        //agent.SetDestination(GetImperfectTarget(waypoints[currentWaypoint].position));
    }

    void Update()
    {
        if (!IsServer) return; // Only the server should control movement & logic

        

        MoveAlongWaypoints();
        DetectObstacles();
        AdjustSpeedBasedOnPlayer();
        HandleAbilities();
    }



    void MoveAlongWaypoints()
    {
        if (agent.remainingDistance < 2f) // Check if the boss reached the waypoint
        {
            currentWaypoint++;

            // If we reached the last waypoint, loop back to the first one
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = 0;
            }

            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }



    //void MoveAlongWaypoints()
    //{
    //    if (agent.remainingDistance < 10f)
    //    {
    //        if (currentWaypoint + 1 < waypoints.Length)
    //        {
    //            currentWaypoint++;
    //            agent.SetDestination(waypoints[currentWaypoint].position);
    //        }
    //    }
    //}

    void DetectObstacles()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, detectionDistance, obstacleDetectionLayer))
        {
            if (hit.collider.CompareTag("Obstacle") && !reactingToObstacle)
            {
                reactingToObstacle = true;
                obstacleReactionTimer = Time.time + reactionDelay;
            }
        }

        if (reactingToObstacle && Time.time >= obstacleReactionTimer)
        {
            reactingToObstacle = false;
            if (Random.value > 0.5f) // 50% chance to fail dodging
            {
                agent.speed = defaultSpeed * 0.7f; // Partially slows down
            }
        }
    }

    void AdjustSpeedBasedOnPlayer()
    {
        nearestPlayer = FindNearestPlayer();
        if (nearestPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, nearestPlayer.position);

            if (distance < 10f)
            {
                agent.speed = defaultSpeed + speedIncrease;
            }
            else
            {
                agent.speed = Mathf.Max(defaultSpeed - speedDecrease, 3f);
            }
        }
    }

    Transform FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = player.transform;
            }
        }
        return closest;
    }

    void HandleAbilities()
    {
        abilityTimer += Time.deltaTime;
        if (abilityTimer >= abilityCooldown)
        {
            UseBossAbility();
            abilityTimer = 0f;
        }
    }

    void UseBossAbility()
    {
        if (nearestPlayer == null) return;

        float distance = Vector3.Distance(transform.position, nearestPlayer.position);

        if (distance < 8f)
        {
            BossBoostClientRpc();
        }
        else if (distance < 15f)
        {
            DropObstacleServerRpc();
        }
    }

    [ClientRpc]
    void BossBoostClientRpc()
    {
        Debug.Log("Boss Boost Activated!");
        agent.speed += 5f;
        Invoke(nameof(ResetSpeed), 3f);
    }

    void ResetSpeed()
    {
        agent.speed = defaultSpeed;
    }

    [ServerRpc]
    void DropObstacleServerRpc()
    {
        GameObject obstacle = Instantiate(obstaclePrefab, transform.position + transform.forward * 2, Quaternion.identity);
        obstacle.GetComponent<NetworkObject>().Spawn();
    }

    Vector3 GetImperfectTarget(Vector3 targetPosition)
    {
        return targetPosition + new Vector3(
            Random.Range(-turnErrorMargin, turnErrorMargin),
            0,
            Random.Range(-turnErrorMargin, turnErrorMargin)
        );
    }
}












//using UnityEngine;
//using UnityEngine.AI;
//using System.Collections.Generic;


//public class BossCarController : MonoBehaviour
//{
//    public NavMeshAgent agent;
//    public Transform[] waypoints;
//    public float detectionDistance = 5f;
//    public float reactionDelay = 0.5f; // Delay before reacting to obstacles
//    public float turnErrorMargin = 5f; // Adds randomness to turning accuracy
//    public float speedIncrease = 2f;
//    public float speedDecrease = 2f;
//    public float abilityCooldown = 5f;

//    private int currentWaypoint = 0;
//    private Transform nearestPlayer;
//    private float defaultSpeed;
//    private float abilityTimer = 0f;
//    private float obstacleReactionTimer = 0f;
//    private bool reactingToObstacle = false;

//    [SerializeField]
//    private LayerMask obstacleDetectionLayer;

//    [SerializeField]
//    private GameObject obstaclePrefab;

//    void Start()
//    {
//        defaultSpeed = agent.speed;
//        agent.SetDestination(GetImperfectTarget(waypoints[currentWaypoint].position));
//    }

//    void Update()
//    {
//        MoveAlongWaypoints();
//        DetectObstacles();
//        AdjustSpeedBasedOnPlayer();
//        HandleAbilities();
//    }

//    void MoveAlongWaypoints()
//    {

//        Debug.Log(agent.remainingDistance);
//        if (agent.remainingDistance < 10f)
//        {

//            //currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
//            if (currentWaypoint + 1 < waypoints.Length)
//            {
//                currentWaypoint++;
//                agent.SetDestination(waypoints[currentWaypoint].position);

//            }
            
//        }
//    }

//    void DetectObstacles()
//    {
//        RaycastHit hit;
//        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, detectionDistance, obstacleDetectionLayer))
//        {
//            if (hit.collider.CompareTag("Obstacle") && !reactingToObstacle)
//            {
//                reactingToObstacle = true;
//                obstacleReactionTimer = Time.time + reactionDelay;
//            }
//        }

//        if (reactingToObstacle && Time.time >= obstacleReactionTimer)
//        {
//            reactingToObstacle = false;
//            if (Random.value > 0.5f) // 50% chance to fail dodging
//            {
//                agent.speed = defaultSpeed * 0.7f; // Partially slows down
//            }
//        }
//    }

//    void AdjustSpeedBasedOnPlayer()
//    {
//        nearestPlayer = FindNearestPlayer();
//        if (nearestPlayer != null)
//        {
//            float distance = Vector3.Distance(transform.position, nearestPlayer.position);

//            if (distance < 10f)
//            {
//                agent.speed = defaultSpeed + speedIncrease;
//                Debug.Log("Boss Speed Increased");
//            }            
//            else
//            {
//                agent.speed = Mathf.Max(defaultSpeed - speedDecrease, 3f);
//                Debug.Log("Boss Speed Decreased");
//            }
                


//        }
//    }

//    Transform FindNearestPlayer()
//    {
//        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
//        Transform closest = null;
//        float minDistance = Mathf.Infinity;

//        foreach (GameObject player in players)
//        {
//            float dist = Vector3.Distance(transform.position, player.transform.position);
//            if (dist < minDistance)
//            {
//                minDistance = dist;
//                closest = player.transform;
//            }
//        }
//        return closest;
//    }

//    void HandleAbilities()
//    {
//        abilityTimer += Time.deltaTime;
//        if (abilityTimer >= abilityCooldown)
//        {
//            UseBossAbility();
//            abilityTimer = 0f;
//        }
//    }

//    void UseBossAbility()
//    {
//        if (nearestPlayer == null) return;

//        float distance = Vector3.Distance(transform.position, nearestPlayer.position);

//        if (distance < 8f)
//        {
//            Debug.Log("Boss Boost Activated!");
//            agent.speed += 5f;
//            Invoke("ResetSpeed", 3f);
//        }
//        else if (distance < 15f)
//        {
//            Debug.Log("Boss Trap Activated!");
//            DropObstacle();
//        }
//    }

//    void ResetSpeed()
//    {
//        agent.speed = defaultSpeed;
//    }

//    void DropObstacle()
//    {
//        Instantiate(obstaclePrefab, transform.position + transform.forward * 2, Quaternion.identity);
//        Destroy(obstaclePrefab, 5f);
//    }

//    Vector3 GetImperfectTarget(Vector3 targetPosition)
//    {
//        // Add slight randomness to the turning accuracy
//        return targetPosition + new Vector3(
//            Random.Range(-turnErrorMargin, turnErrorMargin),
//            0,
//            Random.Range(-turnErrorMargin, turnErrorMargin)
//        );
//    }


//}