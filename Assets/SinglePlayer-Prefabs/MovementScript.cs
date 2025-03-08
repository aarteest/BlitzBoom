using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class MovementScript : NetworkBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 50f;
    private Rigidbody rb;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Camera mainCam;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        Debug.Log($"IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");

        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();

        if (!IsOwner) return;

        if (IsOwner)
        {
            if (brain != null)
            {

                virtualCamera.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Cinemachine Brain not found on the main camera.");
            }
        }
        
        
        
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Get input locally
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (moveInput != 0 || turnInput != 0)
        {
            Debug.Log($"Client Attempting ServerRpc: moveInput={moveInput}, turnInput={turnInput}");
            SubmitMovementServerRpc(moveInput, turnInput);
        }
    }

    [ServerRpc(RequireOwnership = false)] // Allow any client to call this ServerRpc
    public void SubmitMovementServerRpc(float moveInput, float turnInput)
    {
        Debug.Log($"ServerRpc called: moveInput={moveInput}, turnInput={turnInput}");

        // Optional: Call movement handling
        HandleMovement(moveInput, turnInput);
    }

    private void HandleMovement(float moveInput, float turnInput)
    {
        if (rb == null) return;

        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
        rb.AddForce(moveForce, ForceMode.Force);

        float turnForce = turnInput * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0f, turnForce, 0f);

        Debug.Log("HandleMovement executed.");

        
    }
}