//
// AddFileHandlesHandler.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2020 Microsoft Corporation
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace TestManyFileHandlesAddin
{
	class AddFileHandlesHandler : CommandHandler
	{
		protected override void Run ()
		{
			Task.Run (async () => await CreateFileHandles ()).Ignore ();
		}

		public static List<Stream> FileStreams = new List<Stream> ();

		async Task CreateFileHandles ()
		{
			string fileName = typeof (AddFileHandlesHandler).Assembly.Location;

			while (true) {
				if (FileStreams.Count == 0 || (FileStreams.Count % 10) == 0) {
					bool result = await TestTcpListener ();
					if (!result) {
						return;
					}
				}

				var stream = File.Open (fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				FileStreams.Add(stream);
			}
		}

		async Task<bool> TestTcpListener ()
		{
			try {
				var listener = new TcpListener (IPAddress.Loopback, 0);
				listener.Start ();

				listener.BeginAcceptTcpClient (OnConnected, listener);

				listener.Stop ();

				return true;
			} catch (Exception ex) {
				await Runtime.RunInMainThread (() => {
					var builder = new StringBuilder ();
					builder.Append ("TcpListener creation failed. ");
					builder.AppendLine (ex.Message);
					var innerEx = ex.InnerException;
					if (innerEx != null)
						builder.AppendLine (innerEx.Message);
					builder.AppendFormat ("FileStreams created: {0}", FileStreams.Count);
					builder.AppendLine ();
					MessageService.ShowMessage (builder.ToString ());
				});
				return false;
			}
		}

		static void OnConnected (IAsyncResult ar)
		{
		}
	}
}
