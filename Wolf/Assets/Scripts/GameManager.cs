using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject villagerPrefab;
    public GameObject wolfPrefab;

    public Transform spawnPoint;

    public CameraFollow PlayerCamera;

    void Start()
    {
        // Random character
        int randomRole = Random.Range(0, 2); 

        GameObject chosenPrefab;

        if (randomRole == 0)
        {
            chosenPrefab = villagerPrefab;
        }
        else
        {
            chosenPrefab = wolfPrefab;
        }

        // Instantiate the chosen character
        GameObject ChosenCharacter = Instantiate(chosenPrefab, spawnPoint.position, spawnPoint.rotation);

        // Set the player for the follow camera
        PlayerCamera.player = ChosenCharacter.transform;
    }
}
