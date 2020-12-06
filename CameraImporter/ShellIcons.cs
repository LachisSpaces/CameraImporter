using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.ComponentModel;
using System.Windows.Media;
using System.Drawing;
using System.Windows;
using System;

namespace CameraImporter
{
   static class ShellIcons
   {
      [DllImport("shell32.dll", EntryPoint = "ExtractIcon")]
      extern static IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

      const string ShellIconsLib = @"C:\WINDOWS\System32\shell32.dll";

      //static public Icon GetIcon(int index)
      //{
      //   IntPtr Hicon = ExtractIcon(IntPtr.Zero, ShellIconsLib, index);
      //   Icon icon = Icon.FromHandle(Hicon);
      //   return icon;
      //}

      static public ImageSource GetIcon(int index)
      {
         IntPtr Hicon = ExtractIcon(IntPtr.Zero, ShellIconsLib, index);
         Icon icon = Icon.FromHandle(Hicon);
         return ToImageSource(icon);
      }

      private static ImageSource ToImageSource(Icon icon)
      {
         return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      }
   }
}
