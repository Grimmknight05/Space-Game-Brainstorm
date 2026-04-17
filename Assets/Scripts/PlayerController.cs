using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    /*Variables*/

    /*Physics*/
    private Rigidbody rb; //Ref to rigidbody

    /*UI*/
    [SerializeField] private TextMeshProUGUI countText; //Referance to Score UI element
    [SerializeField] private TextMeshProUGUI winUI;//Ref to Win UI 
    [SerializeField] private TextMeshProUGUI deathUI;//Ref to Death UI 


    /*Score*/
    private int playerPoints; //Storing score per player

    /*Sound*/
    [SerializeField] private AudioSource audioSource;//AudioSource Component Ref
    [SerializeField] private AudioClip[] jumpSFX;//List of jumpSFX
    [SerializeField] private AudioClip[] airJumpSFX;//List of airjumpSFX

    /*  Events  */
    public delegate void ScoreChangedDelegate(int newScore);
    public event ScoreChangedDelegate OnScoreChanged; //Score Changed event for efficeincy
    public delegate void DeathDelegate();
    public event DeathDelegate OnPlayerDeath; //Score Changed event for efficeincy

    /*Movement*/
    public enum movementMode
    {
        ForceBased,
        VelocityBased,
        accelerationBased

    }
    [SerializeField] private movementMode moveMode;
    private float moveX; //X Movement variable
    private float moveY; //Y Movement variable
    [SerializeField] private float playerSpeed = 5;//Speed of character movement Default 5
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    /*Jump*/
    [SerializeField] private int maxInAirjumps = 1;//Extra jumps in air
    private int jumpCharges;//air jumps left
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float airJumpForce = 8.0f;
    [SerializeField] private float jumpRayDistance = 0.6f; //Raydistance important to account for player height
    private bool onGround = false;
    [SerializeField] private bool canJump = true;//Default player can jump //maybe also add inair jump only
    private LayerMask jumpable;//Jumpable layer mask

    /*Look*/
    [SerializeField] private float mouseSensitivity = 100.0f;
    [SerializeField] private Transform cameraPivot; // assign your camera (or empty parent)
    private float xRotation = 0.0f;
    private Vector2 lookInput;

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); //Uses GetComponent to set rd to Rigidbody component
        setPlayerScore();// update UI
        jumpable = LayerMask.GetMask("Jumpable");//get layermask

        Cursor.lockState = CursorLockMode.Locked;//Lock cursor
        Cursor.visible = false;
    }
    /*Input Events*/
    void OnLook(InputValue value)//On axis movement
    {
        lookInput = value.Get<Vector2>();//get input
    }

    void OnMove(InputValue movementValue)//On any movement?
    {
        Vector2 movementVector = movementValue.Get<Vector2>();//Getting movement direction from movementValue param, and set it to Vector2 movementVector; (x,y)
        moveX = movementVector.x; // extract x from movementVector (x,y) make avalable to rest of code
        moveY = movementVector.y; // extract y from movementVector (y,x) make avalable to rest of code 
    }
    void OnJump(InputValue jumpValue)//Jump input
    {
        if (jumpValue.isPressed)
        {
            jump();
            //Debug.Log("Jump Pressed");
        }
    }
    /*Movement*/
    void look()
    {
        Vector2 mouseDelta = lookInput * mouseSensitivity * Time.deltaTime; //Input * sense over time
        transform.Rotate(Vector3.up * mouseDelta.x);//yaw player
        xRotation -= mouseDelta.y;//Pitch only cam
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    void jump()//call on input to jump
    {
        //Ground jump logic
        if (onGround && canJump)
        {
            //resetAirJumps();//reset airjumps on ground
            handleJump(jumpForce);//Physics
            playRandomSFX(jumpSFX);//Sound
            Debug.Log("Jump");
        }
        //Air jump logic
        if (!onGround && canJump && jumpCharges > 0)
        {
            --jumpCharges;
            handleJump(airJumpForce);//Physics
            playRandomSFX(airJumpSFX);//Sound
            Debug.Log("DoubleJump");
        }
        //check if on ground and has jumpcharge  max 3  3jump charges
        //

    }
    void checkGround()//raycast bellow player check for ground
    {
        onGround = Physics.Raycast(transform.position, Vector3.down, jumpRayDistance, jumpable); //(origon position, direction, length, Layer(like you cant jump on water) difined in gameobjects
        //Debug.Log("onGround: " + onGround);
    }

    void handleJump(float jumpHight)//Takes in jump hight
    {
        if (rb.linearVelocity.y < 0)//resets verical velocity if player is falling, keeps upward velocity
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
        rb.AddForce(Vector3.up * jumpHight, ForceMode.Impulse); //impulse force
    }

    void resetAirJumps()//Reset air jumps
    {
        jumpCharges = maxInAirjumps;
        //Debug.Log("RESET AIR JUMP");
    }

    void setmaxInAirjumps(int jumps) //not used currently
    {
        maxInAirjumps = jumps;
    }

    /*  Handle players score  */
    public int getPlayerScore()
    {
        return playerPoints;
    }
    void setPlayerScore()//Displays player's score on UI
    {
        countText.text = "Score: " + playerPoints.ToString();
    }
    public void ShowWinScreen()
    {
        winUI.gameObject.SetActive(true);
    }
    public void ShowDeathScreen()
    {
        deathUI.gameObject.SetActive(true);
    }
    /* Sound Helper */
    private void playRandomSFX(AudioClip[] soundList)
    {
        int randomIndex = Random.Range(0, soundList.Length);
        if (soundList.Length > 0)//make sure there is a sound to play
        {
            audioSource.PlayOneShot(soundList[randomIndex]);
        }
    }
    /*  Collision detection  */
    void OnTriggerEnter(Collider other)//execute once on trigger
    {
        if(other.gameObject.CompareTag("PickUp"))//If pickup collected
        {
            PickUpDefault pickUp = other.gameObject.GetComponent<PickUpDefault>();//Get PickUp
            pickUp.onPickup();//call pickUp's onPickup function
            playerPoints += pickUp.points;//check pickups point value stored in PickUpDefault script
            print("PlayerPoints: " + playerPoints);//Debug
            setPlayerScore();// update UI
            OnScoreChanged?.Invoke(playerPoints);//Notify listeners for score update passing playerPoints

        }
        Debug.Log("test");
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy touched");
            ShowDeathScreen();
            OnPlayerDeath?.Invoke();
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if ((jumpable.value & (1 << collision.gameObject.layer)) > 0) //If collision layer == jumpable layer (only reset jumps on jumpable surface)
        {
            resetAirJumps();
        }
    }
    /*Update*/
    void FixedUpdate()//Fixed interval update ensures physics is consistant regaurdless of framerate
    {
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement = forward * moveY + right * moveX;
        switch (moveMode)
        {
            case movementMode.ForceBased:
                //Construct movement vector3
                //Vector3 movement = new Vector3(moveX, 0.0f, moveY); // Asign new Vector3(x,z,y) with moveX and moveY input, and no z input

                //Add force to player in cormovement
                rb.AddForce(movement * playerSpeed);//multiply movement Vector3 by playerSpeed varible
                break;
            case movementMode.VelocityBased:
                Vector3 velocity = new Vector3(moveX * playerSpeed, rb.linearVelocity.y, moveY * playerSpeed);
                rb.linearVelocity = velocity;
                break;
            case movementMode.accelerationBased:

                //Vector3 aMovement = new Vector3(moveX, 0f, moveY); // Asign new Vector3(x,z,y) with moveX and moveY input, and no z input

                if (movement.sqrMagnitude > 1f) //Normalize to limit faster diagonal movement
                    movement.Normalize();

                Vector3 targetVelocity = movement * playerSpeed; //find target velocity
                targetVelocity.y = rb.linearVelocity.y;//keep y velocity uneffected

                float accelRate = (movement.sqrMagnitude > 0.01f) ? acceleration : deceleration;//Calculate acceleration rate

                float t = 1f - Mathf.Exp(-accelRate * Time.fixedDeltaTime);//Smooth t

                Vector3 avelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, t); // Lerp velocity

                // Clamp horizontal speed
                Vector3 horizontal = new Vector3(avelocity.x, 0, avelocity.z);
                horizontal = Vector3.ClampMagnitude(horizontal, playerSpeed);

                rb.linearVelocity = new Vector3(horizontal.x, avelocity.y, horizontal.z); //set velocity
                break;
        }
        
        checkGround();//check if player is on the ground
    }
    void Update()
    {
        look();
    }
}
