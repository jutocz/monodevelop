//
// PickMembersDialog.cs
//
// Author:
//       Marius Ungureanu <maungu@microsoft.com>
//
// Copyright (c) 2018 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PickMembers;
using MonoDevelop.Components.AtkCocoaHelper;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Xwt;
using Xwt.Drawing;

namespace MonoDevelop.Refactoring.PickMembers
{
	public class PickMembersDialog : Xwt.Dialog
	{
		DataField<bool> symbolIncludedField = new DataField<bool> ();
		DataField<string> symbolTextField = new DataField<string> ();
		DataField<Image> symbolIconField = new DataField<Image> ();
		DataField<ISymbol> symbolField = new DataField<ISymbol> ();

		ListStore treeStore;

		public IEnumerable<ISymbol> IncludedMembers {
			get {
				for (int i = 0; i < treeStore.RowCount; i++) {
					if (treeStore.GetValue (i, symbolIncludedField))
						yield return treeStore.GetValue (i, symbolField);
				}
			}
		}

		ListView listViewPublicMembers = new ListView ();

		public PickMembersDialog (string title)
		{
			this.Build (title);
			this.buttonSelectAll.Clicked += delegate {
				for (int i = 0; i < treeStore.RowCount; i++) {
					treeStore.SetValue (i, symbolIncludedField, true);
				}
				UpdateOkButton ();
			};

			this.buttonDeselectAll.Clicked += delegate {
				for (int i = 0; i < treeStore.RowCount; i++) {
					treeStore.SetValue (i, symbolIncludedField, false);
				}
				UpdateOkButton ();
			};

			listViewPublicMembers.HeadersVisible = false;
			listViewPublicMembers.DataSource = treeStore;
			var checkBoxCellView = new CheckBoxCellView (symbolIncludedField);
			checkBoxCellView.Editable = true;
			checkBoxCellView.Toggled += delegate { UpdateOkButton (); };
			listViewPublicMembers.Columns.Add ("", checkBoxCellView);
			listViewPublicMembers.Columns.Add ("", new ImageCellView (symbolIconField), new TextCellView (symbolTextField));
		}

		void Build (string title)
		{
			this.TransientFor = MessageDialog.RootWindow;
			this.Title = GettextCatalog.GetString ("Pick members");

			treeStore = new ListStore (symbolIncludedField, symbolField, symbolTextField, symbolIconField);
			var box = new VBox {
				Margin = 6,
				Spacing = 6
			};

			box.PackStart (new Label {
				Markup = "<b>" + title + "</b>"
			});

			var hbox = new HBox {
				Spacing = 6
			};
			hbox.PackStart (listViewPublicMembers, true);
			listViewPublicMembers.Accessible.Description = title;

			var vbox = new VBox {
				Spacing = 6
			};
			buttonSelectAll = new Button (GettextCatalog.GetString ("Select All"));
			buttonSelectAll.Clicked += delegate {
				UpdateOkButton ();
			};
			vbox.PackStart (buttonSelectAll);

			buttonDeselectAll = new Button (GettextCatalog.GetString ("Clear"));
			buttonDeselectAll.Clicked += delegate {
				UpdateOkButton ();
			};
			vbox.PackStart (buttonDeselectAll);

			hbox.PackStart (vbox);

			box.PackStart (hbox, true);

			Content = box;
			Buttons.Add (okButton = new DialogButton (Command.Ok));
			Buttons.Add (new DialogButton (Command.Cancel));

			this.Width = 400;
			this.Height = 421;
			this.Resizable = false;

			Show ();
		}

		static SymbolDisplayFormat memberDisplayFormat = new SymbolDisplayFormat (
			genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
			memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
			parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeOptionalBrackets,
			miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
		private Button buttonSelectAll;
		private Button buttonDeselectAll;
		private DialogButton okButton;

		ImmutableArray<PickMembersOption> options;
		internal void Init (ImmutableArray<ISymbol> members, ImmutableArray<PickMembersOption> options)
		{
			// options is unused, not even roslyn uses it
			this.options = options;

			treeStore.Clear ();
			foreach (var member in members) {
				var row = treeStore.AddRow ();
				treeStore.SetValue (row, symbolIncludedField, true);
				treeStore.SetValue (row, symbolField, member);
				treeStore.SetValue (row, symbolTextField, member.ToDisplayString (memberDisplayFormat));
				treeStore.SetValue (row, symbolIconField, ImageService.GetIcon (MonoDevelop.Ide.TypeSystem.Stock.GetStockIcon (member)));
			}
		}

		void UpdateOkButton ()
		{
			okButton.Sensitive = TrySubmit ();
		}

		bool TrySubmit ()
		{
			if (!IncludedMembers.Any ()) {
				return false;
			}
			return true;
		}
	}
}
