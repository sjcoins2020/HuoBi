using System;
using System.Runtime.InteropServices;
using System.Text;

public class MyConsole : IDisposable
{
    private const uint STD_INPUT_HANDLE = 0xfffffff6;
    private const uint STD_OUTPUT_HANDLE = 0xfffffff5;
    private const uint STD_ERROR_HANDLE = 0xfffffff4;
    private const uint ATTACH_PARENT_PROCESS = 0xffffffff;
    [DllImport("kernel32.dll")]
    public static extern bool AttachConsole(uint dwProcessId);
    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();
    [DllImport("kernel32.dll")]
    public static extern bool FreeConsole();
    [DllImport("kernel32.dll")]
    public static extern int GetStdHandle(uint nStdHandle);
    [DllImport("kernel32.dll")]
    public static extern bool WriteConsole(int hConsoleOutput,
    string lpBuffer,
    int nNumberOfCharsToWrite,
    ref int
    lpNumberOfCharsWritten,
    int lpReserved);
    [DllImport("kernel32.dll")]
    public static extern bool ReadConsole(int hConsoleInput,
    StringBuilder lpBuffer,
    int nNumberOfCharsToRead,
    ref int lpNumberOfCharsRead,
    int lpReserved);
    private int stdin;
    private int stdout;
    public MyConsole()
    {
        AllocConsole();
        stdin = GetStdHandle(STD_INPUT_HANDLE);
        stdout = GetStdHandle(STD_OUTPUT_HANDLE);
    }
    public void WriteLine(string s)
    {
        int len = 0;
        WriteConsole(stdout, s + "\r\n", s.Length + 2, ref len, 0);
    }
    public string ReadLine()
    {
        int len = 0;
        StringBuilder sb = new StringBuilder();
        ReadConsole(stdin, sb, 256, ref len, 0);
        return sb.ToString(0, sb.Length - 2);
    }
    public void Dispose()
    {
        FreeConsole();
    }
}