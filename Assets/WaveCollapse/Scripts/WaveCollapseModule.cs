using System.Linq;
using System.Text;
using UnityEngine;
using KeepCoding;

public class WaveCollapseModule : ModuleScript {
	public const float CELLS_OFFSET = 0.0226f;

	private static readonly char[] PARTICLE_SYMBOLS = "e\xb5\x03bdg\x03b3uctdsb\x03c4ZWH".ToCharArray();
	private static readonly Color[] PARTICLE_COLORS = Enumerable.Range(0, 15).Select(i => Color.HSVToRGB(i * 3 / 16f % 1f, 1f, 1f)).ToArray();

	public Transform GridContainer;
	public Transform ButtonsContainer;
	public KMSelectable Selectable;
	public CellComponent CellPrefab;
	public ButtonComponent ButtonPrefab;

	private bool _buttonHold = false;
	private bool _collapse = false;
	private int _pressedButton;
	private float _holdTime;
	private bool[] _placed = new bool[WaveCollapsePuzzle.PARTICLES_COUNT];
	private int[] _particleSymbols;
	private WaveCollapsePuzzle _puzzle;
	private CellComponent[][] _grid;
	private ButtonComponent[] _buttons;

	private void Start() {
		_grid = new CellComponent[WaveCollapsePuzzle.WIDTH][];
		for (int x = 0; x < WaveCollapsePuzzle.WIDTH; x++) {
			_grid[x] = new CellComponent[WaveCollapsePuzzle.HEIGHT];
			for (int y = 0; y < WaveCollapsePuzzle.HEIGHT; y++) {
				CellComponent cell = Instantiate(CellPrefab);
				cell.transform.parent = GridContainer;
				cell.transform.localPosition = new Vector3(x * CELLS_OFFSET, 0, -y * CELLS_OFFSET);
				cell.transform.localScale = Vector3.one;
				cell.transform.localRotation = Quaternion.identity;
				cell.Selectable.Parent = Selectable;
				_grid[x][y] = cell;
			}
		}
		_buttons = new ButtonComponent[WaveCollapsePuzzle.PARTICLES_COUNT];
		for (int i = 0; i < WaveCollapsePuzzle.PARTICLES_COUNT; i++) {
			ButtonComponent button = Instantiate(ButtonPrefab);
			button.transform.parent = ButtonsContainer;
			button.transform.localPosition = new Vector3(i % 2 * CELLS_OFFSET, 0, -i / 2 * CELLS_OFFSET);
			button.transform.localScale = Vector3.one;
			button.transform.localRotation = Quaternion.identity;
			button.Selectable.Parent = Selectable;
			button.TextMesh.color = Color.white;
			button.TextMesh.text = "";
			_buttons[i] = button;
		}
		Selectable.Children = _grid.SelectMany(col => col.Select(c => c.Selectable)).Concat(_buttons.Select(b => b.Selectable)).ToArray();
		Selectable.UpdateChildrenProperly();
	}

	public override void OnActivate() {
		base.OnActivate();
		if (RuleSeedId == 1) _particleSymbols = Enumerable.Range(0, WaveCollapsePuzzle.PARTICLE_TYPES_COUNT).ToArray();
		else _particleSymbols = Enumerable.Range(0, PARTICLE_SYMBOLS.Length).OrderBy(x => RuleSeed.NextDouble()).Take(WaveCollapsePuzzle.PARTICLE_TYPES_COUNT).ToArray();
		_puzzle = new WaveCollapsePuzzle(RuleSeed);
		for (int i = 0; i < WaveCollapsePuzzle.PARTICLES_COUNT; i++) {
			Vector2Int wave = _puzzle.Waves[i];
			char symbol = PARTICLE_SYMBOLS[_particleSymbols[_puzzle.ParticleTypes[i]]];
			Log("Particle #{0} is {1}. Wave function: {2}-{3}", i + 1, symbol, FormatPosition(wave), FormatPosition(wave + Vector2Int.one));
		}
		for (int i = 0; i < WaveCollapsePuzzle.PARTICLES_COUNT; i++) _buttons[i].TextMesh.text = "" + PARTICLE_SYMBOLS[_particleSymbols[_puzzle.ParticleTypes[i]]];
		for (int i = 0; i < WaveCollapsePuzzle.PARTICLES_COUNT; i++) {
			int ii = i;
			_buttons[i].Selectable.OnInteract += () => { PressButton(ii); return false; };
			_buttons[i].Selectable.OnInteractEnded += () => ReleaseButton(ii);
		}
		for (int x = 0; x < WaveCollapsePuzzle.WIDTH; x++) {
			for (int y = 0; y < WaveCollapsePuzzle.HEIGHT; y++) {
				int xx = x;
				int yy = y;
				_grid[x][y].Selectable.OnInteract += () => { PressCell(xx, yy); return false; };
			}
		}
	}

