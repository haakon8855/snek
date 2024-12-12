namespace Snek.Client.Services;

public interface IKeyEventListener
{
	public Task KeyEventChanged(string keys);
}
