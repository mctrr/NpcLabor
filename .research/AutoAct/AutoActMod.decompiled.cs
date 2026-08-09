using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Algorithms;
using AutoActMod.Actions;
using AutoActMod.Patches;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETFramework,Version=v4.7.2", FrameworkDisplayName = ".NET Framework 4.7.2")]
[assembly: AssemblyVersion("0.0.0.0")]
[module: RefSafetyRules(11)]
[CompilerGenerated]
internal sealed class <>z__ReadOnlyArray<T> : IEnumerable, ICollection, IList, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList<T>
{
	int ICollection.Count => _items.Length;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	object IList.this[int index]
	{
		get
		{
			return _items[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	int IReadOnlyCollection<T>.Count => _items.Length;

	T IReadOnlyList<T>.this[int index] => _items[index];

	int ICollection<T>.Count => _items.Length;

	bool ICollection<T>.IsReadOnly => true;

	T IList<T>.this[int index]
	{
		get
		{
			return _items[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public <>z__ReadOnlyArray(T[] items)
	{
		_items = items;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_items).GetEnumerator();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_items).CopyTo(array, index);
	}

	int IList.Add(object value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object value)
	{
		return ((IList)_items).Contains(value);
	}

	int IList.IndexOf(object value)
	{
		return ((IList)_items).IndexOf(value);
	}

	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)_items).GetEnumerator();
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Contains(T item)
	{
		return ((ICollection<T>)_items).Contains(item);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		((ICollection<T>)_items).CopyTo(array, arrayIndex);
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	int IList<T>.IndexOf(T item)
	{
		return ((IList<T>)_items).IndexOf(item);
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}
}
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace AutoActMod
{
	[BepInPlugin("redgeioz.plugin.AutoAct", "AutoAct", "1.0.0")]
	public class AutoActMod : BaseUnityPlugin
	{
		public static bool SwitchOn;

		public static bool Active => EClass.pc.ai is AutoAct;

		public static bool IsSwitchOn
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				if (!Settings.KeyMode)
				{
					return Input.GetKey(Settings.KeyCode);
				}
				return SwitchOn;
			}
		}

		public static AutoActMod Instance { get; private set; }

		internal void Awake()
		{
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			Instance = this;
			Settings.detDistSq = ((BaseUnityPlugin)this).Config.Bind<int>("Settings", "DetectionRangeSquared", 100, "Sqaure of detection range.");
			Settings.pourDepth = ((BaseUnityPlugin)this).Config.Bind<int>("Settings", "PouringDepth", 1, "The depth of water pouring");
			Settings.seedReapingCount = ((BaseUnityPlugin)this).Config.Bind<int>("Settings", "SeedReapingCount", 25, (ConfigDescription)null);
			Settings.staminaCheck = ((BaseUnityPlugin)this).Config.Bind<bool>("Settings", "StaminaCheck", true, (ConfigDescription)null);
			Settings.enemyEncounterResponse = ((BaseUnityPlugin)this).Config.Bind<int>("Settings", "enemyEncounterResponse", 2, (ConfigDescription)null);
			Settings.simpleIdentify = ((BaseUnityPlugin)this).Config.Bind<int>("Settings", "SimpleIdentify", 0, (ConfigDescription)null);
			Settings.sameFarmfieldOnly = ((BaseUnityPlugin)this).Config.Bind<bool>("Settings", "SameFarmfieldOnly", true, "Only auto harvest the plants on the same farmfield.");
			Settings.keyMode = ((BaseUnityPlugin)this).Config.Bind<bool>("Settings", "KeyMode", false, "false = Press, true = Toggle");
			Settings.keyCode = ((BaseUnityPlugin)this).Config.Bind<KeyCode>("Settings", "KeyCode", (KeyCode)304, (ConfigDescription)null);
			Settings.rangeSelectKeyCode = ((BaseUnityPlugin)this).Config.Bind<KeyCode>("Settings", "RangeSelectKeyCode", (KeyCode)308, (ConfigDescription)null);
			AutoAct.Register(Assembly.GetExecutingAssembly());
			new Harmony("AutoActMod").PatchAll();
		}

		internal void Update()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if (Settings.KeyMode && Input.GetKeyDown(Settings.KeyCode))
			{
				SwitchOn = !SwitchOn;
				Say(AALang.GetText(SwitchOn ? "aaon" : "aaoff"));
			}
		}

		internal void Start()
		{
			AutoAct.InitTryCreateMethods();
		}

		public static void Say(string text)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Msg.SetColor(Msg.colors.TalkGod);
			Msg.Say(text);
		}

		internal static void Log(object payload)
		{
			((BaseUnityPlugin)Instance).Logger.LogInfo(payload);
		}