	private void Update() {
		if (_buttonHold) {
			float timeDiff = Time.time - _holdTime;
			if (timeDiff > 1f) {
				_buttonHold = false;
				_collapse = true;
				Vector2Int wave = _puzzle.Waves[_pressedButton];
				for (int j = 0; j < 4; j++) _grid[wave.x + j % 2][wave.y + j / 2].Background.material.color = Color.black;
				for (int i = 0; i < WaveCollapsePuzzle.PARTICLES_COUNT; i++) _buttons[i].TextMesh.color = Color.black;
				_buttons[_pressedButton].TextMesh.color = Color.white;
				_puzzle.Collapse(_pressedButton);
				Vector2Int res = GetExpectedParticlePosition(_pressedButton);
				_grid[res.x][res.y].AddParticle(PARTICLE_COLORS[_particleSymbols[_puzzle.ParticleTypes[_pressedButton]]]);
				_placed[_pressedButton] = true;
				char symbol = PARTICLE_SYMBOLS[_particleSymbols[_puzzle.ParticleTypes[_pressedButton]]];
				Log("Particle #{0} ({1}) collapsed. Position: {2}", _pressedButton + 1, symbol, FormatPosition(res));
				Log("Other particles expected positions:");
				for (int i = 0; i < WaveCollapsePuzzle.PARTICLES_COUNT; i++) {
					if (i == _pressedButton) continue;
					Log("Particle #{0} ({1}): {2}", i + 1, PARTICLE_SYMBOLS[_particleSymbols[_puzzle.ParticleTypes[i]]], FormatPosition(GetExpectedParticlePosition(i)));
				}
			}
		}
	}

	private void PressButton(int index) {
		if (_collapse) return;
		if (_pressedButton >= 0) {
			_buttons[_pressedButton].TextMesh.color = Color.white;
			Vector2Int prevWave = _puzzle.Waves[_pressedButton];
			for (int j = 0; j < 4; j++) _grid[prevWave.x + j % 2][prevWave.y + j / 2].Background.material.color = Color.black;
		}
		_buttonHold = true;
		_holdTime = Time.time;
		_buttons[index].TextMesh.color = Color.green;
		_pressedButton = index;
		Vector2Int wave = _puzzle.Waves[_pressedButton];
		Color cl = PARTICLE_COLORS[_particleSymbols[_puzzle.ParticleTypes[_pressedButton]]];
		cl.r *= 0.4f;
		cl.g *= 0.4f;
		cl.b *= 0.4f;
		for (int j = 0; j < 4; j++) _grid[wave.x + j % 2][wave.y + j / 2].Background.material.color = cl;
	}

	private void PressCell(int x, int y) {
		if (!_collapse) return;
		if (IsSolved) return;
		Vector2Int pos = new Vector2Int(x, y);
		int nextParticleId = Enumerable.Range(0, WaveCollapsePuzzle.PARTICLES_COUNT).First(i => !_placed[i]);
		Vector2Int expected = GetExpectedParticlePosition(nextParticleId);
		if (pos != expected) {
			char symbol = PARTICLE_SYMBOLS[_particleSymbols[_puzzle.ParticleTypes[nextParticleId]]];
			Log("Pressed {0} for particle #{1} ({2}). But it collapsed to {3}. Strike!", FormatPosition(pos), nextParticleId + 1, symbol, FormatPosition(expected));
			Strike();
			Log("Scattering of the wave function of each particle...");
			foreach (ButtonComponent btn in _buttons) btn.TextMesh.color = Color.white;
			foreach (CellComponent cell in _grid.SelectMany(col => col)) cell.RemoveAllParticles();
			_placed = new bool[WaveCollapsePuzzle.PARTICLES_COUNT];
			_buttonHold = false;
			_pressedButton = -1;
			_collapse = false;
			return;
		}
		_grid[x][y].AddParticle(PARTICLE_COLORS[_particleSymbols[_puzzle.ParticleTypes[nextParticleId]]]);
		_buttons[nextParticleId].TextMesh.color = Color.white;
		_placed[nextParticleId] = true;
		if (nextParticleId + 1 == WaveCollapsePuzzle.PARTICLES_COUNT) {
			Log("All particles found. Module solved!");
			Solve();
		}
	}

	private Vector2Int GetExpectedParticlePosition(int index) {
		Vector2Int wave = _puzzle.Waves[index];
		int collapseResult = _puzzle.CollapseResult[index];
		int xx = wave.x + collapseResult % 2;
		int yy = wave.y + collapseResult / 2;
		return new Vector2Int(xx, yy);
	}

	private string FormatPosition(Vector2Int pos) {
		return (char)('A' + pos.x) + (pos.y + 1).ToString();
	}

	private void ReleaseButton(int index) {
		if (_collapse) return;
		if (_pressedButton != index) return;
		_buttonHold = false;
		_buttons[_pressedButton].TextMesh.color = new Color32(0, 0xdd, 0, 0xff);
	}
}
