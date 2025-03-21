using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class JankyCarController_MP : NetworkBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody carRB;
	[SerializeField] private Transform[] rayPoints;
	[SerializeField] private LayerMask drivable;
	[SerializeField] private Transform accelerationPoint;

	[Header("Suspension Settings")]
	[SerializeField] private float springStiffness;
	[SerializeField] private float damperStiffness;
	[SerializeField] private float restLength;
	[SerializeField] private float springTravel;
	[SerializeField] private float wheelRadius;

	private int[] wheelIsGrounded = new int[4];
	private bool isGrounded = false;

	[Header("Input")]
	private float moveInput = 0;
	private float steerInput = 0;

	[Header("Car Settings")]
	[SerializeField] private float acceleration = 25f;
	public float maxSpeed = 100f;
	[SerializeField] private float deceleration = 10f;
	[SerializeField] private float steerStrength = 15f;
	[SerializeField] private AnimationCurve turningCurve;
	[SerializeField] private float dragCoefficient = 1f;
	[SerializeField] private float brakingDeceleration = 100f;
	[SerializeField] private float brakingDragCoefficient = 0.5f;

	private Vector3 currentCarLocalVelocity = Vector3.zero;
	private float carVelocityRatio = 0;

	private void Start()
	{
		carRB = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (!IsOwner) return;

		Suspension();
		GroundCheck();
		CalculateCarVelocity();
		Movement();
	}

	private void Update()
	{
		if (!IsOwner) return;

		GetPlayerInput();
	}

	private void Movement()
	{
		if (isGrounded)
		{
			Acceleration();
			Deceleration();
			Turn();
			SideWaysDrag();
		}
	}

	private void Acceleration()
	{
		if (currentCarLocalVelocity.z < maxSpeed)
		{
			carRB.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
		}
	}

	private void Deceleration()
	{
		carRB.AddForce((Input.GetKey(KeyCode.Space) ? brakingDeceleration : deceleration) * carVelocityRatio * -carRB.transform.forward, ForceMode.Acceleration);
	}

	private void Turn()
	{
		carRB.AddTorque(steerStrength * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
	}

	private void SideWaysDrag()
	{
		float currentSidewaysSpeed = currentCarLocalVelocity.x;
		float dragMagnitude = -currentSidewaysSpeed * (Input.GetKey(KeyCode.Space) ? brakingDragCoefficient : dragCoefficient);
		Vector3 dragForce = transform.right * dragMagnitude;
		carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
	}

	private void GroundCheck()
	{
		int tempGroundWheels = 0;
		for (int i = 0; i < wheelIsGrounded.Length; i++)
		{
			tempGroundWheels += wheelIsGrounded[i];
		}
		isGrounded = tempGroundWheels > 1;
	}

	private void CalculateCarVelocity()
	{
		currentCarLocalVelocity = transform.InverseTransformDirection(carRB.velocity);
		carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
	}

	private void GetPlayerInput()
	{
		moveInput = Input.GetAxis("Vertical");
		steerInput = Input.GetAxis("Horizontal");
		UpdateServerMovementServerRpc(moveInput, steerInput);
	}

	[ServerRpc]
	private void UpdateServerMovementServerRpc(float move, float steer)
	{
		moveInput = move;
		steerInput = steer;
	}

	private void Suspension()
	{
		for (int i = 0; i < rayPoints.Length; i++)
		{
			RaycastHit hit;
			float maxLength = restLength + springTravel;

			if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, drivable))
			{
				wheelIsGrounded[i] = 1;
				float currentSpringLength = hit.distance - wheelRadius;
				float springCompression = (restLength - currentSpringLength) / springTravel;
				float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
				float dampForce = damperStiffness * springVelocity;
				float springForce = springStiffness * springCompression;
				float netForce = springForce - dampForce;
				carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);
				Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
			}
			else
			{
				wheelIsGrounded[i] = 0;
				Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.green);
			}
		}
	}
}
