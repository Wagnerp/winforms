﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Drawing.Printing.Tests;

public class PrintDocumentTests : FileCleanupTestBase
{
    private readonly PageSettings _pageSettings = new PageSettings()
    {
        PaperSize = new PaperSize()
        {
            RawKind = (int)PaperKind.A3
        }
    };

    [ConditionalFact(Helpers.WindowsRS3OrEarlier)] // RS4 failures: https://github.com/dotnet/winforms/issues/8816
    public void Ctor_Default_Success()
    {
        using (var document = new PrintDocument())
        {
            Assert.Equal("document", document.DocumentName);
            Assert.False(document.OriginAtMargins);
            AssertDefaultPageSettings(document.DefaultPageSettings);
        }
    }

    [ConditionalFact(Helpers.WindowsRS3OrEarlier)] // RS4 failures: https://github.com/dotnet/winforms/issues/8816
    public void DefaultPageSettings_SetValue_ReturnsExpected()
    {
        using (var document = new PrintDocument())
        {
            document.DefaultPageSettings = null;
            Assert.IsAssignableFrom<PageSettings>(document.DefaultPageSettings);

            document.DefaultPageSettings = _pageSettings;
            Assert.Equal(_pageSettings.PaperSize.Kind, _pageSettings.PaperSize.Kind);
        }
    }