		internal static void LogWarning(object payload)
		{
			((BaseUnityPlugin)Instance).Logger.LogWarning(payload);
		}
	}
	public static class Utils
	{
		[CompilerGenerated]
		private sealed class <Flatten>d__5 : IEnumerable<Thing>, IEnumerable, IEnumerator<Thing>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Thing <>2__current;

			private int <>l__initialThreadId;

			private ThingContainer things;

			public ThingContainer <>3__things;

			private List<Thing>.Enumerator <>7__wrap1;

			private IEnumerator<Thing> <>7__wrap2;

			Thing IEnumerator<Thing>.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Flatten>d__5(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				int num = <>1__state;
				if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
				{
					try
					{
						if (num == -4 || num == 2)
						{
							try
							{
							}
							finally
							{
								<>m__Finally2();
							}
						}
					}
					finally
					{
						<>m__Finally1();
					}
				}
				<>7__wrap1 = default(List<Thing>.Enumerator);
				<>7__wrap2 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				try
				{
					switch (<>1__state)
					{
					default:
						return false;
					case 0:
						<>1__state = -1;
						<>7__wrap1 = ((List<Thing>)(object)things).GetEnumerator();
						<>1__state = -3;
						goto IL_00dd;
					case 1:
						<>1__state = -3;
						goto IL_00dd;
					case 2:
						{
							<>1__state = -4;
							goto IL_00c3;
						}
						IL_00dd:
						if (<>7__wrap1.MoveNext())
						{
							Thing current = <>7__wrap1.Current;
							if (((List<Thing>)(object)((Card)current).things).Count == 0)
							{
								<>2__current = current;
								<>1__state = 1;
								return true;
							}
							<>7__wrap2 = ((Card)current).things.Flatten().GetEnumerator();
							<>1__state = -4;
							goto IL_00c3;
						}
						<>m__Finally1();
						<>7__wrap1 = default(List<Thing>.Enumerator);
						return false;
						IL_00c3:
						if (<>7__wrap2.MoveNext())
						{
							Thing current2 = <>7__wrap2.Current;
							<>2__current = current2;
							<>1__state = 2;
							return true;
						}
						<>m__Finally2();
						<>7__wrap2 = null;
						goto IL_00dd;
					}
				}
				catch
				{
					//try-fault
					((IDisposable)this).Dispose();
					throw;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			private void <>m__Finally1()
			{
				<>1__state = -1;
				((IDisposable)<>7__wrap1).Dispose();
			}

			private void <>m__Finally2()
			{
				<>1__state = -3;
				if (<>7__wrap2 != null)
				{
					<>7__wrap2.Dispose();
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Thing> IEnumerable<Thing>.GetEnumerator()
			{
				<Flatten>d__5 <Flatten>d__;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					<Flatten>d__ = this;
				}
				else
				{
					<Flatten>d__ = new <Flatten>d__5(0);
				}
				<Flatten>d__.things = <>3__things;
				return <Flatten>d__;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Thing>)this).GetEnumerator();
			}
		}

		public static void Trace()
		{
			StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
			AutoActMod.Log("StackTrace:");
			StackFrame[] frames = stackTrace.GetFrames();
			for (int i = 0; i < frames.Length; i++)
			{
				MethodBase method = frames[i].GetMethod();
				if (method.HasValue())
				{
					AutoActMod.Log($"\t{method.DeclaringType?.Name}.{method}");
				}
			}
		}

		public static int Dist2(this Point p1, Point p2)
		{
			int num = p1.x - p2.x;
			int num2 = p1.z - p2.z;
			return num * num + num2 * num2;
		}

		public static int MaxDelta(this Point p1, Point p2)
		{
			int val = Math.Abs(p1.x - p2.x);
			int val2 = Math.Abs(p1.z - p2.z);
			return Math.Max(val, val2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetBit(this int n, int digit)
		{
			return (n >> digit) & 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasValue(this object obj)
		{
			return obj != null;
		}

		[IteratorStateMachine(typeof(<Flatten>d__5))]
		public static IEnumerable<Thing> Flatten(this ThingContainer things)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Flatten>d__5(-2)
			{
				<>3__things = things
			};
		}
	}
	public static class Settings
	{
		public static ConfigEntry<int> detDistSq;

		public static ConfigEntry<int> seedReapingCount;

		public static ConfigEntry<int> pourDepth;

		public static ConfigEntry<bool> staminaCheck;

		public static ConfigEntry<bool> sameFarmfieldOnly;

		public static ConfigEntry<int> enemyEncounterResponse;

		public static ConfigEntry<int> simpleIdentify;

		public static ConfigEntry<bool> keyMode;

		public static ConfigEntry<KeyCode> keyCode;

		public static ConfigEntry<KeyCode> rangeSelectKeyCode;

		public static ConfigEntry<KeyCode> ChangingKey;

		public static int DetRangeSq
		{
			get
			{
				return detDistSq.Value;
			}
			set
			{
				detDistSq.Value = value;
			}
		}

		public static int PourDepth
		{
			get
			{
				return pourDepth.Value;
			}
			set
			{
				pourDepth.Value = value;
			}
		}

		public static int SeedReapingCount
		{
			get
			{
				return seedReapingCount.Value;
			}
			set
			{
				seedReapingCount.Value = value;
			}
		}

		public static bool StaminaCheck
		{
			get
			{
				return staminaCheck.Value;
			}
			set
			{
				staminaCheck.Value = value;
			}
		}

		public static bool SameFarmfieldOnly
		{
			get
			{
				return sameFarmfieldOnly.Value;
			}
			set
			{
				sameFarmfieldOnly.Value = value;
			}
		}

		public static int EnemyEncounterResponse
		{
			get
			{
				return enemyEncounterResponse.Value;
			}
			set
			{
				enemyEncounterResponse.Value = value;
			}
		}

		public static int SimpleIdentify
		{
			get
			{
				return simpleIdentify.Value;
			}
			set
			{
				simpleIdentify.Value = value;
			}
		}

		public static bool KeyMode
		{
			get
			{
				return keyMode.Value;
			}
			set
			{
				keyMode.Value = value;
			}
		}

		public static KeyCode KeyCode
		{
			get
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				return keyCode.Value;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				keyCode.Value = value;
			}
		}

		public static KeyCode RangeSelectKeyCode
		{
			get
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				return rangeSelectKeyCode.Value;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				rangeSelectKeyCode.Value = value;
			}
		}

		public static void InputKey(ConfigEntry<KeyCode> key)
		{
			Dialog val = Layer.Create<Dialog>("DialogKeymap");
			val.textDetail.SetText(AALang.GetText("inputKey"));
			ChangingKey = key;
			((Layer)ELayer.ui).AddLayer((Layer)(object)val);
		}

		public static void SetupSettings(ActPlan actPlan)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			DynamicAct act = new DynamicAct(AALang.GetText("settings"), (Func<bool>)delegate
			{
				//IL_036d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0377: Unknown result type (might be due to invalid IL or missing references)
				//IL_037c: Unknown result type (might be due to invalid IL or missing references)
				//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
				UIContextMenu val = EClass.ui.CreateContextMenu("ContextMenu");
				val.AddButton(AALang.GetText("trigger"), (Action)delegate
				{
					InputKey(keyCode);
				}, true);
				val.AddButton(AALang.GetText("triggerRangeSelect"), (Action)delegate
				{
					InputKey(rangeSelectKeyCode);
				}, true);
				val.AddToggle(AALang.GetText("sameFarmfieldOnly"), SameFarmfieldOnly, (UnityAction<bool>)delegate(bool v)
				{
					SameFarmfieldOnly = v;
				});
				val.AddToggle(AALang.GetText("staminaCheck"), StaminaCheck, (UnityAction<bool>)delegate(bool v)
				{
					StaminaCheck = v;
				});
				val.AddSlider(AALang.GetText("keyMode"), (Func<float, string>)delegate(float v)
				{
					if (v == 1f)
					{
						KeyMode = true;
						return AALang.GetText("toggle");
					}
					KeyMode = false;
					return AALang.GetText("press");
				}, (float)(KeyMode ? 1 : 0), (Action<float>)delegate
				{
				}, 0f, 1f, true, false, false);
				val.AddSlider(AALang.GetText("enemyEncounterResponse"), (Func<float, string>)delegate(float v)
				{
					EnemyEncounterResponse = (int)v;
					return AALang.GetText("eer" + v);
				}, (float)EnemyEncounterResponse, (Action<float>)delegate
				{
				}, 0f, 2f, true, false, false);
				val.AddSlider(AALang.GetText("detDist"), (Func<float, string>)delegate(float v)
				{
					float num2 = v / 2f;
					DetRangeSq = (int)(num2 * num2);
					return num2.ToString();
				}, (float)(int)(Math.Sqrt(DetRangeSq) * 2.0), (Action<float>)delegate
				{
				}, 3f, 50f, true, false, false);
				val.AddSlider(AALang.GetText("pourDepth"), (Func<float, string>)delegate(float v)
				{
					PourDepth = (int)v;
					return v.ToString();
				}, (float)PourDepth, (Action<float>)delegate
				{
				}, 1f, 4f, true, false, false);
				val.AddSlider(AALang.GetText("simpleIdentify"), (Func<float, string>)delegate(float v)
				{
					SimpleIdentify = (int)v;
					return (SimpleIdentify == 0) ? AALang.GetText("off") : v.ToString();
				}, (float)SimpleIdentify, (Action<float>)delegate
				{
				}, 0f, 2f, true, false, false);
				int seedReapingCountMax = 101;
				val.AddSlider(AALang.GetText("seedReapingCount"), (Func<float, string>)delegate(float v)
				{
					SeedReapingCount = (((int)v != seedReapingCountMax) ? ((int)v) : 0);
					return (SeedReapingCount > 0) ? v.ToString() : "∞".ToString();
				}, (float)((SeedReapingCount == 0) ? seedReapingCountMax : SeedReapingCount), (Action<float>)delegate
				{
				}, 1f, (float)seedReapingCountMax, true, false, false);
				val.Show();
				float num = 0f;
				foreach (Transform item in ((Component)val.layoutGroup).transform)
				{
					Rect rect = ((Component)item).GetComponent<RectTransform>().rect;
					num = Mathf.Max(((Rect)(ref rect)).width, num);
				}
				foreach (Transform item2 in ((Component)val.layoutGroup).transform)
				{
					LayoutElement component = ((Component)item2).GetComponent<LayoutElement>();
					if (component != null)
					{
						component.preferredWidth = num;
						component.flexibleWidth = 0f;
					}
				}
				return false;
			}, false);
			((List<Item>)(object)actPlan.list).Add(new Item
			{
				act = (Act)(object)act
			});
		}
	}
	public static class AALang
	{
		public static Dictionary<string, Dictionary<string, string>> langData = new Dictionary<string, Dictionary<string, string>>
		{
			{
				"CN",
				new Dictionary<string, string>
				{
					{ "autoact", "自动行动" },
					{ "settings", "自动行动设置" },
					{ "enemyEncounterResponse", "遇敌时反应" },
					{ "eer0", "停下" },
					{ "eer1", "无视" },
					{ "eer2", "攻击" },
					{ "detDist", "探测距离" },
					{ "pourDepth", "倒水深度" },
					{ "seedReapingCount", "种子收获数" },
					{ "keyMode", "按键模式" },
					{ "press", "按住" },
					{ "toggle", "切换" },
					{ "start", "自动行动，启动！" },
					{ "fail", "自动行动已中断。" },
					{ "noTarget", "自动行动没有找到下一个目标。" },
					{ "aaon", "自动行动，启动！" },
					{ "aaoff", "自动行动，关闭。" },
					{ "staminaCheck", "精力耗尽时停止" },
					{ "simpleIdentify", "简单识别" },
					{ "off", "关闭" },
					{ "sameFarmfieldOnly", "只在同一田地上收割" },
					{ "inputKey", "请输入要设置的按键" },
					{ "trigger", "设置自动行动触发键" },
					{ "triggerRangeSelect", "设置范围选择触发键" }
				}
			},
			{
				"ZHTW",
				new Dictionary<string, string>
				{
					{ "autoact", "自動行動" },
					{ "settings", "自動行動設定" },
					{ "enemyEncounterResponse", "遇敵時反應" },
					{ "eer0", "停下" },
					{ "eer1", "無視" },
					{ "eer2", "攻擊" },
					{ "detDist", "探測距離" },
					{ "pourDepth", "倒水深度" },
					{ "seedReapingCount", "種子收獲數" },
					{ "keyMode", "按鍵模式" },
					{ "press", "按住" },
					{ "toggle", "切換" },
					{ "start", "自動行動，啟動！" },
					{ "fail", "自動行動已中斷。" },
					{ "noTarget", "自動行動沒有找到下一個目標。" },
					{ "aaon", "自動行動，啟動！" },
					{ "aaoff", "自動行動，關閉。" },
					{ "staminaCheck", "精力耗盡時停止" },
					{ "simpleIdentify", "簡單識別" },
					{ "off", "關閉" },
					{ "sameFarmfieldOnly", "只在同一田地上收割" },
					{ "inputKey", "请輸入要設置的按鍵" },
					{ "trigger", "設置自動行動觸發鍵" },
					{ "triggerRangeSelect", "設置自動行動觸發鍵" }
				}
			},
			{
				"JP",
				new Dictionary<string, string>
				{
					{ "autoact", "自動行動" },
					{ "settings", "自動行動設定" },
					{ "enemyEncounterResponse", "敵遭遇時の対応" },
					{ "eer0", "停止" },
					{ "eer1", "無視" },
					{ "eer2", "攻擊" },
					{ "detDist", "検出距離" },
					{ "pourDepth", "注水深さ" },
					{ "seedReapingCount", "種子収穫数" },
					{ "keyMode", "キーモード" },
					{ "press", "押す" },
					{ "toggle", "切り替え" },
					{ "start", "自動行動開始済み。" },
					{ "fail", "自動行動中断済み。" },
					{ "noTarget", "自動行動は次目標を発見できず。" },
					{ "aaon", "自動行動：オン。" },
					{ "aaoff", "自動行動：オフ。" },
					{ "staminaCheck", "精力が尽きた時に停止する" },
					{ "simpleIdentify", "簡単識別" },
					{ "off", "オフ" },
					{ "sameFarmfieldOnly", "同じ農地での収穫のみ" },
					{ "inputKey", "設定するキーを入力" },
					{ "trigger", "自動行動トリガーキーを設定" },
					{ "triggerRangeSelect", "範囲選択トリガーキーを設定" }
				}
			},
			{
				"EN",
				new Dictionary<string, string>
				{
					{ "autoact", "Auto Act" },
					{ "settings", "Auto Act Settings" },
					{ "enemyEncounterResponse", "Enemy Encounter Response" },
					{ "eer0", "Stop" },
					{ "eer1", "Ignore" },
					{ "eer2", "Attack" },
					{ "detDist", "Detection Distance" },
					{ "pourDepth", "Pouring Depth" },
					{ "seedReapingCount", "Count For Seed Reaping" },
					{ "keyMode", "Key Mode" },
					{ "press", "Press" },
					{ "toggle", "Toggle" },
					{ "start", "Auto Act started." },
					{ "fail", "Auto Act was interrupted." },
					{ "noTarget", "Auto Act could not find the next target." },
					{ "aaon", "Auto Act: On." },
					{ "aaoff", "Auto Act: Off." },
					{ "staminaCheck", "Stop When Stamina Runs Out" },
					{ "simpleIdentify", "Simple Identification" },
					{ "off", "Off" },
					{ "sameFarmfieldOnly", "Harvest On The Same Farmfield Only" },
					{ "inputKey", "Input the key to be set" },
					{ "trigger", "Set Auto Act Trigger Key" },
					{ "triggerRangeSelect", "Set Range Selection Trigger Key" }
				}
			},
			{
				"PTBR",
				new Dictionary<string, string>
				{
					{ "autoact", "Ação Automática" },
					{ "settings", "Configurações de Ação Automática" },
					{ "enemyEncounterResponse", "Resposta ao Encontro com Inimigos" },
					{ "eer0", "Parar" },
					{ "eer1", "Ignorar" },
					{ "eer2", "Atacar" },
					{ "detDist", "Distância de Detecção" },
					{ "pourDepth", "Profundidade de Derramamento" },
					{ "seedReapingCount", "Quantidade para Colheita de Sementes" },
					{ "keyMode", "Modo de Tecla" },
					{ "press", "Pressionar" },
					{ "toggle", "Alternar" },
					{ "start", "Ação Automática iniciada." },
					{ "fail", "Ação Automática foi interrompida." },
					{ "noTarget", "Ação Automática não encontrou o próximo alvo." },
					{ "aaon", "Ação Automática: Ligada." },
					{ "aaoff", "Ação Automática: Desligada." },
					{ "staminaCheck", "Parar Quando a Estamina Acabar" },
					{ "simpleIdentify", "Identificação Simples" },
					{ "off", "Desligado" },
					{ "sameFarmfieldOnly", "Colher Apenas na Mesma Área da Fazenda" },
					{ "inputKey", "Digite a tecla a ser configurada" },
					{ "trigger", "Definir tecla de Acionamento de Ação Automática" },
					{ "triggerRangeSelect", "Definir Tecla de Acionamento de Seleção de Alcance" }
				}
			}
		};

		public static string GetText(string text)
		{
			string key = EClass.core.config.lang;
			if (!langData.ContainsKey(key))
			{
				key = "EN";
			}
			return langData[key][text];
		}
	}
}
namespace AutoActMod.Patches
{
	[HarmonyPatch]
	internal static class Entrance
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Chara), "SetAI")]
		private static bool Chara_SetAI_Patch(Chara __instance, AIAct g)
		{
			if (!((Card)__instance).IsPC)
			{
				return true;
			}
			if (AutoActMod.IsSwitchOn)
			{
				return ClassExtension.IsNull((object)AutoAct.TrySetAutoAct(__instance, g));
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Act), "Perform", new Type[]
		{
			typeof(Chara),
			typeof(Card),
			typeof(Point)
		})]
		private static bool Act_Perform_Patch(Act __instance, Chara _cc, Card _tc, Point _tp, ref bool __result)
		{
			if (!((Card)_cc).IsPC || !AutoActMod.IsSwitchOn)
			{
				return true;
			}
			if (AutoAct.TrySetAutoAct(_cc, __instance, _tc, _tp).HasValue())
			{
				__result = false;
				return false;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ActPlan), "ShowContextMenu")]
		public static void ActPlan_ShowContextMenu_Patch(ActPlan __instance)
		{
			if (((object)__instance.pos).Equals((object)((Card)EClass.pc).pos))
			{
				Settings.SetupSettings(__instance);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Item), "Perform")]
		private static bool ActPlan_Item_Perform_Patch(Item __instance)
		{
			Act act = __instance.act;
			DynamicAct val = (DynamicAct)(object)((act is DynamicAct) ? act : null);
			if (val != null && val.id == AALang.GetText("settings"))
			{
				((Act)val).Perform();
				return false;
			}
			return true;
		}
	}
	[HarmonyPatch]
	public static class Gacha
	{
		public static InvOwner InvOwner;

		public static ButtonGrid Coin;

		public static TraitGachaBall GachaBall;

		public static long LastUpdate;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(InvOwner), "OnRightClick")]
		private static void InvOwner_OnRightClick_Patch(InvOwner __instance, ButtonGrid button)
		{
			if (AutoActMod.IsSwitchOn && __instance.destInvOwner is InvOwnerGacha)
			{
				Card card = button.card;
				Thing val = (Thing)(object)((card is Thing) ? card : null);
				if (val != null && !((Card)val).isDestroyed)
				{
					AutoActMod.Say(AALang.GetText("start"));
					InvOwner = __instance;
					Coin = button;
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(TraitGachaBall), "OnUse")]
		private static void TraitGachaBall_OnUse_Patch(TraitGachaBall __instance)
		{
			if (AutoActMod.IsSwitchOn && !GachaBall.HasValue())
			{
				AutoActMod.Say(AALang.GetText("start"));
				GachaBall = __instance;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AutoActMod), "Update")]
		internal static void Update()
		{
			if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
			{
				Reset();
				return;
			}
			long num = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			if (num - LastUpdate >= 40)
			{
				LastUpdate = num;
				AutoFeed();
				AutoOpen();
			}
		}

		internal static void AutoFeed()
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			if (ClassExtension.IsNull((object)InvOwner))
			{
				return;
			}
			InvOwner destInvOwner = InvOwner.destInvOwner;
			InvOwnerGacha val = (InvOwnerGacha)(object)((destInvOwner is InvOwnerGacha) ? destInvOwner : null);
			if (val != null)
			{
				Card card = Coin.card;
				Thing val2 = (Thing)(object)((card is Thing) ? card : null);
				if (val2 != null && !((Card)val2).isDestroyed)
				{
					new Transaction(Coin, ((Card)val2).Num, (InvOwner)null).Process(false);
					((LayerBaseCraft)((InvOwnerDraglet)val).dragGrid).RefreshCurrentGrid();
					return;
				}
			}
			Reset();
		}

		internal static void AutoOpen()
		{
			if (!ClassExtension.IsNull((object)GachaBall))
			{
				((Trait)GachaBall).OnUse(EClass.pc);
				if (((Trait)GachaBall).owner.isDestroyed)
				{
					GachaBall = null;
				}
			}
		}

		internal static void Reset()
		{
			InvOwner = null;
			Coin = null;
			GachaBall = null;
		}
	}
	[HarmonyPatch]
	internal static class HandleEnemy
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(CharaRenderer), "OnEnterScreen")]
		private static IEnumerable<CodeInstruction> CharaRenderer_OnEnterScreen_Patch(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			return new CodeMatcher(instructions, (ILGenerator)null).MatchEndForward((CodeMatch[])(object)new CodeMatch[3]
			{
				new CodeMatch((OpCode?)OpCodes.Ldarg_0, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Ldc_R4, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Stfld, (object)null, (string)null)
			}).Advance(1).InsertAndAdvance((CodeInstruction[])(object)new CodeInstruction[3]
			{
				(CodeInstruction)new CodeMatch((OpCode?)OpCodes.Ldarg_0, (object)null, (string)null),
				Transpilers.EmitDelegate<Action<CharaRenderer>>((Action<CharaRenderer>)delegate(CharaRenderer thiz)
				{
					if (((Card)thiz.owner).ExistsOnMap && !((Spatial)EClass._zone).IsRegion && thiz.owner.IsHostile() && EClass.pc.CanSeeLos((Card)(object)thiz.owner, -1))
					{
						OnSpotEnemy();
					}
				}),
				new CodeInstruction(OpCodes.Ret, (object)null)
			})
				.InstructionEnumeration();
		}

		[HarmonyTranspiler]
		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		private static IEnumerable<CodeInstruction> AI_Goto_Run_Patch(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected O, but got Unknown
			return new CodeMatcher(instructions, (ILGenerator)null).Start().MatchStartForward((CodeMatch[])(object)new CodeMatch[2]
			{
				new CodeMatch((OpCode?)OpCodes.Stfld, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Ldstr, (object)null, (string)null)
			}).InsertAndAdvance((CodeInstruction[])(object)new CodeInstruction[2]
			{
				new CodeInstruction(OpCodes.Ldloc_1, (object)null),
				Transpilers.EmitDelegate<Action<AI_Goto>>((Action<AI_Goto>)delegate(AI_Goto thiz)
				{
					if (((AIAct)thiz).parent is AutoAct autoAct)
					{
						autoAct.CancelRetry();
					}
				})
			})
				.InstructionEnumeration();
		}

		private static void OnSpotEnemy()
		{
			if (EClass.pc.ai.Current is GoalCombat)
			{
				return;
			}
			if (!AutoActMod.Active || Settings.EnemyEncounterResponse == 0)
			{
				if (EClass.core.config.game.haltOnSpotEnemy)
				{
					EClass.player.enemySpotted = true;
				}
			}
			else if (Settings.EnemyEncounterResponse == 2)
			{
				EClass.pc.FindNewEnemy();
				EClass.pc.SetAIAggro();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Chara), "SetAIAggro")]
		private static bool Chara_SetAIAggro_Patch(Chara __instance)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			if (__instance.ai.Current is GoalCombat)
			{
				return false;
			}
			GoalCombat val = (GoalCombat)(((Card)__instance).IsPC ? new GoalAutoCombat(__instance.enemy) : new GoalCombat());
			if (__instance.ai is AutoAct autoAct && ((AIAct)autoAct).IsRunning)
			{
				autoAct.InsertAction((AIAct)(object)val);
				return false;
			}
			__instance.SetAI((AIAct)(object)val);
			return false;
		}
	}
	[HarmonyPatch]
	internal static class HandleTrap
	{
		[HarmonyPatch]
		private static class ActWait_Search_Patch
		{
			private static MethodInfo TargetMethod()
			{
				return AccessTools.Method(AccessTools.FirstInner(typeof(ActWait), (Func<Type, bool>)((Type t) => t.Name.Contains("DisplayClass8_0"))), "<Search>b__0", (Type[])null, (Type[])null);
			}

			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Expected O, but got Unknown
				//IL_002f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Expected O, but got Unknown
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Expected O, but got Unknown
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Expected O, but got Unknown
				//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ad: Expected O, but got Unknown
				return new CodeMatcher(instructions, (ILGenerator)null).MatchStartForward((CodeMatch[])(object)new CodeMatch[4]
				{
					new CodeMatch((OpCode?)OpCodes.Ldsfld, (object)null, (string)null),
					new CodeMatch((OpCode?)OpCodes.Ldfld, (object)null, (string)null),
					new CodeMatch((OpCode?)OpCodes.Ldfld, (object)null, (string)null),
					new CodeMatch((OpCode?)OpCodes.Ldfld, (object)AccessTools.Field(typeof(GameConfig), "haltOnSpotTrap"), (string)null)
				}).RemoveInstructions(4).InsertAndAdvance((CodeInstruction[])(object)new CodeInstruction[1]
				{
					new CodeInstruction(Transpilers.EmitDelegate<Func<bool>>((Func<bool>)(() => !(EClass.pc.ai is AutoAct) && EClass.core.config.game.haltOnSpotTrap)))
				})
					.InstructionEnumeration();
			}
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(AI_Goto), "TryGoTo")]
		private static IEnumerable<CodeInstruction> AI_Goto_TryGoTo_Patch(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected O, but got Unknown
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			return new CodeMatcher(instructions, (ILGenerator)null).MatchStartForward((CodeMatch[])(object)new CodeMatch[5]
			{
				new CodeMatch((OpCode?)OpCodes.Ldarg_0, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Ldfld, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Ldloc_3, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Ldc_I4_1, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Callvirt, (object)AccessTools.Method(typeof(Card), "TryMove", (Type[])null, (Type[])null), (string)null)
			}).RemoveInstructions(5).InsertAndAdvance((CodeInstruction[])(object)new CodeInstruction[2]
			{
				new CodeInstruction(OpCodes.Ldarg_0, (object)null),
				new CodeInstruction(Transpilers.EmitDelegate<Func<AI_Goto, MoveResult>>((Func<AI_Goto, MoveResult>)CheckTrap))
			})
				.InstructionEnumeration();
		}

		private static MoveResult CheckTrap(AI_Goto move)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			Chara owner = ((AIAct)move).owner;
			if (!((Card)owner).IsPC || !(owner.ai is AutoAct autoAct))
			{
				return ((Card)owner).TryMove(Point.shared, true);
			}
			Trait obj = ((Card)(Point.shared.Things.Find(delegate(Thing t)
			{
				if (!((Card)t).isHidden)
				{
					Trait trait = ((Card)t).trait;
					TraitTrap val2 = (TraitTrap)(object)((trait is TraitTrap) ? trait : null);
					if (val2 != null)
					{
						return ((TraitSwitch)val2).CanDisarmTrap;
					}
				}
				return false;
			})?)).trait;
			TraitTrap val = (TraitTrap)(object)((obj is TraitTrap) ? obj : null);
			if (val == null)
			{
				return ((Card)owner).TryMove(Point.shared, true);
			}
			autoAct.InsertAction((AIAct)(object)new AutoActDisarm(val));
			return (MoveResult)1;
		}
	}
	[HarmonyPatch]
	internal static class Misc
	{
		[HarmonyPatch(typeof(TaskBuild), "OnProgressComplete")]
		private static class TaskBuild_OnProgressComplete_Patch
		{
			internal static bool Success;

			private static void Prefix()
			{
				Success = false;
			}

			private static void Postfix(TaskBuild __instance)
			{
				if (Success && ((AIAct)__instance).parent is AutoActBuild autoActBuild)
				{
					autoActBuild.OnChildSuccess();
				}
			}

			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Expected O, but got Unknown
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0085: Expected O, but got Unknown
				return new CodeMatcher(instructions, (ILGenerator)null).MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
				{
					new CodeMatch((OpCode?)OpCodes.Callvirt, (object)AccessTools.Method(typeof(Recipe), "Build", new Type[1] { typeof(TaskBuild) }, (Type[])null), (string)null)
				}).Advance(1).Insert((CodeInstruction[])(object)new CodeInstruction[1]
				{
					new CodeInstruction(Transpilers.EmitDelegate<Action>((Action)delegate
					{
						Success = true;
					}))
				})
					.InstructionEnumeration();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Card), "MoveImmediate")]
		private static void Card_MoveImmediate_Patch(Card __instance, ref bool cancelAI)
		{
			Chara val = (Chara)(object)((__instance is Chara) ? __instance : null);
			if (val != null && val.ai is AutoAct)
			{
				cancelAI = false;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AIAct), "Success")]
		private static bool AIAct_Success_Patch(AIAct __instance, ref Status __result)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Invalid comparison between Unknown and I4
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Invalid comparison between Unknown and I4
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected I4, but got Unknown
			if (__instance.child.HasValue() && (int)__instance.child.status == 1 && (ClassExtension.IsNull((object)__instance.onChildFail) || (int)__instance.onChildFail() == 1))
			{
				if (__instance is AutoAct autoAct)
				{
					autoAct.CancelRetry();
				}
				if (__instance.parent is AutoAct autoAct2)
				{
					autoAct2.CancelRetry();
				}
				__result = (Status)(int)__instance.Cancel();
				return false;
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(AIAct), "OnSuccess")]
		private static void AIAct_OnSuccess_Patch(AIAct __instance)
		{
			if (__instance.parent is AutoAct autoAct)
			{
				autoAct.OnChildSuccess();
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Progress_Custom), "OnProgressComplete")]
		private static void Progress_Custom_OnProgressComplete_Patch(Progress_Custom __instance)
		{
			if (((AIAct)__instance).parent?.parent is AutoAct autoAct)
			{
				autoAct.OnChildSuccess();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HotItemHeld), "OnSetCurrentItem")]
		private static void HotItemHeld_OnSetCurrentItem_Patch()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			if (EClass.pc.ai is AutoActBuild autoActBuild && ((AIAct)autoActBuild).IsRunning && autoActBuild.range.Count > 0 && !autoActBuild.CheckHeld())
			{
				((AIAct)autoActBuild).Cancel();
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Dialog), "OnUpdateInput")]
		private static bool Dialog_OnUpdateInput_Patch(Dialog __instance)
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			if (ClassExtension.IsNull((object)Settings.ChangingKey))
			{
				return true;
			}
			List<KeyCode> list = new List<KeyCode>
			{
				(KeyCode)323,
				(KeyCode)324,
				(KeyCode)325,
				(KeyCode)326,
				(KeyCode)327
			};
			List<KeyCode> list2 = new List<KeyCode>
			{
				(KeyCode)27,
				(KeyCode)13,
				(KeyCode)127,
				(KeyCode)8
			};
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				if (!list.Contains(value) && Input.GetKey(value))
				{
					if (!list2.Contains(value))
					{
						Settings.ChangingKey.Value = value;
						Settings.ChangingKey = null;
					}
					((Layer)__instance).Close();
					return false;
				}
			}
			return true;
		}
	}
	[HarmonyPatch]
	internal static class NameHint
	{
		[HarmonyPatch]
		private static class Rename
		{
			private static IEnumerable<MethodInfo> TargetMethods()
			{
				return new <>z__ReadOnlyArray<MethodInfo>(new MethodInfo[2]
				{
					AccessTools.Method(typeof(AI_Shear), "GetText", (Type[])null, (Type[])null),
					AccessTools.Method(typeof(TaskClean), "GetText", (Type[])null, (Type[])null)
				});
			}

			private static void Postfix(ref string __result)
			{
				if (AutoActMod.IsSwitchOn)
				{
					EditText(ref __result);
				}
			}
		}

		public static void EditText(ref string str)
		{
			str = str + "(" + AALang.GetText("autoact") + ")";
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Act), "GetText")]
		private static void Act_GetText_Patch(Act __instance, ref string __result)
		{
			if (!AutoActMod.IsSwitchOn)
			{
				return;
			}
			Card card = EClass.scene.mouseTarget.card;
			if ((!(__instance is ActDrawWater) && !(__instance is AI_TendAnimal) && !(__instance is TaskMine) && !(__instance is TaskHarvest) && !(__instance is TaskDrawWater)) || 1 == 0)
			{
				TaskWater val = (TaskWater)(object)((__instance is TaskWater) ? __instance : null);
				if ((val == null || !val.dest.cell.HasFire) && (!(__instance is TaskDig) || (!((Spatial)EClass._zone).IsRegion && (ClassExtension.Contains(((RenderRow)Scene.HitPoint.cell.sourceSurface).tag, "grass") || Scene.HitPoint.HasBridge))))
				{
					if (__instance is AI_OpenLock)
					{
						Thing val2 = (Thing)(object)((card is Thing) ? card : null);
						if (val2 != null && AutoActUnlock.NeedUnlock(val2))
						{
							goto IL_00df;
						}
					}
					if (!(__instance is ActThrow))
					{
						return;
					}
					Chara val3 = (Chara)(object)((card is Chara) ? card : null);
					if (val3 == null || !AutoActThrowMilk.NeedMilk(val3))
					{
						return;
					}
				}
			}
			goto IL_00df;
			IL_00df:
			EditText(ref __result);
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(DynamicAct), "GetText")]
		private static void DynamicAct_GetText_Patch(DynamicAct __instance, ref string __result)
		{
			if (AutoActMod.IsSwitchOn)
			{
				Card card = EClass.scene.mouseTarget.card;
				Chara val = (Chara)(object)((card is Chara) ? card : null);
				if (ClassExtension.Contains(new string[3] { "actMilk", "actPickOne", "actHold" }, __instance.id) || (__instance.id == "AI_Slaughter" && val.HasValue() && AutoActSlaughter.CanBeSlaughtered(val)) || (__result == ((BaseRow)Element.Get(6011)).GetName() && (ClassExtension.IsNull((object)val) || AutoActSteal.IsTargetChara(val))) || (__result == AutoActSmash.GetActMeleeLang() && AutoActSmash.CanSmash(card)))
				{
					EditText(ref __result);
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(InvOwner), "GetAutoUseLang")]
		private static void InvOwner_GetAutoUseLang_Patch(InvOwner __instance, ButtonGrid button, ref string __result)
		{
			if (!AutoActMod.IsSwitchOn)
			{
				return;
			}
			Card card = button.card;
			Thing val = (Thing)(object)((card is Thing) ? card : null);
			if (val != null && ((List<Interaction>)(object)__instance.ListInteractions(button, false)).Count != 0)
			{
				bool flag = AutoActRead.CanRead(val);
				if (!flag)
				{
					Trait trait = ((Card)val).trait;
					bool flag2 = ((trait is TraitBookSkill || trait is TraitGachaBall) ? true : false);
					flag = flag2;
				}
				if (flag)
				{
					EditText(ref __result);
				}
			}
		}
	}
	[HarmonyPatch]
	internal static class RangeSelect
	{
		internal static Point LeftClickPoint;

		internal static Point RightClickPoint;

		internal static Point StartPos = new Point();

		internal static HashSet<Point> Range = new HashSet<Point>();

		internal static List<Point> Selected = new List<Point>();

		internal static List<Chara> CharaRange = new List<Chara>();

		internal static Thing LastHeld;

		internal static Action OnSelectComplete;

		internal static bool UseCenter = false;

		internal static int Width;

		internal static int Height;

		public static long LastColorChange = 0L;

		public static int CharaHighlightColor = 0;

		internal static bool Active
		{
			get
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				if (!Input.GetKey(Settings.RangeSelectKeyCode))
				{
					return Selected.Count > 0;
				}
				return true;
			}
		}

		private static void AddRange()
		{
			Func<Chara, bool> check;
			if (OnSelectComplete == new Action(SetAutoActSlaughter))
			{
				check = AutoActSlaughter.CanBeSlaughtered;
			}
			else
			{
				if (!(OnSelectComplete == new Action(SetAutoActBrush)))
				{
					Selected.ForEach(delegate(Point p)
					{
						Range.Add(p);
					});
					UpdateRangeInfo();
					return;
				}
				check = AutoActBrush.CanBeBrushed;
			}
			foreach (Point item in Selected)
			{
				item.Charas.ForEach(delegate(Chara chara)
				{
					if (check(chara) && !CharaRange.Contains(chara))
					{
						CharaRange.Add(chara);
					}
				});
			}
		}

		private static void RemoveRange()
		{
			Selected.ForEach(delegate(Point p)
			{
				if (Range.Contains(p))
				{
					Range.Remove(p);
				}
				CharaRange.RemoveAll((Chara chara) => ((object)((Card)chara).pos).Equals((object)p));
			});
			UpdateRangeInfo();
		}

		private static void UpdateRangeInfo()
		{
			if (Range.Count == 0)
			{
				return;
			}
			Point val = Range.First().Copy();
			Point val2 = val.Copy();
			foreach (Point item in Range)
			{
				val.x = Math.Min(val.x, item.x);
				val.z = Math.Min(val.z, item.z);
				val2.x = Math.Max(val2.x, item.x);
				val2.z = Math.Max(val2.z, item.z);
			}
			Width = val2.x - val.x + 1;
			Height = val2.z - val.z + 1;
			StartPos.Set(val);
		}

		private static void SetSelected(Point p1, Point p2, bool edgeOnly = false)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			Selected.Clear();
			int num = Math.Min(p1.x, p2.x);
			int num2 = Math.Min(p1.z, p2.z);
			int num3 = Math.Max(p1.x, p2.x);
			int num4 = Math.Max(p1.z, p2.z);
			for (int i = num; i <= num3; i++)
			{
				for (int j = num2; j <= num4; j++)
				{
					if (!edgeOnly || i == num || i == num3 || j == num2 || j == num4)
					{
						Point val = new Point(i, j);
						if (val.IsInBounds)
						{
							Selected.Add(val);
						}
					}
				}
			}
		}

		internal static void Reset()
		{
			if (!(EClass.pc.ai is AutoAct) || !EClass.pc.ai.IsRunning)
			{
				Range.Clear();
				Selected.Clear();
				CharaRange.Clear();
				LeftClickPoint = null;
				LastHeld = null;
				OnSelectComplete = null;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AM_Adv), "SetPressedAction")]
		private static bool SetPressedAction_Patch()
		{
			return !Active;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(AM_Adv), "_OnUpdateInput")]
		private static void AM_Adv_OnUpdateInput_Patch()
		{
			if ((LastHeld.HasValue() && LastHeld != EClass.pc.held) || EInput.middleMouse.down)
			{
				Reset();
				return;
			}
			Card held = EClass.pc.held;
			Thing val = (Thing)(object)((held is Thing) ? held : null);
			if (val == null)
			{
				OnSelectComplete = SetAutoActPick;
			}
			else
			{
				bool flag = HotItemHeld.taskBuild.HasValue() && (((Card)val).Num > 1 || ((Card)val).trait is TraitSeed);
				if (flag)
				{
					Trait trait = ((Card)val).trait;
					bool flag2 = ((trait is TraitSeed || trait is TraitFloor || trait is TraitPlatform || trait is TraitBlock || trait is TraitFertilizer) ? true : false);
					flag = flag2;
				}
				if (flag)
				{
					OnSelectComplete = SetAutoActBuild;
				}
				else if (((Card)val).trait is TraitTool && ((Card)val).HasElement(230, 1))
				{
					OnSelectComplete = SetAutoActDig;
				}
				else if (((Card)val).trait is TraitTool && ((Card)val).HasElement(286, 1))
				{
					OnSelectComplete = SetAutoActPlow;
				}
				else if (((Card)val).trait is TraitToolWaterPot)
				{
					OnSelectComplete = SetAutoActPourWaterOrDrawWater;
				}
				else if (((Card)val).trait is TraitToolButcher)
				{
					OnSelectComplete = SetAutoActSlaughter;
				}
				else if (((Card)val).trait is TraitToolBrush && ((Card)val).HasElement(237, 1))
				{
					OnSelectComplete = SetAutoActBrush;
				}
				else
				{
					Trait trait = ((Card)val).trait;
					if ((trait is TraitToolShears || trait is TraitToolWaterCan || trait is TraitToolMusic || trait is TraitFertilizer) ? true : false)
					{
						Reset();
						return;
					}
					OnSelectComplete = SetAutoActHarvestMine;
				}
			}
			Card held2 = EClass.pc.held;
			LastHeld = (Thing)(object)((held2 is Thing) ? held2 : null);
			if (LeftClickPoint.HasValue())
			{
				SetSelected(LeftClickPoint, EClass.scene.mouseTarget.pos, ((Card)(LastHeld?)).trait is TraitBlock);
			}
			else if (RightClickPoint.HasValue())
			{
				SetSelected(RightClickPoint, EClass.scene.mouseTarget.pos);
			}
			else
			{
				Selected.Clear();
			}
			if (!Active)
			{
				if ((Range.Count > 0 || CharaRange.Count > 0) && !(EClass.pc.ai is AutoAct))
				{
					OnSelectComplete?.Invoke();
				}
			}
			else if (EInput.leftMouse.down)
			{
				if (LeftClickPoint.HasValue())
				{
					AddRange();
					Selected.Clear();
					LeftClickPoint = null;
				}
				else if (RightClickPoint.HasValue())
				{
					RemoveRange();
					Selected.Clear();
					RightClickPoint = null;
				}
				else
				{
					LeftClickPoint = Scene.HitPoint.Copy();
				}
			}
			else if (EInput.rightMouse.down)
			{
				if (LeftClickPoint.HasValue())
				{
					Selected.Clear();
					LeftClickPoint = null;
				}
				else if (RightClickPoint.HasValue())
				{
					RemoveRange();
					Selected.Clear();
					RightClickPoint = null;
				}
				else
				{
					RightClickPoint = Scene.HitPoint.Copy();
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "MarkMapHighlights")]
		private static void MarkMapHighlights_Patch()
		{
			if (CharaRange.Count > 0)
			{
				long num = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				if (num - LastColorChange > 400)
				{
					LastColorChange = num;
					CharaHighlightColor = (CharaHighlightColor + 1) % 2;
				}
				int cColor = ((CharaHighlightColor == 0) ? 2 : 8);
				CharaRange.ForEach(delegate(Chara c)
				{
					((Card)c).pos.SetHighlight(cColor);
				});
			}
			else
			{
				Range.RemoveWhere(delegate(Point p)
				{
					if (p.IsInBounds)
					{
						p.SetHighlight(8);
					}
					return !p.IsInBounds;
				});
			}
			int color = (LeftClickPoint.HasValue() ? 8 : 4);
			Selected.RemoveAll(delegate(Point p)
			{
				if (p.IsInBounds)
				{
					p.SetHighlight(color);
				}
				return !p.IsInBounds;
			});
		}

		private static void OnRangeActionStart(AutoAct autoAct)
		{
			autoAct.startPos = StartPos;
			autoAct.startDir = 2;
		}

		private static AutoAct SetAutoAct(AutoAct a)
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Expected O, but got Unknown
			AutoAct autoAct = AutoAct.SetAutoAct(EClass.pc, a);
			autoAct.useOriginalPos = false;
			bool flag = ((autoAct is AutoActPlow || autoAct is AutoActDig || autoAct is AutoActPourWater) ? true : false);
			if ((flag || (autoAct is AutoActBuild autoActBuild && !(autoActBuild.Child.held.trait is TraitBlock))) && Width == Height && (Width == 3 || Width == 5))
			{
				Point val = new Point(StartPos.x + Width / 2, StartPos.z + Width / 2);
				if (Range.Contains(val))
				{
					AIAct child = ((AIAct)autoAct).child;
					((TaskPoint)((child is TaskPoint) ? child : null)).pos = val;
					autoAct.useOriginalPos = true;
					autoAct.InsertAction((AIAct)new AI_Goto(val, 0, true, false));
				}
			}
			return autoAct;
		}

		private static void SetAutoActBuild()
		{
			AutoActBuild autoAct = new AutoActBuild(HotItemHeld.taskBuild)
			{
				hasSowRange = true,
				onStart = OnRangeActionStart
			};
			autoAct.SetRange(Range);
			Func<Point, bool> pointChecker = (Point p) => true;
			Trait trait = EClass.pc.held.trait;
			TraitSeed val = (TraitSeed)(object)((trait is TraitSeed) ? trait : null);
			if (val != null)
			{
				if (((TileRow)val.row).id == 88 && Range.FirstOrDefault((Point p) => p.IsWater).HasValue())
				{
					pointChecker = (Point p) => p.IsWater;
				}
				else if (Range.FirstOrDefault((Point p) => p.IsFarmField).HasValue())
				{
					pointChecker = (Point p) => p.IsFarmField;
				}
			}
			Range.RemoveWhere((Point p) => !autoAct.PointChecker(p) || !pointChecker(p));
			if (Range.Count > 0)
			{
				SetAutoAct(autoAct);
			}
		}

		private static void SetAutoActDig()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TaskDig source = new TaskDig
			{
				pos = StartPos.Copy(),
				mode = (Mode)2
			};
			AutoActDig autoAct = new AutoActDig(source)
			{
				w = Width,
				h = Height,
				onStart = OnRangeActionStart,
				range = Range
			};
			Range.RemoveWhere((Point p) => !autoAct.Filter(p.cell));
			if (Range.Count > 0)
			{
				SetAutoAct(autoAct);
			}
		}

		private static void SetAutoActPlow()
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			Range.RemoveWhere((Point p) => !AutoActPlow.Filter(p.cell));
			if (Range.Count != 0)
			{
				SetAutoAct(new AutoActPlow(new TaskPlow
				{
					pos = StartPos.Copy()
				})
				{
					w = Width,
					h = Height,
					onStart = OnRangeActionStart,
					range = Range
				});
			}
		}

		private static void SetAutoActPourWaterOrDrawWater()
		{
			Trait trait = EClass.pc.held.trait;
			TraitToolWaterPot val = (TraitToolWaterPot)(object)((trait is TraitToolWaterPot) ? trait : null);
			if (((Trait)val).owner.c_charges == 0 || Range.Count((Point p) => AutoActDrawWater.CanDrawWaterSimple(p.cell)) > Range.Count / 2)
			{
				SetAutoActDrawWater(val);
			}
			else
			{
				SetAutoActPourWater(val);
			}
		}

		private static void SetAutoActDrawWater(TraitToolWaterPot pot)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected O, but got Unknown
			Range.RemoveWhere((Point p) => !AutoActDrawWater.CanDrawWaterSimple(p.cell));
			if (Range.Count != 0)
			{
				SetAutoAct(new AutoActDrawWater(new TaskDrawWater
				{
					pos = StartPos,
					pot = pot
				})
				{
					onStart = OnRangeActionStart,
					simpleIdentify = 1,
					range = Range
				});
			}
		}

		private static void SetAutoActPourWater(TraitToolWaterPot pot)
		{
			Range.RemoveWhere((Point p) => !AutoActPourWater.CanPourWater(p.cell));
			if (Range.Count != 0)
			{
				SetAutoAct(new AutoActPourWater(new AutoActPourWater.SubActPourWater
				{
					pos = StartPos.Copy(),
					pot = pot,
					targetCount = Settings.PourDepth
				})
				{
					w = Width,
					h = Height,
					onStart = OnRangeActionStart,
					range = Range
				});
			}
		}

		private static void SetAutoActHarvestMine()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			TaskHarvest source = new TaskHarvest
			{
				pos = StartPos
			};
			Range.RemoveWhere(delegate(Point p)
			{
				BaseRow key;
				if (p.HasObj)
				{
					key = (BaseRow)(object)p.sourceObj;
				}
				else
				{
					if (!p.HasBlock)
					{
						return true;
					}
					key = (BaseRow)(object)p.sourceBlock;
				}
				if (TaskMine.CanMine(p, EClass.pc.held))
				{
					return false;
				}
				if (AutoAct.RowCheckCache.TryGetValue(key, out var value))
				{
					return value;
				}
				value = !AutoActHarvestMine.CanHarvest(EClass.pc, p);
				AutoAct.RowCheckCache.Add(key, value);
				return value;
			});
			AutoAct.RowCheckCache.Clear();
			if (Range.Count != 0)
			{
				SetAutoAct(new AutoActHarvestMine((BaseTaskHarvest)(object)source).SetRange(Range));
			}
		}

		private static void SetAutoActSlaughter()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			if (CharaRange.Count != 0)
			{
				SetAutoAct(new AutoActSlaughter((AIAct)new AI_Slaughter())
				{
					range = CharaRange
				});
			}
		}

		private static void SetAutoActBrush()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			if (CharaRange.Count != 0)
			{
				SetAutoAct(new AutoActBrush(new AI_TendAnimal())
				{
					range = CharaRange
				});
			}
		}

		private static void SetAutoActPick()
		{
			AutoActPick autoAct = new AutoActPick(new AutoActPick.SubActPick
			{
				pos = StartPos.Copy(),
				installed = false,
				pickAll = true
			})
			{
				range = Range
			};
			Range.RemoveWhere((Point p) => ClassExtension.IsNull((object)p.Things.Find((Thing t) => (int)((Card)t).placeState != 2)));
			if (Range.Count > 0)
			{
				SetAutoAct(autoAct);
			}
		}
	}
	[HarmonyPatch]
	internal static class VanillaFix
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(AIAct), "SetChild")]
		private static void AIAct_SetChild_Patch(AIAct __instance, AIAct seq)
		{
			AI_Goto val = (AI_Goto)(object)((seq is AI_Goto) ? seq : null);
			bool flag = val != null;
			bool flag2;
			if (flag)
			{
				if (!(__instance is AI_Shear) && !(__instance is AI_Fuck) && !(__instance is AI_Slaughter))
				{
					TaskPoint val2 = (TaskPoint)(object)((__instance is TaskPoint) ? __instance : null);
					if (val2 == null || val2 is TaskPlow)
					{
						flag2 = false;
						goto IL_003f;
					}
				}
				flag2 = true;
				goto IL_003f;
			}
			goto IL_0041;
			IL_003f:
			flag = flag2;
			goto IL_0041;
			IL_0041:
			if (flag)
			{
				val.ignoreConnection = true;
			}
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(Recipe), "Build", new Type[]
		{
			typeof(Chara),
			typeof(Card),
			typeof(Point),
			typeof(int),
			typeof(int),
			typeof(int),
			typeof(int)
		})]
		private static IEnumerable<CodeInstruction> Recipe_Build_Patch(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			return new CodeMatcher(instructions, (ILGenerator)null).MatchEndForward((CodeMatch[])(object)new CodeMatch[2]
			{
				new CodeMatch((OpCode?)OpCodes.Ldarg_3, (object)null, (string)null),
				new CodeMatch((OpCode?)OpCodes.Callvirt, (object)AccessTools.Method(typeof(Point), "ListCharas", (Type[])null, (Type[])null), (string)null)
			}).RemoveInstruction().Insert((CodeInstruction[])(object)new CodeInstruction[1] { Transpilers.EmitDelegate<Func<Point, IList<Chara>>>((Func<Point, IList<Chara>>)((Point p) => ClassExtension.Copy<Chara>((IList<Chara>)p.ListCharas()))) })
				.InstructionEnumeration();
		}
	}
}
namespace AutoActMod.Actions
{
	public class AutoAct : AIAct
	{
		public delegate AutoAct TryCreateDelegate(AIAct source);

