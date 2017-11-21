//
//  Copyright (C) 2017 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using NUnit.Framework;
using VAS.Core.License;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Core.ViewModel.Statistics;
using VASInt32Converter = VAS.Core.Common.VASInt32Converter;

namespace VAS.Tests.MVVMC
{
	class DummyOneWayPropertyViewModel : ViewModelBase
	{
		string viewModelProperty;

		public string ViewModelProperty {
			get {
				return viewModelProperty;
			}

			set {
				viewModelProperty = value;
				RaisePropertyChanged (nameof (ViewModelProperty));
			}
		}
		int viewModelPropertyInt;

		public int ViewModelPropertyInt {
			get {
				return viewModelPropertyInt;
			}

			set {
				viewModelPropertyInt = value;
				RaisePropertyChanged (nameof (ViewModelPropertyInt));
			}
		}
	}

	class DummyOneWayPropertyView
	{
		public BindingContext BindingContext;
		string viewProperty;

		public string ViewProperty {
			get {
				return viewProperty;
			}
			set {
				viewProperty = value;
			}
		}

		public int ViewPropertyInt { get; set; }


		DummyOneWayPropertyViewModel viewModel;

		public DummyOneWayPropertyViewModel ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				if (viewModel != null)
					BindingContext.UpdateViewModel (viewModel);
			}
		}
	}

	[TestFixture]
	public class TestOneWayPropertyBinding
	{
		[Test ()]
		public void Bind_NewMVVM_PropertyBindedFromVMtoView ()
		{
			///Arrange

			var view = new DummyOneWayPropertyView ();
			var viewModel = new DummyOneWayPropertyViewModel ();
			view.BindingContext = new BindingContext ();
			view.BindingContext.Add (view.Bind (v => v.ViewProperty,
												vm => ((DummyOneWayPropertyViewModel)vm).ViewModelProperty));
			view.ViewModel = viewModel;

			///Act

			viewModel.ViewModelProperty = "Changed!";

			///Assert

			Assert.AreEqual ("Changed!", view.ViewProperty);
		}

		[Test]
		public void OneWayPropertyBinding_WriteSource_DestinationUpdated ()
		{
			// Arrange
			CountLimitationVM source = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "source",
					RegisterName = "source",
					Count = 1,
					Maximum = 5,
					Enabled = true
				}
			};
			CountLimitationVM destination = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "destination",
					RegisterName = "destination",
					Count = 0,
					Maximum = 0,
					Enabled = false
				}
			};

			var binding = new OneWayPropertyBinding<int, int> (destination,
												(vm) => ((CountLimitationVM)vm).Count,
												(vm) => ((CountLimitationVM)vm).Count);

			binding.ViewModel = source;

			// Act
			source.Count = 999;

			// Assert
			Assert.AreEqual ("destination", destination.DisplayName);
			Assert.AreEqual ("source", source.DisplayName);
			Assert.AreEqual (source.Count, destination.Count);
		}

		[Test]
		public void OneWayPropertyBinding_WriteDestination_SourceNotUpdated ()
		{
			// Arrange
			CountLimitationVM source = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "source",
					RegisterName = "source",
					Count = 1,
					Maximum = 5,
					Enabled = true
				}
			};
			CountLimitationVM destination = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "destination",
					RegisterName = "destination",
					Count = 0,
					Maximum = 0,
					Enabled = false
				}
			};

			var binding = new OneWayPropertyBinding<int, int> (destination,
												(vm) => ((CountLimitationVM)vm).Count,
												(vm) => ((CountLimitationVM)vm).Count);

			binding.ViewModel = source;

			// Act
			destination.Count = 999;

			// Assert
			Assert.AreEqual ("destination", destination.DisplayName);
			Assert.AreEqual ("source", source.DisplayName);
			Assert.AreNotEqual (destination.Count, source.Count);
		}

		[Test]
		public void OneWayPropertyBinding_DifferentPropertySameType_BindedCorrectly ()
		{
			// Arrange
			ChartVM source = new ChartVM {
				BottomPadding = 0,
				LeftPadding = 0,
				RightPadding = 0,
				TopPadding = 0,
			};
			ChartVM destination = new ChartVM {
				BottomPadding = 10,
				LeftPadding = 10,
				RightPadding = 10,
				TopPadding = 10,
			};

			var binding = new OneWayPropertyBinding<int, int> (destination,
															   (vm) => ((ChartVM)vm).RightPadding,
															   (vm) => ((ChartVM)vm).BottomPadding);

			binding.ViewModel = source;

			// Act
			source.RightPadding = 999;

			// Assert
			Assert.AreNotEqual (source.BottomPadding, destination.BottomPadding);
			Assert.AreEqual (source.RightPadding, destination.BottomPadding);
		}

		[Test]
		public void OneWayPropertyBinding_SetSource_DestinationUpdated ()
		{
			// Arrange
			CountLimitationVM source = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "source",
					RegisterName = "source",
					Count = 1,
					Maximum = 5,
					Enabled = true
				}
			};
			CountLimitationVM destination = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "destination",
					RegisterName = "destination",
					Count = 0,
					Maximum = 0,
					Enabled = false
				}
			};

			var binding = new OneWayPropertyBinding<int, int> (destination,
												(vm) => ((CountLimitationVM)vm).Count,
												(vm) => ((CountLimitationVM)vm).Count);

			// Act
			binding.ViewModel = source;

			// Assert
			Assert.AreEqual ("destination", destination.DisplayName);
			Assert.AreEqual ("source", source.DisplayName);
			Assert.AreEqual (source, binding.ViewModel);
			Assert.AreEqual (source.Count, destination.Count);
			Assert.AreEqual (1, destination.Count);
		}

		[Test]
		public void OneWayPropertyBindingWithConverter_SetSource_DestinationUpdated ()
		{
			///Arrange

			var view = new DummyOneWayPropertyView ();
			var viewModel = new DummyOneWayPropertyViewModel ();
			view.BindingContext = new BindingContext ();
			view.BindingContext.Add (view.Bind (v => v.ViewPropertyInt,
												vm => ((DummyOneWayPropertyViewModel)vm).ViewModelProperty,
												new VASInt32Converter ()));
			view.ViewModel = viewModel;

			///Act

			viewModel.ViewModelProperty = "12";

			///Assert

			Assert.AreEqual (12, view.ViewPropertyInt);
		}

		[Test]
		public void OneWayPropertyBindingWithConverterFrom_SetSource_DestinationUpdated ()
		{
			///Arrange

			var view = new DummyOneWayPropertyView ();
			var viewModel = new DummyOneWayPropertyViewModel ();
			view.BindingContext = new BindingContext ();
			view.BindingContext.Add (view.Bind (v => v.ViewProperty,
												vm => ((DummyOneWayPropertyViewModel)vm).ViewModelPropertyInt,
												new VASInt32Converter ()));
			view.ViewModel = viewModel;

			///Act

			viewModel.ViewModelPropertyInt = 12;

			///Assert

			Assert.AreEqual ("12", view.ViewProperty);
		}

		[Test]
		public void OneWayPropertyBinding_ChangedSource_BindedWithLast ()
		{
			// Arrange
			CountLimitationVM source = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "source",
					RegisterName = "source",
					Count = 1,
					Maximum = 5,
					Enabled = true
				}
			};
			CountLimitationVM source2 = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "source2",
					RegisterName = "source2",
					Count = 2,
					Maximum = 10,
					Enabled = true
				}
			};
			CountLimitationVM destination = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "destination",
					RegisterName = "destination",
					Count = 0,
					Maximum = 0,
					Enabled = false
				}
			};

			var binding = new OneWayPropertyBinding<int, int> (destination,
												(vm) => ((CountLimitationVM)vm).Count,
												(vm) => ((CountLimitationVM)vm).Count);

			// Act
			binding.ViewModel = source;
			binding.ViewModel = source2;
			source.Count = 777;
			source2.Count = 999;

			// Assert
			Assert.AreEqual ("destination", destination.DisplayName);
			Assert.AreEqual ("source", source.DisplayName);
			Assert.AreEqual ("source2", source2.DisplayName);
			Assert.AreEqual (source2, binding.ViewModel);
			Assert.AreEqual (source2.Count, destination.Count);
			Assert.AreNotEqual (source.Count, source2.Count);
		}

		[Test]
		public void OneWayPropertyBinding_RemovedSource_Unbinded ()
		{
			// Arrange
			CountLimitationVM source = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "source",
					RegisterName = "source",
					Count = 1,
					Maximum = 5,
					Enabled = true
				}
			};
			CountLimitationVM destination = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "destination",
					RegisterName = "destination",
					Count = 0,
					Maximum = 0,
					Enabled = false
				}
			};

			var binding = new OneWayPropertyBinding<int, int> (destination,
												(vm) => ((CountLimitationVM)vm).Count,
												(vm) => ((CountLimitationVM)vm).Count);

			// Act
			binding.ViewModel = source;
			binding.ViewModel = null;
			source.Count = 999;

			// Assert
			Assert.AreEqual ("destination", destination.DisplayName);
			Assert.AreEqual ("source", source.DisplayName);
			Assert.AreEqual (null, binding.ViewModel);
			Assert.AreNotEqual (source.Count, destination.Count);
			Assert.AreEqual (1, destination.Count);
		}

		[Test]
		public void OneWayPropertyBinding_RemovedSourceAfterTrigger_Unbinded ()
		{
			// Arrange
			CountLimitationVM source = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "source",
					RegisterName = "source",
					Count = 1,
					Maximum = 5,
					Enabled = true
				}
			};
			CountLimitationVM destination = new CountLimitationVM {
				Model = new CountLicenseLimitation {
					DisplayName = "destination",
					RegisterName = "destination",
					Count = 0,
					Maximum = 0,
					Enabled = false
				}
			};

			var binding = new OneWayPropertyBinding<int, int> (destination,
												(vm) => ((CountLimitationVM)vm).Count,
												(vm) => ((CountLimitationVM)vm).Count);

			// Act
			binding.ViewModel = source;
			source.Count = 999;
			binding.ViewModel = null;
			source.Count = 777;

			// Assert
			Assert.AreEqual ("destination", destination.DisplayName);
			Assert.AreEqual ("source", source.DisplayName);
			Assert.AreEqual (null, binding.ViewModel);
			Assert.AreNotEqual (source.Count, destination.Count);
			Assert.AreEqual (999, destination.Count);
		}
	}
}
