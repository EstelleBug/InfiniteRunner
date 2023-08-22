using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public PlayerInput playerInput;

    [SerializeField]
    private CapsuleCollider normalCollider;
    [SerializeField]
    private CapsuleCollider slideCollider;

    [SerializeField]
    private float translationSpeed;

    [SerializeField]
    private float moveSpeed;
    private float currentSpeed = 0;

    [SerializeField]
    private float jumpForce; // Force du saut
    [SerializeField]
    private float maxJumpHeight; // Hauteur maximale du saut
    private float gravity = 9.81f; // Gravité

    private bool isJumping = false;
    private bool isSliding = false;
    private float initialYPosition;

    private const float MIN_X = -2f;
    private const float MAX_X = 4f;

    private Rigidbody rb;

    public AudioClip HitSound;
    public AudioClip PainSound;
    public AudioClip SlideSound;
    public AudioClip JumpSound;

    void Awake()
    {
        playerInput = new PlayerInput();
        animator = this.GetComponent<Animator>();
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
            AudioManager.Instance.PlaySound(SlideSound);
        }
        if (!playerInput.CharacterControl.Slide.triggered)
        {
            animator.SetBool("isSliding", false);
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Running Slide"))
        {
            isSliding = true;
            normalCollider.enabled = false;
            slideCollider.enabled = true;
        }
        else
        {
            isSliding = false;
            normalCollider.enabled = true;
            slideCollider.enabled = false;
        }

        if (playerInput.CharacterControl.Jump.triggered && !isJumping && !isSliding)
        {
            animator.SetBool("reachedHeight", false);
            animator.SetBool("isJumping", true);
            AudioManager.Instance.PlaySound(JumpSound);
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
            rb.position += transform.forward * currentSpeed * Time.deltaTime;
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

    }

    public void UpdateMoveSpeed(float increment)
    {
        if (moveSpeed < 30)
        {
            moveSpeed += increment;
            currentSpeed = moveSpeed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("hitsomething");
        if (other.gameObject.tag == ("Obstacle"))
        {
            //Debug.Log("obstacle");
            animator.SetTrigger("isHit");
            AudioManager.Instance.PlaySound(HitSound);
            AudioManager.Instance.PlaySound(PainSound);
            currentSpeed = 0;
            rb.velocity = Vector3.zero; // Arrête la vélocité du Rigidbody
            StartCoroutine(WaitAndGameOver());
        }
        else if (other.gameObject.tag == ("Reward"))
        {
            GameControl.Instance.DestroyCoin(other.gameObject); // Call the DestroyCoin method in GameControl
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
