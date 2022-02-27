using UnityEngine;

public class ParticleComponent : MonoBehaviour {
	public const float HALO_SIZE = 0.02f;

	public Light Halo;

	public void UpdateHaloSize() {
		Halo.range = HALO_SIZE * (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3;
	}
}
