using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.Netcode.Components;
using TMPro;


public class PlayerController : NetworkBehaviour
{
    protected Transform cam; // Reference to the main camera's transform
    private NetworkAnimator networkAnimator;
    public CharacterController controller;

    private DoorInteraction nearbyDoor;

    // Inventory
    public InventoryManager inventory;
    public HarvestingManager harvestingManager;
    public RoleManager roleManager;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        controller = GetComponent<CharacterController>();
        inventory = GetComponent<InventoryManager>();
        harvestingManager = GetComponent<HarvestingManager>();
        roleManager = GetComponent<RoleManager>();

        if (IsOwner)
        {
            CreateCamera();
        }
    }

    // ------------------------
    // Camera
    // ------------------------

    public void CreateCamera()
    {
        // Create camera and assign player
        cam = Instantiate(Resources.Load<GameObject>("Main Camera")).transform;
        cam.GetComponent<Camera>().enabled = true;
        cam.GetComponent<AudioListener>().enabled = true;

        // Follow the character
        cam.GetComponent<CameraFollow>().SetTarget(transform);
    }
}