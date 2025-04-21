using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject villagerPrefab;
    public GameObject wolfPrefab;

    public Transform spawnPoint;

    public GameObject cameraPrefab;

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

        
        // Create camera and assign player
        GameObject camera = Instantiate(cameraPrefab);
        CameraFollow camScript = camera.GetComponent<CameraFollow>();
        camScript.player = ChosenCharacter.transform;
    }
}
