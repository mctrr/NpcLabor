using System;
using System.Collections.Generic;
using UnityEngine;

public class LayerList : ELayer
{
	public class CustomItem
	{
		public string lang;

		public Action<int> action;

		public int id;
	}

	public UIList list;

	public UIButton buttonReroll;

	public UIButton highlightTarget;

	public UIItem moldItemCheck;

	public Transform moldItemGeneral;

	public Transform moldItemDetail;

	public bool useItem;

	public bool autoX;

	public bool autoY;

	public bool noSound;

	public UIScrollView scroll;

	public Vector2 sizeMin;

	public Vector2 sizeMax;

	public Vector2 paddings;

	private bool initialized;

	public List<CustomItem> customItems = new List<CustomItem>();

	protected override void Awake()
	{
		base.Awake();
		if ((bool)buttonReroll)
		{
			buttonReroll.SetActive(enable: false);
		}
	}

	public override void OnInit()
	{
		if ((bool)highlightTarget)
		{
			highlightTarget.DoHighlightTransition();
		}
		EInput.WaitReleaseKey();
	}

	public override void OnUpdateInput()
	{
		foreach (UIList.ButtonPair button in list.buttons)
		{
			ItemGeneral itemGeneral = button.component as ItemGeneral;
			UIButton uIButton = (itemGeneral ? itemGeneral.button1 : (button.component as UIButton));
			if ((bool)uIButton && uIButton.interactable && !EInput.waitReleaseAnyKey && (bool)uIButton.keyText && !uIButton.keyText.text.IsEmpty() && uIButton.keyText.text == Input.inputString)
			{
				SE.ClickOk();
				uIButton.onClick.Invoke();
				return;
			}
		}
		base.OnUpdateInput();
	}

	public LayerList SetMold(int index)
	{
		switch (index)
		{
		case 0:
			list.moldItem = moldItemGeneral;
			break;
		case 1:
			list.moldItem = moldItemDetail;
			break;
		}
		return this;
	}

	public LayerList SetSize(float w = 450f, float h = -1f)
	{
		Vector2 sizeDelta = windows[0].Rect().sizeDelta;
		windows[0].Rect().sizeDelta = new Vector2((w == -1f) ? sizeDelta.x : w, (h == -1f) ? sizeDelta.y : h);
		if (w != -1f)
		{
			autoX = false;
		}
		if (h != -1f)
		{
			autoY = false;
		}
		return this;
	}

	public LayerList SetPivot(float x, float y = -1f)
	{
		windows[0].Rect().pivot = new Vector2(x, y);
		return this;
	}

	public void RefreshSize()
	{
		RectTransform rectTransform = windows[0].Rect();
		float x = rectTransform.sizeDelta.x;
		float y = rectTransform.sizeDelta.y;
		this.RebuildLayout(recursive: true);
		Vector2 sizeDelta = scroll.content.Rect().sizeDelta;
		if (autoX)
		{
			x = Mathf.Clamp(sizeDelta.x, sizeMin.x, sizeMax.x) + paddings.x;
		}
		if (autoY)
		{
			y = Mathf.Clamp(sizeDelta.y, sizeMin.y, sizeMax.y) + paddings.y;
		}
		rectTransform.sizeDelta = new Vector2(x, y);
	}

	public LayerList SetList<TValue>(ICollection<TValue> items, Func<TValue, string> getString, Action<int, string> onSelect, bool autoClose = true)
	{
		List<string> strs = new List<string>();
		foreach (TValue item in items)
		{
			strs.Add(getString(item));
		}
		SetStringList(() => strs, onSelect, autoClose);
		return this;
	}

	public LayerList SetStringList(Func<ICollection<string>> getList, Action<int, string> onSelect, bool autoClose = true)
	{
		list.callbacks = new UIList.Callback<string, ItemGeneral>
		{
			onClick = delegate(string a, ItemGeneral b)
			{
				onSelect(list.items.IndexOf(a), a);
				if (autoClose)
				{
					Close();
				}
			},
			onInstantiate = delegate(string a, ItemGeneral item)
			{
				item.button1.mainText.text = a;
				item.DisableIcon();
				item.Build();
				if (noSound)
				{
					item.button1.soundClick = null;
				}
			},
			onList = delegate
			{
				foreach (string item in getList())
				{
					list.Add(item);
				}
			}
		};
		list.List();
		RefreshSize();
		return this;
	}