    [Fact]
    [ActiveIssue("https://github.com/dotnet/winforms/issues/8812")]
    public void DefaultPageSettings_Null_ReturnsExpected()
    {
        using (var document = new PrintDocument())
        {
            document.DefaultPageSettings = null;
            AssertDefaultPageSettings(document.DefaultPageSettings);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("newDocument")]
    public void DocumentName_SetValue_ReturnsExpected(string documentName)
    {
        using (var document = new PrintDocument())
        {
            document.DocumentName = documentName;
            Assert.Equal(documentName, document.DocumentName);
        }
    }

    [Fact]
    public void DocumentName_Null_ReturnsExpected()
    {
        using (var document = new PrintDocument())
        {
            document.DocumentName = null;
            Assert.Equal(string.Empty, document.DocumentName);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OriginAtMargins_SetValue_ReturnsExpected(bool originAtMargins)
    {
        using (var document = new PrintDocument())
        {
            document.OriginAtMargins = originAtMargins;
            Assert.Equal(originAtMargins, document.OriginAtMargins);
        }
    }

    [Fact]
    public void PrintController_SetValue_ReturnsExpected()
    {
        using (var document = new PrintDocument())
        {
            document.PrintController = null;
            Assert.NotNull(document.PrintController);

            var printController = new StandardPrintController();
            document.PrintController = printController;
            Assert.Same(printController, document.PrintController);
        }
    }

    [ConditionalFact(Helpers.AnyInstalledPrinters, Helpers.WindowsRS3OrEarlier)] // RS4 failures: https://github.com/dotnet/winforms/issues/8816
    public void PrinterSettings_SetValue_ReturnsExpected()
    {
        using (var document = new PrintDocument())
        {
            document.PrinterSettings = null;
            Assert.IsAssignableFrom<PrinterSettings>(document.PrinterSettings);

            var printerSettings = new PrinterSettings();
            document.PrinterSettings = printerSettings;
            Assert.Same(printerSettings, document.PrinterSettings);
            Assert.Equal(
                document.PrinterSettings.DefaultPageSettings.PaperSize.Kind,
                document.DefaultPageSettings.PaperSize.Kind);

            document.DefaultPageSettings = _pageSettings;
            document.PrinterSettings = printerSettings;
            Assert.Equal(
                _pageSettings.PaperSize.Kind,
                document.DefaultPageSettings.PaperSize.Kind);
        }
    }

    [ConditionalFact(Helpers.AnyInstalledPrinters, Helpers.WindowsRS3OrEarlier)] // RS4 failures: https://github.com/dotnet/winforms/issues/8816
    public void BeginPrint_SetValue_ReturnsExpected()
    {
        bool flag = false;
        var beginPrintHandler = new PrintEventHandler((sender, e) => flag = true);

        using (var document = new PrintDocument())
        {
            document.PrintController = new TestPrintController();
            document.BeginPrint += beginPrintHandler;
            document.Print();
            Assert.True(flag);

            flag = false;
            document.BeginPrint -= beginPrintHandler;
            document.Print();
            Assert.False(flag);
        }
    }

    [ActiveIssue("https://github.com/dotnet/winforms/issues/8815")]
    [ConditionalFact(Helpers.AnyInstalledPrinters)]
    public void EndPrint_SetValue_ReturnsExpected()
    {
        bool flag = false;
        var endPrintHandler = new PrintEventHandler((sender, e) => flag = true);

        using (var document = new PrintDocument())
        {
            document.PrintController = new TestPrintController();
            document.EndPrint += endPrintHandler;
            document.Print();
            Assert.True(flag);

            flag = false;
            document.EndPrint -= endPrintHandler;
            document.Print();
            Assert.False(flag);
        }
    }

    [ConditionalFact(nameof(CanPrintToPdf))]
    public void Print_DefaultPrintController_Success()
    {
        bool endPrintCalled = false;
        var endPrintHandler = new PrintEventHandler((sender, e) => endPrintCalled = true);
        using (var document = new PrintDocument())
        {
            document.PrinterSettings.PrinterName = PrintToPdfPrinterName;
            document.PrinterSettings.PrintFileName = GetTestFilePath();
            document.PrinterSettings.PrintToFile = true;
            document.EndPrint += endPrintHandler;
            document.Print();
            document.EndPrint -= endPrintHandler;
        }

        // File may not have finished saving to disk when Print returns,
        // so we check for EndPrint being called instead of file existence.
        Assert.True(endPrintCalled);
    }

    [ActiveIssue("https://github.com/dotnet/winforms/issues/8815")]
    [ConditionalFact(Helpers.AnyInstalledPrinters)]
    public void PrintPage_SetValue_ReturnsExpected()
    {
        bool flag = false;
        var printPageHandler = new PrintPageEventHandler((sender, e) => flag = true);

        using (var document = new PrintDocument())
        {
            document.PrintController = new TestPrintController();
            document.PrintPage += printPageHandler;
            document.Print();
            Assert.True(flag);

            flag = false;
            document.PrintPage -= printPageHandler;
            document.Print();
            Assert.False(flag);
        }
    }

    [ConditionalFact(Helpers.AnyInstalledPrinters, Helpers.WindowsRS3OrEarlier)] // RS4 failures: https://github.com/dotnet/winforms/issues/8816
    public void QueryPageSettings_SetValue_ReturnsExpected()
    {
        bool flag = false;
        var queryPageSettingsHandler = new QueryPageSettingsEventHandler((sender, e) => flag = true);

        using (var document = new PrintDocument())
        {
            document.PrintController = new TestPrintController();
            document.QueryPageSettings += queryPageSettingsHandler;
            document.Print();
            Assert.True(flag);

            flag = false;
            document.QueryPageSettings -= queryPageSettingsHandler;
            document.Print();
            Assert.False(flag);
        }
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        using (var document = new PrintDocument())
        {
            var expected = string.Format("[PrintDocument {0}]", document.DocumentName);
            Assert.Equal(expected, document.ToString());
        }
    }

    private void AssertDefaultPageSettings(PageSettings pageSettings)
    {
        // A4 and Letter are both common default sizes for systems to have.
        switch (pageSettings.PaperSize.Kind)
        {
            case PaperKind.A4:
                Assert.Equal(new Rectangle(0, 0, 827, 1169), pageSettings.Bounds);
                break;

            case PaperKind.Letter:
                Assert.Equal(new Rectangle(0, 0, 850, 1100), pageSettings.Bounds);
                break;
        }

        Assert.True(Enum.IsDefined(typeof(PrinterResolutionKind), pageSettings.PrinterResolution.Kind));
        Assert.True(pageSettings.PrinterSettings.IsDefaultPrinter);
    }

    private const string PrintToPdfPrinterName = "Microsoft Print to PDF";
    private static bool CanPrintToPdf()
    {
        if (!PlatformDetection.IsWindows || !PlatformDetection.IsDrawingSupported)
            return false;

        foreach (string name in PrinterSettings.InstalledPrinters)
        {
            if (name == PrintToPdfPrinterName)
            {
                return true;
            }
        }

        return false;
    }

    private class TestPrintController : PrintController
    {
        public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
        {
            using (var bitmap = new Bitmap(20, 20))
            {
                return Graphics.FromImage(bitmap);
            }
        }

        public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
        {
            base.OnStartPrint(document, e);
        }

        public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
        {
            base.OnEndPrint(document, e);
        }

        public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
        {
            base.OnEndPage(document, e);
            e.Graphics.Dispose();
        }
    }
}
