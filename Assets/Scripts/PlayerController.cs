using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class animationStateControler : MonoBehaviour
{
    public Animator animator;
    public PlayerInput playerInput;

    [SerializeField]
    private float translationSpeed;

    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float slideSpeed;
    private float currentSpeed = 0;

    public float jumpForce = 5.0f; // Force du saut
    public float maxJumpHeight = 2.0f; // Hauteur maximale du saut
    public float gravity = 9.81f; // Gravité

    private bool isJumping = false;
    private float initialYPosition;

    private const float MIN_X = -2f;
    private const float MAX_X = 4f;

    private Rigidbody rb;

    void Awake()
    {
        playerInput = new PlayerInput();
        animator = GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>(); 

    }

    // Start is called before the first frame update
    void Start()
    {
        initialYPosition = rb.position.y;
    }

    // Update is called once per frame
    void Update()
    {

        float moveInput = playerInput.CharacterControl.Move.ReadValue<float>();
        float horizontalMove = moveInput * translationSpeed * Time.deltaTime;

        if (playerInput.CharacterControl.LaunchRun.triggered)
        {
            animator.SetBool("isRunning", true);
            currentSpeed = moveSpeed;
        }

        if (playerInput.CharacterControl.Slide.triggered)
        {
            animator.SetBool("isSliding", true);
            currentSpeed = slideSpeed;
        }
        if (!playerInput.CharacterControl.Slide.triggered)
        {
            animator.SetBool("isSliding", false);
            currentSpeed = moveSpeed;
        }

        if (playerInput.CharacterControl.Jump.triggered && !isJumping)
        {
            animator.SetBool("reachedHeight", false);
            animator.SetBool("isJumping", true);
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (isJumping)
        {
            if (rb.position.y - initialYPosition >= maxJumpHeight)
            {
                isJumping = false;
                animator.SetBool("isJumping", false);
                animator.SetBool("reachedHeight", true);
            }
        }
        else if (!isJumping && rb.position.y > initialYPosition)
        {
            rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration); // Appliquer la gravité
            // Arrêter la descente lorsque le joueur atteint la hauteur initiale
            if (rb.position.y <= initialYPosition)
            {
                rb.position = new Vector3(rb.position.x, initialYPosition, rb.position.z);
            }
        }

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.GetCurrentAnimatorStateInfo(0).IsName("Hit And Fall"))
        {
            rb.position += transform.forward * currentSpeed;
            rb.position += transform.right * horizontalMove;

        }
        if (rb.position.x > MAX_X)
        {
            rb.position = new Vector3(MAX_X, transform.position.y, transform.position.z);
        }
        else if (rb.position.x < MIN_X)
        {
            rb.position = new Vector3(MIN_X, transform.position.y, transform.position.z);
        }

        //Debug.Log(currentSpeed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("Obstacle"))
        {
            animator.SetTrigger("isHit");
            currentSpeed = 0;
            rb.velocity = Vector3.zero; // Arrête la vélocité du Rigidbody
            StartCoroutine(WaitAndGameOver());
        }
        else if (other.gameObject.tag == ("Reward"))
        {
            GameControl.Instance.DestroyCoin(other.gameObject); // Call the DestroyCoin method in GameControl
            //GameControl.Instance.CheckForCoinBonus();
            Destroy(other.gameObject); // Destroy the hit coin
        }
    }

    private IEnumerator WaitAndGameOver()
    {
        yield return new WaitForSeconds(2f);
        GameControl.Instance.GameOver();

    }

    void OnEnable()
    {
        playerInput.CharacterControl.Enable();
    }

    void OnDisable()
    {
        playerInput.CharacterControl.Disable();
    }
}
