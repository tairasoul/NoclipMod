using BepInEx;
using UnityEngine;
using SettingsAPI;
using UnityEngine.UI;
using Invector.vCharacterController;
using BepInEx.Configuration;

namespace NoclipMod 
{
	[BepInPlugin("tairasoul.vaproxy.noclip", "NoclipMod", "1.0.5")]
	public class Plugin: BaseUnityPlugin
	{

		ConfigEntry<int> SpeedMult;
		private void Awake() {
			SpeedMult = Config.Bind("Multipliers", "Speed", 1, "Noclip speed multiplier.");
			Logger.LogInfo("Noclip mod awake.");
		}

		bool NoclipActive = false;

		bool init = false;

		internal void OnDestroy() => Init();
		internal void Start() => Init();

		internal MeshRenderer[] SenRenderers = [];
		internal MeshRenderer[] V06Renderers = [];

		bool SenPopulated = false;
		bool V06Populated = false;

		bool ShiftHeld = false;

		private void Init() {
			if (init) return;
			Logger.LogInfo("\n-- NOCLIP INSTRUCTIONS --\nTo enable, go to Mod Settings -> NoclipMod and enable Noclip.\n-- NOCLIP CONTROLS --\nW - Forward\nA - Left\nS - Backward\nD - Right\nLeft Control - Down\nSpace - Up\nShift - Double speed while held\n-- ADDITIONAL INFO --\nCode partially taken from https://github.com/KaylinOwO/Project-Apparatus/blob/main/src/Features.cs#L278");
			Option NoclipOption = new() {
				Create = (GameObject Page) => 
				{
					GameObject toggle = ComponentUtils.CreateToggle("Noclip", "tairasoul.noclipmod.noclip");
					toggle.SetParent(Page, false);
					toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-449.6534f, 158.6981f);
					GameObject label = toggle.Find("Label");
					label.GetComponent<RectTransform>().anchoredPosition = new Vector2(52.16f, -1);
					Toggle toggleComp = toggle.GetComponent<Toggle>();
					toggleComp.onValueChanged.AddListener((bool toggled) =>
					{
						NoclipActive = toggled;
					});
					GameObject button = ComponentUtils.CreateButton("Refresh Config", "tairasoul.noclipmod.refresh");
					button.SetParent(Page, false);
					button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-393.3322f, 75.7367f);
					GameObject blabel = button.Find("ItemName");
					blabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(12.8028f, 0);
					Button b = button.GetComponent<Button>();
					b.onClick.AddListener(() => {
						Config.Reload();
						Config.TryGetEntry("Multipliers", "Speed", out SpeedMult);
						Logger.LogInfo("SpeedMult is now " + SpeedMult.Value.ToString());
					});
				}
			};
			Option HideSen = new() {
				Create = (GameObject Page) =>
				{
					GameObject toggle = ComponentUtils.CreateToggle("Hide Sen & V-06", "tairasoul.noclipmod.hidesen");
					toggle.SetParent(Page, false);
					toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-249.5085f, 74.6256f);
					GameObject label = toggle.Find("Label");
					label.GetComponent<RectTransform>().anchoredPosition = new Vector2(52.16f, -1);
					Toggle toggleComp = toggle.GetComponent<Toggle>();
					toggleComp.onValueChanged.AddListener((bool toggled) =>
					{
						GameObject Sen = GameObject.Find("S-105.1");
						GameObject V06 = GameObject.Find("V-06");
						if (Sen) {
							if (!SenPopulated) {
								Logger.LogInfo("Populating Sen's renderer list.");
								foreach (MeshRenderer renderer in Sen.GetComponentsInChildren<MeshRenderer>()) {
									if (renderer.enabled) {
										SenRenderers = [ .. SenRenderers, renderer ];
									}
								}
								SenPopulated = true;
							}
							foreach (MeshRenderer renderer in SenRenderers) {
								renderer.enabled = !toggled;
							}
							GameObject Geom = Sen.Find("Humanbot_A_Geom");
							Geom.SetActive(!toggled);
						}
						if (V06) {
							if (!V06Populated) {
								Logger.LogInfo("Populating V-06's renderer list.");
								foreach (MeshRenderer renderer in V06.GetComponentsInChildren<MeshRenderer>()) {
									if (renderer.enabled) {
										V06Renderers = [ .. V06Renderers, renderer ];
									}
								}
								V06Populated = true;
							}
							foreach (MeshRenderer renderer in V06Renderers) {
								renderer.enabled = !toggled;
							}
							SpriteRenderer Eyeball = V06.Find("Drone").Find("Bod").Find("Cylinder").Find("Eye0").GetComponent<SpriteRenderer>();
							Eyeball.enabled = !toggled;
						}
					});
					GameObject button = ComponentUtils.CreateButton("Force Hide Sen", "tairasoul.noclipmod.hidesen");
					button.SetParent(Page, false);
					button.GetComponent<RectTransform>().anchoredPosition = new Vector2(-393.3322f, -26.6804f);
					GameObject blabel = button.Find("ItemName");
					blabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(12.8028f, 0);
					Button buttonComp = button.GetComponent<Button>();
					buttonComp.onClick.AddListener(() => {
						GameObject Sen = GameObject.Find("S-105.1");
						GameObject V06 = GameObject.Find("V-06");
						if (Sen) {
							Logger.LogInfo("Populating Sen's renderer list.");
							foreach (MeshRenderer renderer in Sen.GetComponentsInChildren<MeshRenderer>()) {
								if (renderer.enabled) {
									renderer.enabled = false;
								}
							}
						}
						if (V06) {
							Logger.LogInfo("Populating V-06's renderer list.");
							foreach (MeshRenderer renderer in V06.GetComponentsInChildren<MeshRenderer>()) {
								if (renderer.enabled) {
									renderer.enabled = false;
								}
							}
						}
					});
				}
			};
			SettingsAPI.Plugin.API.RegisterMod("tairasoul.noclipmod", "NoclipMod", new Option[] {NoclipOption, HideSen}, (GameObject obj) => { obj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); });
			init = true;
		}

		private static bool IsKeyDown(KeyCode code) {
			return UnityInput.Current.GetKey(code);
		}

		private void Update() {
			GameObject Sen = GameObject.Find("S-105.1");;
			if (Sen) {
				vThirdPersonController controller = Sen.GetComponent<vThirdPersonController>();
				Transform SenTransform = Sen.transform;
				Inventory inv = Sen.GetComponent<Inventory>();
				Transform TPCTransform = GameObject.Find("TPC").transform;
				Collider col = Sen.GetComponent<CapsuleCollider>();
				AntiClip clip = Sen.GetComponent<AntiClip>();
				Rigidbody body = Sen.GetComponent<Rigidbody>();
				if (NoclipActive) {
					col.enabled = false;
					controller.extraGravity = 0;
					clip.enabled = false;
					body.useGravity = false;
					body.detectCollisions = false;
					body.velocity = Vector3.zero;
					inv.enabled = false;
					bool WKey = IsKeyDown(KeyCode.W),
						AKey = IsKeyDown(KeyCode.A),
						SKey = IsKeyDown(KeyCode.S),
						DKey = IsKeyDown(KeyCode.D),
						Space = IsKeyDown(KeyCode.Space),
						Ctrl = IsKeyDown(KeyCode.LeftControl),
						Shift = IsKeyDown(KeyCode.LeftShift);

					Vector3 Movement = new(0, 0, 0);

					if (WKey)
						Movement += TPCTransform.forward;
					if (SKey)
						Movement -= TPCTransform.forward;
					if (AKey)
						Movement -= TPCTransform.right;
					if (DKey)
						Movement += TPCTransform.right;
					if (Space)
						Movement.y += TPCTransform.up.y;
					if (Ctrl)
						Movement.y -= TPCTransform.up.y;
					if (Shift)
						Movement *= 2.25f;
					SenTransform.position += Movement * SpeedMult.Value;
				}
				else {
					col.enabled = true;
					controller.extraGravity = -40;
					body.useGravity = true;
					body.detectCollisions = true;
					inv.enabled = true;
					clip.enabled = true;
				}
			}
		}
	}
}
