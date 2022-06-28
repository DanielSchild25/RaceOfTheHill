using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public Camera coCamera;
    public GameObject virtCamera;

    private GameManager gameManager;

    public float speed = 6f;

    public float turnSmoothTime = 0.1f;

    private float turnSmoothVelocity;

    public float gravity = -9.81f;

    public float jumpHeight = 3f;

    private Vector3 impact = Vector3.zero;

    public Transform groudCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private bool canMove = false;

    public GameObject goBody;
    bool hasBomb;
    public GameObject goBomb;
    public GameObject goPreviewBomb;

    Vector2 input;

    private Vector3 direction;

    Vector3 velocity;
    public bool isGrounded;

    List<Collider> checkpointsActivated = new List<Collider>();
    Vector3 checkpointLast = new Vector3(10, 20, 10);

    public TextMeshProUGUI countdown;


    private AudioSource playerAudio;
    public AudioClip itemPickupSound;
    public AudioClip explosionSound;

    // Start is called before the first frame update
    void Start()
    {
        playerAudio = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        int layer = 20 + FindObjectsOfType<ThirdPersonMovement>().Length;
        virtCamera.layer = layer;
        var mask = 0b1111111111 | (1 << layer) | 1 << (layer - 10);
        coCamera.cullingMask = mask;
        coCamera.gameObject.layer = layer;
        virtCamera.GetComponent<Cinemachine.CinemachineInputProvider>().PlayerIndex = GetComponent<PlayerInput>().playerIndex;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        transform.parent.gameObject.name = $"Player {gameManager.playerCount+1}";
        gameManager.PlayerJoins(transform.parent.gameObject.name);
        canMove = gameManager.gameHasStarted;
        if (!canMove && gameManager.countdownIsRunning)
        {
            StartCountdown();
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        input = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!isGrounded || !canMove) return;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void OnUse(InputAction.CallbackContext ctx)
    {
        if (hasBomb)
        {
            var go = Instantiate(goBomb, transform.position + transform.forward * 3, Quaternion.identity);
            go.GetComponent<Rigidbody>().AddForce(transform.forward * 500, ForceMode.Impulse);
            goBody.GetComponent<Animator>().SetBool("HasItem", false);
            goPreviewBomb.SetActive(false);
            hasBomb = false;
        }
    }

    public void OnStartGame(InputAction.CallbackContext ctx)
    {
        if(gameManager != null && !gameManager.gameHasStarted)
            gameManager.StartGame();
        if (gameManager != null && gameManager.gamefinished)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.gameObject.CompareTag("Player")) return;
        AddImpact(transform.position - hit.transform.position, 20);
        hit.gameObject.GetComponent<ThirdPersonMovement>().AddImpact(hit.transform.position - transform.position, 20);
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = input.x; //Input.GetAxisRaw("Horizontal");
        float vertical = input.y; //Input.GetAxisRaw("Vertical");

        direction = new Vector3(horizontal, 0, vertical).normalized;

        isGrounded = Physics.CheckSphere(groudCheck.position, groundDistance, groundMask);

        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = gravity;
        }

        /*if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumgHeight * -2f * gravity);
        }*/

        controller.Move(velocity * Time.deltaTime);


        if (direction.magnitude >= 0.1f && canMove)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);

        }

        // apply the impact force:
        if (impact.magnitude > 0.2F)
        {
            controller.Move(impact * Time.deltaTime);
        }
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, Time.deltaTime * (isGrounded ? 5 : 1));
    }

    private void LateUpdate()
    {
        var animator = goBody.GetComponent<Animator>();
        animator.SetBool("OnGround", isGrounded);
        animator.SetFloat("Velocity Horizontal", direction.magnitude * speed / 20);
        animator.SetFloat("Velocity Vertical", velocity.y);
    }
    private void FixedUpdate()
    {
        if (transform.position.y < 0) transform.position = checkpointLast + Vector3.up * 5;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishRadius"))
        {
            gameManager.FinishedGame(transform.parent.gameObject.name);
            canMove = false;
            return;
        }

        if (other.CompareTag("Item_SpeedBoost"))
        {
            
            speed *= 2;
            Destroy(other.gameObject);
            StartCoroutine(Item_SpeedBoostCountdownRoutine());
            playerAudio.PlayOneShot(itemPickupSound, 1);
        }

        if (other.CompareTag("Item_Invisible"))
        {
            
            StartCoroutine(Item_InvisibleRoutine());
            Destroy(other.gameObject);
            playerAudio.PlayOneShot(itemPickupSound, 1);
        }

        if (other.CompareTag("Item_JumpBoost"))
        {
            
            jumpHeight *= 2;
            Destroy(other.gameObject);
            StartCoroutine(Item_JumpBoostCountdownRoutine());
            playerAudio.PlayOneShot(itemPickupSound, 1);
        }

        if (other.CompareTag("Item_Bomb"))
        {
            
            hasBomb = true;
            goBody.GetComponent<Animator>().SetBool("HasItem", true);
            goPreviewBomb.SetActive(true);
            Destroy(other.gameObject);
            playerAudio.PlayOneShot(itemPickupSound, 1);
        }

        if (other.CompareTag("Landmine"))
        {
            
            Vector3 awayFormLandmine = (transform.position - other.gameObject.transform.position);
            awayFormLandmine.y = 1;
            awayFormLandmine.Normalize();
            AddImpact(awayFormLandmine, 75);

            Destroy(other.gameObject);
            playerAudio.PlayOneShot(explosionSound, 1);
        }

        if (other.CompareTag("Bomb"))
        {
            
            Vector3 awayFromBomb = (transform.position - other.gameObject.transform.position);
            awayFromBomb.y = 1;
            awayFromBomb.Normalize();
            AddImpact(awayFromBomb, 50);
            playerAudio.PlayOneShot(explosionSound, 1);
        }

        if (!other.CompareTag("Checkpoint")) return;
        if (checkpointsActivated.Contains(other)) return;

        checkpointsActivated.Add(other);
        checkpointLast = other.transform.position;
    }

    void SetLayerOfChildren(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform) SetLayerOfChildren(child.gameObject, layer);
    }

    IEnumerator Item_InvisibleRoutine()
    {
        SetLayerOfChildren(goBody, virtCamera.layer);
        yield return new WaitForSeconds(10);
        SetLayerOfChildren(goBody, 0);
    }

    IEnumerator Item_SpeedBoostCountdownRoutine()
    {
        yield return new WaitForSeconds(10);
        speed /= 2;
    }

    IEnumerator Item_JumpBoostCountdownRoutine()
    {
        yield return new WaitForSeconds(10);
        jumpHeight /= 2;
    }

    // call this function to add an impact force:
    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force;
    }

    public void StartCountdown()
    {
        canMove = false;
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        var count = 3;
        while (true && count > 0)
        {
            countdown.text = count.ToString();
            count--;

            yield return new WaitForSeconds(1);
        }
        countdown.text = "GO";
        canMove = true;
        yield return new WaitForSeconds(1);

        countdown.text = "";
    }
}
