using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.Util
{
    public class RawPrint
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        private static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "XiaoPiao";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        private static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            // Open the file.
            FileStream fs = new FileStream(szFileName, FileMode.Open);
            // Create a BinaryReader on the file.
            BinaryReader br = new BinaryReader(fs);
            // Dim an array of bytes big enough to hold the file's contents.
            Byte[] bytes = new Byte[fs.Length];
            bool bSuccess = false;
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            int nLength;

            nLength = Convert.ToInt32(fs.Length);
            // Read the contents of the file into the array.
            bytes = br.ReadBytes(nLength);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
            // Send the unmanaged bytes to the printer.
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }

        public static bool SendBytesToPrinter(string szPrinterName, byte[] buf)
        {
            bool bSuccess = false;
            // Your unmanaged pointer.
            IntPtr pUnmanagedBytes = new IntPtr(0);
            // Allocate some unmanaged memory for those bytes.
            pUnmanagedBytes = Marshal.AllocCoTaskMem(buf.Length);
            // Copy the managed byte array into the unmanaged array.
            Marshal.Copy(buf, 0, pUnmanagedBytes, buf.Length);
            // Send the unmanaged bytes to the printer.
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, buf.Length);
            // Free the unmanaged memory that you allocated earlier.
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }

        /// <summary>
        /// 切纸
        /// </summary>
        /// <param name="szPrinterName">打印机名</param>
        /// <returns></returns>
        public static bool Cut(string szPrinterName)
        {
            bool bSuccess = false;

            IntPtr pUnmanagedBytes = new IntPtr(0);

            byte[] data = new byte[] { 0x1B, 0x69 };
            pUnmanagedBytes = Marshal.AllocCoTaskMem(data.Length);
            Marshal.Copy(data, 0, pUnmanagedBytes, data.Length);
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, data.Length);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return true;
        }


        public static bool SendBytesToPrinterImg(string szPrinterName, Bitmap bmp, bool needWarning)
        {
            bool bSuccess = false;

            //Byte[] byte_send = Encoding.GetEncoding("GB2312").GetBytes("\x1b\x40");
            IntPtr pUnmanagedBytes = new IntPtr(0);
            //pUnmanagedBytes = Marshal.AllocCoTaskMem(byte_send.Length);
            //Marshal.Copy(byte_send, 0, pUnmanagedBytes, byte_send.Length);
            //bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, byte_send.Length);
            //Marshal.FreeCoTaskMem(pUnmanagedBytes);

            byte[] data = new byte[] { 0x1B, 0x33, 0x00 };
            //pUnmanagedBytes = new IntPtr(0);
            pUnmanagedBytes = Marshal.AllocCoTaskMem(data.Length);
            Marshal.Copy(data, 0, pUnmanagedBytes, data.Length);
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, data.Length);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            data[0] = (byte)'\x00';
            data[1] = (byte)'\x00';
            data[2] = (byte)'\x00';

            Color pixelColor;

            // ESC * m nL nH 点阵图  
            byte[] escBmp = new byte[] { 0x1B, 0x2A, 0x00, 0x00, 0x00 };
            escBmp[2] = (byte)'\x21';
            //nL, nH  
            escBmp[3] = (byte)(bmp.Width % 256);
            escBmp[4] = (byte)(bmp.Width / 256);

            // data  
            for (int i = 0; i < (bmp.Height / 24) + 1; i++)
            {
                //pUnmanagedBytes = new IntPtr(0);
                pUnmanagedBytes = Marshal.AllocCoTaskMem(escBmp.Length);
                Marshal.Copy(escBmp, 0, pUnmanagedBytes, escBmp.Length);
                bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, escBmp.Length);
                Marshal.FreeCoTaskMem(pUnmanagedBytes);

                byte[] temptype = new byte[bmp.Width * 3];
                int lengthNow = 0;
                for (int j = 0; j < bmp.Width; j++)
                {
                    for (int k = 0; k < 24; k++)
                    {
                        if (((i * 24) + k) < bmp.Height)   // if within the BMP size  
                        {
                            pixelColor = bmp.GetPixel(j, (i * 24) + k);
                            if (pixelColor.R == 0)
                            {
                                data[k / 8] += (byte)(128 >> (k % 8));
                            }
                        }
                    }
                    data.CopyTo(temptype, lengthNow);
                    lengthNow += 3;

                    data[0] = (byte)'\x00';
                    data[1] = (byte)'\x00';
                    data[2] = (byte)'\x00';
                }

                //pUnmanagedBytes = new IntPtr(0);
                pUnmanagedBytes = Marshal.AllocCoTaskMem(temptype.Length);
                Marshal.Copy(temptype, 0, pUnmanagedBytes, temptype.Length);
                bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, temptype.Length);
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
                System.Threading.Thread.Sleep(10);
            }
            byte[] dataClear = new byte[] { 0x1B, 0x40 };
            pUnmanagedBytes = Marshal.AllocCoTaskMem(dataClear.Length);
            Marshal.Copy(dataClear, 0, pUnmanagedBytes, dataClear.Length);
            bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, dataClear.Length);
            Marshal.FreeCoTaskMem(pUnmanagedBytes);
            return bSuccess;
        }

        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            try
            {
                //指令见打印机官方文档    http://www.xprinter.net/index.php/Server/index/cid/3
                byte[] smallArray = new byte[] { 29, 33, 0 };
                List<byte> list = new List<byte>();

                list.AddRange(smallArray);
                while (szString.Contains("<B>") || szString.Contains("<A>"))
                {
                    if (!szString.Contains("<B>"))
                    {
                        ReplaceAB(ref szString, ref list, 2);
                    }
                    else if (!szString.Contains("<A>"))
                    {
                        ReplaceAB(ref szString, ref list, 3);
                    }
                    else
                    {
                        int indexA = szString.IndexOf("<A>");
                        int indexB = szString.IndexOf("<B>");
                        if (indexA < indexB)
                        {
                            ReplaceAB(ref szString, ref list, 2);
                        }
                        else
                        {
                            ReplaceAB(ref szString, ref list, 3);
                        }
                    }
                }
                Encoding enc = Encoding.GetEncoding("gb2312");
                list.AddRange(enc.GetBytes(szString));
                return RawPrint.SendBytesToPrinter(szPrinterName, list.ToArray());
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void ReplaceAB(ref string szString, ref List<byte> list, int mul)
        {
            //指令见打印机官方文档    http://www.xprinter.net/index.php/Server/index/cid/3
            string replaceStr1 = "<B>";
            string replaceStr2 = "</B>";
            byte[] smallArray = new byte[] { 29, 33, 0 };
            byte[] bigArray = new byte[] { 29, 33, 34 };   //放大三倍    //29, 33字体放大指令  34放大倍数 ( 0一倍    17两倍   34三倍     51四倍    68五倍     85六倍)
            if (mul == 2)
            {
                replaceStr1 = "<A>";
                replaceStr2 = "</A>";
                bigArray = new byte[] { 29, 33, 17 };   //放大两倍
            }
            Encoding enc = Encoding.GetEncoding("gb2312");
            int index = szString.IndexOf(replaceStr1);
            string first = szString.Substring(0, index);
            list.AddRange(enc.GetBytes(first));//第一段
            list.AddRange(bigArray);//变成大写
            szString = szString.Substring(index + 3);
            int index2 = szString.IndexOf(replaceStr2);
            string second = szString.Substring(0, index2);
            list.AddRange(enc.GetBytes(second));//大写段
            list.AddRange(smallArray);//变小写
            szString = szString.Substring(index2 + 4);
        }
    }
}
