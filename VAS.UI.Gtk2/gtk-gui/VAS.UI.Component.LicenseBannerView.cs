
// This file has been generated by the GUI designer. Do not modify.
namespace VAS.UI.Component
{
	public partial class LicenseBannerView
	{
		private global::Gtk.HBox hbox1;

		private global::Gtk.Alignment alignment1;

		private global::Gtk.Label licenseTextLabel;

		private global::Gtk.Button upgradeButton;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget VAS.UI.Component.LicenseBannerView
			global::Stetic.BinContainer.Attach (this);
			this.Name = "VAS.UI.Component.LicenseBannerView";
			// Container child VAS.UI.Component.LicenseBannerView.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 20;
			// Container child hbox1.Gtk.Box+BoxChild
			this.alignment1 = new global::Gtk.Alignment (0.5F, 0.5F, 1F, 1F);
			this.alignment1.Name = "alignment1";
			this.hbox1.Add (this.alignment1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.alignment1]));
			w1.Position = 0;
			w1.Expand = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.licenseTextLabel = new global::Gtk.Label ();
			this.licenseTextLabel.Name = "licenseTextLabel";
			this.licenseTextLabel.UseMarkup = true;
			this.licenseTextLabel.Wrap = true;
			this.licenseTextLabel.Justify = ((global::Gtk.Justification)(2));
			this.hbox1.Add (this.licenseTextLabel);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.licenseTextLabel]));
			w2.Position = 1;
			// Container child hbox1.Gtk.Box+BoxChild
			this.upgradeButton = new global::Gtk.Button ();
			this.upgradeButton.CanFocus = true;
			this.upgradeButton.Name = "upgradeButton";
			this.upgradeButton.UseUnderline = true;
			this.upgradeButton.Label = "Upgrade now";
			this.hbox1.Add (this.upgradeButton);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.upgradeButton]));
			w3.PackType = ((global::Gtk.PackType)(1));
			w3.Position = 2;
			w3.Expand = false;
			w3.Fill = false;
			this.Add (this.hbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
