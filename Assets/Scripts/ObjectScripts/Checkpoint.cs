using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public AudioSource checkpointSound;
    public GameObject Player;
    public Animator animator;
    public int laneNumber;

    private float _catRspawnOffsetY;

    public ScoreManager scoreManager;
    public SingleButtonAction jumpAction;
    public SingleButtonAction stompAction;
    public PlayerSideAction sideAction;
    private int playerFishScore;
    private int playerAccuracyScore;

    private int inputIndexJA; // Jump Action
    private int inputIndexPSA; // Player Side Action
    private int inputIndexRightPSA; // PSA Input index right
    private int inputIndexSA; // Stomp Action

    //Note indexes for each lane in the format: [up down left right]
    private int[] lane1Indexes;
    private int[] lane2Indexes;
    private int[] lane3Indexes;
    private int[] lane4Indexes;
    private int[] lane5Indexes;

    private int hopIndex;

    private void Start()
    {
        Player = GameObject.Find("Player");
        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        jumpAction = GameObject.Find("ActionJump").GetComponent<SingleButtonAction>();
        sideAction = GameObject.Find("ActionSide").GetComponent<PlayerSideAction>();
        stompAction = GameObject.Find("ActionStomp").GetComponent<SingleButtonAction>();
        animator = Player.GetComponent<Animator>();
        var checkpointSoundGameObject = GameObject.FindGameObjectWithTag("checkpointSound");

        playerFishScore = scoreManager.GetPlayerFishScore();
        playerAccuracyScore = scoreManager.GetPlayerAccuracyScore();
        inputIndexJA = jumpAction.GetInputIndex();
        inputIndexPSA = sideAction.GetInputIndex();
        inputIndexRightPSA = sideAction.GetInputIndexRight();
        inputIndexSA = stompAction.GetInputIndex();
        hopIndex = PlayerHopManager.Current.GetHopIndex();
        //Get the note indexes for each lane for correct colour change
        lane1Indexes = GameObject.Find("Lane1").GetComponent<Lane>().GetLaneIndexes();
        lane2Indexes = GameObject.Find("Lane2").GetComponent<Lane>().GetLaneIndexes();
        lane3Indexes = GameObject.Find("Lane3").GetComponent<Lane>().GetLaneIndexes();
        lane4Indexes = GameObject.Find("Lane4").GetComponent<Lane>().GetLaneIndexes();
        lane5Indexes = GameObject.Find("Lane5").GetComponent<Lane>().GetLaneIndexes();
        _catRspawnOffsetY = 3f;
        
        if (checkpointSoundGameObject != null)
        {
            checkpointSound = checkpointSoundGameObject.GetComponent<AudioSource>();
        }
        else
        {
            throw new Exception("No game object with checkpointSound tag in the scene");
        }
    }
    
    private void OnTriggerEnter(Collider otherCollider)
    {
        if (!otherCollider.gameObject.CompareTag("Player")) return;
        // TODO: Add revised checkpoint animation
        //animator.Play("CatCheckpointCycle", 0, 0f);
        Destroy(gameObject);
        //RespawnManager.current.SetMidiTime(MidiManager.current.GetPlaybackTime());
        RespawnManager.Current.SetMusicTime(MusicPlayer.Current.audioSource.time);

        playerFishScore = scoreManager.GetPlayerFishScore();
        RespawnManager.Current.SetPlayerFishScore(playerFishScore);
        playerAccuracyScore = scoreManager.GetPlayerAccuracyScore();
        RespawnManager.Current.SetPlayerAccuracyScore(playerAccuracyScore);

        inputIndexJA = jumpAction.GetInputIndex();
        RespawnManager.Current.SetInputIndexJA(inputIndexJA);
        inputIndexPSA = sideAction.GetInputIndex();
        RespawnManager.Current.SetInputIndexPSA(inputIndexPSA);
        inputIndexRightPSA = sideAction.GetInputIndexRight();
        RespawnManager.Current.SetInputIndexRightPSA(inputIndexRightPSA);
        inputIndexSA = stompAction.GetInputIndex();
        RespawnManager.Current.SetInputIndexSA(inputIndexSA);

        hopIndex = PlayerHopManager.Current.GetHopIndex();
        RespawnManager.Current.SetHopIndex(hopIndex);

        lane1Indexes = GameObject.Find("Lane1").GetComponent<Lane>().GetLaneIndexes();
        RespawnManager.Current.SetLane1Indexes(lane1Indexes);
        lane2Indexes = GameObject.Find("Lane2").GetComponent<Lane>().GetLaneIndexes();
        RespawnManager.Current.SetLane2Indexes(lane2Indexes);
        lane3Indexes = GameObject.Find("Lane3").GetComponent<Lane>().GetLaneIndexes();
        RespawnManager.Current.SetLane3Indexes(lane3Indexes);
        lane4Indexes = GameObject.Find("Lane4").GetComponent<Lane>().GetLaneIndexes();
        RespawnManager.Current.SetLane4Indexes(lane4Indexes);
        lane5Indexes = GameObject.Find("Lane5").GetComponent<Lane>().GetLaneIndexes();
        RespawnManager.Current.SetLane5Indexes(lane5Indexes);

        RespawnManager.Current.SetRespawnPoint(
            PlayerSyncPosition.Current.GetPlayerPosMusicTimeSyncedPosition(transform.position.y + _catRspawnOffsetY),
            laneNumber);
        checkpointSound.Play();
    }
}
