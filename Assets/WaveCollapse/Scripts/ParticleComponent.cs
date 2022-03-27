using UnityEngine;

public class ParticleComponent : MonoBehaviour {
	public const float HALO_SIZE = 0.02f;

	public Light Halo;
	public Renderer Renderer;

	public void UpdateHaloSize() {
		Halo.range = HALO_SIZE * (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3;
	}

	public void UpdateColor(Color cl) {
		Halo.color = cl;
		Renderer.material.color = cl;
	}
}
