using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class HUD : RootPanel
{
	private bool _mouseEnabled = true;
	public bool MouseEnabled
	{
		get { return _mouseEnabled; }
		set
		{
			if(value != _mouseEnabled)
			{
				SetClass( "accept-input", value );
				Style.PointerEvents = value ? "visible" : "none";
				Style.Dirty();
			}
			_mouseEnabled = value;
		}
	}
}
