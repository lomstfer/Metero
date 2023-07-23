using Android.App;
using Android.Runtime;
using Plugin.CurrentActivity;

namespace Metero;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{

    }

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
