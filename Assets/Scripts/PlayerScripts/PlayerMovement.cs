using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Current;
    
    [SerializeField]
    private InputActionReference movement;

    [Header("Sound Effects")]
    public AudioSource jumpSound1;
    public AudioSource jumpSound2;
    public AudioSource jumpSound3;
    public AudioSource jumpSound4;
    private AudioSource[] _jumpSounds;
    public AudioSource stompSound; //Changes based on level
    private int _lastJumpSound = -1;
    
    public AudioSource landFromJumpSound;
    private bool _justLanded;

    [Header("Movement")]
    public float sidewayWalkSpeed;
    public float forwardWalkSpeed;
    public float[] lanePositions;
    public int laneOfTheEndbox;
    public int currentLane;
    public int numberOfLanes;
    private bool _movingSideway;
    private bool _movePlayerEnabled;
    private bool _playerInputEnabled;

    public float groundDrag;

    public float maxJumpForce;
    public float jumpCooldown;
    private bool _readyToJump;
    private bool _readyToStomp;
    private bool _canSaveJump;
    public float stompForce = 3f;
    public float jumpingGravity;
    private bool _hitFish;
    private bool _ateFish;
    private Collider _fishtreatCollider;

    public ParticleSystem particleSystem;

    [Header("Movement Animation")]
    public Animator animator;

    [Header("Ground Check")]
    public float playerHeight;
    public float playerWidth;
    private bool _grounded;

    private Rigidbody _rb;

    public MovementState state;

    //Tracking current player's movement
    public enum MovementState {
        Walking,
        Air
    }

    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        
        //Set lane positions for side movements
        currentLane = numberOfLanes / 2;
        lanePositions = new float[numberOfLanes];
        for(int i = 0; i < numberOfLanes; i++)
        {
            string laneName = "Lane" + i.ToString();
            lanePositions[i] = GameObject.Find(laneName).GetComponent<Transform>().position.x;
        }
        _movingSideway = false;
        _movePlayerEnabled = true;
        _playerInputEnabled = false;

        _readyToJump = true;
        _readyToStomp = true;
        //_canDoubleJump = false;
        _canSaveJump = true;
        _hitFish = false;
        _ateFish = false;

        animator = GetComponent<Animator>();

        _jumpSounds = new[] { jumpSound1, jumpSound2, jumpSound3, jumpSound4 };
        _rb.drag = groundDrag;
    }


    private void Update()
    {
        // ground check shoot a sphere to the foot of the player
        // Cast origin and the sphere must not overlap for it to work, thus we make the origin higher
        var sphereCastRadius = playerWidth * 0.5f;
        var sphereCastTravelDist = playerHeight * 0.5f - playerWidth * 0.5f + 0.3f;
        _grounded = Physics.SphereCast(transform.position + Vector3.up * (sphereCastTravelDist * 2),
            sphereCastRadius, Vector3.down, out _, sphereCastTravelDist*2.1f);

        if (Time.timeSinceLevelLoad > 5)
        {
            if (GameManager.Current.gameIsEnding)
            {
                return;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeSinceLevelLoad > 5 && _movePlayerEnabled){

            if (GameManager.Current.playerIsDying)
            {
                //Add some additional gravity to not make the control floaty
                var velocity = _rb.velocity;
                _rb.velocity = new Vector3(velocity.x, velocity.y, 2f);
                _rb.AddForce(Vector3.down * (20 * _rb.mass));
                return;
            }

            //Movement
            if (_movingSideway){
                var step =  Mathf.Sqrt(Mathf.Pow(forwardWalkSpeed, 2) + Mathf.Pow(sidewayWalkSpeed, 2)) * Time.fixedDeltaTime;
                Vector3 desiredPosition = _rb.transform.position;
                desiredPosition.x = lanePositions[currentLane];
                var estimatedTime = Mathf.Abs((desiredPosition.x - _rb.transform.position.x)) / sidewayWalkSpeed;
                desiredPosition.z += forwardWalkSpeed * estimatedTime * Time.fixedDeltaTime;
                desiredPosition.y += _rb.velocity.y * estimatedTime;
                _rb.transform.position = Vector3.MoveTowards(_rb.transform.position, desiredPosition, step);

                if (Mathf.Abs(_rb.transform.position.x - desiredPosition.x) < 0.001f){
                    var newPos = _rb.transform.position;
                    newPos.x = desiredPosition.x;
                    _rb.transform.position = newPos;
                    _movingSideway = false;
                    
                }
            }
            //Stomping
            if(!_readyToStomp){
                var velocity = _rb.velocity;
                velocity = new Vector3(velocity.x, 0f, velocity.z);
                _rb.velocity = velocity;
                _rb.AddForce(-transform.up * stompForce, ForceMode.Impulse);
            }

            MovementStateHandler();
            _rb.drag = groundDrag;

            if (_justLanded && _grounded)
            {
                landFromJumpSound.Play();
                _justLanded = false;
                _readyToStomp = true;
            }

            if (_grounded && !_canSaveJump)
            {
                Invoke(nameof(ResetJump), jumpCooldown);
            }

            MovePlayer();
            //Limits downward velocity to be too high (happens sometimes when jumping and switching lanes at the same time)
            if (_rb.velocity.y < -6f){
                var tempVelo = _rb.velocity;
                tempVelo.y = -6f;
                _rb.velocity = tempVelo;
            }
        }
    }

    private void OnTriggerEnter(Collider otherCollider) 
    {
        if (otherCollider.gameObject.CompareTag("fishtreat"))
        {
            _hitFish = true;
            _fishtreatCollider = otherCollider;
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.gameObject.CompareTag("fishtreat"))
        {
            _hitFish = false;
            if (_ateFish){
                _ateFish = false;
            }
        }
    }

    public void PutPlayerOnLaneOfEndbox()
    {
        var playerTransform = _rb.transform;
        var newPos = playerTransform.position;
        newPos.x = lanePositions[laneOfTheEndbox];
        playerTransform.position = newPos;
    }

    public void triggerMove(InputAction.CallbackContext context){
        // don't detect input if this is disabled
        if (!_playerInputEnabled) return;

        if (enabled){
            if (context.ReadValue<Vector2>().x < 0 && !_movingSideway && currentLane>0){
                animator.Play("CatLeft", 0, 0f);
                _movingSideway = true;
                currentLane -= 1;
            }
            else if (context.ReadValue<Vector2>().x > 0 && !_movingSideway && currentLane<6){
                animator.Play("CatRight", 0, 0f);
                _movingSideway = true;
                currentLane += 1;
            }
        }
    }
    public void TriggerJump(InputAction.CallbackContext context)
    {
        // don't detect input if this is disabled
        if (!_playerInputEnabled) return;

        if (!context.started && enabled)
        {
            if(_readyToJump && _grounded){

                _readyToJump = false;
                //_canDoubleJump = true;

                animator.Play("CatJumpFull", 0, 0f);
                Jump();
                PickJumpSound().Play();
                Invoke(nameof(SetCanSaveJumpFalse), 0.1f);
            } 
            // Commenting out double jump
            else if (context.performed && _canSaveJump && !_grounded) { //else if (context.performed && (_canDoubleJump || _canSaveJump) && !_grounded){

                //_canDoubleJump = false;
                _canSaveJump = false;

                animator.Play("CatJumpFull", 0, 0f);
                HalfJump();
                PickJumpSound().Play();
                Invoke(nameof(SetCanSaveJumpFalse), 0.1f);
            }
        }
    }

    public void TriggerStomp(InputAction.CallbackContext context)
    {
        if (!_playerInputEnabled) return;

        if (context.performed && !_grounded && _readyToStomp){
            Stomp();
            stompSound.Play();
        }
    }

    public void TriggerEat(InputAction.CallbackContext context)
    {
        if (!_playerInputEnabled) return;

        if (context.performed){
            if (_hitFish){
                //perfect!
                _ateFish = true;
                _fishtreatCollider.gameObject.GetComponent<FishHit>().HideFishTreat();

                UnityEngine.Debug.Log("particles");

                // Activate eat particle effect
                particleSystem.Play();
                //ParticleSystem.EmissionModule em = GetComponent<ParticleSystem>().emission;
                //em.enabled = true;

                ScoreManager.current.UpdateFishScore(1);
            }
        }
    }
    
    private AudioSource PickJumpSound() {
        int jumpSoundIndex;
        if (_lastJumpSound == -1) {
            jumpSoundIndex = Random.Range(0, 3);
        } else {
            jumpSoundIndex = Random.Range(0, 3);
            if (jumpSoundIndex == _lastJumpSound) {
                jumpSoundIndex = (jumpSoundIndex + Random.Range(1, 3)) % 4;
            }
        }
        _lastJumpSound = jumpSoundIndex;
        return _jumpSounds[jumpSoundIndex];
    }

    private void MovementStateHandler()
    {
        //Walking
        state = _grounded ? MovementState.Walking : MovementState.Air;
    }

    private void MovePlayer()
    {
        
        //Add some additional gravity to not make the control floaty
        _rb.AddForce(Vector3.down * (jumpingGravity * _rb.mass));

        Vector3 velocity = _rb.velocity;
        velocity.z = forwardWalkSpeed;
        _rb.velocity = velocity;
    }

    public void Stomp()
    {
        // reset y velocity
        _readyToStomp = false;
    }

    private void Jump()
    {
        // reset y velocity
        var velocity = new Vector3(0f, 0f, 0f);
        _rb.velocity = velocity;

        _rb.AddForce(transform.up * maxJumpForce, ForceMode.Impulse);
    }
    private void HalfJump()
    {
        
        // reset y velocity
        var velocity = _rb.velocity;
        velocity = new Vector3(velocity.x, 0f, velocity.z);
        _rb.velocity = velocity;

        _rb.AddForce(transform.up * (maxJumpForce/2), ForceMode.Impulse);

    }
    private void ResetJump()
    {
        _readyToJump = true;
        _justLanded = true;
        _canSaveJump = true;
    }

    public void SetCanSaveJumpFalse()
    {
        _canSaveJump = false;
    }

    public void SetMovePlayerEnabled(bool enable)
    {
        _movePlayerEnabled = enable;
    }

    public void SetPlayerInputEnabled(bool enable)
    {
        _playerInputEnabled = enable;
    }

}