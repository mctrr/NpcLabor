using System;

public class Progress_Custom : AIProgress
{
	public string textHint;

	public int maxProgress = 20;

	public int interval = 2;

	public bool cancelWhenMoved = true;

	public bool cancelWhenDamaged = true;

	public bool showProgress = true;

	public Func<bool> canProgress;

	public Action onBeforeProgress;

	public Action onProgressComplete;

	public Action onProgressBegin;

	public Action<Progress_Custom> onProgress;

	public override bool CancelWhenMoved => cancelWhenMoved;

	public override bool CancelWhenDamaged => cancelWhenDamaged;

	public override int MaxProgress => maxProgress;

	public override bool ShowProgress => showProgress;

	public override int Interval => interval;

	public override string TextHint => textHint;

	public override bool CanProgress()
	{
		return canProgress?.Invoke() ?? true;
	}

	public override void OnProgress()
	{
		onProgress?.Invoke(this);
	}

	public override void OnProgressComplete()
	{
		onProgressComplete?.Invoke();
	}

	public override void OnBeforeProgress()
	{
		onBeforeProgress?.Invoke();
	}

	public override void OnProgressBegin()
	{
		onProgressBegin?.Invoke();
	}

	public Progress_Custom SetDuration(int max, int _interval = 2)
	{
		maxProgress = max;
		interval = _interval;
		return this;
	}
}
