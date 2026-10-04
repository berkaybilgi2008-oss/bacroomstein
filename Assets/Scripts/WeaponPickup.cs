using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private int weaponIndex = 0;
    [SerializeField] private string pickupMessage = "Press E to collect weapon";
    [SerializeField] private float pickupDistance = 2.5f;
    [SerializeField] private bool destroyAfterPickup = true;
    private Transform player;

    private void Update()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found == null) return;
            player = found.transform;
        }
        if (Vector3.Distance(player.position, transform.position) > pickupDistance) return;
        if (!PressedInteract()) return;
        WeaponHolder holder = player.GetComponentInChildren<WeaponHolder>();
        if (holder == null) return;
        holder.AddWeapon(weaponIndex);
        holder.EquipWeapon(weaponIndex);
        if (destroyAfterPickup) Destroy(gameObject);
    }

    private bool PressedInteract()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E)) return true;
#endif
        return false;
    }
}