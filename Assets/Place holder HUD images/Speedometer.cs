using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Speedometer : MonoBehaviour
{
	public TextMeshProUGUI speedText; // Assign in Inspector
	private Rigidbody rb;

	void Start()
	{
		if (!NetworkManager.Singleton.IsClient) // Ensure only clients run this
		{
			enabled = false;
			return;
		}

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players)
		{
			NetworkObject netObj = player.GetComponent<NetworkObject>();
			if (netObj != null && netObj.IsOwner)
			{
				rb = player.GetComponent<Rigidbody>();
				break;
			}
		}
	}

	void Update()
	{
		if (rb != null && speedText != null)
		{
			float speed = rb.velocity.magnitude;
			speedText.text = (speed * 3.6f).ToString("F1") + " km/h";
		}
	}
}
