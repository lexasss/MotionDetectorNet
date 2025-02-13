﻿using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MotionDetectorNet.Video;

public static class CameraNameEnumerator
{
    public static string[] Get()
    {
        IMoniker[] moniker = new IMoniker[100];
        object? bagObj = null;
        List<string> result = [];

        try
        {
            // Get the system device enumerator
            var srvType = Type.GetTypeFromCLSID(SystemDeviceEnum);
            if (srvType == null)
                throw new Exception();

            // create device enumerator
            var comObj = Activator.CreateInstance(srvType);
            if (comObj == null)
                throw new Exception();

            ICreateDevEnum enumDev = (ICreateDevEnum)comObj;
            // Create an enumerator to find filters of specified category
            enumDev.CreateClassEnumerator(VideoInputDevice, out IEnumMoniker enumMon, 0);
            Guid bagId = typeof(IPropertyBag).GUID;
            while (enumMon.Next(1, moniker, nint.Zero) == 0)
            {
                // get property bag of the moniker
                moniker[0].BindToStorage(null, null, ref bagId, out bagObj);
                var bag = (IPropertyBag)bagObj;
                // read FriendlyName
                object val = "";
                bag.Read("FriendlyName", ref val, nint.Zero);
                //list in box
                result.Add((string)val);
            }

        }
        finally
        {
            if (bagObj != null)
            {
                Marshal.ReleaseComObject(bagObj);
            }
        }

        return result.ToArray();
    }

    // Internal

    internal static readonly Guid SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
    internal static readonly Guid VideoInputDevice = new Guid(0x860BB310, 0x5D01, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

    [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyBag
    {
        [PreserveSig]
        int Read(
            [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
            [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pVar,
            [In] nint pErrorLog);
        [PreserveSig]
        int Write(
            [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
            [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
    }

    [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICreateDevEnum
    {
        [PreserveSig]
        int CreateClassEnumerator([In] ref Guid type, [Out] out IEnumMoniker enumMoniker, [In] int flags);
    }
}
