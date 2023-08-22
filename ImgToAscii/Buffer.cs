using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Buffer
{
    internal class ConsoleBuffer
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
           string fileName,
           [MarshalAs(UnmanagedType.U4)] uint fileAccess,
           [MarshalAs(UnmanagedType.U4)] uint fileShare,
           IntPtr securityAttributes,
           [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
           [MarshalAs(UnmanagedType.U4)] int flags,
           IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutputW(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref Rectangle lpWriteRegion);

        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };
        public struct CharInfo
        {
            public char Char;
            public short Color;
        }
        public struct Rectangle
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }
        public short Width { get { return width; } }
        public short Height { get { return height; } }

        SafeFileHandle fileHandle;
        CharInfo[] memo1, memo2, curBuff;
        Rectangle writeArea;
        short width, height;
        public ConsoleBuffer(short width, short height)
        {
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);
            fileHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            if (fileHandle.IsInvalid)
            {
                throw new Exception("fileHandle create error");
            }
            writeArea = new Rectangle() { Left = 0, Top = 0, Right = width, Bottom = height };
            memo1 = new CharInfo[width * height];
            memo2 = new CharInfo[width * height];
            curBuff = memo1;
            this.width = width;
            this.height = height;
        }

        public void WriteToBuff(int x, int y, char symbol, short color)
        {
            curBuff[x + y * width].Char = symbol;
            curBuff[x + y * width].Color = color;
        }
        public void Swap()
        {
            WriteConsoleOutputW(fileHandle, curBuff,
                          new Coord() { X = width, Y = height },
                          new Coord() { X = 0, Y = 0 },
                          ref writeArea);
            curBuff = curBuff == memo1 ? memo2 : memo1;
        }
        public void Clear()
        {
            for (int i = 0; i < memo1.Length; ++i)
            {
                curBuff[i].Color = 0; //char color 0-15, char color+background color 0-256
                curBuff[i].Char = ' ';
            }
        }
    }
}
