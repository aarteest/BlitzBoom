using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class JankyCarControl_Multiplayer : NetworkBehaviour
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

    [Header("Lap Settings")]
    private int finishedLapCount = 0;

	[Header("Audio")]
	[SerializeField] private AudioSource accelerationSFX;
	[SerializeField] private AudioSource idleSFX;

	private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;


	public Transform bombHolder; // Assign this in the Inspector

    [SerializeField] private ParticleSystem explosionVFX;


    private void Start()
    {
        carRB = GetComponent<Rigidbody>();

        Debug.Log($"IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");

        if (!IsOwner) return;

		if (bombHolder == null)
		{
			Debug.LogError("BombHolder not assigned on " + gameObject.name);
		}

	}

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();

        //if (!IsOwner || finishedLapCount >= CheckpointManager.instance.totalLaps) return;
        if (!IsOwner) return;

        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");
        float controllerMoveInput = Input.GetAxis("AccelerationAxis");
        float controllerTurnInput = Input.GetAxis("TurningAxis");

        // Play acceleration sound if moving, otherwise play idle sound
        if (moveInput != 0)
		{
			if (!accelerationSFX.isPlaying)
			{
				accelerationSFX.Play();
				idleSFX.Stop();
			}
		}
		else
		{
			if (!idleSFX.isPlaying)
			{
				idleSFX.Play();
				accelerationSFX.Stop();
			}
		}

		if (moveInput != 0 || turnInput != 0 || controllerMoveInput != 0 || controllerTurnInput != 0)
        {
            SubmitMovementServerRpc(moveInput, turnInput, controllerMoveInput, controllerTurnInput);
        }



    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitMovementServerRpc(float moveInput, float turnInput, float controllerMoveInput, float controllerTurnInput)
    {
        if (isGrounded)
        {
            Acceleration(moveInput);
            Deceleration();
            Turn(turnInput);
            SideWaysDrag();


            ControllerAcceleration(controllerMoveInput);
            ControllerDeceleration();
            ControllerTurn(controllerTurnInput);
            ControllerSideWaysDrag();
        }
        

    }



    private void Acceleration(float moveInput)
    {
        if (currentCarLocalVelocity.z < maxSpeed)
        {
            carRB.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);

        }
        //Debug.Log(currentCarLocalVelocity.z);
    }

    private void Deceleration()
    {
        carRB.AddForce((Input.GetKey(KeyCode.Space) ? brakingDeceleration : deceleration) * carVelocityRatio * -carRB.transform.forward, ForceMode.Acceleration);
    }

    private void Turn(float turnInput)
    {
        carRB.AddTorque(steerStrength * turnInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
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

        if (tempGroundWheels > 1)
        {
            isGrounded = true;
        }

        else
        {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.velocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
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

    public void StopCar()
    {
        carRB.velocity = Vector3.zero;
    }

    public void FinishCurrentLap(int points)
    {
        finishedLapCount++;

		if (TryGetComponent<PlayerBombTrackerPointSystem>(out PlayerBombTrackerPointSystem pointSystem))
        {
            //pointSystem.AddToTotalPointsServerRpc(points);
            pointSystem.AddToTotalPointsServerRpc(points, pointSystem.OwnerClientId);

            if (finishedLapCount >= CheckpointManager.instance.totalLaps)
            {
                CheckpointManager.instance.AddPlayerToLeaderboard(pointSystem, pointSystem.totalPoints.Value);
                pointSystem.UpdateFinalCharacterScore();
            }
            Debug.Log($"Gained {points} points");
        }
    }

    public int GetFinishedLapCount()
    {
        return finishedLapCount;
    }



    //Controller Inputs////////////

    private void ControllerAcceleration(float controllerMoveInput)
    {
        if (currentCarLocalVelocity.z < maxSpeed)
        {
            carRB.AddForceAtPosition(acceleration * controllerMoveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);

        }
        //Debug.Log(currentCarLocalVelocity.z);
    }

    //private void ControllerDeceleration()
    //{
    //    carRB.AddForce((Input.GetAxis("AccelerationAxis") ? brakingDeceleration : deceleration) * carVelocityRatio * -carRB.transform.forward, ForceMode.Acceleration);
    //}

    private void ControllerDeceleration()
    {
        float brakeInput = Input.GetAxis("BreakingAxis"); // Get the brake input value

        // Determine whether to apply braking or normal deceleration based on input
        float decelerationForce = (brakeInput > 0.1f) ? brakingDeceleration : deceleration;

        carRB.AddForce(decelerationForce * carVelocityRatio * -carRB.transform.forward, ForceMode.Acceleration);
    }


    private void ControllerTurn(float controllerTurnInput)
    {
        carRB.AddTorque(steerStrength * controllerTurnInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    //private void ControllerSideWaysDrag()
    //{
    //    float currentSidewaysSpeed = currentCarLocalVelocity.x;

    //    float dragMagnitude = -currentSidewaysSpeed * (Input.GetAxis("BreakingAxis") ? brakingDragCoefficient : dragCoefficient);

    //    Vector3 dragForce = transform.right * dragMagnitude;

    //    carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    //}

    private void ControllerSideWaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;
        float brakeInput = Input.GetAxis("BreakingAxis"); // Get the brake input value

        // Determine the drag coefficient based on braking input
        float appliedDragCoefficient = (brakeInput > 0.1f) ? brakingDragCoefficient : dragCoefficient;

        float dragMagnitude = -currentSidewaysSpeed * appliedDragCoefficient;
        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }





}

