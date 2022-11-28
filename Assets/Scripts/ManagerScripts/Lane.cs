using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName platformNote;
    public Melanchall.DryWetMidi.MusicTheory.NoteName fishTreatNote;
    public GameObject platformPrefab;
    public GameObject fishTreatPrefab;
    public GameObject checkpointPrefab;
    public List<Platform> platforms = new List<Platform>();
    public List<FishHit> fishtreats = new List<FishHit>();
    
    public int laneNumber;
    public float spacingSize; //based on the size of the current neutral platform
    
    private const float X = 0F;
    private float _y, _z;

    public void SpawnPlatformsAndFishTreats(IEnumerable<Note> array, float bpm)
    {
        var oneEighthofBeat = (1 / (bpm / 60f)) / 2;
        foreach (var note in array)
        {
            //Octave 1 is for player input
            if (note.Octave == 1) continue;
            
            var metricTimeSpan =
                TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, MusicPlayer.Current.MidiFileTest.GetTempoMap());

            var spawnTime = ((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds +
                             (double)metricTimeSpan.Milliseconds / 1000f);
            if (note.NoteName == platformNote) {
                /* Pre-spawning platforms before game starts */
                SpawnPlatform(note.Octave, note.Velocity, (float)spawnTime, oneEighthofBeat);
            }
            if (note.NoteName == fishTreatNote)
            {
                ScoreManager.current.maximumFishScore += 1;
                SpawnFishTreat(note.Octave, note.Velocity, (float)spawnTime);
            }
        }
    }

    private void SpawnPlatform(int octave, Melanchall.DryWetMidi.Common.SevenBitNumber velocity, float spawnTime,
            float oneEighthofBeat)
    //TODO: Check note velocity to spawn different types of platform
    {
        var newPlatform = Instantiate(platformPrefab, transform, true);
        _y = (octave - 2) * 2F;
        _z = (spawnTime / oneEighthofBeat) * spacingSize;
        var position = new Vector3(X, _y, _z);
        newPlatform.transform.localPosition = position;
        newPlatform.transform.rotation = transform.rotation;

        var alteredColor = new Color
        {
            r = newPlatform.GetComponent<Renderer>().material.color.r,
            g = newPlatform.GetComponent<Renderer>().material.color.g,
            b = newPlatform.GetComponent<Renderer>().material.color.b + (_y/50)
        };

        newPlatform.GetComponent<Renderer>().material.color = alteredColor;

        platforms.Add(newPlatform.GetComponent<Platform>());

        if (velocity == (Melanchall.DryWetMidi.Common.SevenBitNumber)83){
            //Checkpoint
            var newCheckpoint = Instantiate(checkpointPrefab, transform, true);
            _y = (octave - 2) * 2F - 1.8F;
            _z = (spawnTime / oneEighthofBeat) * spacingSize;
            position = new Vector3(0.6F, _y, _z);
            newCheckpoint.transform.localPosition = position;
            newCheckpoint.transform.rotation = transform.rotation;
            newCheckpoint.GetComponent<Checkpoint>().laneNumber = laneNumber;
        }
    }

    private void SpawnFishTreat(int octave, Melanchall.DryWetMidi.Common.SevenBitNumber velocity, float spawnTime)
    {
        // Debug.Log("spawned");
        var newFishtreat = Instantiate(fishTreatPrefab, transform, true);
        _y = (octave - 2) * 2F + 3;
        if (velocity == (Melanchall.DryWetMidi.Common.SevenBitNumber)120)
        {
            _y += 2f;
        }
        _z = (spawnTime / 0.25F) * spacingSize - 3.5f;
        var position = new Vector3(X, _y, _z);
        // Debug.Log(spawn_time);
        newFishtreat.transform.localPosition = position;
        newFishtreat.transform.rotation = transform.rotation;
        fishtreats.Add(newFishtreat.GetComponent<FishHit>());
    }

    public void RespawnAllFishTreats(IEnumerable<Note> array)
    {
        foreach (var note in array)
        {
            var metricTimeSpan =
                TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, MusicPlayer.Current.MidiFileTest.GetTempoMap());
            var spawnTime = ((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds +
                             (double)metricTimeSpan.Milliseconds / 1000f);
            if (note.NoteName == fishTreatNote)
            {
                SpawnFishTreat(note.Octave, note.Velocity, (float)spawnTime);
            }
        }
    }

    public void DestroyAllFishTreats()
    {
        foreach (var fishHit in fishtreats)
        {
            // Check it is not NULL
            if (fishHit)
            {
                fishHit.DestroyFishTreat();
            }
        }
    }
}