
namespace VAS.UI.Dialog
{
	public partial class VideoEditionProperties
	{
		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox2;

		private global::Gtk.Label label1;

		private global::Gtk.ComboBox qualitycombobox;

		private global::Gtk.HBox hbox4;

		private global::Gtk.Label label2;

		private global::Gtk.ComboBox sizecombobox;

		private global::Gtk.HBox hbox5;

		private global::Gtk.Label label3;

		private global::Gtk.ComboBox formatcombobox;

		private global::Gtk.HBox hbox6;

		private global::Gtk.CheckButton descriptioncheckbutton;

		private global::Gtk.CheckButton audiocheckbutton;

		private global::Gtk.CheckButton splitfilesbutton;

		private global::Gtk.HBox filebox;

		private global::Gtk.Label filenamelabel;

		private global::VAS.UI.Component.MediaFileChooser mediafilechooser1;

		private global::Gtk.HBox dirbox;

		private global::Gtk.Label directorynamelabel1;

		private global::VAS.UI.Component.MediaFileChooser mediafilechooser2;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget VAS.UI.Dialog.VideoEditionProperties
			this.Name = "VAS.UI.Dialog.VideoEditionProperties";
			this.Title = global::Mono.Unix.Catalog.GetString ("Video Properties");
			this.Icon = global::Stetic.IconLoader.LoadIcon (this, "longomatch", global::Gtk.IconSize.Dialog);
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.Modal = true;
			this.Gravity = ((global::Gdk.Gravity)(5));
			this.SkipPagerHint = true;
			this.SkipTaskbarHint = true;
			// Internal child VAS.UI.Dialog.VideoEditionProperties.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Homogeneous = true;
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Quality:");
			this.hbox2.Add (this.label1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label1]));
			w2.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.qualitycombobox = global::Gtk.ComboBox.NewText ();
			this.qualitycombobox.Name = "qualitycombobox";
			this.hbox2.Add (this.qualitycombobox);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.qualitycombobox]));
			w3.Position = 1;
			this.vbox2.Add (this.hbox2);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox2]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox ();
			this.hbox4.Name = "hbox4";
			this.hbox4.Homogeneous = true;
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 0F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Image format: ");
			this.hbox4.Add (this.label2);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.label2]));
			w5.Position = 0;
			// Container child hbox4.Gtk.Box+BoxChild
			this.sizecombobox = global::Gtk.ComboBox.NewText ();
			this.sizecombobox.Name = "sizecombobox";
			this.hbox4.Add (this.sizecombobox);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.sizecombobox]));
			w6.Position = 1;
			this.vbox2.Add (this.hbox4);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox4]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox5 = new global::Gtk.HBox ();
			this.hbox5.Name = "hbox5";
			this.hbox5.Homogeneous = true;
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.Xalign = 0F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("Encoding Format:");
			this.hbox5.Add (this.label3);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.label3]));
			w8.Position = 0;
			// Container child hbox5.Gtk.Box+BoxChild
			this.formatcombobox = global::Gtk.ComboBox.NewText ();
			this.formatcombobox.Name = "formatcombobox";
			this.hbox5.Add (this.formatcombobox);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.formatcombobox]));
			w9.Position = 1;
			this.vbox2.Add (this.hbox5);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox5]));
			w10.Position = 2;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox6 = new global::Gtk.HBox ();
			this.hbox6.Name = "hbox6";
			this.hbox6.Spacing = 6;
			// Container child hbox6.Gtk.Box+BoxChild
			this.descriptioncheckbutton = new global::Gtk.CheckButton ();
			this.descriptioncheckbutton.CanFocus = true;
			this.descriptioncheckbutton.Name = "descriptioncheckbutton";
			this.descriptioncheckbutton.Label = global::Mono.Unix.Catalog.GetString ("Enable title overlay");
			this.descriptioncheckbutton.DrawIndicator = true;
			this.descriptioncheckbutton.UseUnderline = true;
			this.hbox6.Add (this.descriptioncheckbutton);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox6 [this.descriptioncheckbutton]));
			w11.Position = 0;
			// Container child hbox6.Gtk.Box+BoxChild
			this.audiocheckbutton = new global::Gtk.CheckButton ();
			this.audiocheckbutton.CanFocus = true;
			this.audiocheckbutton.Name = "audiocheckbutton";
			this.audiocheckbutton.Label = global::Mono.Unix.Catalog.GetString ("Enable audio");
			this.audiocheckbutton.DrawIndicator = true;
			this.audiocheckbutton.UseUnderline = true;
			this.hbox6.Add (this.audiocheckbutton);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox6 [this.audiocheckbutton]));
			w12.Position = 1;
			this.vbox2.Add (this.hbox6);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox6]));
			w13.Position = 3;
			w13.Expand = false;
			w13.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.splitfilesbutton = new global::Gtk.CheckButton ();
			this.splitfilesbutton.CanFocus = true;
			this.splitfilesbutton.Name = "splitfilesbutton";
			this.splitfilesbutton.Label = global::Mono.Unix.Catalog.GetString ("Split output in one file per playlist element");
			this.splitfilesbutton.DrawIndicator = true;
			this.splitfilesbutton.UseUnderline = true;
			this.vbox2.Add (this.splitfilesbutton);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.splitfilesbutton]));
			w14.Position = 4;
			w14.Expand = false;
			w14.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.filebox = new global::Gtk.HBox ();
			this.filebox.Name = "filebox";
			this.filebox.Spacing = 6;
			// Container child filebox.Gtk.Box+BoxChild
			this.filenamelabel = new global::Gtk.Label ();
			this.filenamelabel.Name = "filenamelabel";
			this.filenamelabel.Xalign = 1F;
			this.filenamelabel.LabelProp = global::Mono.Unix.Catalog.GetString ("File name: ");
			this.filebox.Add (this.filenamelabel);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.filebox [this.filenamelabel]));
			w15.Position = 0;
			w15.Expand = false;
			// Container child filebox.Gtk.Box+BoxChild
			this.mediafilechooser1 = new global::VAS.UI.Component.MediaFileChooser ();
			this.mediafilechooser1.Events = ((global::Gdk.EventMask)(256));
			this.mediafilechooser1.Name = "mediafilechooser1";
			this.filebox.Add (this.mediafilechooser1);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.filebox [this.mediafilechooser1]));
			w16.Position = 1;
			this.vbox2.Add (this.filebox);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.filebox]));
			w17.Position = 5;
			w17.Expand = false;
			w17.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.dirbox = new global::Gtk.HBox ();
			this.dirbox.Name = "dirbox";
			this.dirbox.Spacing = 6;
			// Container child dirbox.Gtk.Box+BoxChild
			this.directorynamelabel1 = new global::Gtk.Label ();
			this.directorynamelabel1.Name = "directorynamelabel1";
			this.directorynamelabel1.Xalign = 1F;
			this.directorynamelabel1.LabelProp = global::Mono.Unix.Catalog.GetString ("Directory name: ");
			this.dirbox.Add (this.directorynamelabel1);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.dirbox [this.directorynamelabel1]));
			w18.Position = 0;
			w18.Expand = false;
			// Container child dirbox.Gtk.Box+BoxChild
			this.mediafilechooser2 = new global::VAS.UI.Component.MediaFileChooser ();
			this.mediafilechooser2.Events = ((global::Gdk.EventMask)(256));
			this.mediafilechooser2.Name = "mediafilechooser2";
			this.dirbox.Add (this.mediafilechooser2);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.dirbox [this.mediafilechooser2]));
			w19.Position = 1;
			this.vbox2.Add (this.dirbox);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.dirbox]));
			w20.Position = 6;
			w20.Expand = false;
			w20.Fill = false;
			w1.Add (this.vbox2);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(w1 [this.vbox2]));
			w21.Position = 0;
			w21.Expand = false;
			w21.Fill = false;
			// Internal child VAS.UI.Dialog.VideoEditionProperties.ActionArea
			global::Gtk.HButtonBox w22 = this.ActionArea;
			w22.Name = "dialog1_ActionArea";
			w22.Spacing = 6;
			w22.BorderWidth = ((uint)(5));
			w22.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w23 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w22 [this.buttonCancel]));
			w23.Expand = false;
			w23.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			w22.Add (this.buttonOk);
			global::Gtk.ButtonBox.ButtonBoxChild w24 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w22 [this.buttonOk]));
			w24.Position = 1;
			w24.Expand = false;
			w24.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 633;
			this.DefaultHeight = 379;
			this.dirbox.Hide ();
			this.Show ();
			this.splitfilesbutton.Clicked += new global::System.EventHandler (this.OnSplitfilesbuttonClicked);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}