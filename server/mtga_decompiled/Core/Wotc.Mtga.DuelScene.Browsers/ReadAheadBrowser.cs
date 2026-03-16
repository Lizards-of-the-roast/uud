using System;

namespace Wotc.Mtga.DuelScene.Browsers;

public class ReadAheadBrowser : CardBrowserBase
{
	private readonly IBasicBrowserProvider _provider;

	private SpinnerAnimated _spinner;

	public Action<int> SpinnerValueChanged;

	public ReadAheadBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_provider = provider as IBasicBrowserProvider;
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(_provider.GetHeaderText());
		component.SetSubheaderText(_provider.GetSubHeaderText());
		_spinner = GetBrowserElement("Spinner").GetComponent<SpinnerAnimated>();
		_spinner.ValueChanged += Spinner_ValueChanged;
		base.InitializeUIElements();
	}

	protected override void ReleaseUIElements()
	{
		_spinner.ValueChanged -= Spinner_ValueChanged;
		base.ReleaseUIElements();
	}

	public void SetMinVal(int min)
	{
		if (!(_spinner == null))
		{
			_spinner.UseMin = true;
			_spinner.MinValue = min;
		}
	}

	public void SetMaxVal(int max)
	{
		if (!(_spinner == null))
		{
			_spinner.UseMax = true;
			_spinner.MaxValue = max;
		}
	}

	public void SetValue(int val)
	{
		if (!(_spinner == null))
		{
			_spinner.InitValue(val);
		}
	}

	public override void Close()
	{
		cardViews.Clear();
		SpinnerValueChanged = null;
		base.Close();
	}

	private void Spinner_ValueChanged(object sender, ValueChangedEventArgs e)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		SpinnerValueChanged?.Invoke(e.NewValue);
	}

	protected override void SetupCards()
	{
		MoveCardViewsToBrowser(_provider.GetCardsToDisplay());
	}
}