		public delegate AutoAct TryCreateByActDelegate(string id, Card target, Point pos);

		public class Selector
		{
			public Point curtPoint;

			public Card item;

			public int factor1;

			public int factor2;

			public int factor3;

			public int factor4;

			public Point FinalPoint
			{
				get
				{
					Point result = curtPoint;
					Reset();
					return result;
				}
			}

			public Card FinalTarget
			{
				get
				{
					Card result = item;
					Reset();
					return result;
				}
			}

			public int MaxDist2 => (int)Math.Pow((float)factor1 + 1.5f, 2.0);

			public void Reset()
			{
				curtPoint = null;
				item = null;
				factor1 = 0;
				factor2 = 0;
				factor3 = 0;
				factor4 = 0;
				RowCheckCache.Clear();
			}

			public void Set(Point p, int v1, int v2, int v3 = 0, int v4 = 0)
			{
				curtPoint = p;
				factor1 = v1;
				factor2 = v2;
				factor3 = v3;
				factor4 = v4;
			}

			public void Set(Card c, int v1, int v2, int v3 = 0, int v4 = 0)
			{
				curtPoint = c.pos;
				item = c;
				factor1 = v1;
				factor2 = v2;
				factor3 = v3;
				factor4 = v4;
			}

			public bool TrySet(Point p, int v1, int v2 = 0, int v3 = 0, int v4 = 0)
			{
				if (ClassExtension.IsNull((object)p))
				{
					return false;
				}
				if (ClassExtension.IsNull((object)curtPoint))
				{
					Set(p, v1, v2, v3, v4);
					return true;
				}
				if (v1 < factor1 || (v1 == factor1 && v2 < factor2) || (v1 == factor1 && v2 == factor2 && v3 < factor3))
				{
					Set(p, v1, v2, v3, v4);
					return true;
				}
				return false;
			}

			public bool TrySet(Card c, int v1, int v2 = 0, int v3 = 0, int v4 = 0)
			{
				if (ClassExtension.IsNull((object)c))
				{
					return false;
				}
				if (ClassExtension.IsNull((object)curtPoint))
				{
					Set(c, v1, v2, v3, v4);
					return true;
				}
				if (v1 < factor1 || (v1 == factor1 && v2 < factor2) || (v1 == factor1 && v2 == factor2 && v3 < factor3))
				{
					Set(c, v1, v2, v3, v4);
					return true;
				}
				return false;
			}
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass48_0
		{
			public AIAct last;

			public AutoAct <>4__this;
		}

		public int targetId;

		public TileRow targetRow;

		public string targetName;

		public bool useOriginalPos;

		public bool canContinue = true;

		public PlaceState targetPlaceState;

		public Point startPos;

		public int startDir;

		public Action<AutoAct> onStart;

		public Selector selector = new Selector();

		public static bool IsSetting = false;

		public static readonly Dictionary<BaseRow, bool> RowCheckCache = new Dictionary<BaseRow, bool>();

		public static readonly List<Type> SubClasses = new List<Type>();

		public static readonly List<TryCreateDelegate> TryCreateMethods = new List<TryCreateDelegate>();

		public static readonly List<TryCreateByActDelegate> TryCreateByActMethods = new List<TryCreateByActDelegate>();

		public virtual Point Pos
		{
			get
			{
				AIAct child = base.child;
				return ((TaskPoint)(((child is TaskPoint) ? child : null)?)).pos;
			}
		}

		public Cell Cell => Pos.cell;

		public PathProgress Path => base.owner.path;

		public override int MaxRestart => 1;

		public static void Register(Assembly assembly)
		{
			SubClasses.AddRange(from t in assembly.GetTypes()
				where t.IsSubclassOf(typeof(AutoAct))
				select t);
		}

		public static void InitTryCreateMethods()
		{
			SubClasses.Sort((Type a, Type b) => GetPriority(a) - GetPriority(b));
			SubClasses.ForEach(delegate(Type t)
			{
				MethodInfo method = t.GetMethod("TryCreate", new Type[1] { typeof(AIAct) });
				if (method.HasValue())
				{
					TryCreateMethods.Add((TryCreateDelegate)method.CreateDelegate(typeof(TryCreateDelegate)));
				}
				MethodInfo method2 = t.GetMethod("TryCreate", new Type[3]
				{
					typeof(string),
					typeof(Card),
					typeof(Point)
				});
				if (method2.HasValue())
				{
					TryCreateByActMethods.Add((TryCreateByActDelegate)method2.CreateDelegate(typeof(TryCreateByActDelegate)));
				}
			});
			static int GetPriority(Type t)
			{
				FieldInfo field = t.GetField("Priority");
				if (!ClassExtension.IsNull((object)field))
				{
					return (int)field.GetValue(null);
				}
				return 100;
			}
		}

		public AutoAct()
		{
		}

		public AutoAct(AIAct source)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			base.child = source;
			base.child.status = (Status)1;
		}

		public static AIAct TryGetAutoAct(AIAct source)
		{
			AIAct val = source;
			do
			{
				if (val is AutoAct autoAct && ((AIAct)autoAct).IsRunning)
				{
					return null;
				}
				val = val.parent;
			}
			while (val.HasValue());
			foreach (TryCreateDelegate tryCreateMethod in TryCreateMethods)
			{
				AutoAct autoAct2 = tryCreateMethod(source);
				if (autoAct2.HasValue())
				{
					return (AIAct)(object)autoAct2;
				}
			}
			return null;
		}

		public static AutoAct TryGetAutoAct(string lang, Card target, Point p)
		{
			foreach (TryCreateByActDelegate tryCreateByActMethod in TryCreateByActMethods)
			{
				AutoAct autoAct = tryCreateByActMethod(lang, target, p);
				if (autoAct.HasValue())
				{
					return autoAct;
				}
			}
			return null;
		}

		public static AutoAct TrySetAutoAct(Chara chara, AIAct source)
		{
			DynamicAIAct val = (DynamicAIAct)(object)((source is DynamicAIAct) ? source : null);
			if (val != null)
			{
				return TrySetAutoAct(chara, (Act)(object)val, Act.TC, Act.TP.Copy());
			}
			source.owner = chara;
			if (!(TryGetAutoAct(source) is AutoAct autoAct))
			{
				return null;
			}
			SetAutoAct(chara, autoAct);
			return autoAct;
		}

		public static AutoAct TrySetAutoAct(Chara chara, Act source, Card target, Point p)
		{
			DynamicAct val = (DynamicAct)(object)((source is DynamicAct) ? source : null);
			string text;
			if (val == null)
			{
				DynamicAIAct val2 = (DynamicAIAct)(object)((source is DynamicAIAct) ? source : null);
				text = ((val2 == null) ? source.GetText("") : val2.lang);
			}
			else
			{
				text = ((Act)val).GetText("");
			}
			string text2 = text;
			string oldValue = "(" + AALang.GetText("autoact") + ")";
			AutoAct autoAct = TryGetAutoAct(text2.Replace(oldValue, ""), target, p);
			if (autoAct == null)
			{
				return null;
			}
			SetAutoAct(chara, autoAct, isAct: true);
			return autoAct;
		}

		public static AutoAct SetAutoAct(Chara chara, AutoAct autoAct, bool isAct = false)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			bool hasNoGoal = chara.HasNoGoal;
			IsSetting = true;
			autoAct.useOriginalPos = ((Card)chara).IsPC;
			chara.ai.status = (Status)((!hasNoGoal) ? 1 : 2);
			chara.SetAI((AIAct)(object)autoAct);
			IsSetting = false;
			if (isAct && (EClass.scene.actionMode != ActionMode.Sim || !EClass.scene.paused) && hasNoGoal && !((CardRenderer)/*isinst with value type is only supported in some contexts*/).IsMoving)
			{
				((AIAct)autoAct).Tick();
			}
			return autoAct;
		}

