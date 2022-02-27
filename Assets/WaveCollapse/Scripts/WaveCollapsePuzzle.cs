using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCollapsePuzzle {
	public const int SCHEMES_COUNT = 10;
	public const int WIDTH = 5;
	public const int HEIGHT = 7;
	public const int PARTICLES_COUNT = 8;
	public const int PARTICLE_TYPES_COUNT = 5;

	private static readonly int[][] SCHEMES = new int[SCHEMES_COUNT][] {
		new[] { 0, 1, 2, 3 },
		new[] { 1, 0, 2, 3 },
		new[] { 0, 3, 2, 1 },
		new[] { 0, 1, 3, 2 },
		new[] { 2, 1, 0, 3 },
		new[] { 1, 0, 3, 2 },
		new[] { 2, 3, 0, 1 },
		new[] { 3, 1, 2, 0 },
		new[] { 0, 2, 1, 3 },
		new[] { 3, 2, 1, 0 },
	};

	public int[] ParticleTypes;
	public int[][] Schemes;
	public Vector2Int[] Waves;
	public List<int>[] Connections;
	public Vector2Int[] ParticlePositions;
	public int[] CollapseResult;

	public WaveCollapsePuzzle(KeepCoding.MonoRandom rnd) {
		ParticleTypes = Enumerable.Range(0, PARTICLES_COUNT).Select(_ => Random.Range(0, PARTICLE_TYPES_COUNT)).ToArray();
		Generate();
		GenerateSchemes(rnd);
	}

	private void Generate() {
		Waves = Enumerable.Range(0, PARTICLES_COUNT).Select(_ => Vector2Int.zero).ToArray();
		Connections = Enumerable.Range(0, PARTICLES_COUNT).Select(_ => new List<int>()).ToArray();
		int[] rndOrder = Enumerable.Range(0, PARTICLES_COUNT).OrderBy(_ => Random.value).ToArray();
		HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
		Vector2Int startCell = new Vector2Int(Random.Range(0, WIDTH - 1), Random.Range(0, HEIGHT - 1));
		cells.Add(startCell);
		int[][] grid = Enumerable.Range(0, WIDTH - 1).Select(_ => Enumerable.Range(0, HEIGHT - 1).Select(_1 => -1).ToArray()).ToArray();
		grid[startCell.x][startCell.y] = rndOrder[0];
		for (int i = 0; i < PARTICLES_COUNT; i++) {
			int particleId = rndOrder[i];
			if (cells.Count == 0) {
				Generate();
				return;
			}
			Vector2Int cell = cells.PickRandom();
			cells.Remove(cell);
			if (grid[cell.x][cell.y] < -1) {
				i -= 1;
				continue;
			}
			if (i > 0) {
				int conn = grid[cell.x][cell.y];
				Connections[particleId].Add(conn);
				Connections[conn].Add(particleId);
			}
			Waves[particleId] = cell;
			for (int dx = -1; dx < 2; dx++) {
				int xx = cell.x + dx;
				if (xx < 0 || xx >= WIDTH - 1) continue;
				for (int dy = -1; dy < 2; dy++) {
					int yy = cell.y + dy;
					if (yy < 0 || yy >= HEIGHT - 1) continue;
					if (grid[xx][yy] < -1) continue;
					if (grid[xx][yy] >= 0) grid[xx][yy] = -2;
					else {
						grid[xx][yy] = particleId;
						cells.Add(new Vector2Int(xx, yy));
					}
				}
			}
		}
	}

	private void GenerateSchemes(KeepCoding.MonoRandom rnd) {
		int[] diagSchemes = Enumerable.Range(0, SCHEMES_COUNT).OrderBy(_ => rnd.NextDouble()).ToArray();
		Schemes = new int[PARTICLE_TYPES_COUNT][];
		HashSet<int>[] rowsParticles = Enumerable.Range(0, PARTICLE_TYPES_COUNT).Select(_ => new HashSet<int>()).ToArray();
		for (int i = 0; i < PARTICLE_TYPES_COUNT; i++) {
			Schemes[i] = new int[PARTICLE_TYPES_COUNT];
			int schema = diagSchemes[i];
			Schemes[i][i] = schema;
			rowsParticles[i].Add(schema);
		}
		for (int i = 0; i < PARTICLE_TYPES_COUNT; i++) {
			for (int j = 0; j < i; j++) {
				int[] filterSchemes = Enumerable.Range(0, SCHEMES_COUNT).Where(k => !rowsParticles[i].Contains(k) && !rowsParticles[j].Contains(k)).ToArray();
				int schema = filterSchemes[rnd.Next(filterSchemes.Length)];
				Schemes[j][i] = Schemes[i][j] = schema;
				rowsParticles[i].Add(schema);
				rowsParticles[j].Add(schema);
			}
		}
		Debug.LogFormat("[Wave Collapse] Schemes: {0}", Schemes.Select(row => row.Join(",")).Join("\n"));
	}

	public void Collapse(int collapsedParticleId) {
		CollapseResult = new int[PARTICLES_COUNT];
		CollapseResult[collapsedParticleId] = Random.Range(0, 4);
		bool[] collapsed = new bool[PARTICLES_COUNT];
		collapsed[collapsedParticleId] = true;
		Queue<int> q = new Queue<int>(Connections[collapsedParticleId]);
		while (q.Count > 0) {
			int pId = q.Dequeue();
			if (collapsed[pId]) continue;
			int otherPId = Connections[pId].First(p => collapsed[p]);
			int schema = Schemes[ParticleTypes[pId]][ParticleTypes[otherPId]];
			Debug.LogFormat("{0}({1}) + {2}({3}) = {4}", pId + 1, ParticleTypes[pId] + 1, otherPId + 1, ParticleTypes[otherPId] + 1, schema);
			CollapseResult[pId] = SCHEMES[schema][CollapseResult[otherPId]];
			Debug.Log(CollapseResult[otherPId]);
			foreach (int p in Connections[pId].Where(p => !collapsed[p])) q.Enqueue(p);
			collapsed[pId] = true;
		}
	}
}
