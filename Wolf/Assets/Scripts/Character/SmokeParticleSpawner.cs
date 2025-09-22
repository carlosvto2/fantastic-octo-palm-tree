using UnityEngine;

public class SmokeParticleSpawner : MonoBehaviour
{
    public GameObject particlePrefab;

    void SpawnParticle()
    {
        // Posición donde aparecerá (puedes cambiarlo por la que quieras)
        Vector3 spawnPos = transform.position;

        // Rotación aleatoria
        Quaternion randomRot = Random.rotation;

        // Instanciamos el objeto
        GameObject particle = Instantiate(particlePrefab, spawnPos, randomRot);

        // Escala aleatoria (ej: entre 0.5 y 2.0)
        float randomScale = Random.Range(0.5f, 2f);
        particle.transform.localScale = Vector3.one * randomScale;

        // Lo destruimos al segundo
        Destroy(particle, 1f);
    }
}
