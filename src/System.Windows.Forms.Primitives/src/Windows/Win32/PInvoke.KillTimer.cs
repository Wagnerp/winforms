﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Windows.Win32;

internal static partial class PInvoke
{
    public static BOOL KillTimer<T>(T hWnd, IntPtr uIDEvent)
        where T : IHandle<HWND>
    {
        BOOL result = KillTimer(hWnd.Handle, uIDEvent);
        GC.KeepAlive(hWnd.Wrapper);
        return result;
    }
}
