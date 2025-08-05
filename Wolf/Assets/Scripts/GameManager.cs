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

        // Instantiate the chosen character and create his camera
        GameObject ChosenCharacter = Instantiate(chosenPrefab, spawnPoint.position, spawnPoint.rotation);
        PlayerController CharacterScript = ChosenCharacter.GetComponent<PlayerController>();
    }
}
