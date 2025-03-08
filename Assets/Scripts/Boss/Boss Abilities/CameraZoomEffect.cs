using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomEffect : MonoBehaviour
{
	public CinemachineVirtualCamera virtualCamera;
	public Transform player; // Reference to the player's transform
	public Transform boss;   // Reference to the boss's transform
	public float normalFOV = 60f;
	public float zoomedFOV = 40f;
	public float zoomSpeed = 5f;
	public float zoomDistance = 20f; // Distance at which the zoom effect triggers

	private void Update()
	{
		// Calculate the distance between the player and the boss
		float distance = Vector3.Distance(player.position, boss.position);

		// Determine the target FOV based on the distance
		float targetFOV = (distance <= zoomDistance) ? zoomedFOV : normalFOV;

		// Smoothly transition the camera's FOV
		virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
	}
}
