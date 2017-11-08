using System;
using Gtk;
using VAS.Core.Interfaces.GUI;

public partial class MainWindow : Gtk.Window, IMainController
{
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}

	public bool SetPanel (IPanel newPanel)
	{
		return true;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
