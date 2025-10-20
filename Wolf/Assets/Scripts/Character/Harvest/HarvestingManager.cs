using UnityEngine;

public class HarvestingManager : MonoBehaviour
{
    private bool isHarvesting = false;
    private float harvestTimer;
    private float currentHarvestTimer = 0;
    private Crop currentCrop;
    private Role currentRole;
    public CircularProgressBar harvestProgressBar;
    private CircularProgressBar harvestProgressBarInstance;
    public Transform HarvestBarPosition;
    private PlayerController playerController;
    private RoleManager PlayerRoleManager;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        PlayerRoleManager = playerController.roleManager;
        currentRole = PlayerRoleManager.GetCurrentRole();
    }

    public void BeginHarvest(Crop crop)
    {
        if (isHarvesting) return;

        isHarvesting = true;
        currentCrop = crop;
        currentHarvestTimer = 0;
        harvestTimer = crop.GetHarvestingTime();

        if (currentRole == null)
        {
            currentRole = PlayerRoleManager.GetCurrentRole();
            if (currentRole == null) return;
        }


        // Block movement
        if (currentRole.movement != null)
            currentRole.movement.SetIsHarvestingServerRpc(true);

        // Show progress bar UI
        if (harvestProgressBar)
            InitiateHarvestProgressBar(harvestTimer);
    }

    private void UpdateHarvesting()
    {
        if (!isHarvesting) return;

        currentHarvestTimer += Time.deltaTime;
        
        if (harvestProgressBarInstance)
            harvestProgressBarInstance.UpdateCountdown(currentHarvestTimer);

        if (currentHarvestTimer >= harvestTimer)
        {
            EndHarvest();
        }
    }

    private void EndHarvest()
    {
        isHarvesting = false;

        // Remove ProgressBar
        Destroy(harvestProgressBarInstance.gameObject);

        // Activate movement
        if (currentRole.movement != null)
            currentRole.movement.SetIsHarvestingServerRpc(false);

        // Notify the server to pick up crop
        currentCrop.FinishHarvestServerRpc();
        currentCrop = null;
    }

    private void InitiateHarvestProgressBar(float HarvestTime)
    {
        Role gameRole = PlayerRoleManager.GetGameRole();
        if (gameRole == null)
            return;
            
        if (harvestProgressBar)
        {
            harvestProgressBarInstance = Instantiate(harvestProgressBar, HarvestBarPosition.position, Quaternion.identity);
            harvestProgressBarInstance.target = HarvestBarPosition;
            harvestProgressBarInstance.SetProgressBarText(gameRole.harvestTextLoading);
            harvestProgressBarInstance.ActivateCountdown(HarvestTime);
        }
    }

    void Update()
    {
        UpdateHarvesting();
    }
}
