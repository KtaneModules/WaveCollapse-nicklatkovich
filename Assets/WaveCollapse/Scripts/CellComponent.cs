using System.Collections.Generic;
using UnityEngine;

public class CellComponent : MonoBehaviour {
	public const float PARTICLE_CONTAINER_RADIUS = 0.008f;
	public const float PARTICLE_CONTAINER_ROTATION_SPEED = 200f;

	public Transform ParticleContainer;
	public Renderer Background;
	public KMSelectable Selectable;
	public ParticleComponent ParticlePrefab;

	private float _rotationSpeed;
	private float _particleContainerAngle;
	private List<ParticleComponent> _particles = new List<ParticleComponent>();

	private void Start() {
		_particleContainerAngle = Random.Range(0f, 2 * Mathf.PI);
	}

	private void SetRotationSpeed() {
		_rotationSpeed = Random.Range(100f, 200f);
		if (Random.Range(0, 2) == 0) _rotationSpeed *= 1;
	}

	private void Update() {
		_particleContainerAngle += Time.deltaTime * PARTICLE_CONTAINER_ROTATION_SPEED;
		ParticleContainer.transform.localRotation = Quaternion.Euler(0, _particleContainerAngle, 0);
	}

	public void AddParticle(Color color) {
		ParticleComponent particle = Instantiate(ParticlePrefab);
		particle.transform.parent = ParticleContainer;
		particle.transform.localScale = Vector3.one;
		particle.transform.localRotation = Quaternion.identity;
		particle.UpdateHaloSize();
		particle.UpdateColor(color);
		_particles.Add(particle);
		if (_particles.Count == 1) {
			particle.transform.localPosition = Vector3.zero;
			return;
		}
		float angle = Random.Range(0f, 2 * Mathf.PI);
		float angleDiff = 2 * Mathf.PI / _particles.Count;
		foreach (ParticleComponent p in _particles) {
			p.transform.localPosition = new Vector3(Mathf.Cos(angle) * PARTICLE_CONTAINER_RADIUS, 0, Mathf.Sin(angle) * PARTICLE_CONTAINER_RADIUS);
			angle += angleDiff;
		}
		SetRotationSpeed();
	}

	public void RemoveAllParticles() {
		foreach (ParticleComponent p in _particles) Destroy(p.gameObject);
		_particles = new List<ParticleComponent>();
	}
}
