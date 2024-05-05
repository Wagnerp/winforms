﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms.BinaryFormat;

/// <summary>
///  Record that marks the end of the binary format stream.
/// </summary>
internal sealed class MessageEnd : IRecord<MessageEnd>, IBinaryFormatParseable<MessageEnd>
{
    public static MessageEnd Instance { get; } = new();

    private MessageEnd() { }

    public static RecordType RecordType => RecordType.MessageEnd;

    static MessageEnd IBinaryFormatParseable<MessageEnd>.Parse(
        BinaryFormattedObject.IParseState state) => Instance;

    public void Write(BinaryWriter writer) => writer.Write((byte)RecordType);
}
