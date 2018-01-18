//
// PickMembersService.cs
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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.PickMembers;
using MonoDevelop.Core;

namespace MonoDevelop.Refactoring.PickMembers
{
	//[ExportWorkspaceService(typeof(IPickMembersService), ServiceLayer.Host), System.Composition.Shared]
	class PickMembersService : IPickMembersService
	{
		public PickMembersResult PickMembers (string title, ImmutableArray<ISymbol> members, ImmutableArray<PickMembersOption> options = default (ImmutableArray<PickMembersOption>))
		{
			var dialog = new PickMembersDialog (title);
			try {
				dialog.Init (members, options);
				bool performChange = dialog.Run () == Xwt.Command.Ok;
				if (!performChange)
					return PickMembersResult.Canceled;

				return new PickMembersResult (dialog.IncludedMembers.ToImmutableArray (), options);
			} catch (Exception ex) {
				LoggingService.LogError ("Error while signature changing.", ex);
				return PickMembersResult.Canceled;
			} finally {
				dialog.Dispose ();
			}
		}
	}
}
