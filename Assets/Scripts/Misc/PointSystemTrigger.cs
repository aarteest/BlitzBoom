//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PointSystemTrigger : MonoBehaviour
//{
//	private PointSystem pointSystem;

//	void Start()
//	{
//		pointSystem = GetComponent<PointSystem>();
//	}
//	void Update()
//	{
//		// Slipstream detection logic
//		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
//		bool isInSlipstream = false;

//		foreach (Collider col in hitColliders)
//		{
//			if (col.CompareTag("Player") || col.CompareTag("Boss"))
//			{
//				isInSlipstream = true;
//				break;
//			}
//		}

//		if (isInSlipstream)
//		{
//			pointSystem.EnterSlipstream();
//		}
//		else
//		{
//			pointSystem.ExitSlipstream();
//		}

//		// Near Miss Detection
//		GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

//		foreach (GameObject obstacle in obstacles)
//		{
//			float distance = Vector3.Distance(transform.position, obstacle.transform.position);

//			if (distance <= 1.5f) // Near miss range
//			{
//				pointSystem.OnNearMiss();
//				Debug.Log("Near miss detected with " + obstacle.name);
//			}
//			else if (distance > 0.5f && distance <= 2f) // Slightly safer dodge
//			{
//				pointSystem.OnDodgeObstacle();
//				Debug.Log("Dodged obstacle: " + obstacle.name);
//			}
//		}
//	}

//	void OnTriggerEnter(Collider other)
//	{
//		// Overtaking another player
//		if (other.CompareTag("Player"))
//		{
//			pointSystem.OnOvertakePlayer();
//		}

//		// Near miss with an obstacle or another player
//		if (other.CompareTag("Obstacle") || other.CompareTag("Player"))
//		{
//			float distance = Vector3.Distance(transform.position, other.transform.position);
//			if (distance <= 0.5f)
//			{
//				pointSystem.OnNearMiss();
//			}
//		}
//	}

//	void OnTriggerExit(Collider other)
//	{
//		// Dodging an obstacle (player goes near but doesn't collide)
//		if (other.CompareTag("Obstacle"))
//		{
//			pointSystem.OnDodgeObstacle();
//		}
//	}

//}