	public LayerList SetList2<TValue>(ICollection<TValue> _list, Func<TValue, string> getText, Action<TValue, ItemGeneral> onClick, Action<TValue, ItemGeneral> onInstantiate, bool autoClose = true)
	{
		list.callbacks = new UIList.Callback<TValue, ItemGeneral>
		{
			onClick = delegate(TValue a, ItemGeneral b)
			{
				onClick(a, b);
				if (autoClose)
				{
					Close();
				}
			},
			onInstantiate = delegate(TValue a, ItemGeneral item)
			{
				item.button1.mainText.text = getText(a);
				item.DisableIcon();
				if (noSound)
				{
					item.button1.soundClick = null;
				}
				if (onInstantiate != null)
				{
					onInstantiate(a, item);
				}
				item.Build();
			},
			onList = delegate
			{
				foreach (TValue item in _list)
				{
					list.Add(item);
				}
			}
		};
		list.List();
		RefreshSize();
		UIButton.TryShowTip(base.transform);
		return this;
	}

	public LayerList SetListCheck<TValue>(ICollection<TValue> _list, Func<TValue, string> getText, Action<TValue, ItemGeneral> onClick, Action<List<UIList.ButtonPair>> onValidate)
	{
		list.moldItem = moldItemCheck.transform;
		list.callbacks = new UIList.Callback<TValue, ItemGeneral>
		{
			onClick = delegate
			{
			},
			onInstantiate = delegate(TValue a, ItemGeneral item)
			{
				item.button1.mainText.text = getText(a);
				item.DisableIcon();
				item.Build();
				item.button1.SetOnClick(delegate
				{
					onClick(a, item);
					Validate();
				});
			},
			onList = delegate
			{
				foreach (TValue item in _list)
				{
					list.Add(item);
				}
			}
		};
		list.List();
		Validate();
		RefreshSize();
		UIButton.TryShowTip(base.transform);
		return this;
		void Validate()
		{
			onValidate(list.buttons);
		}
	}

	public LayerList EnableReroll()
	{
		buttonReroll.SetActive(enable: true);
		buttonReroll.onClick.RemoveAllListeners();
		buttonReroll.onClick.AddListener(delegate
		{
			list.List();
		});
		list.RebuildLayout(recursive: true);
		RefreshSize();
		return this;
	}

	public LayerList ManualList(Action<UIList, LayerList> onInit)
	{
		buttonReroll.SetActive(enable: false);
		list.moldItem = moldItemGeneral;
		onInit(list, this);
		list.List();
		RefreshSize();
		return this;
	}

	public LayerList SetHeader(string lang)
	{
		windows[0].SetCaption(lang.lang());
		return this;
	}

	public LayerList SetNoSound()
	{
		noSound = true;
		return this;
	}

	public void SetHighlightTarget(UIButton _target)
	{
		highlightTarget = _target;
		highlightTarget.DoHighlightTransition();
	}

	private void Update()
	{
		if ((bool)highlightTarget)
		{
			highlightTarget.DoHighlightTransition();
		}
	}

	private void LateUpdate()
	{
		if ((bool)highlightTarget)
		{
			highlightTarget.DoHighlightTransition();
		}
	}

	public override void OnKill()
	{
		if ((bool)highlightTarget)
		{
			ELayer.core.WaitForEndOfFrame(delegate
			{
				if ((bool)highlightTarget)
				{
					if (!InputModuleEX.IsPointerOver(highlightTarget))
					{
						highlightTarget.DoNormalTransition();
					}
					else
					{
						highlightTarget.DoHighlightTransition();
					}
				}
			});
		}
		TooltipManager.Instance.HideTooltips(immediate: true);
		TooltipManager.Instance.disableHide = null;
		EInput.WaitReleaseKey();
	}

	public void Add(string lang, Action<int> action)
	{
		if (!initialized)
		{
			initialized = true;
		}
		customItems.Add(new CustomItem
		{
			lang = lang,
			action = action,
			id = customItems.Count
		});
	}

	public void Show(bool autoClose = true)
	{
		list.callbacks = new UIList.Callback<CustomItem, ItemGeneral>
		{
			onClick = delegate(CustomItem a, ItemGeneral b)
			{
				a.action(a.id);
				if (autoClose)
				{
					Close();
				}
			},
			onInstantiate = delegate(CustomItem a, ItemGeneral item)
			{
				item.button1.mainText.text = a.lang.lang();
				item.DisableIcon();
				item.Build();
			},
			onList = delegate
			{
				foreach (CustomItem customItem in customItems)
				{
					list.Add(customItem);
				}
			}
		};
		buttonReroll.SetActive(enable: false);
		list.List();
		RefreshSize();
		if (!autoClose)
		{
			windows[0].AddBottomButton("back", Close, setFirst: true);
		}
	}
}
