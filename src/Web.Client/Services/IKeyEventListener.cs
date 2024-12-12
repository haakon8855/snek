namespace Web.Client.Services;

public interface IKeyEventListener
{
	public Task KeyEventChanged(string keys);
}
