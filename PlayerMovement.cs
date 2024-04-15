using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed;

    [SerializeField]
    private float maxSpeed;

    private Vector3 forceDirection;

    [SerializeField]
    private float jumpSpeed;

    InputSystem inputActions;
    InputAction move;

    //jump and gravity with character controller
    [SerializeField]
    private float gravity = -9.81f;
    private Vector3 playerVelocity;

    public bool isGrounded;
    private CharacterController conn;
    public bool isWalking;

    public AudioSource enemySfx;

    public Animator anim;

    public Camera playerCamera;

    public float rotationSpeed;

    public GameObject playerModel;

    public EnemyFollow enemyScript;
    public NavMeshAgent enemyObject;

    public GameObject pauseObject;
    public GameObject pivot;
    void Awake()
    {
        conn = GetComponent<CharacterController>();
        inputActions = new InputSystem();
        isWalking = false;
    }

    private void Start()
    {
        pauseObject.SetActive(false);
    }

    void OnEnable()
    {
        inputActions.Land.Jump.performed += OnJump;
        inputActions.Land.Pause.performed += OnGamePause;
        inputActions.PauseAction.Resume.performed += OnGameResume;
        move = inputActions.Land.Movement;
        conn.enabled = true;
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Land.Jump.performed -= OnJump;
        inputActions.Land.Pause.performed -= OnGamePause;
        inputActions.PauseAction.Resume.performed -= OnGameResume;
        conn.enabled = false;
        inputActions.Disable();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isGrounded)
        {
            float jumpVel = Mathf.Sqrt(jumpSpeed * -2 * gravity);
            playerVelocity.y = jumpVel;
            isGrounded = false;
            Invoke("groundCoroutine", 2f);
        }
    }

    void OnGamePause(InputAction.CallbackContext ctx)
    {
        if (!pauseObject.activeInHierarchy && ctx.performed)
        {
            movementSpeed = 0f;
            jumpSpeed = 0f;
            rotationSpeed = 0f;
            enemyScript.enabled = false;
            enemyObject.enabled = false;
            pauseObject.SetActive(true);
        }
    }

    void OnGameResume(InputAction.CallbackContext ctx)
    {
        if (pauseObject.activeInHierarchy && ctx.performed)
        {
            movementSpeed = 20f;
            jumpSpeed = 5f;
            rotationSpeed = 360f;
            enemyScript.enabled = true;
            enemyObject.enabled = true;
            pauseObject.SetActive(false);
        }
    }

    void groundCoroutine()
    {
        isGrounded = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void GameOver()
    {
        SceneManager.LoadScene(1);
    }

    void FixedUpdate()
    {

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementSpeed;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementSpeed;

        conn.SimpleMove(forceDirection * movementSpeed * Time.deltaTime);
        //rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        //anim.SetBool("isWalking", isWalking);

        anim.SetBool("isGrounded", conn.isGrounded);

        Vector3 horizontalVel = conn.velocity;
        horizontalVel.y = 0;

        //gravity of the player
        playerVelocity.y += gravity * Time.deltaTime;
        conn.Move(playerVelocity * Time.deltaTime);

        if (horizontalVel.sqrMagnitude > maxSpeed * maxSpeed)
        {
            isWalking = true;
            anim.SetBool("isWalking", true);
            //horizontalVel = horizontalVel.normalized * maxSpeed + Vector3.up * conn.velocity.y;
        }

        if (horizontalVel.magnitude == 0)
        {
            isWalking = false;
            anim.SetBool("isWalking", false);
        }

        Vector3 moveDirection = transform.right * move.ReadValue<Vector2>().x + transform.forward * move.ReadValue<Vector2>().y;
        moveDirection.Normalize();
        float magnitude = moveDirection.magnitude;
        magnitude = Mathf.Clamp01(magnitude);

        //moveCharacter(forceDirection);

        if (move.ReadValue<Vector2>().x != 0 || move.ReadValue<Vector2>().y != 0)
        {
            transform.rotation = Quaternion.Euler(0f, pivot.transform.rotation.eulerAngles.y, 0f);
            Quaternion rot = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));
            playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, rot, rotationSpeed * Time.deltaTime);

            //Quaternion toRotate = Quaternion.LookRotation(moveDirection, Vector3.up);
            //transform.rotation = Quaternion.RotateTowards(playerModel.transform.rotation, toRotate, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        isGrounded = hit.normal.y > 0.9f;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    /*void moveCharacter(Vector3 direction)
    {
        Vector3 offset = new Vector3(forceDirection.x * transform.position.x, forceDirection.y * transform.position.y, forceDirection.z * transform.position.z);
        rb.MovePosition(transform.position + (offset * movementSpeed * Time.deltaTime));
    }*/
}