		public Status StartNextTask(bool resetRestartCount = true)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			return SetNextTask(base.child, null, resetRestartCount);
		}

		public Status StartNextTask(Func<Status> _onChildFail, bool resetRestartCount = true)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			return SetNextTask(base.child, _onChildFail, resetRestartCount);
		}

		public Status SetNextTask(AIAct a, Func<Status> _onChildFail = null, bool resetRestartCount = true)
		{
			((AIAct)this).SetChild(a, _onChildFail);
			Task val = (Task)(object)((a is Task) ? a : null);
			if (val != null)
			{
				val.isDestroyed = false;
			}
			BaseTaskHarvest val2 = (BaseTaskHarvest)(object)((a is BaseTaskHarvest) ? a : null);
			if (val2 != null)
			{
				val2.SetTarget(base.owner, (Thing)null);
			}
			if (resetRestartCount)
			{
				base.restartCount = 0;
			}
			return (Status)0;
		}

		public override bool CanProgress()
		{
			if (!canContinue)
			{
				return false;
			}
			Chara owner = base.owner;
			if ((!((owner != null) ? new bool?(((Card)owner).IsPCFaction) : null)) ?? false)
			{
				return true;
			}
			if (Settings.StaminaCheck)
			{
				return base.owner.stamina.value >= 0;
			}
			return true;
		}

		public override bool CanManualCancel()
		{
			CancelRetry();
			RangeSelect.Reset();
			return true;
		}

		public void CancelRetry()
		{
			base.restartCount = (byte)((AIAct)this).MaxRestart;
		}

		public override void OnStart()
		{
			SayStart();
			SetStartPos();
			AIAct child = base.child;
			if (child != null)
			{
				child.Reset();
			}
			onStart?.Invoke(this);
		}

		public override void OnSuccess()
		{
			CancelRetry();
		}

		public virtual void OnChildSuccess()
		{
		}

		public Status Retry()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (ClassExtension.IsNull((object)base.child) || !((AIAct)this).CanProgress())
			{
				return Fail();
			}
			base.child.status = (Status)2;
			return ((AIAct)this).KeepRunning();
		}

		public Status Fail()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			CancelRetry();
			return ((AIAct)this).Cancel();
		}

		public Status FailOrSuccess()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (canContinue)
			{
				return Fail();
			}
			return ((AIAct)this).Success((Action)null);
		}

		public override Status Cancel()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			base.restartCount++;
			if (base.restartCount <= ((AIAct)this).MaxRestart)
			{
				return Retry();
			}
			SayFail();
			return ((AIAct)this).Cancel();
		}

		public override void OnCancelOrSuccess()
		{
			RangeSelect.Reset();
		}

		public void InsertAction(AIAct action)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			<>c__DisplayClass48_0 CS$<>8__locals0 = new <>c__DisplayClass48_0();
			CS$<>8__locals0.<>4__this = this;
			if (ClassExtension.IsNull((object)base.Enumerator))
			{
				((AIAct)this).Tick();
			}
			if (ClassExtension.IsNull((object)base.child))
			{
				((AIAct)this).SetChild(action, (Func<Status>)base.KeepRunning);
				return;
			}
			base.child.SetOwner(base.owner);
			CS$<>8__locals0.last = (AIAct)(object)this;
			while (true)
			{
				AIAct child = CS$<>8__locals0.last.child;
				if ((!((child != null) ? new bool?(child.IsRunning) : null)) ?? true)
				{
					break;
				}
				CS$<>8__locals0.last = CS$<>8__locals0.last.child;
				CS$<>8__locals0.last.Enumerator = Enumerable.Repeat<Status>((Status)2, 1).GetEnumerator();
			}
			CS$<>8__locals0.last.Enumerator = OnEnd().GetEnumerator();
			CS$<>8__locals0.last.SetChild(action, (Func<Status>)base.KeepRunning);
			[IteratorStateMachine(typeof(<>c__DisplayClass48_0.<<InsertAction>g__OnEnd|0>d))]
			IEnumerable<Status> OnEnd()
			{
				//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
				return new <>c__DisplayClass48_0.<<InsertAction>g__OnEnd|0>d(-2)
				{
					<>4__this = CS$<>8__locals0
				};
			}
		}

		public void SetTarget(TileRow r)
		{
			int num = r.id;
			if (num == 167)
			{
				num = 1;
			}
			targetId = num;
			targetRow = r;
			targetName = ((RenderRow)r).name;
		}

		public void SetTarget(Card c)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			targetName = c.id;
			targetPlaceState = c.placeState;
		}

		public bool IsTarget(TileRow r)
		{
			if (targetId == -1)
			{
				if (!(r is Row))
				{
					Row val = (Row)(object)((r is Row) ? r : null);
					if (val != null)
					{
						return ((RenderRow)val).tileType.IsBlockMount;
					}
					return false;
				}
				return true;
			}
			if (targetId == -2)
			{
				Row val2 = (Row)(object)((r is Row) ? r : null);
				if (val2 != null && val2.HasGrowth)
				{
					return val2.growth.IsTree;
				}
				return false;
			}
			if (targetId == -3)
			{
				Row val3 = (Row)(object)((r is Row) ? r : null);
				if (val3 != null && val3.HasGrowth)
				{
					return !val3.growth.IsTree;
				}
				return false;
			}
			if (targetId == -4)
			{
				return true;
			}
			int num = r.id;
			if (num == 167)
			{
				num = 1;
			}
			if (num == targetId)
			{
				return ((object)targetRow).GetType() == ((object)r).GetType();
			}
			return false;
		}

		public bool IsTarget(Card c)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (c.HasValue())
			{
				if (targetId != -1)
				{
					if (c.id == targetName)
					{
						return c.placeState == targetPlaceState;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public void SetStartPos()
		{
			if (!ClassExtension.IsNull((object)Pos))
			{
				startPos = Pos.Copy();
				int num = startPos.x - ((Card)base.owner).pos.x;
				int num2 = startPos.z - ((Card)base.owner).pos.z;
				if ((num2 == -1 || num2 == 0) && num == -1)
				{
					startDir = 3;
				}
				else if ((num == -1 || num == 0) && num2 == 1)
				{
					startDir = 2;
				}
				else if ((num2 == 1 || num2 == 0) && num == 1)
				{
					startDir = 1;
				}
				else if ((num == 0 || num == 1) && num2 == -1)
				{
					startDir = 0;
				}
				else
				{
					startDir = ((Card)base.owner).dir;
				}
			}
		}

		public int CalcBuildDirection(int n)
		{
			return CalcBuildDirection(n, startDir);
		}

		public int CalcBuildDirection(int n, int dir)
		{
			n = (n >> dir) | (n << 4 - dir);
			int num = (n >> 3) & 1;
			int num2 = (n >> 2) & 1;
			int num3 = (n >> 1) & 1;
			int num4 = n & 1;
			if (num3 == 1 && num4 == 1)
			{
				return 2;
			}
			if (num == 1 && num2 == 1)
			{
				return 3;
			}
			if (num == 1 || (num3 == 1 && num2 == 0))
			{
				return 0;
			}
			if (num2 == 1 || num4 == 1)
			{
				return 1;
			}
			return 3;
		}

		public (int, int) CalcStartPosDelta(Point p)
		{
			return CalcDelta(p, startPos, startDir);
		}

		public (int, int) CalcDelta(Point p)
		{
			return CalcDelta(p, ((Card)base.owner).pos, ((Card)base.owner).dir);
		}

		public static (int, int) CalcDelta(Point p, Point refPoint, int dir)
		{
			int num = p.x - refPoint.x;
			int num2 = p.z - refPoint.z;
			int item = 0;
			int item2 = 0;
			switch (dir)
			{
			case 0:
				item = num2 * -1;
				item2 = num * -1;
				break;
			case 1:
				item = num;
				item2 = num2 * -1;
				break;
			case 2:
				item = num2;
				item2 = num;
				break;
			case 3:
				item = num * -1;
				item2 = num2;
				break;
			}
			return (item, item2);
		}

		public static HashSet<Point> InitFarmField(Point p)
		{
			Predicate<Point> filter = ((!p.IsWater) ? ((Predicate<Point>)((Point pt) => pt.IsFarmField)) : ((Predicate<Point>)((Point pt) => pt.IsWater)));
			return InitRange(p, filter);
		}

		public static HashSet<Point> InitRange(Point start, Predicate<Point> filter)
		{
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			HashSet<Point> hashSet = new HashSet<Point>();
			(int, int, int, int)[] array = new(int, int, int, int)[4]
			{
				(-1, 0, 1, 13),
				(1, 0, 2, 14),
				(0, -1, 4, 7),
				(0, 1, 8, 11)
			};
			Stack<(Point, int)> stack = new Stack<(Point, int)>();
			stack.Push((start, 15));
			while (stack.Count > 0)
			{
				(Point, int) tuple = stack.Pop();
				Point item = tuple.Item1;
				int item2 = tuple.Item2;
				(int, int, int, int)[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					var (num, num2, num3, item3) = array2[i];
					if ((item2 & num3) != 0)
					{
						Point val = new Point(item.x + num, item.z + num2);
						if (val.IsInBounds && filter(val) && hashSet.Add(val))
						{
							stack.Push((val, item3));
						}
					}
				}
			}
			hashSet.Add(start);
			return hashSet;
		}

		public void Say(string text)
		{
			if (!ClassExtension.IsNull((object)base.owner) && !IsSetting && ((Card)base.owner).IsPC && !(base.parent is AutoAct))
			{
				AutoActMod.Say(text);
			}
		}

		public void SayStart()
		{
			Say(AALang.GetText("start"));
		}

		public void SayNoTarget()
		{
			Say(AALang.GetText("noTarget"));
		}

		public void SayFail()
		{
			Say(AALang.GetText("fail"));
		}

		public int CalcDist2(Point p)
		{
			return ((Card)base.owner).pos.Dist2(p);
		}

		public int CalcDist2ToLastPoint(Point p)
		{
			return Pos.Dist2(p);
		}

		public int CalcMaxDelta(Point p)
		{
			return ((Card)base.owner).pos.MaxDelta(p);
		}

		public int CalcMaxDeltaToStartPos(Point p)
		{
			return startPos.MaxDelta(p);
		}

		public Point FindPos(Predicate<Cell> filter, int detRangeSq = 2, int tryBetterPath = 0, HashSet<Point> range = null)
		{
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Invalid comparison between Unknown and I4
			if (useOriginalPos && Pos.IsInBounds)
			{
				useOriginalPos = false;
				return Pos;
			}
			if (range.HasValue())
			{
				detRangeSq = 80000;
			}
			List<(Point, int, int)> list = new List<(Point, int, int)>();
			if (range.HasValue())
			{
				foreach (Point item in range)
				{
					ForEach(item);
				}
			}
			else
			{
				EClass._map.bounds.ForeachPoint((Action<Point>)delegate(Point p)
				{
					ForEach(p.Copy());
				});
			}
			Point p2;
			int dist2;
			int dist2ToLastPoint;
			foreach (var item2 in list.OrderBy(((Point, int, int) tuple) => tuple.Item2))
			{
				(p2, dist2, dist2ToLastPoint) = item2;
				if (selector.curtPoint.HasValue() && dist2 > selector.MaxDist2)
				{
					break;
				}
				Path.RequestPathImmediate(((Card)base.owner).pos, p2, 1, true, -1);
				if ((int)Path.state == 3)
				{
					TryDestroyObstacle();
				}
				else if (Path.nodes.Count < dist2 || !TryDestroyObstacle())
				{
					int v = 0;
					if (p2.HasBlock)
					{
						v = Math.Abs(CalcDelta(p2).Item2);
					}
					int num = Path.nodes.Count;
					if (tryBetterPath == 2 && num > dist2ToLastPoint && dist2ToLastPoint <= 2)
					{
						num = 1;
					}
					selector.TrySet(p2, num, dist2ToLastPoint, v);
				}
			}
			return selector.FinalPoint;
			void ForEach(Point p)
			{
				int num4 = CalcDist2(p);
				if (num4 <= detRangeSq)
				{
					Cell cell = p.cell;
					if (filter(cell))
					{
						int num5 = ((base.child is TaskPoint) ? CalcDist2ToLastPoint(p) : num4);
						if (num4 <= 2)
						{
							selector.TrySet(p, (num4 == 0) ? (-1) : 0, num5);
						}
						else
						{
							list.Add((p, num4, num5));
						}
					}
				}
			}
			bool TryDestroyObstacle()
			{
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_009f: Expected O, but got Unknown
				//IL_0104: Unknown result type (might be due to invalid IL or missing references)
				//IL_010e: Expected O, but got Unknown
				if (dist2 > 5 || dist2 < 4 || tryBetterPath != 1)
				{
					return false;
				}
				int num2 = p2.x - ((Card)base.owner).pos.x;
				int num3 = p2.z - ((Card)base.owner).pos.z;
				Point obstacle = new Point(((Card)base.owner).pos.x + num2 / 2, ((Card)base.owner).pos.z + num3 / 2);
				if (CanDestroyObstacle())
				{
					selector.TrySet(obstacle, 1, dist2ToLastPoint);
					return true;
				}
				if (!obstacle.HasBlock && !obstacle.HasObj)
				{
					obstacle = new Point(p2.x - num2 / 2, p2.z - num3 / 2);
					if (CanDestroyObstacle())
					{
						selector.TrySet(obstacle, 1, dist2ToLastPoint);
						return true;
					}
				}
				return false;
				bool CanDestroyObstacle()
				{
					if (obstacle.HasBlock && (((TileRow)obstacle.sourceBlock).id == 1 || ((TileRow)obstacle.sourceBlock).id == 167))
					{
						if (obstacle.HasObj)
						{
							return ((TileRow)obstacle.sourceObj).id == 24;
						}
						return true;
					}
					return false;
				}
			}
		}

		public Point FindPosRefToStartPos(Predicate<Cell> filter, HashSet<Point> range)
		{
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Invalid comparison between Unknown and I4
			if (useOriginalPos && Pos.IsInBounds)
			{
				useOriginalPos = false;
				return Pos;
			}
			List<(Point, int, int)> list = new List<(Point, int, int)>();
			if (range.HasValue())
			{
				foreach (Point item3 in range)
				{
					ForEach(item3);
				}
			}
			else
			{
				EClass._map.bounds.ForeachPoint((Action<Point>)delegate(Point p)
				{
					ForEach(p.Copy());
				});
			}
			foreach (var (val, num, v) in list.OrderBy(((Point, int, int) tuple) => tuple.Item2))
			{
				if (selector.curtPoint.HasValue() && num > selector.MaxDist2)
				{
					break;
				}
				int v2 = ((num == 0) ? (-1) : 0);
				var (v3, v4) = CalcStartPosDelta(val);
				if (num > 2)
				{
					Path.RequestPathImmediate(((Card)base.owner).pos, val, 1, false, -1);
					if ((int)Path.state == 3)
					{
						continue;
					}
					v2 = Path.nodes.Count;
				}
				selector.TrySet(val, v2, v, v3, v4);
			}
			return selector.FinalPoint;
			void ForEach(Point p)
			{
				Cell cell = p.cell;
				if (filter(cell))
				{
					int item = CalcDist2(p);
					int item2 = CalcDist2ToLastPoint(p);
					list.Add((p, item, item2));
				}
			}
		}

		public Thing FindThing(Predicate<Thing> filter, int detRangeSq)
		{
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Invalid comparison between Unknown and I4
			if (useOriginalPos && Pos.IsInBounds)
			{
				useOriginalPos = false;
				Thing val = Pos.cell.Things.Find(filter);
				if (val.HasValue())
				{
					return val;
				}
			}
			List<(Thing, int, int)> list = new List<(Thing, int, int)>();
			EClass._map.bounds.ForeachCell((Action<Cell>)delegate(Cell cell)
			{
				Point point = cell.GetPoint();
				int num2 = CalcDist2(point);
				if (num2 <= detRangeSq && point.HasThing)
				{
					Thing val3 = point.Things.Find(filter);
					if (!ClassExtension.IsNull((object)val3))
					{
						int num3 = CalcDist2ToLastPoint(point);
						if (num2 <= 2)
						{
							selector.TrySet((Card)(object)val3, (num2 == 0) ? (-1) : 0, num3);
						}
						else
						{
							list.Add((val3, num2, num3));
						}
					}
				}
			});
			foreach (var (val2, num, v) in list.OrderBy(((Thing, int, int) tuple) => tuple.Item2))
			{
				if (selector.curtPoint.HasValue() && num > selector.MaxDist2)
				{
					break;
				}
				Path.RequestPathImmediate(((Card)base.owner).pos, ((Card)val2).pos, 1, true, -1);
				if ((int)Path.state != 3)
				{
					selector.TrySet((Card)(object)val2, Path.nodes.Count, v);
				}
			}
			Card finalTarget = selector.FinalTarget;
			return (Thing)(object)((finalTarget is Thing) ? finalTarget : null);
		}

		public Chara FindChara(Predicate<Chara> filter, int detRangeSq = 80000, List<Chara> range = null)
		{
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Invalid comparison between Unknown and I4
			if (useOriginalPos && Pos.IsInBounds)
			{
				useOriginalPos = false;
				Chara val = Pos.cell.Charas.Find(filter);
				if (val.HasValue())
				{
					return val;
				}
			}
			List<(Chara, int)> list = new List<(Chara, int)>();
			if (range.HasValue())
			{
				range.ForEach(ForEach);
			}
			else
			{
				EClass._map.charas.ForEach(ForEach);
			}
			foreach (var (val2, num) in list.OrderBy(((Chara, int) Tuple) => Tuple.Item2))
			{
				if (selector.curtPoint.HasValue() && num > selector.MaxDist2)
				{
					break;
				}
				Path.RequestPathImmediate(((Card)base.owner).pos, ((Card)val2).pos, 1, true, -1);
				if ((int)Path.state != 3)
				{
					selector.TrySet((Card)(object)val2, Path.nodes.Count);
				}
			}
			Card finalTarget = selector.FinalTarget;
			return (Chara)(object)((finalTarget is Chara) ? finalTarget : null);
			void ForEach(Chara chara)
			{
				_ = ((Card)chara).pos;
				int num2 = CalcDist2(((Card)chara).pos);
				if (num2 <= detRangeSq && ((Card)chara).IsAliveInCurrentZone && filter(chara))
				{
					if (num2 <= 2)
					{
						selector.TrySet((Card)(object)chara, (num2 == 0) ? (-1) : 0);
					}
					else
					{
						list.Add((chara, num2));
					}
				}
			}
		}
	}
	public class AutoActBrush : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__17 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActBrush <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__17(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0091: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				//IL_006d: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActBrush CS$<>8__locals0 = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_0082;
				case 1:
					<>1__state = -1;
					goto IL_0082;
				case 2:
					{
						<>1__state = -1;
						return false;
					}
					IL_0082:
					if (((AIAct)CS$<>8__locals0).CanProgress())
					{
						Chara val = CS$<>8__locals0.FindChara((Chara chara) => CanBeBrushed(chara) && (CS$<>8__locals0.range.HasValue() || ((Card)chara).IsPCFaction == CS$<>8__locals0.isTargetPCFaction), Settings.DetRangeSq, CS$<>8__locals0.range);
						if (ClassExtension.IsNull((object)val))
						{
							CS$<>8__locals0.SayNoTarget();
							return false;
						}
						((AI_Fuck)CS$<>8__locals0.Child).target = val;
						<>2__current = CS$<>8__locals0.StartNextTask();
						<>1__state = 1;
						return true;
					}
					<>2__current = CS$<>8__locals0.FailOrSuccess();
					<>1__state = 2;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__17 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__17(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq = Settings.DetRangeSq;

		public bool isTargetPCFaction;

		public List<Chara> range;

		public AI_TendAnimal Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (AI_TendAnimal)(object)((child is AI_TendAnimal) ? child : null);
			}
		}

		public override Point Pos => ((Card)(((AI_Fuck)Child).target?)).pos;

		public override bool IsAutoTurn
		{
			get
			{
				AI_TendAnimal child = Child;
				if (child != null)
				{
					AIAct child2 = ((AIAct)child).child;
					AI_Goto val = (AI_Goto)(object)((child2 is AI_Goto) ? child2 : null);
					if (val != null)
					{
						return !((AIAct)val).IsRunning;
					}
					return true;
				}
				return false;
			}
		}

		public override int CurrentProgress => ((AI_Fuck)Child).progress;

		public override int MaxProgress => ((AI_Fuck)Child).maxProgress;

		public AutoActBrush(AI_TendAnimal source)
		{
			Chara target = ((AI_Fuck)source).target;
			isTargetPCFaction = ((target != null) ? new bool?(((Card)target).IsPCFaction) : null) ?? false;
			base..ctor((AIAct)(object)source);
		}

		public static AutoActBrush TryCreate(AIAct source)
		{
			AI_TendAnimal val = (AI_TendAnimal)(object)((source is AI_TendAnimal) ? source : null);
			if (val == null)
			{
				return null;
			}
			return new AutoActBrush(val);
		}

		public override bool CanProgress()
		{
			if (base.CanProgress() && ((Card)(((Card)((AIAct)this).owner).Tool?)).trait is TraitToolBrush)
			{
				return ((Card)((Card)((AIAct)this).owner).Tool).HasElement(237, 1);
			}
			return false;
		}

		public static bool CanBeBrushed(Chara chara)
		{
			return chara.interest > 0;
		}

		[IteratorStateMachine(typeof(<Run>d__17))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__17(-2)
			{
				<>4__this = this
			};
		}

		public override void OnChildSuccess()
		{
			if (!CanBeBrushed(((AI_Fuck)Child).target))
			{
				range?.Remove(((AI_Fuck)Child).target);
			}
		}
	}
	public class AutoActBuild : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__14 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActBuild <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__14(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0086: Unknown result type (might be due to invalid IL or missing references)
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActBuild autoActBuild = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					break;
				case 1:
					<>1__state = -1;
					if (((Card)((AIAct)autoActBuild).owner).IsPCParty && !autoActBuild.CheckHeld())
					{
						if (autoActBuild.range.Count == 0)
						{
							return false;
						}
						<>2__current = autoActBuild.Fail();
						<>1__state = 2;
						return true;
					}
					break;
				case 2:
					<>1__state = -1;
					break;
				}
				if (((AIAct)autoActBuild).CanProgress())
				{
					Point val = autoActBuild.FindNextBuildPosition();
					if (ClassExtension.IsNull((object)val))
					{
						return false;
					}
					autoActBuild.SetPosition(val);
					<>2__current = autoActBuild.StartNextTask();
					<>1__state = 1;
					return true;
				}
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__14 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__14(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public bool hasSowRange;

		public HashSet<Point> range = new HashSet<Point>();

		public Dictionary<Point, int> directions = new Dictionary<Point, int>();

		[CompilerGenerated]
		private Func<Thing, bool> <HeldChecker>k__BackingField;

		[CompilerGenerated]
		private Func<Point, bool> <PointChecker>k__BackingField;

		public TaskBuild Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (TaskBuild)(object)((child is TaskBuild) ? child : null);
			}
		}

		public Card Held => Child.held;

		public override int MaxRestart => 0;

		public Func<Thing, bool> HeldChecker
		{
			get
			{
				if (<HeldChecker>k__BackingField.HasValue())
				{
					return <HeldChecker>k__BackingField;
				}
				if (Held.trait is TraitSeed)
				{
					int seedId = ((TileRow)((SourceData<Row, int>)(object)EClass.sources.objs).map[Held.refVal]).id;
					<HeldChecker>k__BackingField = delegate(Thing t)
					{
						Trait trait = ((Card)t).trait;
						TraitSeed val = (TraitSeed)(object)((trait is TraitSeed) ? trait : null);
						return val != null && ((TileRow)val.row).id == seedId;
					};
				}
				else if (Held.trait is TraitDefertilizer)
				{
					<HeldChecker>k__BackingField = (Thing t) => ((Card)t).trait is TraitDefertilizer;
				}
				else if (Held.trait is TraitFertilizer)
				{
					<HeldChecker>k__BackingField = (Thing t) => ((Card)t).trait is TraitFertilizer && !(((Card)t).trait is TraitDefertilizer);
				}
				return <HeldChecker>k__BackingField;
			}
		}

		public Func<Point, bool> PointChecker
		{
			get
			{
				if (<PointChecker>k__BackingField.HasValue())
				{
					return <PointChecker>k__BackingField;
				}
				if (Held.trait is TraitSeed)
				{
					<PointChecker>k__BackingField = delegate(Point p)
					{
						bool flag = (!p.HasThing || ((Card)p.Things[0]).IsInstalled) && (!p.HasBlock || p.HasWallOrFence) && !p.HasObj && ClassExtension.IsNull((object)p.growth);
						if (flag)
						{
							Trait val = ((Card)(p.Installed?)).trait;
							bool flag2 = ((val == null || val is TraitLight || val is TraitSpot) ? true : false);
							flag = flag2;
						}
						return flag;
					};
				}
				else if (Held.trait is TraitFertilizer)
				{
					<PointChecker>k__BackingField = ShouldFertilize;
				}
				else
				{
					Trait trait = Held.trait;
					if ((trait is TraitFloor || trait is TraitPlatform) ? true : false)
					{
						int rowId = default(int);
						ref int reference = ref rowId;
						Trait trait2 = Held.trait;
						reference = ((TraitTile)((trait2 is TraitTile) ? trait2 : null)).source.id;
						<PointChecker>k__BackingField = (Point p) => !p.HasThing && !p.HasBlock && !p.HasObj && ((TileRow)p.cell.sourceSurface).id != rowId;
					}
					else
					{
						<PointChecker>k__BackingField = (Point p) => !p.HasThing && !p.HasBlock;
					}
				}
				return <PointChecker>k__BackingField;
			}
		}

		public AutoActBuild(TaskBuild source)
			: base((AIAct)(object)source)
		{
		}

		public static AutoActBuild TryCreate(AIAct source)
		{
			TaskBuild val = (TaskBuild)(object)((source is TaskBuild) ? source : null);
			if (val == null)
			{
				return null;
			}
			bool flag = ((Card)source.owner).IsPC;
			if (flag)
			{
				Card held = source.owner.held;
				Thing val2 = (Thing)(object)((held is Thing) ? held : null);
				bool flag2 = val2 == null;
				if (!flag2)
				{
					bool flag3 = ((Card)val2).Num == 1;
					if (flag3)
					{
						Trait trait = ((Card)val2).trait;
						bool flag4 = ((trait is TraitSeed || trait is TraitFertilizer) ? true : false);
						flag3 = !flag4;
					}
					flag2 = flag3;
				}
				bool flag5 = flag2;
				if (!flag5)
				{
					Trait trait = ((Card)val2).trait;
					bool flag3 = ((trait is TraitSeed || trait is TraitFertilizer) ? true : false);
					flag5 = !flag3;
				}
				flag = flag5;
			}
			if (flag)
			{
				return null;
			}
			return new AutoActBuild(val);
		}

		public override bool CanProgress()
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Invalid comparison between Unknown and I4
			if (base.CanProgress() && !Held.isDestroyed && ((AIAct)this).owner.held == Held)
			{
				return (int)Held.placeState != 2;
			}
			return false;
		}

		public override void OnStart()
		{
			base.OnStart();
			Init();
		}

		public override void OnChildSuccess()
		{
			range.Remove(Pos);
		}

		[IteratorStateMachine(typeof(<Run>d__14))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__14(-2)
			{
				<>4__this = this
			};
		}

		public void Init()
		{
			RestoreChild();
			_ = HeldChecker;
			if ((Held.trait is TraitSeed || Held.trait is TraitFertilizer) && range.Count == 0)
			{
				range = AutoAct.InitFarmField(startPos);
			}
		}

		public void SetRange(HashSet<Point> range)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			this.range = range;
			if (!Child.recipe.IsWallOrFence)
			{
				return;
			}
			Point val = new Point(0, 0);
			foreach (Point item in range)
			{
				if (range.Contains(val.Set(item.x - 1, item.z)) && range.Contains(val.Set(item.x, item.z + 1)))
				{
					directions.Add(item, 2);
				}
				else if (range.Contains(val.Set(item.x - 1, item.z)) && range.Contains(val.Set(item.x, item.z - 1)))
				{
					directions.Add(item, 0);
				}
				else if (range.Contains(val.Set(item.x + 1, item.z)) && range.Contains(val.Set(item.x, item.z - 1)) && (!range.Contains(val.Set(item.x - 1, item.z)) || !range.Contains(val.Set(item.x + 1, item.z))) && (!range.Contains(val.Set(item.x, item.z - 1)) || !range.Contains(val.Set(item.x, item.z + 1))))
				{
					directions.Add(item, 3);
				}
				else if (range.Contains(val.Set(item.x, item.z - 1)) || range.Contains(val.Set(item.x, item.z + 1)))
				{
					directions.Add(item, 1);
				}
				else
				{
					directions.Add(item, 0);
				}
			}
		}

		public Point FindNextBuildPosition()
		{
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Invalid comparison between Unknown and I4
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Invalid comparison between Unknown and I4
			bool flag = false;
			if (Held.trait is TraitBlock)
			{
				flag = true;
			}
			if (useOriginalPos)
			{
				useOriginalPos = false;
				return Pos;
			}
			List<(Point, int, int)> list = new List<(Point, int, int)>();
			foreach (Point item3 in range)
			{
				if (PointChecker(item3))
				{
					int item = CalcDist2(item3);
					int item2 = CalcDist2ToLastPoint(item3);
					list.Add((item3, item, item2));
				}
			}
			foreach (var (val, num, v) in from tuple in list
				orderby tuple.Item2, tuple.Item3
				select tuple)
			{
				if (selector.curtPoint.HasValue() && !flag && num > selector.MaxDist2)
				{
					break;
				}
				int v2 = ((num == 0) ? (-1) : 0);
				if (flag)
				{
					if (num > 2)
					{
						base.Path.RequestPathImmediate(((Card)((AIAct)this).owner).pos, val, 1, true, -1);
						if ((int)base.Path.state == 3)
						{
							continue;
						}
						v2 = base.Path.nodes.Count;
					}
					if (!Child.recipe.IsWallOrFence)
					{
						selector.TrySet(val, v2, v);
						continue;
					}
					int num2 = directions[val];
					if (num2 != 3 && selector.TrySet(val, v2, v))
					{
						Child.recipe._dir = num2;
					}
					continue;
				}
				if (num > 2)
				{
					base.Path.RequestPathImmediate(((Card)((AIAct)this).owner).pos, val, 1, true, -1);
					if ((int)base.Path.state == 3)
					{
						continue;
					}
					v2 = base.Path.nodes.Count;
				}
				var (num3, v3) = CalcStartPosDelta(val);
				if (num3 >= 0)
				{
					(num3, v3) = CalcDelta(val);
					if (num3 < 0)
					{
						num3 = -num3 * 2;
					}
				}
				selector.TrySet(val, v2, v, num3, v3);
			}
			return selector.FinalPoint;
		}

		public bool CheckHeld()
		{
			Card val = ((AIAct)this).owner.held;
			if (((Card)((AIAct)this).owner).IsPC && val == null)
			{
				val = (Card)(object)HotItemHeld.lastHeld;
			}
			if (ClassExtension.IsNull((object)val) || val.isDestroyed || val.GetRootCard() != EClass.pc)
			{
				return TrySwitchHeld();
			}
			if (val == Held)
			{
				return true;
			}
			if (HeldChecker?.Invoke((Thing)(object)((val is Thing) ? val : null)) ?? false)
			{
				Child.held = val;
				return true;
			}
			return TrySwitchHeld();
		}

		public bool TrySwitchHeld()
		{
			Thing nextHeld = FindNextHeld();
			if (nextHeld.HasValue())
			{
				EClass.pc.HoldCard((Card)(object)nextHeld, -1);
				EClass.pc.party.members.ForEach(delegate(Chara chara)
				{
					if (chara.ai is AutoActBuild autoActBuild && ((AIAct)autoActBuild).IsRunning)
					{
						chara.held = (Card)(object)nextHeld;
						autoActBuild.Child.held = (Card)(object)nextHeld;
					}
				});
				return true;
			}
			return false;
		}

		public Thing FindNextHeld()
		{
			if (ClassExtension.IsNull((object)HeldChecker))
			{
				return null;
			}
			Thing val = null;
			foreach (Thing item in ((Card)EClass.pc).things.Flatten())
			{
				if (HeldChecker(item))
				{
					if (!(((Card)item).trait is TraitSeed))
					{
						return item;
					}
					if (ClassExtension.IsNull((object)val) || ((Card)item).encLV > ((Card)val).encLV)
					{
						val = item;
					}
				}
			}
			return val;
		}

		public void SetPosition(Point p)
		{
			((TaskPoint)Child).pos = p;
			RestoreChild();
		}

		public void RestoreChild()
		{
			Child.lastPos = null;
			((Task)Child).isDestroyed = false;
		}

		private static bool ShouldFertilize(Point p)
		{
			bool flag = p.growth.HasValue();
			if (!p.HasThing)
			{
				return flag;
			}
			bool fert = false;
			bool seed = false;
			p.Things.ForEach(delegate(Thing t)
			{
				if (((Card)t).trait is TraitFertilizer)
				{
					fert = true;
				}
				else if (((Card)t).trait is TraitSeed)
				{
					seed = true;
				}
			});
			if (seed || flag)
			{
				return !fert;
			}
			return false;
		}
	}
	public class AutoActClean : AutoAct
	{
		public class SubActClean : AIAct
		{
			[CompilerGenerated]
			private sealed class <Run>d__3 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
			{
				private int <>1__state;

				private Status <>2__current;

				private int <>l__initialThreadId;

				public SubActClean <>4__this;

				Status IEnumerator<Status>.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				object IEnumerator.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				[DebuggerHidden]
				public <Run>d__3(int <>1__state)
				{
					this.<>1__state = <>1__state;
					<>l__initialThreadId = Environment.CurrentManagedThreadId;
				}

				[DebuggerHidden]
				void IDisposable.Dispose()
				{
					<>1__state = -2;
				}

				private bool MoveNext()
				{
					//IL_003c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0041: Unknown result type (might be due to invalid IL or missing references)
					//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
					//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
					//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
					//IL_0104: Unknown result type (might be due to invalid IL or missing references)
					//IL_0083: Unknown result type (might be due to invalid IL or missing references)
					//IL_0088: Unknown result type (might be due to invalid IL or missing references)
					int num = <>1__state;
					SubActClean subActClean = <>4__this;
					switch (num)
					{
					default:
						return false;
					case 0:
						<>1__state = -1;
						<>2__current = ((AIAct)subActClean).DoGoto(subActClean.pos, 1, true, (Func<Status>)null);
						<>1__state = 1;
						return true;
					case 1:
						<>1__state = -1;
						if (!(((AIAct)subActClean).owner.held?.trait is TraitBroom) || !CanClean(subActClean.pos))
						{
							<>2__current = ((AIAct)subActClean).Cancel();
							<>1__state = 2;
							return true;
						}
						goto IL_009d;
					case 2:
						<>1__state = -1;
						goto IL_009d;
					case 3:
						<>1__state = -1;
						goto IL_00c0;
					case 4:
						{
							<>1__state = -1;
							return false;
						}
						IL_009d:
						subActClean.progress = 0;
						subActClean.maxProgress = ((!subActClean.pos.cell.HasLiquid) ? 1 : 5);
						goto IL_00c0;
						IL_00c0:
						subActClean.progress++;
						((Card)((AIAct)subActClean).owner).LookAt(subActClean.pos);
						((Card)((AIAct)subActClean).owner).renderer.NextFrame();
						if (subActClean.progress != subActClean.maxProgress)
						{
							<>2__current = ((AIAct)subActClean).KeepRunning();
							<>1__state = 3;
							return true;
						}
						EClass._map.SetDecal(subActClean.pos.x, subActClean.pos.z, 0, 1, true);
						EClass._map.SetLiquid(subActClean.pos.x, subActClean.pos.z, 0, 0);
						subActClean.pos.PlayEffect("vanish");
						((Card)((AIAct)subActClean).owner).Say("clean", (Card)(object)((AIAct)subActClean).owner, (string)null, (string)null);
						((Card)((AIAct)subActClean).owner).PlaySound("clean_floor", 1f, true);
						((AIAct)subActClean).owner.stamina.Mod(-1);
						((Card)((AIAct)subActClean).owner).ModExp(293, 30);
						<>2__current = ((AIAct)subActClean).KeepRunning();
						<>1__state = 4;
						return true;
					}
				}

				bool IEnumerator.MoveNext()
				{
					//ILSpy generated this explicit interface implementation from .override directive in MoveNext
					return this.MoveNext();
				}

				[DebuggerHidden]
				void IEnumerator.Reset()
				{
					throw new NotSupportedException();
				}

				[DebuggerHidden]
				IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
				{
					<Run>d__3 result;
					if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
					{
						<>1__state = 0;
						result = this;
					}
					else
					{
						result = new <Run>d__3(0)
						{
							<>4__this = <>4__this
						};
					}
					return result;
				}

				[DebuggerHidden]
				IEnumerator IEnumerable.GetEnumerator()
				{
					return ((IEnumerable<Status>)this).GetEnumerator();
				}
			}

			public Point pos;

			public int maxProgress;

			public int progress;

			[IteratorStateMachine(typeof(<Run>d__3))]
			public override IEnumerable<Status> Run()
			{
				//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
				return new <Run>d__3(-2)
				{
					<>4__this = this
				};
			}
		}

		[CompilerGenerated]
		private sealed class <Run>d__16 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActClean <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__16(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0098: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0074: Unknown result type (might be due to invalid IL or missing references)
				//IL_0079: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActClean autoActClean = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_008e;
				case 1:
					<>1__state = -1;
					goto IL_008e;
				case 2:
					{
						<>1__state = -1;
						return false;
					}
					IL_008e:
					if (((AIAct)autoActClean).CanProgress())
					{
						Point val = autoActClean.FindPos(CanClean, autoActClean.detRangeSq);
						if (ClassExtension.IsNull((object)val))
						{
							autoActClean.SayNoTarget();
							return false;
						}
						autoActClean.Child.pos = val;
						<>2__current = autoActClean.StartNextTask();
						<>1__state = 1;
						return true;
					}
					<>2__current = autoActClean.FailOrSuccess();
					<>1__state = 2;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__16 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__16(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq;

		public SubActClean Child => ((AIAct)this).child as SubActClean;

		public override Point Pos => Child.pos;

		public override bool IsAutoTurn
		{
			get
			{
				SubActClean child = Child;
				if (child != null)
				{
					AIAct child2 = ((AIAct)child).child;
					AI_Goto val = (AI_Goto)(object)((child2 is AI_Goto) ? child2 : null);
					if (val != null)
					{
						return !((AIAct)val).IsRunning;
					}
					return true;
				}
				return false;
			}
		}

		public override int CurrentProgress => Child.progress;

		public override int MaxProgress => Child.maxProgress;

		public AutoActClean(Point p)
		{
			detRangeSq = Settings.DetRangeSq;
			((AIAct)this).child = (AIAct)(object)new SubActClean
			{
				pos = p
			};
		}

		public static AutoActClean TryCreate(AIAct source)
		{
			TaskClean val = (TaskClean)(object)((source is TaskClean) ? source : null);
			if (val == null)
			{
				return null;
			}
			return new AutoActClean(val.dest);
		}

		public static bool CanClean(Point p)
		{
			return TaskClean.CanClean(p);
		}

		public static bool CanClean(Cell cell)
		{
			return CanClean(cell.GetPoint());
		}

		public override bool CanProgress()
		{
			if (base.CanProgress())
			{
				return ((AIAct)this).owner.held?.trait is TraitBroom;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__16))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__16(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActDig : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__9 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActDig <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__9(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActDig CS$<>8__locals0 = <>4__this;
				Point val;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_002d;
				case 1:
					<>1__state = -1;
					goto IL_00e4;
				case 2:
					<>1__state = -1;
					goto IL_00e4;
				case 3:
					{
						<>1__state = -1;
						return false;
					}
					IL_00e4:
					if (!((AIAct)CS$<>8__locals0).CanProgress())
					{
						<>2__current = CS$<>8__locals0.FailOrSuccess();
						<>1__state = 3;
						return true;
					}
					goto IL_002d;
					IL_002d:
					if (((Spatial)EClass._zone).IsRegion)
					{
						<>2__current = CS$<>8__locals0.StartNextTask();
						<>1__state = 1;
						return true;
					}
					val = ((!CS$<>8__locals0.range.HasValue()) ? CS$<>8__locals0.FindPos((Cell cell) => CS$<>8__locals0.IsTarget((TileRow)(object)cell.sourceSurface) && CS$<>8__locals0.Filter(cell), CS$<>8__locals0.detRangeSq) : CS$<>8__locals0.FindPosRefToStartPos(CS$<>8__locals0.Filter, CS$<>8__locals0.range));
					if (ClassExtension.IsNull((object)val))
					{
						if (ClassExtension.IsNull((object)CS$<>8__locals0.range))
						{
							CS$<>8__locals0.SayNoTarget();
						}
						return false;
					}
					((TaskPoint)CS$<>8__locals0.Child).pos = val;
					<>2__current = CS$<>8__locals0.StartNextTask();
					<>1__state = 2;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__9 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__9(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int w;

		public int h;

		public HashSet<Point> range;

		public int detRangeSq;

		public TaskDig Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (TaskDig)(object)((child is TaskDig) ? child : null);
			}
		}

		public AutoActDig(TaskDig source)
			: base((AIAct)(object)source)
		{
			Row sourceSurface = ((TaskPoint)source).pos.cell.sourceSurface;
			SetTarget((TileRow)(object)sourceSurface);
			detRangeSq = Settings.DetRangeSq;
		}

		public static AutoActDig TryCreate(AIAct source)
		{
			TaskDig val = (TaskDig)(object)((source is TaskDig) ? source : null);
			if (val == null)
			{
				return null;
			}
			Row sourceSurface = ((TaskPoint)val).pos.cell.sourceSurface;
			if (!((Spatial)EClass._zone).IsRegion && (ClassExtension.Contains(((RenderRow)sourceSurface).tag, "grass") || ((TaskPoint)val).pos.HasBridge))
			{
				return null;
			}
			return new AutoActDig(val);
		}

		public override bool CanProgress()
		{
			if (base.CanProgress())
			{
				Card held = ((AIAct)this).owner.held;
				if (((held != null) ? new bool?(held.HasElement(230, 1)) : null) ?? false)
				{
					return ((AIAct)this).owner.held == ((BaseTaskHarvest)Child).tool;
				}
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__9))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__9(-2)
			{
				<>4__this = this
			};
		}

		public override void OnChildSuccess()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Invalid comparison between Unknown and I4
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Invalid comparison between Unknown and I4
			if (ClassExtension.IsNull((object)range))
			{
				return;
			}
			if (Settings.SimpleIdentify == 2)
			{
				HitResult hitResult = ((Task)Child).GetHitResult();
				if ((int)hitResult == 3 || (int)hitResult == 5)
				{
					return;
				}
			}
			range.Remove(Pos);
		}

		public bool Filter(Cell cell)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Invalid comparison between Unknown and I4
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Invalid comparison between Unknown and I4
			int x = ((TaskPoint)Child).pos.x;
			int z = ((TaskPoint)Child).pos.z;
			((TaskPoint)Child).pos.Set((int)cell.x, (int)cell.z);
			HitResult hitResult = ((Task)Child).GetHitResult();
			((TaskPoint)Child).pos.Set(x, z);
			if ((int)hitResult != 3)
			{
				return (int)hitResult == 5;
			}
			return true;
		}
	}
	public class AutoActDisarm : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__8 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActDisarm <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__8(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActDisarm autoActDisarm = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_00a3;
				case 1:
					<>1__state = -1;
					goto IL_005b;
				case 2:
					{
						<>1__state = -1;
						goto IL_00a3;
					}
					IL_00a3:
					if (((AIAct)autoActDisarm).CanProgress())
					{
						if (((TraitSwitch)autoActDisarm.target).TryDisarmTrap(((AIAct)autoActDisarm).owner))
						{
							<>2__current = ((AIAct)autoActDisarm).Success((Action)null);
							<>1__state = 1;
							return true;
						}
						goto IL_005b;
					}
					return false;
					IL_005b:
					if (((Card)((AIAct)autoActDisarm).owner).Evalue(1656) < 3 && EClass.rnd(2) == 0)
					{
						((TraitSwitch)autoActDisarm.target).ActivateTrap(((AIAct)autoActDisarm).owner);
					}
					<>2__current = ((AIAct)autoActDisarm).KeepRunning();
					<>1__state = 2;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__8 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__8(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public TraitTrap target;

		public override int MaxRestart => 0;

		public override Point Pos => ((Card)((AIAct)this).owner).pos;

		public AutoActDisarm(TraitTrap target)
		{
			this.target = target;
			base..ctor();
		}

		public static AutoActDisarm TryCreate(string lang, Card target, Point pos)
		{
			if (lang != ClassExtension.lang("actDisarm"))
			{
				return null;
			}
			Trait obj = target?.trait;
			TraitTrap val = (TraitTrap)(((object)((obj is TraitTrap) ? obj : null)) ?? ((object)/*isinst with value type is only supported in some contexts*/));
			if (ClassExtension.IsNull((object)val))
			{
				return null;
			}
			return new AutoActDisarm(val);
		}

		public override bool CanProgress()
		{
			if (((TraitSwitch)target).CanDisarmTrap && !((Trait)target).owner.isDestroyed)
			{
				return canContinue;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__8))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__8(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActDrawWater : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__9 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActDrawWater <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__9(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
				//IL_008d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActDrawWater autoActDrawWater = <>4__this;
				Point val;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_0029;
				case 1:
					<>1__state = -1;
					if (!((AIAct)autoActDrawWater).CanProgress())
					{
						<>2__current = autoActDrawWater.FailOrSuccess();
						<>1__state = 2;
						return true;
					}
					goto IL_0029;
				case 2:
					{
						<>1__state = -1;
						return false;
					}
					IL_0029:
					val = autoActDrawWater.FindPos((Settings.SimpleIdentify > 0) ? new Predicate<Cell>(CanDrawWaterSimple) : new Predicate<Cell>(autoActDrawWater.CanDrawWater), autoActDrawWater.detRangeSq, 0, autoActDrawWater.range);
					if (ClassExtension.IsNull((object)val))
					{
						autoActDrawWater.SayNoTarget();
						return false;
					}
					((TaskPoint)autoActDrawWater.Child).pos = val;
					<>2__current = autoActDrawWater.StartNextTask();
					<>1__state = 1;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__9 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__9(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public static int priority = 120;

		public int detRangeSq;

		public int simpleIdentify;

		public HashSet<Point> range;

		public TaskDrawWater Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (TaskDrawWater)(object)((child is TaskDrawWater) ? child : null);
			}
		}

		public AutoActDrawWater(TaskDrawWater source)
			: base((AIAct)(object)source)
		{
			targetName = (Pos.HasBridge ? Pos.matBridge : Pos.matFloor).alias;
			detRangeSq = Settings.DetRangeSq;
			simpleIdentify = Settings.SimpleIdentify;
		}

		public static AutoActDrawWater TryCreate(AIAct source)
		{
			TaskDrawWater val = (TaskDrawWater)(object)((source is TaskDrawWater) ? source : null);
			if (val == null)
			{
				return null;
			}
			return new AutoActDrawWater(val);
		}

		public override bool CanProgress()
		{
			TraitToolWaterPot pot = Child.pot;
			if (canContinue && ((AIAct)this).owner.held == ((Trait)pot).owner)
			{
				return ((Trait)pot).owner.c_charges < pot.MaxCharge;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__9))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__9(-2)
			{
				<>4__this = this
			};
		}

		public bool CanDrawWater(Cell cell)
		{
			if (cell.IsTopWaterAndNoSnow && (cell.HasBridge ? cell.matBridge : cell.matFloor).alias == targetName && !cell.HasObj)
			{
				return !cell.HasBlock;
			}
			return false;
		}

		public static bool CanDrawWaterSimple(Cell cell)
		{
			if (cell.IsTopWaterAndNoSnow && !cell.HasObj)
			{
				return !cell.HasBlock;
			}
			return false;
		}
	}
	public class AutoActHarvestMine : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__20 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActHarvestMine <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__20(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0228: Unknown result type (might be due to invalid IL or missing references)
				//IL_022d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
				//IL_019f: Unknown result type (might be due to invalid IL or missing references)
				//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
				//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0200: Unknown result type (might be due to invalid IL or missing references)
				//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActHarvestMine autoActHarvestMine = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_021b;
				case 1:
					<>1__state = -1;
					goto IL_021b;
				case 2:
					<>1__state = -1;
					goto IL_0215;
				case 3:
					<>1__state = -1;
					goto IL_0215;
				case 4:
					<>1__state = -1;
					goto IL_0215;
				case 5:
					{
						<>1__state = -1;
						return false;
					}
					IL_0215:
					autoActHarvestMine.RestoreChild();
					goto IL_021b;
					IL_021b:
					if (((AIAct)autoActHarvestMine).CanProgress())
					{
						if (autoActHarvestMine.IsSeedCountEnough())
						{
							return false;
						}
						if (autoActHarvestMine.Child is TaskHarvest && autoActHarvestMine.Child.target.HasValue())
						{
							Thing val = autoActHarvestMine.FindThing(autoActHarvestMine.IsTarget, autoActHarvestMine.detRangeSq);
							if (ClassExtension.IsNull((object)val))
							{
								autoActHarvestMine.SayNoTarget();
								return false;
							}
							((BaseTaskHarvest)autoActHarvestMine.taskHarvest).target = val;
							autoActHarvestMine.SetPosition(((Card)val).pos);
							<>2__current = autoActHarvestMine.StartNextTask();
							<>1__state = 1;
							return true;
						}
						Point val2;
						if (autoActHarvestMine.hasRange)
						{
							val2 = autoActHarvestMine.FindPos(autoActHarvestMine.CommonFilter, 2, 0, autoActHarvestMine.range);
						}
						else
						{
							int tryBetterPath = 0;
							if (autoActHarvestMine.isHarvest && autoActHarvestMine.Child is TaskMine)
							{
								tryBetterPath = 2;
							}
							else if (!autoActHarvestMine.SimpleIdentify)
							{
								Card held = ((AIAct)autoActHarvestMine).owner.held;
								if (((held != null) ? new bool?(held.HasElement(220, 1)) : null) ?? false)
								{
									tryBetterPath = 1;
								}
							}
							val2 = autoActHarvestMine.FindPos(autoActHarvestMine.CommonFilter, autoActHarvestMine.detRangeSq, tryBetterPath);
						}
						if (ClassExtension.IsNull((object)val2))
						{
							autoActHarvestMine.SayNoTarget();
							return false;
						}
						autoActHarvestMine.SetPosition(val2);
						if (((AIAct)autoActHarvestMine.taskHarvest).CanProgress())
						{
							<>2__current = autoActHarvestMine.SetNextTask((AIAct)(object)autoActHarvestMine.taskHarvest);
							<>1__state = 2;
							return true;
						}
						if (TaskMine.CanMine(autoActHarvestMine.Pos, ((AIAct)autoActHarvestMine).owner.held))
						{
							<>2__current = autoActHarvestMine.SetNextTask((AIAct)(object)autoActHarvestMine.taskMine);
							<>1__state = 3;
							return true;
						}
						<>2__current = autoActHarvestMine.Fail();
						<>1__state = 4;
						return true;
					}
					<>2__current = autoActHarvestMine.FailOrSuccess();
					<>1__state = 5;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__20 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__20(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int simpleIdentify;

		public int detRangeSq;

		public bool hasRange;

		public bool targetIsWithered;

		public bool targetIsWoodTree;

		public bool targetCanHarvest;

		public static int SeedId = -1;

		public static int OriginalSeedCount = 0;

		public int targetSeedCount;

		public HashSet<Point> range = new HashSet<Point>();

		public BaseTaskHarvest initTask;

		public TaskHarvest taskHarvest;

		public TaskMine taskMine;

		public bool isHarvest;

		public BaseTaskHarvest Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (BaseTaskHarvest)(object)((child is BaseTaskHarvest) ? child : null);
			}
		}

		public bool SimpleIdentify => simpleIdentify > 0;

		public AutoActHarvestMine(BaseTaskHarvest source)
			: base((AIAct)(object)source)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected O, but got Unknown
			initTask = source;
			TaskHarvest val = (TaskHarvest)(object)((source is TaskHarvest) ? source : null);
			if (val != null)
			{
				isHarvest = true;
				taskHarvest = val;
				taskMine = new TaskMine
				{
					pos = ((TaskPoint)val).pos
				};
			}
			else
			{
				TaskMine val2 = (TaskMine)(object)((source is TaskMine) ? source : null);
				if (val2 != null)
				{
					taskMine = val2;
					taskHarvest = new TaskHarvest
					{
						pos = ((TaskPoint)val2).pos
					};
				}
			}
			simpleIdentify = Settings.SimpleIdentify;
			detRangeSq = Settings.DetRangeSq;
			targetSeedCount = Settings.SeedReapingCount;
		}

		public static AutoActHarvestMine TryCreate(AIAct source)
		{
			if ((source is TaskMine || source is TaskHarvest) ? true : false)
			{
				return new AutoActHarvestMine((BaseTaskHarvest)(object)((source is BaseTaskHarvest) ? source : null));
			}
			return null;
		}

		[IteratorStateMachine(typeof(<Run>d__20))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__20(-2)
			{
				<>4__this = this
			};
		}

		public AutoActHarvestMine SetRange(HashSet<Point> range)
		{
			this.range = range;
			hasRange = true;
			simpleIdentify = 2;
			targetSeedCount = 0;
			return this;
		}

		public bool IsWoodTree(GrowSystem growth)
		{
			if (growth.IsTree)
			{
				return !growth.CanHarvest();
			}
			return false;
		}

		public void Init()
		{
			RestoreChild();
			if (hasRange)
			{
				targetId = -4;
				return;
			}
			if (Child.target.HasValue())
			{
				SetTarget((Card)(object)Child.target);
			}
			else if ((!Pos.HasObj || SimpleIdentify) && Pos.HasBlock)
			{
				if (SimpleIdentify)
				{
					targetId = -1;
					return;
				}
				SetTarget((TileRow)(object)Pos.sourceBlock);
			}
			else if (Pos.HasObj)
			{
				if (SimpleIdentify && Pos.sourceObj.HasGrowth)
				{
					targetId = (IsWoodTree(Pos.growth) ? (-2) : (-3));
					if (Child is TaskHarvest)
					{
						PrepareForHarvest();
					}
					return;
				}
				SetTarget((TileRow)(object)Pos.sourceObj);
			}
			if (Child is TaskHarvest)
			{
				PrepareForHarvest();
			}
		}

		private void PrepareForHarvest()
		{
			if (Pos.growth.HasValue())
			{
				GrowSystem growth = Pos.sourceObj.growth;
				targetIsWithered = growth.IsWithered();
				targetIsWoodTree = IsWoodTree(Pos.growth);
				targetCanHarvest = (targetIsWoodTree ? growth.IsMature : growth.CanHarvest());
			}
			if (Settings.SameFarmfieldOnly && (Pos.IsFarmField || (((TileRow)Pos.sourceObj).id == 88 && Pos.IsWater)))
			{
				range = AutoAct.InitFarmField(Pos);
				hasRange = true;
			}
			if (taskHarvest.IsReapSeed)
			{
				taskHarvest.wasReapSeed = true;
			}
			if (((Card)((AIAct)this).owner).IsPC)
			{
				SeedId = ((TileRow)Pos.sourceObj).id;
				OriginalSeedCount = CountSeed();
			}
		}

		public override void OnStart()
		{
			base.OnStart();
			Init();
		}

		public override void OnChildSuccess()
		{
			if (Settings.SimpleIdentify != 2 || (!CanHarvest(((AIAct)this).owner, Pos) && !TaskMine.CanMine(Pos, ((AIAct)this).owner.held)))
			{
				range.Remove(Pos);
			}
		}

		private bool IsSeedCountEnough()
		{
			if (!((Card)((AIAct)this).owner).IsPCParty || !taskHarvest.wasReapSeed || targetSeedCount <= 0)
			{
				return false;
			}
			if (CountSeed() >= targetSeedCount + OriginalSeedCount)
			{
				return true;
			}
			return false;
		}

		public int CountSeed()
		{
			int count = 0;
			EClass.pc.party.members.ForEach(delegate(Chara chara)
			{
				foreach (Thing item in ((Card)chara).things.Flatten())
				{
					Trait trait = ((Card)item).trait;
					TraitSeed val = (TraitSeed)(object)((trait is TraitSeed) ? trait : null);
					if (val != null && (((TileRow)val.row).id == SeedId || SimpleIdentify))
					{
						count += ((Card)item).Num;
					}
				}
			});
			return count;
		}

		public bool PlantFilter(Cell cell)
		{
			if (taskHarvest.wasReapSeed)
			{
				return cell.CanReapSeed();
			}
			if (simpleIdentify == 2)
			{
				return true;
			}
			bool flag = targetIsWoodTree && !cell.CanHarvest();
			if ((flag && cell.growth.IsMature != targetCanHarvest) || (!flag && cell.growth.CanHarvest() != targetCanHarvest))
			{
				return false;
			}
			if (targetIsWithered && !cell.growth.IsWithered())
			{
				return false;
			}
			return true;
		}

		public bool CommonFilter(Cell cell)
		{
			BaseRow key;
			if (cell.HasObj)
			{
				if (!IsTarget((TileRow)(object)cell.sourceObj))
				{
					return false;
				}
				key = (BaseRow)(object)cell.matObj;
			}
			else
			{
				if (!cell.HasBlock)
				{
					return false;
				}
				if (!IsTarget((TileRow)(object)cell.sourceBlock))
				{
					return false;
				}
				key = (BaseRow)(object)cell.matBlock;
			}
			if (cell.sourceObj.HasGrowth && !PlantFilter(cell))
			{
				return false;
			}
			if (AutoAct.RowCheckCache.TryGetValue(key, out var value))
			{
				return value;
			}
			value = false;
			int x = ((TaskPoint)Child).pos.x;
			int z = ((TaskPoint)Child).pos.z;
			((TaskPoint)Child).pos.Set((int)cell.x, (int)cell.z);
			if (((AIAct)taskHarvest).CanProgress())
			{
				if (!taskHarvest.IsObj)
				{
					((BaseTaskHarvest)taskHarvest).SetTarget(((AIAct)this).owner, (Thing)null);
				}
				value = ((BaseTaskHarvest)taskHarvest).difficulty != 3;
			}
			else if (TaskMine.CanMine(Pos, ((AIAct)this).owner.held))
			{
				((BaseTaskHarvest)taskMine).SetTarget(((AIAct)this).owner, (Thing)null);
				value = ((BaseTaskHarvest)taskMine).difficulty != 3;
			}
			((TaskPoint)Child).pos.Set(x, z);
			AutoAct.RowCheckCache.Add(key, value);
			return value;
		}

		public void SetPosition(Point p)
		{
			((TaskPoint)Child).pos.Set(p.x, p.z);
		}

		public void RestoreChild()
		{
			((AIAct)taskHarvest).SetOwner(((AIAct)this).owner);
			((AIAct)taskMine).SetOwner(((AIAct)this).owner);
			((Task)taskHarvest).isDestroyed = false;
			taskHarvest.harvestingCrop = false;
			((Task)taskMine).isDestroyed = false;
		}

		public static bool CanHarvest(Chara c, Point p)
		{
			Thing t = ((Card)c).Tool;
			bool hasTool = t != null && (((Card)t).HasElement(225, false) || ((Card)t).HasElement(220, false));
			bool hasDiggingTool = t != null && ((Card)t).HasElement(230, false);
			if (t != null)
			{
				if (((Card)t).trait is TraitToolShears)
				{
					return false;
				}
				if (((Card)t).trait is TraitToolWaterCan)
				{
					return false;
				}
				if (((Card)t).trait is TraitToolMusic)
				{
					return false;
				}
				if (((Card)t).trait is TraitToolSickle && !p.cell.CanReapSeed())
				{
					return false;
				}
			}
			if (p.HasObj && IsValidTarget(p.sourceObj.reqHarvest))
			{
				return true;
			}
			if (p.HasThing)
			{
				for (int num = p.Things.Count - 1; num >= 0; num--)
				{
					t = p.Things[num];
					if (((Card)t).trait.ReqHarvest != null && IsValidTarget(((Card)t).trait.ReqHarvest.Split(',')))
					{
						return true;
					}
				}
				for (int num2 = p.Things.Count - 1; num2 >= 0; num2--)
				{
					t = p.Things[num2];
					if (!((Card)t).isHidden && !((Card)t).isMasked && ((Card)t).trait.CanBeDisassembled && ((Card)(((Card)c).Tool?)).trait is TraitToolHammer)
					{
						return true;
					}
				}
			}
			return false;
			bool IsValidTarget(string[] raw)
			{
				if (raw[0] == "digging")
				{
					return hasDiggingTool;
				}
				bool num3 = p.cell.CanHarvest();
				int num4 = (num3 ? 250 : ((SourceData<Row, int>)(object)EClass.sources.elements).alias[raw[0]].id);
				bool flag = !num3 && num4 != 250;
				if (!flag && t != null && !((Card)t).trait.CanHarvest)
				{
					return false;
				}
				return !flag || hasTool;
			}
		}
	}
	public class AutoActPick : AutoAct
	{
		public class SubActPick : AIAct
		{
			[CompilerGenerated]
			private sealed class <>c__DisplayClass5_0
			{
				public SubActPick <>4__this;

				public bool success;

				internal bool <Run>b__1(Thing t)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0007: Invalid comparison between Unknown and I4
					if ((int)((Card)t).placeState == 2)
					{
						return <>4__this.IsTarget((Card)(object)t);
					}
					return false;
				}

				internal void <Run>b__0(Thing t)
				{
					EClass.pc.Pick(t, true, true);
					success = true;
				}
			}

			[CompilerGenerated]
			private sealed class <Run>d__5 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
			{
				private int <>1__state;

				private Status <>2__current;

				private int <>l__initialThreadId;

				public SubActPick <>4__this;

				private <>c__DisplayClass5_0 <>8__1;

				Status IEnumerator<Status>.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				object IEnumerator.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				[DebuggerHidden]
				public <Run>d__5(int <>1__state)
				{
					this.<>1__state = <>1__state;
					<>l__initialThreadId = Environment.CurrentManagedThreadId;
				}

				[DebuggerHidden]
				void IDisposable.Dispose()
				{
					<>8__1 = null;
					<>1__state = -2;
				}

				private bool MoveNext()
				{
					//IL_0069: Unknown result type (might be due to invalid IL or missing references)
					//IL_006e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0281: Unknown result type (might be due to invalid IL or missing references)
					//IL_0278: Unknown result type (might be due to invalid IL or missing references)
					//IL_0286: Unknown result type (might be due to invalid IL or missing references)
					//IL_0162: Unknown result type (might be due to invalid IL or missing references)
					//IL_0168: Unknown result type (might be due to invalid IL or missing references)
					int num = <>1__state;
					SubActPick subActPick = <>4__this;
					switch (num)
					{
					default:
						return false;
					case 0:
						<>1__state = -1;
						<>8__1 = new <>c__DisplayClass5_0();
						<>8__1.<>4__this = <>4__this;
						if (((Card)((AIAct)subActPick).owner).pos.Dist2(subActPick.pos) > 2)
						{
							<>2__current = ((AIAct)subActPick).DoGoto(subActPick.pos, 1, true, (Func<Status>)null);
							<>1__state = 1;
							return true;
						}
						goto IL_0083;
					case 1:
						<>1__state = -1;
						goto IL_0083;
					case 2:
						{
							<>1__state = -1;
							return false;
						}
						IL_0083:
						<>8__1.success = false;
						if (subActPick.installed)
						{
							Thing val = subActPick.pos.Installed;
							if ((ClassExtension.IsNull((object)val) || !subActPick.IsTarget((Card)(object)val)) && subActPick.pos.HasThing)
							{
								val = subActPick.pos.Things.Find((Thing t) => (int)((Card)t).placeState == 2 && <>8__1.<>4__this.IsTarget((Card)(object)t));
							}
							if (val.HasValue() && subActPick.IsTarget((Card)(object)val))
							{
								if (!EClass.pc.CanLift((Card)(object)val))
								{
									((Card)EClass.pc).Say("tooHeavy", (Card)(object)val, (string)null, (string)null);
								}
								if (((Card)val).HasEditorTag((EditorTag)110))
								{
									if (EClass.player.flags.pickedMelilithTreasure)
									{
										((Card)EClass.pc).PlaySound("curse3", 1f, true);
										((Card)EClass.pc).PlayEffect("curse", true, 0f, default(Vector3));
										EClass.pc.SetFeat(1206, 1, true);
										EClass.player.flags.gotMelilithCurse = true;
									}
									else
									{
										Msg.Say("pickedMelilithTreasure");
										EClass.player.flags.pickedMelilithTreasure = true;
										QuestCursedManor obj = EClass.game.quests.Get<QuestCursedManor>();
										if (obj != null)
										{
											((Quest)obj).NextPhase();
										}
									}
									((Card)val).c_editorTags = null;
								}
								EClass.pc.HoldCard((Card)(object)val, -1);
								if (EClass.pc.held.HasValue())
								{
									((Card)val).PlaySoundHold(false);
									EClass.player.RefreshCurrentHotItem();
									ActionMode.Adv.planRight.Update(ActionMode.Adv.mouseTarget);
									((Card)EClass.pc).renderer.Refresh();
									<>8__1.success = true;
								}
							}
						}
						else
						{
							ClassExtension.ForeachReverse<Thing>((IList<Thing>)subActPick.pos.Things.Where(subActPick.IsTarget).ToArray(), (Action<Thing>)delegate(Thing t)
							{
								EClass.pc.Pick(t, true, true);
								<>8__1.success = true;
							});
						}
						<>2__current = (<>8__1.success ? ((AIAct)subActPick).Success((Action)null) : ((AIAct)subActPick).Cancel());
						<>1__state = 2;
						return true;
					}
				}

				bool IEnumerator.MoveNext()
				{
					//ILSpy generated this explicit interface implementation from .override directive in MoveNext
					return this.MoveNext();
				}

				[DebuggerHidden]
				void IEnumerator.Reset()
				{
					throw new NotSupportedException();
				}

				[DebuggerHidden]
				IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
				{
					<Run>d__5 result;
					if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
					{
						<>1__state = 0;
						result = this;
					}
					else
					{
						result = new <Run>d__5(0)
						{
							<>4__this = <>4__this
						};
					}
					return result;
				}

				[DebuggerHidden]
				IEnumerator IEnumerable.GetEnumerator()
				{
					return ((IEnumerable<Status>)this).GetEnumerator();
				}
			}

			public Thing refThing;

			public Point pos;

			public bool installed;

			public bool pickAll;

			public bool IsTarget(Card t)
			{
				if ((!pickAll || t.IsInstalled) && t != refThing)
				{
					return t.CanStackTo(refThing);
				}
				return true;
			}

			[IteratorStateMachine(typeof(<Run>d__5))]
			public override IEnumerable<Status> Run()
			{
				//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
				return new <Run>d__5(-2)
				{
					<>4__this = this
				};
			}
		}

		[CompilerGenerated]
		private sealed class <Run>d__9 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActPick <>4__this;

			private Point <targetPos>5__2;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__9(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<targetPos>5__2 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0111: Unknown result type (might be due to invalid IL or missing references)
				//IL_0116: Unknown result type (might be due to invalid IL or missing references)
				//IL_009a: Unknown result type (might be due to invalid IL or missing references)
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActPick autoActPick = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					break;
				case 1:
					<>1__state = -1;
					autoActPick.range.Remove(<targetPos>5__2);
					break;
				case 2:
					<>1__state = -1;
					break;
				}
				if (((AIAct)autoActPick).CanProgress())
				{
					if (autoActPick.range.HasValue())
					{
						<targetPos>5__2 = autoActPick.FindPos((Cell c) => true, 2, 0, autoActPick.range);
						if (ClassExtension.IsNull((object)<targetPos>5__2))
						{
							autoActPick.SayNoTarget();
							return false;
						}
						autoActPick.Child.pos = <targetPos>5__2;
						<>2__current = autoActPick.StartNextTask();
						<>1__state = 1;
						return true;
					}
					Thing val = autoActPick.FindThing(autoActPick.IsTarget, autoActPick.detRangeSq);
					if (ClassExtension.IsNull((object)val))
					{
						autoActPick.SayNoTarget();
						return false;
					}
					autoActPick.Child.pos = ((Card)val).pos;
					autoActPick.Child.refThing = val;
					<>2__current = autoActPick.StartNextTask();
					<>1__state = 2;
					return true;
				}
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__9 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__9(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq;

		public HashSet<Point> range;

		public SubActPick Child => ((AIAct)this).child as SubActPick;

		public override Point Pos => Child.pos;

		public AutoActPick(SubActPick source)
			: base((AIAct)(object)source)
		{
			if (source.refThing.HasValue())
			{
				detRangeSq = Settings.DetRangeSq;
				SetTarget((Card)(object)source.refThing);
			}
		}

		public static AutoActPick TryCreate(string lang, Card target, Point pos)
		{
			if ((lang == ClassExtension.lang("actPickOne") || lang == ClassExtension.lang("actHold")) && (Settings.SimpleIdentify == 2 || target.SelfWeight < 160000))
			{
				return new AutoActPick(new SubActPick
				{
					pos = pos,
					refThing = (Thing)(object)((target is Thing) ? target : null),
					installed = target.IsInstalled
				});
			}
			return null;
		}

		public new bool IsTarget(Card t)
		{
			if (!base.IsTarget(t))
			{
				return false;
			}
			if (Settings.SimpleIdentify == 0)
			{
				Trait trait = ((Card)Child.refThing).trait;
				TraitSeed val = (TraitSeed)(object)((trait is TraitSeed) ? trait : null);
				if (val != null)
				{
					Trait trait2 = t.trait;
					TraitSeed val2 = (TraitSeed)(object)((trait2 is TraitSeed) ? trait2 : null);
					if (val2 != null)
					{
						return ((TileRow)val2.row).id == ((TileRow)val.row).id;
					}
					return false;
				}
			}
			return true;
		}

		[IteratorStateMachine(typeof(<Run>d__9))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__9(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActPlow : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__7 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActPlow <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__7(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActPlow autoActPlow = <>4__this;
				Point val;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_0029;
				case 1:
					<>1__state = -1;
					if (!((AIAct)autoActPlow).CanProgress())
					{
						<>2__current = autoActPlow.FailOrSuccess();
						<>1__state = 2;
						return true;
					}
					goto IL_0029;
				case 2:
					{
						<>1__state = -1;
						return false;
					}
					IL_0029:
					val = autoActPlow.FindPosRefToStartPos(Filter, autoActPlow.range);
					if (ClassExtension.IsNull((object)val))
					{
						return false;
					}
					((TaskPoint)autoActPlow.Child).pos = val;
					<>2__current = autoActPlow.StartNextTask();
					<>1__state = 1;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__7 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__7(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int w;

		public int h;

		public HashSet<Point> range;

		public TaskPlow Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (TaskPlow)(object)((child is TaskPlow) ? child : null);
			}
		}

		public AutoActPlow(TaskPlow source)
			: base((AIAct)(object)source)
		{
		}

		public static AutoActPlow TryCreate(AIAct source)
		{
			TaskPlow val = (TaskPlow)(object)((source is TaskPlow) ? source : null);
			if (val == null || !Filter(((TaskPoint)val).pos.cell))
			{
				return null;
			}
			return new AutoActPlow(val)
			{
				range = AutoAct.InitRange(((TaskPoint)val).pos, (Point p) => Filter(p.cell))
			};
		}

		[IteratorStateMachine(typeof(<Run>d__7))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__7(-2)
			{
				<>4__this = this
			};
		}

		public override void OnChildSuccess()
		{
			range?.Remove(Pos);
		}

		public static bool Filter(Cell cell)
		{
			bool flag = !cell.HasBlock && !cell.HasObj;
			if (flag)
			{
				Trait val = ((Card)(cell.Installed?)).trait;
				bool flag2 = ((val == null || val is TraitLight) ? true : false);
				flag = flag2;
			}
			if (flag && !cell.IsTopWater && !cell.IsFarmField)
			{
				return ClassExtension.Contains(((RenderRow)(cell.HasBridge ? cell.sourceBridge : cell.sourceFloor)).tag, "soil");
			}
			return false;
		}
	}
	public class AutoActPourWater : AutoAct
	{
		public class SubActPourWater : TaskPourWater
		{
			public int count;

			public int targetCount;

			public SubActPourWater()
			{
			}

			public SubActPourWater(TaskPourWater source, int depth)
			{
				((TaskPoint)this).pos = ((TaskPoint)source).pos;
				base.pot = source.pot;
				targetCount = depth;
			}

			public override bool CanProgress()
			{
				if (((TaskPourWater)this).CanProgress())
				{
					return count < targetCount;
				}
				return false;
			}

			public override void OnCreateProgress(Progress_Custom p)
			{
				((TaskPourWater)this).OnCreateProgress(p);
				Action action = p.onProgressComplete;
				p.onProgressComplete = delegate
				{
					action();
					count++;
				};
			}

			public override void OnReset()
			{
				count = 0;
			}
		}

		[CompilerGenerated]
		private sealed class <Run>d__9 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActPourWater <>4__this;

			private TraitToolWaterPot <pot>5__2;

			private Point <targetPos>5__3;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__9(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<pot>5__2 = null;
				<targetPos>5__3 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0178: Unknown result type (might be due to invalid IL or missing references)
				//IL_017d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				//IL_012a: Unknown result type (might be due to invalid IL or missing references)
				//IL_012f: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActPourWater autoActPourWater = <>4__this;
				ref TraitToolWaterPot reference;
				Trait obj;
				ref TraitToolWaterPot pot;
				Trait trait;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_01a0;
				case 1:
					<>1__state = -1;
					goto IL_007d;
				case 2:
					<>1__state = -1;
					goto IL_0144;
				case 3:
					{
						<>1__state = -1;
						<pot>5__2 = null;
						<targetPos>5__3 = null;
						goto IL_01a0;
					}
					IL_01a0:
					if (!((AIAct)autoActPourWater).CanProgress())
					{
						return false;
					}
					reference = ref <pot>5__2;
					obj = ((AIAct)autoActPourWater).owner.held?.trait;
					reference = (TraitToolWaterPot)(object)((obj is TraitToolWaterPot) ? obj : null);
					if (ClassExtension.IsNull((object)<pot>5__2))
					{
						<>2__current = autoActPourWater.Fail();
						<>1__state = 1;
						return true;
					}
					goto IL_007d;
					IL_0144:
					((TaskPoint)autoActPourWater.Child).pos = <targetPos>5__3;
					pot = ref ((TaskPourWater)autoActPourWater.Child).pot;
					trait = ((AIAct)autoActPourWater).owner.held.trait;
					pot = (TraitToolWaterPot)(object)((trait is TraitToolWaterPot) ? trait : null);
					<>2__current = autoActPourWater.StartNextTask();
					<>1__state = 3;
					return true;
					IL_007d:
					<targetPos>5__3 = autoActPourWater.FindPosRefToStartPos(CanPourWater, autoActPourWater.range);
					if (ClassExtension.IsNull((object)<targetPos>5__3))
					{
						return false;
					}
					if (((Trait)<pot>5__2).owner.c_charges == 0)
					{
						Thing val = ((Card)((AIAct)autoActPourWater).owner).things.Flatten().FirstOrDefault(delegate(Thing t)
						{
							Trait trait3 = ((Card)t).trait;
							TraitToolWaterPot val2 = (TraitToolWaterPot)(object)((trait3 is TraitToolWaterPot) ? trait3 : null);
							return val2 != null && ((Trait)val2).owner.c_charges > 0;
						});
						if (!val.HasValue())
						{
							<>2__current = autoActPourWater.Fail();
							<>1__state = 2;
							return true;
						}
						ref TraitToolWaterPot reference2 = ref <pot>5__2;
						Trait trait2 = ((Card)val).trait;
						reference2 = (TraitToolWaterPot)(object)((trait2 is TraitToolWaterPot) ? trait2 : null);
						((AIAct)autoActPourWater).owner.HoldCard((Card)(object)val, -1);
					}
					goto IL_0144;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__9 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__9(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int w;

		public int h;

		public HashSet<Point> range;

		public SubActPourWater pourWater;

		public SubActPourWater Child => pourWater;

		public AutoActPourWater(SubActPourWater source)
		{
			pourWater = source;
			base..ctor((AIAct)(object)source);
		}

		public override bool CanProgress()
		{
			if (canContinue)
			{
				return ((AIAct)this).owner.held?.trait is TraitToolWaterPot;
			}
			return false;
		}

		public static bool CanPourWater(Cell cell)
		{
			if (!cell.HasBridge && !cell.HasObj)
			{
				return ClassExtension.Contains(((RenderRow)cell.sourceSurface).tag, "soil");
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__9))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__9(-2)
			{
				<>4__this = this
			};
		}

		public override void OnChildSuccess()
		{
			range?.Remove(Pos);
		}
	}
	public class AutoActRead : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__8 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActRead <>4__this;

			private Card <originalTarget>5__2;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__8(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<originalTarget>5__2 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActRead autoActRead = <>4__this;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					<>1__state = -1;
					if (<originalTarget>5__2.trait is TraitBookSkill)
					{
						autoActRead.Child.target = <originalTarget>5__2;
					}
					if (!((AIAct)autoActRead).CanProgress())
					{
						if (Settings.SimpleIdentify != 0 && autoActRead.Child.target.trait is TraitBaseSpellbook)
						{
							Thing val = ((Card)((AIAct)autoActRead).owner).things.Flatten().FirstOrDefault(CanRead);
							if (val.HasValue())
							{
								autoActRead.Child.target = (Card)(object)val;
								goto IL_002f;
							}
						}
						return false;
					}
				}
				else
				{
					<>1__state = -1;
					<originalTarget>5__2 = autoActRead.Child.target;
				}
				goto IL_002f;
				IL_002f:
				<>2__current = autoActRead.StartNextTask();
				<>1__state = 1;
				return true;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__8 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__8(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public AI_Read Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (AI_Read)(object)((child is AI_Read) ? child : null);
			}
		}

		public override Point Pos => ((Card)((AIAct)this).owner).pos;

		public AutoActRead(AIAct source)
			: base(source)
		{
		}

		public static AutoActRead TryCreate(AIAct source)
		{
			AI_Read val = (AI_Read)(object)((source is AI_Read) ? source : null);
			bool flag = val == null;
			if (!flag)
			{
				Trait trait = val.target.trait;
				bool flag2 = ((trait is TraitBaseSpellbook || trait is TraitBookSkill) ? true : false);
				flag = !flag2;
			}
			if (flag)
			{
				return null;
			}
			return new AutoActRead((AIAct)(object)val);
		}

		public override bool CanProgress()
		{
			if (!Child.target.isDestroyed)
			{
				if (Child.target.trait is TraitAncientbook)
				{
					return !Child.target.isOn;
				}
				return true;
			}
			return false;
		}

		public static bool CanRead(Thing t)
		{
			if (((Card)t).trait is TraitBaseSpellbook && (!(((Card)t).trait is TraitAncientbook) || !((Card)t).isOn))
			{
				return !(((Card)t).trait is TraitUsuihon);
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__8))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__8(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActShear : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__7 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActShear <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__7(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_009a: Unknown result type (might be due to invalid IL or missing references)
				//IL_009f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				//IL_007b: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActShear autoActShear = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_0090;
				case 1:
					<>1__state = -1;
					goto IL_0090;
				case 2:
					{
						<>1__state = -1;
						return false;
					}
					IL_0090:
					if (((AIAct)autoActShear).CanProgress())
					{
						Chara val = autoActShear.FindChara((Chara chara) => ((Card)chara).CanBeSheared());
						if (ClassExtension.IsNull((object)val))
						{
							autoActShear.SayNoTarget();
							return false;
						}
						((AI_TargetCard)autoActShear.Child).target = (Card)(object)val;
						<>2__current = autoActShear.StartNextTask();
						<>1__state = 1;
						return true;
					}
					<>2__current = autoActShear.FailOrSuccess();
					<>1__state = 2;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__7 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__7(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public AI_Shear Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (AI_Shear)(object)((child is AI_Shear) ? child : null);
			}
		}

		public override Point Pos => ((AI_TargetCard)Child).target?.pos;

		public AutoActShear(AIAct source)
			: base(source)
		{
		}

		public static AutoActShear TryCreate(AIAct source)
		{
			AI_Shear val = (AI_Shear)(object)((source is AI_Shear) ? source : null);
			if (val == null)
			{
				return null;
			}
			return new AutoActShear((AIAct)(object)val);
		}

		public override bool CanProgress()
		{
			if (base.CanProgress())
			{
				return ((Card)(((Card)((AIAct)this).owner).Tool?)).trait is TraitToolShears;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__7))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__7(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActSlaughter : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__11 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActSlaughter <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__11(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActSlaughter autoActSlaughter = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_0092;
				case 1:
					<>1__state = -1;
					goto IL_0092;
				case 2:
					{
						<>1__state = -1;
						return false;
					}
					IL_0092:
					if (((AIAct)autoActSlaughter).CanProgress())
					{
						Chara val = autoActSlaughter.FindChara(CanBeSlaughtered, autoActSlaughter.detRangeSq, autoActSlaughter.range);
						if (ClassExtension.IsNull((object)val))
						{
							autoActSlaughter.SayNoTarget();
							return false;
						}
						((AI_TargetCard)autoActSlaughter.Child).target = (Card)(object)val;
						<>2__current = autoActSlaughter.StartNextTask();
						<>1__state = 1;
						return true;
					}
					<>2__current = autoActSlaughter.FailOrSuccess();
					<>1__state = 2;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__11 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__11(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq = Settings.DetRangeSq;

		public List<Chara> range;

		public AI_Slaughter Child
		{
			get
			{
				AIAct child = ((AIAct)this).child;
				return (AI_Slaughter)(object)((child is AI_Slaughter) ? child : null);
			}
		}

		public override Point Pos => ((AI_TargetCard)Child).target?.pos;

		public AutoActSlaughter(AIAct source)
			: base(source)
		{
		}

		public static AutoActSlaughter TryCreate(AIAct source)
		{
			AI_Slaughter val = (AI_Slaughter)(object)((source is AI_Slaughter) ? source : null);
			if (val == null)
			{
				return null;
			}
			return new AutoActSlaughter((AIAct)(object)val);
		}

		public static AutoActSlaughter TryCreate(string lang, Card target, Point pos)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			if (lang != ClassExtension.lang("AI_Slaughter"))
			{
				return null;
			}
			return new AutoActSlaughter((AIAct)new AI_Slaughter
			{
				target = target
			});
		}

		public override bool CanProgress()
		{
			if (canContinue)
			{
				return ((Card)(((Card)((AIAct)this).owner).Tool?)).trait is TraitToolButcher;
			}
			return false;
		}

		public static bool CanBeSlaughtered(Chara chara)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Invalid comparison between Unknown and I4
			if (((Card)chara).IsPCFaction)
			{
				return (int)chara.memberType == 1;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__11))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__11(-2)
			{
				<>4__this = this
			};
		}

		public override void OnChildSuccess()
		{
			range?.Remove((Chara)/*isinst with value type is only supported in some contexts*/);
		}

		public override void OnCancelOrSuccess()
		{
			base.OnCancelOrSuccess();
			AI_Slaughter child = Child;
			if (child != null)
			{
				((AIAct)child).OnCancelOrSuccess();
			}
		}
	}
	public class AutoActSmash : AutoAct
	{
		public class SubActSmash : AIAct
		{
			[CompilerGenerated]
			private sealed class <Run>d__4 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
			{
				private int <>1__state;

				private Status <>2__current;

				private int <>l__initialThreadId;

				public SubActSmash <>4__this;

				Status IEnumerator<Status>.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				object IEnumerator.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				[DebuggerHidden]
				public <Run>d__4(int <>1__state)
				{
					this.<>1__state = <>1__state;
					<>l__initialThreadId = Environment.CurrentManagedThreadId;
				}

				[DebuggerHidden]
				void IDisposable.Dispose()
				{
					<>1__state = -2;
				}

				private bool MoveNext()
				{
					//IL_0038: Unknown result type (might be due to invalid IL or missing references)
					//IL_003d: Unknown result type (might be due to invalid IL or missing references)
					//IL_007a: Unknown result type (might be due to invalid IL or missing references)
					//IL_007f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0097: Unknown result type (might be due to invalid IL or missing references)
					//IL_009c: Unknown result type (might be due to invalid IL or missing references)
					int num = <>1__state;
					SubActSmash subActSmash = <>4__this;
					switch (num)
					{
					default:
						return false;
					case 0:
						<>1__state = -1;
						<>2__current = ((AIAct)subActSmash).DoGoto(subActSmash.pos, 1, true, (Func<Status>)null);
						<>1__state = 1;
						return true;
					case 1:
						<>1__state = -1;
						if (!CanSmash((Card)(object)subActSmash.refThing) || !((Act)ACT.Melee).Perform(((AIAct)subActSmash).owner, (Card)(object)subActSmash.refThing, (Point)null))
						{
							<>2__current = ((AIAct)subActSmash).Cancel();
							<>1__state = 2;
							return true;
						}
						goto IL_0094;
					case 2:
						<>1__state = -1;
						goto IL_0094;
					case 3:
						{
							<>1__state = -1;
							return false;
						}
						IL_0094:
						<>2__current = ((AIAct)subActSmash).Success((Action)null);
						<>1__state = 3;
						return true;
					}
				}

				bool IEnumerator.MoveNext()
				{
					//ILSpy generated this explicit interface implementation from .override directive in MoveNext
					return this.MoveNext();
				}

				[DebuggerHidden]
				void IEnumerator.Reset()
				{
					throw new NotSupportedException();
				}

				[DebuggerHidden]
				IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
				{
					<Run>d__4 result;
					if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
					{
						<>1__state = 0;
						result = this;
					}
					else
					{
						result = new <Run>d__4(0)
						{
							<>4__this = <>4__this
						};
					}
					return result;
				}

				[DebuggerHidden]
				IEnumerator IEnumerable.GetEnumerator()
				{
					return ((IEnumerable<Status>)this).GetEnumerator();
				}
			}

			public Thing refThing;

			public Point pos;

			public SubActSmash()
			{
			}

			public SubActSmash(Point pos, Thing refThing)
			{
				this.pos = pos;
				this.refThing = refThing;
			}

			[IteratorStateMachine(typeof(<Run>d__4))]
			public override IEnumerable<Status> Run()
			{
				//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
				return new <Run>d__4(-2)
				{
					<>4__this = this
				};
			}
		}

		[CompilerGenerated]
		private sealed class <Run>d__9 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActSmash <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__9(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
				//IL_0083: Unknown result type (might be due to invalid IL or missing references)
				//IL_0088: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActSmash autoActSmash = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_009d;
				case 1:
					<>1__state = -1;
					goto IL_009d;
				case 2:
					{
						<>1__state = -1;
						return false;
					}
					IL_009d:
					if (((AIAct)autoActSmash).CanProgress())
					{
						Thing val = autoActSmash.FindThing(CanSmash, autoActSmash.detRangeSq);
						if (ClassExtension.IsNull((object)val))
						{
							autoActSmash.SayNoTarget();
							return false;
						}
						autoActSmash.Child.pos = ((Card)val).pos;
						autoActSmash.Child.refThing = val;
						<>2__current = autoActSmash.StartNextTask();
						<>1__state = 1;
						return true;
					}
					<>2__current = autoActSmash.FailOrSuccess();
					<>1__state = 2;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__9 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__9(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq;

		public SubActSmash Child => ((AIAct)this).child as SubActSmash;

		public override Point Pos => Child.pos;

		public AutoActSmash(SubActSmash source)
			: base((AIAct)(object)source)
		{
			detRangeSq = Settings.DetRangeSq;
			SetTarget((Card)(object)source.refThing);
		}

		public static AutoActSmash TryCreate(string lang, Card target, Point pos)
		{
			if (EClass.pc.ai is AutoActSmash)
			{
				return null;
			}
			if (lang != GetActMeleeLang())
			{
				return null;
			}
			if (!CanSmash(target))
			{
				return null;
			}
			return new AutoActSmash(new SubActSmash(pos, (Thing)(object)((target is Thing) ? target : null)));
		}

		public static bool CanSmash(Card t)
		{
			if (t is Thing && t.trait.CanBeAttacked && t.trait.CanBeSmashedToDeath)
			{
				return !(t.trait is TraitTrainingDummy);
			}
			return false;
		}

		public static string GetActMeleeLang()
		{
			return ((BaseRow)((Element)ACT.Melee).source).GetText("name", false);
		}

		[IteratorStateMachine(typeof(<Run>d__9))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__9(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActSteal : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__11 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActSteal <>4__this;

			private Card <lastTarget>5__2;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__11(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<lastTarget>5__2 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				//IL_0126: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
				//IL_0102: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActSteal CS$<>8__locals0 = <>4__this;
				Card obj;
				Chara val2;
				Card val;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					<lastTarget>5__2 = ((AI_TargetCard)CS$<>8__locals0.Child).target;
					goto IL_003a;
				case 1:
					<>1__state = -1;
					<>2__current = CS$<>8__locals0.SetNextTask((AIAct)(object)CS$<>8__locals0.steal);
					<>1__state = 2;
					return true;
				case 2:
					{
						<>1__state = -1;
						if (!((AIAct)CS$<>8__locals0).CanProgress())
						{
							return false;
						}
						goto IL_003a;
					}
					IL_003a:
					val = null;
					obj = <lastTarget>5__2;
					val2 = (Chara)(object)((obj is Chara) ? obj : null);
					if (ClassExtension.IsNull((object)val2))
					{
						val = (Card)(object)CS$<>8__locals0.FindThing((Thing t) => CS$<>8__locals0.IsTarget((Card)(object)t) && CS$<>8__locals0.CanSteal((Card)(object)t), CS$<>8__locals0.detRangeSq);
					}
					else
					{
						bool useOriginalPos = CS$<>8__locals0.useOriginalPos;
						val = (Card)(object)CS$<>8__locals0.FindChara(IsTargetChara, CS$<>8__locals0.detRangeSq);
						if (useOriginalPos && ((AI_TargetCard)CS$<>8__locals0.Child).target != val)
						{
							((Card)((AIAct)CS$<>8__locals0).owner).Say("steal_chara_nothing", (Card)(object)((AIAct)CS$<>8__locals0).owner, (Card)(object)val2, (string)null, (string)null);
						}
					}
					if (ClassExtension.IsNull((object)val))
					{
						CS$<>8__locals0.SayNoTarget();
						return false;
					}
					CS$<>8__locals0.pos = val.pos;
					<lastTarget>5__2 = val;
					((AI_TargetCard)CS$<>8__locals0.Child).target = val;
					<>2__current = ((AIAct)CS$<>8__locals0).DoGoto(CS$<>8__locals0.Pos, 1, true, (Func<Status>)null);
					<>1__state = 1;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__11 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__11(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq;

		public AI_Steal steal;

		public Point pos;

		public AI_Steal Child => steal;

		public override Point Pos => pos;

		public AutoActSteal(AIAct source)
			: base(source)
		{
			steal = (AI_Steal)(object)((source is AI_Steal) ? source : null);
			AI_Steal val = steal;
			if (((AI_TargetCard)val).target == null)
			{
				((AI_TargetCard)val).target = Act.TC;
			}
			pos = ((AI_TargetCard)Child).target.pos;
			detRangeSq = Settings.DetRangeSq;
			if (Settings.SimpleIdentify > 0 && !(((AI_TargetCard)Child).target is Chara))
			{
				targetId = -1;
			}
			else
			{
				SetTarget(((AI_TargetCard)Child).target);
			}
		}

		public static AutoActSteal TryCreate(AIAct source)
		{
			AI_Steal val = (AI_Steal)(object)((source is AI_Steal) ? source : null);
			if (val == null)
			{
				return null;
			}
			return new AutoActSteal((AIAct)(object)val);
		}

		public bool CanSteal(Card c)
		{
			if (ClassExtension.IsNull((object)c.things.FindStealable()) && c.ChildrenAndSelfWeight > ((Card)((AIAct)this).owner).Evalue(281) * 200 + ((Card)((AIAct)this).owner).STR * 100 + 1000)
			{
				return false;
			}
			if (!EClass._zone.IsUserZone && !(c.isThing & (EClass._zone is Zone_LittleGarden)) && (c.isNPCProperty || !c.isThing) && c.trait.CanBeStolen && c.c_lockLv <= 0)
			{
				if (!c.isThing)
				{
					return !c.IsPCFaction;
				}
				return true;
			}
			return false;
		}

		public static bool IsTargetChara(Chara chara)
		{
			if (!((Card)chara).IsPCFaction)
			{
				if (!((Card)chara).things.FindStealable().HasValue())
				{
					return ((BaseCard)chara).GetInt(30, (int?)null) < ((Date)EClass.world.date).GetRaw(0);
				}
				return true;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__11))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__11(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActThrowMilk : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__8 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActThrowMilk <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__8(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActThrowMilk autoActThrowMilk = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_0029;
				case 1:
				{
					<>1__state = -1;
					if (!((AIAct)autoActThrowMilk).CanProgress() && !autoActThrowMilk.TrySwitchToMilk())
					{
						return false;
					}
					Chara val = autoActThrowMilk.FindChara(NeedMilk);
					if (ClassExtension.IsNull((object)val))
					{
						autoActThrowMilk.SayNoTarget();
						return false;
					}
					autoActThrowMilk.target = val;
					<>2__current = ((AIAct)autoActThrowMilk).DoGoto(autoActThrowMilk.Pos, 6, true, (Func<Status>)null);
					<>1__state = 2;
					return true;
				}
				case 2:
					{
						<>1__state = -1;
						goto IL_0029;
					}
					IL_0029:
					ActThrow.Throw((Card)(object)((AIAct)autoActThrowMilk).owner, ((Card)autoActThrowMilk.target).pos, (Card)(object)autoActThrowMilk.target, ((AIAct)autoActThrowMilk).owner.held.Split(1), (ThrowMethod)0);
					<>2__current = ((AIAct)autoActThrowMilk).KeepRunning();
					<>1__state = 1;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__8 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__8(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq = Settings.DetRangeSq;

		public Chara target;

		public override Point Pos => ((Card)target).pos;

		public AutoActThrowMilk(Chara target)
		{
			this.target = target;
			base..ctor();
		}

		public static AutoActThrowMilk TryCreate(string lang, Card target, Point pos)
		{
			if ((!(lang != ClassExtension.lang("ActThrow")) || !(lang != ClassExtension.lang("actMilk"))) && EClass.pc.held.trait is TraitDrinkMilkMother)
			{
				Chara val = pos.FindChara((Func<Chara, bool>)NeedMilk);
				if (val != null)
				{
					return new AutoActThrowMilk(val);
				}
			}
			return null;
		}

		public override bool CanProgress()
		{
			return ((AIAct)this).owner.held?.trait is TraitDrinkMilkMother;
		}

		public static bool NeedMilk(Chara chara)
		{
			if (((Card)chara).Evalue(1232) > 0)
			{
				return ((Card)chara).IsPCFaction;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__8))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__8(-2)
			{
				<>4__this = this
			};
		}

		public bool TrySwitchToMilk()
		{
			Thing val = null;
			foreach (Thing item in ((Card)EClass.pc).things.Flatten())
			{
				if (((Card)item).trait is TraitDrinkMilkMother && (ClassExtension.IsNull((object)val) || ((Card)item).encLV > ((Card)val).encLV))
				{
					val = item;
				}
			}
			if (val.HasValue())
			{
				EClass.pc.HoldCard((Card)(object)val, -1);
			}
			return val.HasValue();
		}
	}
	public class AutoActUnlock : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__9 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActUnlock <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__9(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActUnlock autoActUnlock = <>4__this;
				Thing val;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					goto IL_0029;
				case 1:
					<>1__state = -1;
					<>2__current = autoActUnlock.SetNextTask((AIAct)(object)autoActUnlock.openLock);
					<>1__state = 2;
					return true;
				case 2:
					{
						<>1__state = -1;
						if (!((AIAct)autoActUnlock).CanProgress())
						{
							return false;
						}
						goto IL_0029;
					}
					IL_0029:
					val = autoActUnlock.FindThing(NeedUnlock, autoActUnlock.detRangeSq);
					if (ClassExtension.IsNull((object)val))
					{
						autoActUnlock.SayNoTarget();
						return false;
					}
					((AI_TargetThing)autoActUnlock.Child).target = val;
					<>2__current = ((AIAct)autoActUnlock).DoGoto(autoActUnlock.Pos, 1, true, (Func<Status>)null);
					<>1__state = 1;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__9 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__9(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public int detRangeSq = Settings.DetRangeSq;

		public AI_OpenLock openLock = (AI_OpenLock)(object)((source is AI_OpenLock) ? source : null);

		public AI_OpenLock Child => openLock;

		public override Point Pos => ((Card)((AI_TargetThing)openLock).target).pos;

		public AutoActUnlock(AIAct source)
			: base(source)
		{
		}

		public static AutoActUnlock TryCreate(AIAct source)
		{
			AI_OpenLock val = (AI_OpenLock)(object)((source is AI_OpenLock) ? source : null);
			if (val == null || !NeedUnlock(((AI_TargetThing)val).target))
			{
				return null;
			}
			return new AutoActUnlock((AIAct)(object)val);
		}

		public static bool NeedUnlock(Thing t)
		{
			if (((Card)t).trait is TraitContainer)
			{
				return ((Card)t).c_lockLv > 0;
			}
			return false;
		}

		[IteratorStateMachine(typeof(<Run>d__9))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__9(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActWait : AutoAct
	{
		[CompilerGenerated]
		private sealed class <Run>d__10 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActWait <>4__this;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__10(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActWait autoActWait = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					break;
				case 1:
					<>1__state = -1;
					break;
				}
				if (((AIAct)autoActWait).CanProgress())
				{
					<>2__current = ((AIAct)autoActWait).KeepRunning();
					<>1__state = 1;
					return true;
				}
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__10 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__10(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public new Func<bool> canContinue;

		public override int MaxRestart => 0;

		public override Point Pos => ((Card)((AIAct)this).owner).pos;

		public override bool CancelWhenDamaged => !EClass.pc.party.members.Any((Chara chara) => chara.ai.Current is GoalCombat && chara.ai is AutoAct);

		public override void OnStart()
		{
			SetStartPos();
			AIAct child = ((AIAct)this).child;
			if (child != null)
			{
				child.Reset();
			}
		}

		public override bool CanProgress()
		{
			if (!ClassExtension.IsNull((object)canContinue))
			{
				return canContinue();
			}
			return true;
		}

		[IteratorStateMachine(typeof(<Run>d__10))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__10(-2)
			{
				<>4__this = this
			};
		}
	}
	public class AutoActWater : AutoAct
	{
		public class SubActWater : AIAct
		{
			[CompilerGenerated]
			private sealed class <Run>d__1 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
			{
				private int <>1__state;

				private Status <>2__current;

				private int <>l__initialThreadId;

				public SubActWater <>4__this;

				private AutoActWater <parent>5__2;

				Status IEnumerator<Status>.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				object IEnumerator.Current
				{
					[DebuggerHidden]
					get
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return <>2__current;
					}
				}

				[DebuggerHidden]
				public <Run>d__1(int <>1__state)
				{
					this.<>1__state = <>1__state;
					<>l__initialThreadId = Environment.CurrentManagedThreadId;
				}

				[DebuggerHidden]
				void IDisposable.Dispose()
				{
					<parent>5__2 = null;
					<>1__state = -2;
				}

				private bool MoveNext()
				{
					//IL_0034: Unknown result type (might be due to invalid IL or missing references)
					//IL_0039: Unknown result type (might be due to invalid IL or missing references)
					//IL_006e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0073: Unknown result type (might be due to invalid IL or missing references)
					int num = <>1__state;
					SubActWater CS$<>8__locals0 = <>4__this;
					switch (num)
					{
					default:
						return false;
					case 0:
						<>1__state = -1;
						<>2__current = ((AIAct)CS$<>8__locals0).DoGoto(CS$<>8__locals0.dest, 1, true, (Func<Status>)null);
						<>1__state = 1;
						return true;
					case 1:
						<>1__state = -1;
						<parent>5__2 = ((AIAct)CS$<>8__locals0).parent as AutoActWater;
						if (!<parent>5__2.IsWaterCanValid())
						{
							<>2__current = ((AIAct)CS$<>8__locals0).Cancel();
							<>1__state = 2;
							return true;
						}
						break;
					case 2:
						<>1__state = -1;
						break;
					}
					int num2 = ((Trait)<parent>5__2.waterCan).owner.Evalue(770);
					num2 = ((num2 <= 0) ? 1 : Mathf.Min(((Trait)<parent>5__2.waterCan).owner.c_charges, 2 + num2 / 10));
					if (num2 > 1)
					{
						List<Point> list = CS$<>8__locals0.ListPointsInSquare(CS$<>8__locals0.dest, num2 - 1, mustBeWalkable: false, los: false);
						list.Sort((Point a, Point b) => a.Distance(CS$<>8__locals0.dest) - b.Distance(CS$<>8__locals0.dest));
						foreach (Point item in list)
						{
							CS$<>8__locals0.Water(item);
						}
					}
					else
					{
						CS$<>8__locals0.Water(CS$<>8__locals0.dest);
					}
					((Card)((AIAct)CS$<>8__locals0).owner).PlaySound("water_farm", 1f, true);
					((Card)((AIAct)CS$<>8__locals0).owner).Say("water_farm", (Card)(object)((AIAct)CS$<>8__locals0).owner, CS$<>8__locals0.dest.cell.GetFloorName(), (string)null);
					((Trait)<parent>5__2.waterCan).owner.ModCharge(-num2, false);
					return false;
				}

				bool IEnumerator.MoveNext()
				{
					//ILSpy generated this explicit interface implementation from .override directive in MoveNext
					return this.MoveNext();
				}

				[DebuggerHidden]
				void IEnumerator.Reset()
				{
					throw new NotSupportedException();
				}

				[DebuggerHidden]
				IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
				{
					<Run>d__1 result;
					if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
					{
						<>1__state = 0;
						result = this;
					}
					else
					{
						result = new <Run>d__1(0)
						{
							<>4__this = <>4__this
						};
					}
					return result;
				}

				[DebuggerHidden]
				IEnumerator IEnumerable.GetEnumerator()
				{
					return ((IEnumerable<Status>)this).GetEnumerator();
				}
			}

			public Point dest;

			[IteratorStateMachine(typeof(<Run>d__1))]
			public override IEnumerable<Status> Run()
			{
				//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
				return new <Run>d__1(-2)
				{
					<>4__this = this
				};
			}

			public void Water(Point point)
			{
				point.cell.isWatered = true;
				if (!((WeightCell)point.cell).blocked && EClass.rnd(5) == 0)
				{
					EClass._map.SetLiquid(point.x, point.z, 1, 1);
				}
				if (point.cell.HasFire)
				{
					point.ModFire(-50, true);
				}
				((Card)base.owner).ModExp(286, 15);
			}

			public List<Point> ListPointsInSquare(Point center, int radius, bool mustBeWalkable = true, bool los = true)
			{
				List<Point> list = new List<Point>();
				ForeachSquare(center.x, center.z, radius, delegate(Point p)
				{
					if ((!mustBeWalkable || !((WeightCell)p.cell).blocked) && (!los || Los.IsVisible(center, p, (Action<Point, bool>)null)))
					{
						list.Add(p.Copy());
					}
				});
				return list;
			}

			public void ForeachSquare(int _x, int _z, int r, Action<Point> action)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Expected O, but got Unknown
				Point val = new Point();
				for (int i = _x - r; i < _x + r + 1; i++)
				{
					if (i < 0 || i >= ((MapBounds)EClass._map).Size)
					{
						continue;
					}
					for (int j = _z - r; j < _z + r + 1; j++)
					{
						if (j >= 0 && j < ((MapBounds)EClass._map).Size)
						{
							val.Set(i, j);
							action(val);
						}
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class <>c__DisplayClass10_0
		{
			public HashSet<Point> range;

			internal void <Run>b__0(Point p)
			{
				if (TaskWater.ShouldWater(p))
				{
					range.Add(p.Copy());
				}
			}
		}

		[CompilerGenerated]
		private sealed class <Run>d__10 : IEnumerable<Status>, IEnumerable, IEnumerator<Status>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private Status <>2__current;

			private int <>l__initialThreadId;

			public AutoActWater <>4__this;

			private <>c__DisplayClass10_0 <>8__1;

			private Point <targetPos>5__2;

			Status IEnumerator<Status>.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Run>d__10(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>8__1 = null;
				<targetPos>5__2 = null;
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				//IL_014f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0154: Unknown result type (might be due to invalid IL or missing references)
				//IL_0166: Expected O, but got Unknown
				//IL_0161: Unknown result type (might be due to invalid IL or missing references)
				//IL_0166: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_006b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0293: Unknown result type (might be due to invalid IL or missing references)
				//IL_0298: Unknown result type (might be due to invalid IL or missing references)
				//IL_0247: Unknown result type (might be due to invalid IL or missing references)
				//IL_024c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				//IL_0126: Unknown result type (might be due to invalid IL or missing references)
				int num = <>1__state;
				AutoActWater CS$<>8__locals0 = <>4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
				{
					<>1__state = -1;
					ref TraitToolWaterCan waterCan = ref CS$<>8__locals0.waterCan;
					Trait obj = ((AIAct)CS$<>8__locals0).owner.held?.trait;
					waterCan = (TraitToolWaterCan)(object)((obj is TraitToolWaterCan) ? obj : null);
					if (ClassExtension.IsNull((object)CS$<>8__locals0.waterCan))
					{
						<>2__current = CS$<>8__locals0.Fail();
						<>1__state = 1;
						return true;
					}
					goto IL_0080;
				}
				case 1:
					<>1__state = -1;
					goto IL_0080;
				case 2:
					<>1__state = -1;
					goto IL_013b;
				case 3:
					<>1__state = -1;
					<targetPos>5__2 = null;
					goto IL_0182;
				case 4:
					<>1__state = -1;
					goto IL_0261;
				case 5:
					{
						<>1__state = -1;
						return false;
					}
					IL_0182:
					CS$<>8__locals0.waterFirst = false;
					<>8__1.range = new HashSet<Point>();
					EClass._map.ForeachPoint((Action<Point>)delegate(Point p)
					{
						if (TaskWater.ShouldWater(p))
						{
							<>8__1.range.Add(p.Copy());
						}
					});
					if (<>8__1.range.Count == 0)
					{
						CS$<>8__locals0.SayNoTarget();
						return false;
					}
					goto IL_0261;
					IL_013b:
					<>2__current = ((AIAct)CS$<>8__locals0).Do((AIAct)new DynamicAIAct("SubActDrawWater_AutoAct", (Func<bool>)delegate
					{
						((Card)((AIAct)CS$<>8__locals0).owner).PlaySound("water_draw", 1f, true);
						((Trait)CS$<>8__locals0.waterCan).owner.SetCharge(CS$<>8__locals0.waterCan.MaxCharge);
						((Card)((AIAct)CS$<>8__locals0).owner).Say("water_draw", (Card)(object)((AIAct)CS$<>8__locals0).owner, ((Trait)CS$<>8__locals0.waterCan).owner, (string)null, (string)null);
						return true;
					}, false)
					{
						pos = <targetPos>5__2
					}, (Func<Status>)null);
					<>1__state = 3;
					return true;
					IL_0261:
					if (<>8__1.range.Count > 0 && CS$<>8__locals0.IsWaterCanValid())
					{
						Point val = CS$<>8__locals0.FindPos((Cell cell) => TaskWater.ShouldWater(cell.GetPoint()), 2, 0, <>8__1.range);
						if (ClassExtension.IsNull((object)val))
						{
							CS$<>8__locals0.SayNoTarget();
							return false;
						}
						<>8__1.range.Remove(val);
						CS$<>8__locals0.subActWater.dest = val;
						<>2__current = CS$<>8__locals0.SetNextTask((AIAct)(object)CS$<>8__locals0.subActWater, (Func<Status>)((AIAct)CS$<>8__locals0).KeepRunning, resetRestartCount: true);
						<>1__state = 4;
						return true;
					}
					<>8__1 = null;
					if (!((AIAct)CS$<>8__locals0).CanProgress())
					{
						<>2__current = CS$<>8__locals0.FailOrSuccess();
						<>1__state = 5;
						return true;
					}
					goto IL_0080;
					IL_0080:
					<>8__1 = new <>c__DisplayClass10_0();
					if (((Trait)CS$<>8__locals0.waterCan).owner.c_charges < CS$<>8__locals0.waterCan.MaxCharge && !CS$<>8__locals0.waterFirst)
					{
						<targetPos>5__2 = CS$<>8__locals0.FindPos((Cell c) => ActDrawWater.HasWaterSource(c.GetPoint()), 80000);
						if (ClassExtension.IsNull((object)<targetPos>5__2))
						{
							if (((Card)((AIAct)CS$<>8__locals0).owner).IsPC && ((Trait)CS$<>8__locals0.waterCan).owner.c_charges == 0)
							{
								Msg.Say("water_deplete");
							}
							<>2__current = CS$<>8__locals0.Fail();
							<>1__state = 2;
							return true;
						}
						goto IL_013b;
					}
					goto IL_0182;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<Status> IEnumerable<Status>.GetEnumerator()
			{
				<Run>d__10 result;
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					result = this;
				}
				else
				{
					result = new <Run>d__10(0)
					{
						<>4__this = <>4__this
					};
				}
				return result;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<Status>)this).GetEnumerator();
			}
		}

		public bool waterFirst;

		public TraitToolWaterCan waterCan;

		public SubActWater subActWater = new SubActWater();

		public override Point Pos => subActWater.dest;

		public AutoActWater(Point pos)
		{
			subActWater.dest = pos;
		}

		public static AutoActWater TryCreate(AIAct source)
		{
			TaskWater val = (TaskWater)(object)((source is TaskWater) ? source : null);
			if (val == null || val.dest.cell.HasFire)
			{
				return null;
			}
			return new AutoActWater(val.dest)
			{
				waterFirst = true
			};
		}

		public static AutoActWater TryCreate(string lang, Card target, Point pos)
		{
			if (lang != ClassExtension.lang("ActDrawWater"))
			{
				return null;
			}
			return new AutoActWater(pos);
		}

		public bool IsWaterCanValid()
		{
			if (((object)waterCan)?.Equals((object)((AIAct)this).owner.held?.trait) ?? false)
			{
				return ((Trait)waterCan).owner.c_charges > 0;
			}
			return false;
		}

		public override bool CanProgress()
		{
			return ((object)waterCan)?.Equals((object)((AIAct)this).owner.held?.trait) ?? false;
		}

		[IteratorStateMachine(typeof(<Run>d__10))]
		public override IEnumerable<Status> Run()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Run>d__10(-2)
			{
				<>4__this = this
			};
		}
	}
}
