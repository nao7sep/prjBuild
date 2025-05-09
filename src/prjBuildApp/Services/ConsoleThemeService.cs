using System;
using System.Runtime.InteropServices;
using Serilog.Sinks.SystemConsole.Themes;

namespace prjBuildApp.Services
{
    public enum TerminalEnvironment
    {
        WindowsTerminal,
        MacTerminal,
        VisualStudioCode,
        Unknown
    }

    public enum ConsoleTheme
    {
        Dark,
        Light
    }

    public class ConsoleThemeService
    {
        private ConsoleTheme _currentTheme;
        private TerminalEnvironment _detectedEnvironment;

        public ConsoleThemeService(bool useLightTheme = false)
        {
            try
            {
                // Detect the terminal environment
                _detectedEnvironment = DetectTerminalEnvironment();

                // Determine the theme based on environment or override
                if (useLightTheme)
                {
                    _currentTheme = ConsoleTheme.Light;
                }
                else
                {
                    _currentTheme = DetermineThemeForEnvironment(_detectedEnvironment);
                }

                // Apply the theme
                ApplyTheme();
            }
            catch (Exception)
            {
                // If anything fails, fall back to dark theme
                _detectedEnvironment = TerminalEnvironment.Unknown;
                _currentTheme = ConsoleTheme.Dark;
                ApplyDarkTheme();
            }
        }

        public ConsoleTheme CurrentTheme => _currentTheme;
        public TerminalEnvironment DetectedEnvironment => _detectedEnvironment;

        private TerminalEnvironment DetectTerminalEnvironment()
        {
            try
            {
                // Check for VSCode integrated terminal
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM_PROGRAM")) &&
                    Environment.GetEnvironmentVariable("TERM_PROGRAM") == "vscode")
                {
                    return TerminalEnvironment.VisualStudioCode;
                }

                // Check for Windows Terminal
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION")))
                {
                    return TerminalEnvironment.WindowsTerminal;
                }

                // Check for macOS Terminal
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
                    !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM_PROGRAM")) &&
                    Environment.GetEnvironmentVariable("TERM_PROGRAM") == "Apple_Terminal")
                {
                    return TerminalEnvironment.MacTerminal;
                }
            }
            catch
            {
                // Ignore any exceptions during environment detection
            }

            // Default to unknown
            return TerminalEnvironment.Unknown;
        }

        private ConsoleTheme DetermineThemeForEnvironment(TerminalEnvironment environment)
        {
            return environment switch
            {
                TerminalEnvironment.WindowsTerminal => ConsoleTheme.Dark,
                TerminalEnvironment.VisualStudioCode => ConsoleTheme.Dark,
                TerminalEnvironment.MacTerminal => ConsoleTheme.Light,
                _ => ConsoleTheme.Dark // Default to dark theme for unknown environments
            };
        }

        public void ApplyTheme()
        {
            switch (_currentTheme)
            {
                case ConsoleTheme.Dark:
                    ApplyDarkTheme();
                    break;
                case ConsoleTheme.Light:
                    ApplyLightTheme();
                    break;
            }
        }

        private void ApplyDarkTheme()
        {
            try
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Clear();
            }
            catch
            {
                // Ignore any exceptions during theme application
            }
        }

        private void ApplyLightTheme()
        {
            try
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Clear();
            }
            catch
            {
                // Ignore any exceptions during theme application
            }
        }

        public SystemConsoleTheme GetSerilogTheme()
        {
            // Return the appropriate Serilog theme based on the console theme
            return _currentTheme == ConsoleTheme.Dark
                ? SystemConsoleTheme.Literate
                : SystemConsoleTheme.Grayscale;
        }
    }
}