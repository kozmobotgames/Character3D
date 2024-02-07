using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed;

    [SerializeField]
    private float maxSpeed;

    private Vector3 forceDirection;

    [SerializeField]
    private float jumpSpeed;

    private float velocityJump;

    [SerializeField]
    private float gravity;

    [SerializeField]
    private float gravityMultiplier = 1;

    InputSystem inputActions;
    InputAction move;

    private CharacterController conn;
    public bool isWalking;

    public AudioSource enemySfx;

    //public Animator anim;

    public Camera playerCamera;

    public float rotationSpeed;

    public GameObject playerModel;

    public EnemyFollow enemyScript;
    public NavMeshAgent enemyObject;

    public GameObject pauseObject;

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
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Land.Jump.performed -= OnJump;
        inputActions.Land.Pause.performed -= OnGamePause;
        inputActions.PauseAction.Resume.performed -= OnGameResume;
        inputActions.Disable();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        if (!IsGrounded()) return;
        velocityJump += jumpSpeed;

        if (ctx.performed)
        {
            if (IsGrounded() && velocityJump < 0.0f)
            {
                velocityJump = -1.0f;
            }
            else
            {
                velocityJump += velocityJump * gravityMultiplier * Time.deltaTime;
            }
        }
        forceDirection.y = velocityJump;
    }

    bool IsGrounded() => conn.isGrounded;

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
            movementSpeed = 4f;
            jumpSpeed = 8f;
            rotationSpeed = 360f;
            enemyScript.enabled = true;
            enemyObject.enabled = true;
            pauseObject.SetActive(false);
        }
    }

    /*void groundCoroutine()
    {
        IsGrounded();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            IsGrounded();
        }
    }*/

    void GameOver()
    {
        SceneManager.LoadScene(1);
    }

    void FixedUpdate()
    {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementSpeed;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementSpeed;

        conn.SimpleMove(forceDirection * movementSpeed * Time.deltaTime);
        //rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        //anim.SetBool("isWalking", isWalking);

        Vector3 horizontalVel = conn.velocity;
        horizontalVel.y = 0;
        if(horizontalVel.sqrMagnitude > maxSpeed * maxSpeed)
        {
            isWalking = true;
            //horizontalVel = horizontalVel.normalized * maxSpeed + Vector3.up * conn.velocity.y;
        }

        if (horizontalVel.magnitude == 0)
        {
            isWalking = false;
        }

        Vector3 moveDirection = new Vector3(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
        moveDirection.Normalize();
        float magnitude = moveDirection.magnitude;
        magnitude = Mathf.Clamp01(magnitude);

        //moveCharacter(forceDirection);

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotate = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(playerModel.transform.rotation, toRotate, rotationSpeed * Time.deltaTime);
        }
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
