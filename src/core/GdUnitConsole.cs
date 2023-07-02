
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GdUnit4.Core
{
    public sealed class GdUnitConsole
    {
        public const int BOLD = 0x1;
        public const int ITALIC = 0x2;
        public const int UNDERLINE = 0x4;
        private const String __CSI_BOLD = "\u001b[1m";
        private const String __CSI_ITALIC = "\u001b[3m";
        private const String __CSI_UNDERLINE = "\u001b[4m";

        static object lockObj = new object();

        internal Dictionary<string, (int Left, int Top)> SavedCursors = new Dictionary<string, (int, int)>();

        public GdUnitConsole NewLine()
        {
            Console.WriteLine();
            return this;
        }

        public GdUnitConsole PrintError(String message)
        {
            lock (lockObj)
            {
                return BeginColor(ConsoleColor.DarkRed)
                    .WriteLine(message)
                    .EndColor()
                    .NewLine();
            }
        }

        public GdUnitConsole Print(String message, ConsoleColor color = ConsoleColor.White, int flags = 0)
        {
            lock (lockObj)
            {
                return BeginColor(color)
                    .Bold((flags & BOLD) == BOLD)
                    .Italic((flags & ITALIC) == ITALIC)
                    .Underline((flags & UNDERLINE) == UNDERLINE)
                    .Write(message)
                    .EndColor();
            }
        }

        public GdUnitConsole Println(String message, ConsoleColor color = ConsoleColor.White, int flags = 0)
        {
            lock (lockObj)
            {
                return BeginColor(color)
                    .Bold((flags & BOLD) == BOLD)
                    .Italic((flags & ITALIC) == ITALIC)
                    .Underline((flags & UNDERLINE) == UNDERLINE)
                    .WriteLine(message)
                    .EndColor();
            }
        }

        private GdUnitConsole BeginColor(ConsoleColor color)
        {
            Color c = ToColor(color);
            Console.Write("\u001b[38;2;{0};{1};{2}m", c.R, c.G, c.B);
            return this;
        }

        private GdUnitConsole EndColor()
        {
            Console.Write("\u001b[0m");
            return this;
        }


        private GdUnitConsole Underline(bool enable)
        {
            if (enable)
                Console.Write(__CSI_UNDERLINE);
            return this;
        }

        private GdUnitConsole Bold(bool enable)
        {
            if (enable)
                Console.Write(__CSI_BOLD);
            return this;
        }

        private GdUnitConsole Italic(bool enable)
        {
            if (enable)
                Console.Write(__CSI_ITALIC);
            return this;
        }

        private static Color ToColor(ConsoleColor color)
        {
            string? colorName = Enum.GetName(typeof(ConsoleColor), color);
            return Color.FromName(colorName!);
        }

        private GdUnitConsole Write(string message)
        {
            Console.Write(message);
            return this;
        }

        private GdUnitConsole WriteLine(string message)
        {
            Console.WriteLine(message);
            return this;
        }

        internal void SaveCursor(string name) =>
            SavedCursors[name] = Console.GetCursorPosition();

        internal void RestoreCursor(string name)
        {
            if (SavedCursors.TryGetValue(name, out var position))
            {
                Console.SetCursorPosition(position.Left, position.Top);
                SavedCursors.Remove(name);
            }
        }
    }
}
