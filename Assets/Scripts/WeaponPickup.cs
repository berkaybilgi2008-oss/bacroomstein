using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private int weaponIndex = 0;
    [Min(0.1f)] [SerializeField] private float pickupDistance = 1.5f;
    [SerializeField] private bool destroyAfterPickup = true;
    private Transform player;
    private bool collected;

    private void Update()
    {
        if (collected) return;
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found == null) return;
            player = found.transform;
        }

        if (Vector3.Distance(player.position, transform.position) > pickupDistance) return;

        WeaponHolder holder = player.GetComponentInChildren<WeaponHolder>();
        if (holder == null) return;

        holder.AddWeapon(weaponIndex);
        holder.EquipWeapon(weaponIndex);
        collected = true;
        if (destroyAfterPickup) Destroy(gameObject);
    }
}