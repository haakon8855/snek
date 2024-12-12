using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Snek.Client.Services;

public static class KeyEventHandler
{
	private static List<string> KeyboardEvent { get; } = new();
	private static readonly List<IKeyEventListener> KeyEventListeners = new();

	[JSInvokable("JsKeyDown")]
	public static void JsKeyDown(KeyboardEventArgs e)
	{
		KeyboardEvent.Add(e.Key);

		string keys = JsonSerializer.Serialize(KeyboardEvent);

		foreach (IKeyEventListener listener in KeyEventListeners)
		{
			listener.KeyEventChanged(keys);
		}
	}

	[JSInvokable("JsKeyUp")]
	public static void JsKeyUp(KeyboardEventArgs e)
	{
		KeyboardEvent.Remove(e.Key);
	}

	public static void RegisterListener(IKeyEventListener newListener)
	{
		KeyEventListeners.Add(newListener);
	}
}
