#if NET9_0_OR_GREATER
#    define SUPPORT_GENERATED_REGEX_PROPERTY
#endif  // NET9_0_OR_GREATER
#if NET7_0_OR_GREATER
#    define SUPPORT_GENERATED_REGEX
#endif  // NET7_0_OR_GREATER
#if NET7_0_OR_GREATER
#    define SUPPORT_LIBRARY_IMPORT
#endif  // NET7_0_OR_GREATER

#define USE_WIN32ICON_AS_FAVICON
// #define USE_EMBEDDED_ICON_AS_FAVICON
// #define USE_SYSTEM_WEB_MIME_MAPPING
#define USE_ASMGUID_FOR_CHROME_DEVTOOL_JSON

using System;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif  // NET8_0_OR_GREATER
using System.Collections.Generic;
#if USE_WIN32ICON_AS_FAVICON
using System.ComponentModel;
#endif  // USE_WIN32ICON_AS_FAVICON
using System.Diagnostics;
#if NETFRAMEWORK || WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
#endif  // NETFRAMEWORK || WINDOWS
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
#if USE_ASMGUID_FOR_CHROME_DEVTOOL_JSON
using System.Reflection;
#endif  // USE_ASMGUID_FOR_CHROME_DEVTOOL_JSON
using System.Runtime.InteropServices;
#if USE_WIN32ICON_AS_FAVICON
using System.Security;
#endif  // USE_WIN32ICON_AS_FAVICON
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#if USE_SYSTEM_WEB_MIME_MAPPING
using System.Web;
#endif  //USE_SYSTEM_WEB_MIME_MAPPING


namespace SimpleHttpServer
{
    /// <summary>
    /// The entry point class.
    /// </summary>
    /// <remarks>
    /// <para>This program can be compile with default C# compiler:</para>
    /// <para><c>C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc /nologo /w:4 /o /out:SimpleHttpServer.exe Program.cs</c></para>
    /// </remarks>
    public static class Program
    {
        /// <summary>
        /// Default local root path.
        /// </summary>
        private const string DefaultLocalRootPath = ".";
        /// <summary>
        /// Default host name for IPv4.
        /// </summary>
        private const string DefaultHostIpv4 = "127.0.0.1";
        /// <summary>
        /// Default host name for IPv6.
        /// </summary>
        private const string DefaultHostIpv6 = "[::1]";
        /// <summary>
        /// Default port number.
        /// </summary>
        private const int DefaultPort = 8000;
#if USE_EMBEDDED_ICON_AS_FAVICON
        /// <summary>
        /// Resource path to favicon.ico.
        /// </summary>
        private const string FaviconResourcePath = "SimpleHttpServer.favicon.ico";
#endif  // USE_EMBEDDED_ICON_AS_FAVICON
        /// <summary>
        /// Format string for <see cref="DateTime"/>.
        /// </summary>
        private const string DateTimeFormat = "dd/MMM/yyyy:HH:mm:ss K";
        /// <summary>
        /// <see cref="ConsoleColor"/> for remote end point address.
        /// </summary>
        private const ConsoleColor RemoteEndPointAddressColor = ConsoleColor.Blue;
        /// <summary>
        /// <see cref="ConsoleColor"/> for non important part.
        /// </summary>
        private const ConsoleColor NonImportantPartColor = ConsoleColor.DarkGray;
        /// <summary>
        /// <see cref="ConsoleColor"/> for timestamp.
        /// </summary>
        private const ConsoleColor TimestampColor = ConsoleColor.Green;
        /// <summary>
        /// <see cref="ConsoleColor"/> for response header.
        /// </summary>
        private const ConsoleColor ResponseHeaderColor = ConsoleColor.Yellow;
        /// <summary>
        /// <see cref="ConsoleColor"/> for HTTP status code that is OK (2XX).
        /// </summary>
        private const ConsoleColor HttpStatusOkColor = ConsoleColor.DarkGreen;
        /// <summary>
        /// <see cref="ConsoleColor"/> for HTTP status code that is NG (4XX or 5XX).
        /// </summary>
        private const ConsoleColor HttpStatusNgColor = ConsoleColor.Red;
        /// <summary>
        /// <see cref="ConsoleColor"/> for HTTP status code that is other (1XX or 3XX).
        /// </summary>
        private const ConsoleColor HttpStatusOtherColor = ConsoleColor.DarkMagenta;
        /// <summary>
        /// <see cref="ConsoleColor"/> for content length.
        /// </summary>
        private const ConsoleColor ContentLengthColor = ConsoleColor.Cyan;
        /// <summary>
        /// <see cref="ConsoleColor"/> for URL referer.
        /// </summary>
        private const ConsoleColor UrlRefererColor = ConsoleColor.DarkCyan;
        /// <summary>
        /// <see cref="ConsoleColor"/> for User Agent.
        /// </summary>
        private const ConsoleColor UserAgentColor = ConsoleColor.DarkYellow;

#if !NETFRAMEWORK && !WINDOWS
        /// <summary>
        /// A flag whether current running platform is Windows or not.
        /// </summary>
        private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif  // !NETFRAMEWORK && !WINDOWS
        /// <summary>
        /// Directory separator.
        /// </summary>
        private static readonly string _dirSep = Path.DirectorySeparatorChar.ToString();
        /// <summary>
        /// Directory separator - ".." - Directory separator.
        /// </summary>
        private static readonly string _parentMid = _dirSep + ".." + _dirSep;
        /// <summary>
        /// Directory separator and "..".
        /// </summary>
        private static readonly string _parentLast = _dirSep + "..";
        /// <summary>
        /// Lock object for console.
        /// </summary>
#if NET9_0_OR_GREATER
        private static readonly Lock _consoleLock = new Lock();
#else
        private static readonly object _consoleLock = new object();
#endif  // NET9_0_OR_GREATER
        /// <summary>
        /// Cached embedded icon data.
        /// </summary>
        private static byte[] _iconData = null;
#if (USE_WIN32ICON_AS_FAVICON || USE_EMBEDDED_ICON_AS_FAVICON) && (NETFRAMEWORK || WINDOWS)
        /// <summary>
        /// Cache dictionary of apple-touch-icon data.
        /// </summary>
        private static readonly Dictionary<int, byte[]> _appleTouchIconDataCacheDict = new Dictionary<int, byte[]>();
#endif  // WINDOWS
        /// <summary>
        /// Cached <see cref="Guid"/>.
        /// </summary>
        private static Guid _guid;

        /// <summary>
        /// The entry point of this program.
        /// </summary>
        /// <param name="args">Comamnd line arguments.</param>
        /// <returns>Status code.</returns>
        public static int Main(string[] args)
        {
            SetupCulture();

            HttpListener listener = null;
            try
            {
                var appOptions = ParseArguments(args);

                listener = new HttpListener();

                int index = 0;
                foreach (var prefix in appOptions.PrefixList)
                {
                    Console.ForegroundColor = TimestampColor;
                    Console.Error.Write("[{0}]", GetCurrentTimestamp());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Error.WriteLine(" prefix[{0}]: {1}", index, prefix);
                    listener.Prefixes.Add(prefix);
                    index++;
                }

                Console.ForegroundColor = TimestampColor;
                Console.Error.Write("[{0}]", GetCurrentTimestamp());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Error.WriteLine(" Path binding: {0} -> {1}", appOptions.PrefixRoot.Length == 0 ? "/" : appOptions.PrefixRoot, appOptions.LocalRootPath);

                Console.ForegroundColor = TimestampColor;
                Console.Error.Write("[{0}]", GetCurrentTimestamp());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Error.WriteLine(" Start listening ...");

                listener.Start();

                Console.ForegroundColor = TimestampColor;
                Console.Error.Write("[{0}]", GetCurrentTimestamp());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Error.WriteLine(" Enter \"exit\", \"quit\" or EOF (Ctrl-Z) to terminate this program.");
                Console.ResetColor();

                var thread = StartListenThread(listener, appOptions);

                if (appOptions.ShouldLaunchWebBrowser)
                {
                    Thread.Sleep(100);
                    if (appOptions.PrefixList.Count > 0)
                    {
                        OpenWithDefaultWebBrowser(appOptions.PrefixList[0].Replace("+", DefaultHostIpv4).Replace("*", DefaultHostIpv4));
                    }
                }

                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line == "exit" || line == "quit")
                    {
                        break;
                    }
                }

                listener.Stop();
                thread.Join(3000);

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Error.WriteLine(ex);
                Console.ResetColor();

                return 1;
            }
            finally
            {
                if (listener != null)
                {
                    listener.Close();
                }
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Setup culture.
        /// </summary>
        private static void SetupCulture()
        {
            var culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        /// <summary>
        /// Parse command-line arguments.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns><see cref="AppOptions"/> instance.</returns>
        private static AppOptions ParseArguments(string[] args)
        {
            var hostPartList = new List<string>();
            var port = DefaultPort;
            var prefixRoot = "";
            var localRootPath = DefaultLocalRootPath;
            var treatPrefixRootAsLocalRoot = false;
            var useGzip = false;
            var shouldLaunchWebBrowser = false;
            var isGenerateHtml5IndexPage = true;
            var logFormatType = LogFormatType.Combined;

            var fallbackToFreePort = true;
            var treatNonOptionArg = false;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg[0] != '-' || treatNonOptionArg)
                {
                    port = int.Parse(arg);
                }
                else
                {
                    switch (arg)
                    {
                        case "-g":
                            hostPartList.Clear();
                            hostPartList.Add("+");
                            break;
                        case "-h":
                            ShowUsage();
                            Environment.Exit(0);
                            break;
                        case "-H":
                            if (i + 1 >= args.Length)
                            {
                                throw new InvalidOperationException(string.Format("Argument required for option \"{0}\"", arg));
                            }
                            i++;
                            if (hostPartList.Count == 0 || hostPartList[0] != "+")
                            {
                                hostPartList.Add(args[i]);
                            }
                            break;
                        case "-l":
                            if (i + 1 >= args.Length)
                            {
                                throw new InvalidOperationException(string.Format("Argument required for option \"{0}\"", arg));
                            }
                            i++;
                            localRootPath = args[i];
                            if (!Directory.Exists(localRootPath))
                            {
                                throw new DirectoryNotFoundException(localRootPath);
                            }
                            break;
                        case "-r":
                            if (i + 1 >= args.Length)
                            {
                                throw new InvalidOperationException(string.Format("Argument required for option \"{0}\"", arg));
                            }
                            i++;
                            prefixRoot = args[i];
                            break;
                        case "-R":
                            treatPrefixRootAsLocalRoot = true;
                            break;
                        case "-t":
                            hostPartList.Clear();
                            hostPartList.Add("+");
                            port = 80;
                            prefixRoot = "/Temporary_Listen_Addresses";
                            fallbackToFreePort = false;
                            break;
                        case "-w":
                            shouldLaunchWebBrowser = true;
                            break;
                        case "--gzip":
                            useGzip = true;
                            break;
                        case "--legacy-index-page":
                            isGenerateHtml5IndexPage = false;
                            break;
                        case "--log-format-common":
                            logFormatType = LogFormatType.Common;
                            break;
                        case "--log-format-combined":
                            logFormatType = LogFormatType.Combined;
                            break;
                        case "--":
                            treatNonOptionArg = true;
                            break;
                        default:
                            throw new InvalidOperationException(string.Format("Unknown option \"{0}\"", arg));
                    }
                }
            }

            if (hostPartList.Count == 0)
            {
                hostPartList.Add(DefaultHostIpv4);
                hostPartList.Add(DefaultHostIpv6);
            }

            if (prefixRoot.Length > 0 && prefixRoot[0] != '/')
            {
                prefixRoot = "/" + prefixRoot;
            }

            if (fallbackToFreePort)
            {
                var newPort = EnsureAvailablePort(port);
                if (newPort != port)
                {
                    Console.ForegroundColor = TimestampColor;
                    Console.Error.Write("[{0}]", GetCurrentTimestamp());
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Error.WriteLine(" Failed to bind port {0}, fallback to port {1}.", port, newPort);
                    Console.ResetColor();
                }
                port = newPort;
            }

            var prefixList = new List<string>(hostPartList.Count);
            foreach (var hostPart in hostPartList)
            {
                prefixList.Add(string.Format("http://{0}:{1}{2}/", hostPart, port, prefixRoot));
            }

            return new AppOptions(
                prefixList,
                localRootPath,
                prefixRoot,
                treatPrefixRootAsLocalRoot,
                useGzip,
                shouldLaunchWebBrowser,
                isGenerateHtml5IndexPage,
                logFormatType);
        }

        /// <summary>
        /// Get current application file path.
        /// </summary>
        /// <returns>Current application file path.</returns>
        private static string GetCurrentApplicationPath()
        {
#if NET5_0_OR_GREATER
            return Path.GetFileName(Environment.ProcessPath);
#else
            return Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
#endif  // NET5_0_OR_GREATER
        }

        /// <summary>
        /// Show usage of this program.
        /// </summary>
        /// <param name="progName">Program name.</param>
        /// <param name="writer"><see cref="TextWriter"/> instance to output.</param>
        private static void ShowUsage(string progName = null, TextWriter writer = null)
        {
            if (progName == null)
            {
                progName = Path.GetFileName(GetCurrentApplicationPath());
            }
            if (writer == null)
            {
                writer = Console.Out;
            }
            writer.WriteLine("[USAGE]");
            writer.WriteLine("  {0} {{OPTIONS...}} {{PORT}}", progName);
            writer.WriteLine("[OPTIONS]");
            writer.WriteLine("  -g");
            writer.WriteLine("    Use \"+\" as host part.");
            writer.WriteLine("  -h");
            writer.WriteLine("    Show this message and exit program.");
            writer.WriteLine("  -H HOST");
            writer.WriteLine("    Use HOST as host part.");
            writer.WriteLine("  -l DIR");
            writer.WriteLine("    Use specified local directory as the root. (Default: .)");
            writer.WriteLine("  -r DIR");
            writer.WriteLine("    Use specified directory for the part of directory of prefix.");
            writer.WriteLine("  -R");
            writer.WriteLine("    Treat the prefix root directory as the local root directory.");
            writer.WriteLine("  -t");
            writer.WriteLine("    Use \"http://+:80/Temporary_Listen_Addresses/\" as prefix.");
            writer.WriteLine("    Same as specifying \"-g\", \"-H Temporary_Listen_Addresses\" and port 80.");
            writer.WriteLine("  -w");
            writer.WriteLine("    Launch default web browser after starting listening.");
            writer.WriteLine("  --gzip");
            writer.WriteLine("    Compress text data with gzip.");
            writer.WriteLine("  --legacy-index-page");
            writer.WriteLine("    Generate index page written in HTML4.01.");
            writer.WriteLine("  --log-format-common");
            writer.WriteLine("    Write logs in common log format.");
            writer.WriteLine("  --log-format-combined");
            writer.WriteLine("    Write logs in combined log format. (Default)");
        }

        /// <summary>
        /// Start <see cref="HttpListener"/> in new thread.
        /// </summary>
        /// <param name="listener"><see cref="HttpListener"/> instance to start.</param>
        /// <param name="appOptions">Application options.</param>
        /// <returns>Listening thread.</returns>
        private static Thread StartListenThread(HttpListener listener, AppOptions appOptions)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    while (listener.IsListening)
                    {
                        var context = listener.GetContext();  // Wait for request.
                        StartHandleRequest(context, appOptions);
                    }
                }
                catch (HttpListenerException)
                {
                    // Reached when listener.Stop() is called.
                }
            });
            thread.Start();
            return thread;
        }

        /// <summary>
        /// Ensure to get available port number.
        /// </summary>
        /// <param name="desiredPort">Port number you want to use.</param>
        /// <param name="startPort">Minimum port number for the search range.</param>
        /// <param name="endPort">Maximum port number for the search range.</param>
        /// <returns><paramref name="desiredPort"/> if free to use, else available port number if found, otherwise -1.</returns>
        private static int EnsureAvailablePort(int desiredPort, int startPort = 49152, int endPort = 65535)
        {
            var usedPortSet = GetUsedPortSet();

            if (!usedPortSet.Contains(desiredPort))
            {
                return desiredPort;
            }

            for (var port = startPort; port <= endPort; port++)
            {
                if (!usedPortSet.Contains(port))
                {
                    return port;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get <see cref="HashSet{T}"/> of used port number in specified range.
        /// </summary>
        /// <param name="startPort">Minimum port number for the search range.</param>
        /// <param name="endPort">Maximum port number for the search range.</param>
        /// <returns><see cref="HashSet{T}"/> of used port number.</returns>
        private static HashSet<int> GetUsedPortSet(int startPort = 1, int endPort = 65535)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            var usedPortSet = new HashSet<int>();
            foreach (var activeTcpConnection in ipGlobalProperties.GetActiveTcpConnections())
            {
                var port = activeTcpConnection.LocalEndPoint.Port;
                if (startPort <= port && port <= endPort)
                {
                    usedPortSet.Add(port);
                }
            }
            foreach (var activeTcpListener in ipGlobalProperties.GetActiveTcpListeners())
            {
                var port = activeTcpListener.Port;
                if (startPort <= port && port <= endPort)
                {
                    usedPortSet.Add(port);
                }
            }
            foreach (var activeUdpListener in ipGlobalProperties.GetActiveUdpListeners())
            {
                var port = activeUdpListener.Port;
                if (startPort <= port && port <= endPort)
                {
                    usedPortSet.Add(port);
                }
            }

            return usedPortSet;
        }

        /// <summary>
        /// Open specified URL with default web browser.
        /// </summary>
        /// <param name="url">URL to open.</param>
        private static void OpenWithDefaultWebBrowser(string url)
        {
            lock (_consoleLock)
            {
                Console.ForegroundColor = TimestampColor;
                Console.Error.Write("[{0}]", DateTime.Now.ToString(DateTimeFormat));
                Console.ForegroundColor = ConsoleColor.White;
                Console.Error.WriteLine(" Open with default web browser: {0}", url);
                Console.ResetColor();
            }

#if NETFRAMEWORK
            Process.Start(new ProcessStartInfo("rundll32.exe")
            {
                Arguments = "url.dll,FileProtocolHandler " + url,
                UseShellExecute = true
            });
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("rundll32.exe")
                {
                    Arguments = "url.dll,FileProtocolHandler " + url,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
#endif  // NETFRAMEWORK
        }

        /// <summary>
        /// Get current timestamp string.
        /// </summary>
        /// <returns>Current timestamp string.</returns>
        private static string GetCurrentTimestamp()
        {
            return DateTime.Now.ToString(DateTimeFormat);
        }

        /// <summary>
        /// Handle <see cref="HttpListenerRequest"/> in specified <see cref="HttpListenerContext"/>.
        /// </summary>
        /// <param name="context">A <see cref="HttpListenerContext"/>.</param>
        /// <param name="appOptions">Application options.</param>
        /// <returns>Handling <see cref="Task"/>.</returns>
        private static Task StartHandleRequest(HttpListenerContext context, AppOptions appOptions)
        {
            return Task.Run(() =>
            {
                var request = context.Request;
                try
                {
                    using (var response = context.Response)
                    {
                        switch (request.HttpMethod)
                        {
                            case "GET":
                            case "HEAD":
                                HandleAsGetOrHeadRequest(request, response, appOptions);
                                break;
                            case "OPTIONS":
                                response.Headers.Add("Allow", "GET, HEAD, OPTIONS, TRACE");
                                break;
                            case "TRACE":
                                HandleAsTraceRequest(request, response);
                                break;
                            default:
                                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                                break;
                        }

                        lock (_consoleLock)
                        {
                            if (appOptions.LogFormatType == LogFormatType.Combined)
                            {
                                WriteLogInCombinedLogFormat(Console.Out, request, response);
                            }
                            else
                            {
                                WriteLogInCommonLogFormat(Console.Out, request, response);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (_consoleLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Error.WriteLine(ex);
                        Console.ResetColor();
                    }
                }
            });
        }

        /// <summary>
        /// Handle <see cref="HttpListenerRequest"/> as a "GET" or "HEAD" request.
        /// </summary>
        /// <param name="request">A <see cref="HttpListenerRequest"/>.</param>
        /// <param name="response">A <see cref="HttpListenerResponse"/>.</param>
        /// <param name="appOptions">Application options.</param>
        private static void HandleAsGetOrHeadRequest(HttpListenerRequest request, HttpListenerResponse response, AppOptions appOptions)
        {
            string paramPart;
            var rawPath = RemoveUrlParameter(request.RawUrl, out paramPart);

            var collapsedPath = CollapseSeqChar(rawPath, '/');
            if (collapsedPath != rawPath)
            {
                response.Headers.Set("Location", paramPart.Length == 0 ? collapsedPath : (collapsedPath + "?" + paramPart));
                response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
                return;
            }

            if (appOptions.TreatPrefixRootAsLocalRoot)
            {
                if (rawPath == appOptions.PrefixRoot)
                {
                    rawPath = "/.";
                }
                else if (rawPath.StartsWith(appOptions.PrefixRoot + "/"))
                {
                    rawPath = rawPath.Substring(appOptions.PrefixRoot.Length);
                }
            }
            else if (rawPath.Length == 0)
            {
                rawPath = "/.";
            }

            var entryPath = (appOptions.LocalRootPath + WebUtility.UrlDecode(rawPath)).Replace("/", _dirSep);
            if (entryPath.Contains(_parentMid) || entryPath.EndsWith(_parentLast))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var doGzip = false;
            if (appOptions.UseGzip)
            {
                foreach (var key in request.Headers.AllKeys)
                {
#if NETCOREAPP2_1_OR_GREATER
                    if (string.Equals(key, "Accept-Encoding", StringComparison.OrdinalIgnoreCase)
                        && request.Headers[key].Contains("gzip", StringComparison.OrdinalIgnoreCase))
#else
                    if (string.Equals(key, "Accept-Encoding", StringComparison.OrdinalIgnoreCase)
                        && request.Headers[key].ToLower().Contains("gzip"))
#endif  // NETCOREAPP2_1_OR_GREATER
                    {
                        doGzip = true;
                        break;
                    }
                }
            }
            var shouldTransferBody = request.HttpMethod != "HEAD";

            if (Directory.Exists(entryPath))
            {
                if (!entryPath.EndsWith(_dirSep))
                {
                    var redirectPath = rawPath + "/";
                    response.Headers.Set("Location", paramPart.Length == 0 ? redirectPath : (redirectPath + "?" + paramPart));
                    response.StatusCode = (int)HttpStatusCode.TemporaryRedirect;
                    return;
                }

                response.ContentType = "text/html";

                var indexPath = entryPath + "index.html";
                if (File.Exists(indexPath))
                {
                    TransferFile(response, indexPath, doGzip, shouldTransferBody);
                }
                else
                {
                    var rootFaviconPath = appOptions.PrefixRoot + "/favicon.ico";
                    var indexPage = CreateIndexPage(entryPath, rawPath, rootFaviconPath, appOptions.IsGenerateHtml5IndexPage);
                    TransferTextData(response, indexPage, Encoding.UTF8, doGzip, shouldTransferBody);
                }
            }
            else if (File.Exists(entryPath))
            {
                try
                {
#if USE_SYSTEM_WEB_MIME_MAPPING
                    response.ContentType = MimeMapping.GetMimeMapping(entryPath);
#else
                    response.ContentType = MimeMapper.GetMimeType(entryPath);
#endif  // USE_SYSTEM_WEB_MIME_MAPPING
                    TransferFile(response, entryPath, doGzip, shouldTransferBody);
                }
                catch (Exception ex)
                {
                    lock (_consoleLock)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Error.WriteLine(ex);
                        Console.ResetColor();
                    }
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
            else
            {
#if USE_WIN32ICON_AS_FAVICON || USE_EMBEDDED_ICON_AS_FAVICON
                if (rawPath == "/favicon.ico")
                {
                    var iconData = GetSelfIconData();
                    if (iconData.Length == 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    else
                    {
                        response.ContentType = "image/x-icon";
                        TransferData(response, iconData, doGzip, shouldTransferBody);
                    }
                }
#if NETFRAMEWORK || WINDOWS
                else if (rawPath.StartsWith("/apple-touch-icon"))
                {
                    var match = RegexProvider.AppleTouchIconSuffixRegex.Match(rawPath, 17, rawPath.Length - 17);
                    if (!match.Success)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }

                    var sizeText = match.Groups[1].Value;
                    var size = sizeText.Length == 0 ? -1 : int.Parse(sizeText);

                    var faviconPath = Path.Combine(appOptions.LocalRootPath, "favicon.ico");
                    var appleTouchIconData = CreateAppleTouchIconData(faviconPath, size);
                    if (appleTouchIconData.Length == 0)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }

                    response.ContentType = "image/png";
                    TransferData(response, appleTouchIconData, false, shouldTransferBody);
                }
#endif  // NETFRAMEWORK || WINDOWS
                else
#endif  // USE_WIN32ICON_AS_FAVICON || USE_EMBEDDED_ICON_AS_FAVICON
                if (rawPath == "/.well-known/appspecific/com.chrome.devtools.json" && request.IsLocal)
                {
                    var chromeDevToolJson = CreateChromeDevToolJson(appOptions.LocalRootPath);
                    response.ContentType = "application/json";
                    TransferTextData(response, chromeDevToolJson, Encoding.UTF8, doGzip, shouldTransferBody);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
        }

        /// <summary>
        /// Handle <see cref="HttpListenerRequest"/> as a "TRACE" request.
        /// </summary>
        /// <param name="request">A <see cref="HttpListenerRequest"/>.</param>
        /// <param name="response">A <see cref="HttpListenerResponse"/>.</param>
        private static void HandleAsTraceRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            var sb = new StringBuilder()
                .AppendFormat("{0} {1} HTTP/{2}", request.HttpMethod, request.RawUrl, request.ProtocolVersion)
                .AppendLine();

            var headers = request.Headers;
            foreach (var key in headers.AllKeys)
            {
                sb.Append(key).Append(": ").AppendLine(headers[key]);
            }

            TransferTextData(response, sb.ToString(), Encoding.UTF8, false, true);
        }

        /// <summary>
        /// Write access log in the combined log format.
        /// </summary>
        /// <param name="writer"><see cref="TextWriter"/> to output.</param>
        /// <param name="request">HTTP request instance.</param>
        /// <param name="response">HTTP response instance.</param>
        private static void WriteLogInCombinedLogFormat(TextWriter writer, HttpListenerRequest request, HttpListenerResponse response)
        {
            WriteLogInCommonLogFormat(writer, request, response, false);
            Console.ForegroundColor = UrlRefererColor;
            writer.Write(" \"{0}\"", request.UrlReferrer == null ? "-" : request.UrlReferrer.ToString());
            Console.ForegroundColor = UserAgentColor;
            writer.WriteLine(" \"{0}\"", request.UserAgent);
            Console.ResetColor();
        }

        /// <summary>
        /// Write access log in the combined log format.
        /// </summary>
        /// <param name="writer"><see cref="TextWriter"/> to output.</param>
        /// <param name="request">HTTP request instance.</param>
        /// <param name="response">HTTP response instance.</param>
        /// <param name="shouldWriteNewLine">True to write new line code and reset console color.</param>
        private static void WriteLogInCommonLogFormat(TextWriter writer, HttpListenerRequest request, HttpListenerResponse response, bool shouldWriteNewLine = true)
        {
            Console.ForegroundColor = RemoteEndPointAddressColor;
            writer.Write("{0}", request.RemoteEndPoint.Address);
            Console.ForegroundColor = NonImportantPartColor;
            writer.Write(" - -");
            Console.ForegroundColor = TimestampColor;
            writer.Write(" [{0}]", GetCurrentTimestamp());
            Console.ForegroundColor = ResponseHeaderColor;
            writer.Write(" \"{0} {1} HTTP/{2}\"", request.HttpMethod, request.RawUrl, request.ProtocolVersion);
            var statusType = response.StatusCode / 100;
            Console.ForegroundColor = statusType == 2 ? HttpStatusOkColor
                : (statusType == 4 || statusType == 5) ? HttpStatusNgColor
                : HttpStatusOtherColor;
            writer.Write(" {0}", response.StatusCode);
            Console.ForegroundColor = ContentLengthColor;
            if (response.StatusCode == (int)HttpStatusCode.OK)
            {
                writer.Write(" {0}", response.ContentLength64);
            }
            else
            {
                writer.Write(" -");
            }
            if (shouldWriteNewLine)
            {
                writer.WriteLine();
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Remove parameter part from specified URL.
        /// </summary>
        /// <param name="url">URL string.</param>
        /// <param name="paramPart">Parameter part.</param>
        /// <returns>URL string which is removed parameter part.</returns>
        private static string RemoveUrlParameter(string url, out string paramPart)
        {
            var index = url.IndexOf('?');
            if (index == -1)
            {
                paramPart = string.Empty;
                return url;
            }
            else
            {
                paramPart = index == url.Length - 1 ? string.Empty : url.Substring(index + 1);
                return url.Substring(0, index);
            }
        }

        /// <summary>
        /// Collapce specified character.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="target">Target character.</param>
        /// <returns>Collapsed string.</returns>
        private static string CollapseSeqChar(string source, char target)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }
#if NET5_0_OR_GREATER
            var src = source.AsSpan();

            var needsChange = false;
            for (int i = 1; i < src.Length; i++)
            {
                if (src[i] == target && src[i - 1] == target)
                {
                    needsChange = true;
                    break;
                }
            }

            if (!needsChange)
            {
                return source;
            }

            var count = 1;
            for (int i = 1; i < src.Length; i++)
            {
                if (!(src[i] == target && src[i - 1] == target))
                {
                    count++;
                }
            }

            return string.Create(count, (source, target), (dst, state) =>
            {
                var (s0, t) = state;
                var s = s0.AsSpan();
                int di = 0;

                dst[di++] = s[0];
                for (int i = 1; i < s.Length; i++)
                {
                    if (!(s[i] == t && s[i - 1] == t))
                    {
                        dst[di++] = s[i];
                    }
                }
            });
#else
            var sb = new StringBuilder(source.Length);
            var prev = source[0];
            sb.Append(prev);

            for (int i = 1; i < source.Length; i++)
            {
                var c = source[i];
                if (c != target || prev != target)
                {
                    sb.Append(c);
                }
                prev = c;
            }

            return sb.ToString();
#endif  // NET5_0_OR_GREATER
        }

        /// <summary>
        /// Transfer specified byte data.
        /// </summary>
        /// <param name="response">Destination <see cref="HttpListenerResponse"/>.</param>
        /// <param name="data">Byte data to transfer.</param>
        /// <param name="doGzip">True to compress data with gzip.</param>
        /// <param name="shouldTransferBody">True to write actual data.</param>
        private static void TransferData(HttpListenerResponse response, byte[] data, bool doGzip, bool shouldTransferBody)
        {
            if (doGzip)
            {
                using (var ms = new MemoryStream(data.Length))
                {
                    using (var gs = new GZipStream(ms, CompressionMode.Compress, true))
                    {
                        gs.Write(data, 0, data.Length);
                    }
                    if (ms.Length < data.LongLength)
                    {
                        response.AddHeader("Content-Encoding", "gzip");
                        response.ContentLength64 = ms.Length;
                        if (shouldTransferBody)
                        {
                            ms.Position = 0;
                            ms.CopyTo(response.OutputStream);
                        }
                        return;
                    }
                }
            }

            response.ContentLength64 = data.Length;
            if (shouldTransferBody)
            {
                response.OutputStream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Transfer specified byte data.
        /// </summary>
        /// <param name="response">Destination <see cref="HttpListenerResponse"/>.</param>
        /// <param name="text">Text to transfer.</param>
        /// <param name="encoding">Encoding for <paramref name="text"/>.</param>
        /// <param name="doGzip">True to compress data with gzip.</param>
        /// <param name="shouldTransferBody">True to write actual data.</param>
        private static void TransferTextData(HttpListenerResponse response, string text, Encoding encoding, bool doGzip, bool shouldTransferBody)
        {
            if (doGzip || shouldTransferBody)
            {
                TransferData(response, encoding.GetBytes(text), doGzip, shouldTransferBody);
            }
            else
            {
                response.ContentLength64 = encoding.GetByteCount(text);
            }
        }

        /// <summary>
        /// Transfer file content.
        /// </summary>
        /// <param name="response">Destination <see cref="HttpListenerResponse"/>.</param>
        /// <param name="filePath">File path to read from.</param>
        /// <param name="doGzip">True to compress data with gzip.</param>
        /// <param name="shouldTransferBody">True to write actual data.</param>
        private static void TransferFile(HttpListenerResponse response, string filePath, bool doGzip, bool shouldTransferBody)
        {
            var fileSize = new FileInfo(filePath).Length;
            if (doGzip)
            {
                using (var ms = new MemoryStream((int)fileSize))
                {
                    using (var gs = new GZipStream(ms, CompressionMode.Compress, true))
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, (int)Math.Min(81920, fileSize), FileOptions.SequentialScan))
                    {
                        fs.CopyTo(gs);
                    }
                    if (ms.Length < fileSize)
                    {
                        response.AddHeader("Content-Encoding", "gzip");
                        response.ContentLength64 = ms.Length;
                        if (shouldTransferBody)
                        {
                            ms.Position = 0;
                            ms.CopyTo(response.OutputStream);
                        }
                        return;
                    }
                }
            }

            response.ContentLength64 = fileSize;
            if (shouldTransferBody)
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, (int)Math.Min(81920, fileSize), FileOptions.SequentialScan))
                {
                    fs.CopyTo(response.OutputStream);
                }
            }
        }

        /// <summary>
        /// Create string of the index page of specified path written in HTML5.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="urlPath">URL path.</param>
        /// <param name="faviconPath">favicon.ico path.</param>
        /// <param name="isHtml5">True to create HTML5 index page, otherwise false (create HTML4.01 index page).</param>
        /// <returns>Index page HTML string.</returns>
        private static string CreateIndexPage(string path, string urlPath, string faviconPath, bool isHtml5 = true)
        {
            return isHtml5 ? CreateHtml5IndexPage(path, urlPath, faviconPath) : CreateHtml4IndexPage(path, urlPath, faviconPath);
        }

        /// <summary>
        /// Create string of the index page of specified path written in HTML5.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="urlPath">URL path.</param>
        /// <param name="faviconPath">favicon.ico path.</param>
        /// <returns>Index page HTML string.</returns>
        private static string CreateHtml5IndexPage(string path, string urlPath, string faviconPath)
        {
            var encodedPath = WebUtility.HtmlEncode(urlPath);
            var sb = new StringBuilder("<!DOCTYPE html>\n")
                .Append("<html lang=\"en\">\n")
                .Append("<head>\n")
                .Append("  <meta charset=\"utf-8\"/>\n")
                .AppendFormat("  <title>{0}</title>\n", encodedPath)
                .Append("  <style>\n")
                .Append("  .entry-item { font-family: ui-monospace, SFMono-Regular, 'SF Mono', Menlo, Consolas, 'Courier New', Courier, 'Liberation Mono', monospace; }\n")
                .Append("  @media (prefers-color-scheme: dark) { body { color: #ffffff; background-color: #000000; } a:link { color: #7690ed; } a:visited { color: #7a6c87; } }\n")
                .Append("  </style>\n");
            if (faviconPath != null)
            {
                sb.AppendFormat("  <link rel=\"shortcut icon\" href=\"{0}\">\n", faviconPath);
            }
            sb.Append("</head>\n")
                .Append("<body>\n")
                .AppendFormat("<h1>Directory listing for {0}</h1>\n", encodedPath)
                .Append("<hr>\n");

            EmitHtmlUlListOfDirectoryEntries(sb, path, urlPath);

            return sb.Append("</ul>\n")
                .Append("<hr>\n")
                .Append("</body>\n")
                .Append("</html>\n")
                .ToString();
        }

        /// <summary>
        /// Create string of the index page of specified path written in HTML4.01.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="urlPath">URL path.</param>
        /// <param name="faviconPath">favicon.ico path.</param>
        /// <returns>Index page HTML string.</returns>
        private static string CreateHtml4IndexPage(string path, string urlPath, string faviconPath)
        {
            var encodedPath = WebUtility.HtmlEncode(urlPath);
            var sb = new StringBuilder("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">\n")
                .Append("<html lang=\"en\">\n")
                .Append("<head>\n")
                .Append("  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\n")
                .Append("  <meta http-equiv=\"Content-Style-Type\" content=\"text/css\">\n")
                .AppendFormat("  <title>{0}</title>\n", encodedPath)
                .Append("  <style type=\"text/css\"><!--\n")
                .Append("  .entry-item { font-family: ui-monospace, SFMono-Regular, 'SF Mono', Menlo, Consolas, 'Courier New', Courier, 'Liberation Mono', monospace; }\n")
                .Append("  --></style>\n");
            if (faviconPath != null)
            {
                sb.AppendFormat("  <link rel=\"shortcut icon\" href=\"{0}\">\n", faviconPath);
            }
            sb.Append("</head>\n")
                .Append("<body>\n")
                .AppendFormat("<h1>Directory listing for {0}</h1>\n", encodedPath)
                .Append("<hr>\n");

            EmitHtmlUlListOfDirectoryEntries(sb, path, urlPath);

            return sb.Append("</ul>\n")
                .Append("<hr>\n")
                .Append("</body>\n")
                .Append("</html>\n")
                .ToString();
        }

        /// <summary>
        /// Emit HTML unordered list (&lt;ul&gt;) of the directory entries to specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">Output <see cref="StringBuilder"/>.</param>
        /// <param name="path">Directory path.</param>
        /// <param name="urlPath">URL path.</param>
        private static void EmitHtmlUlListOfDirectoryEntries(StringBuilder sb, string path, string urlPath)
        {
            sb.Append("<ul>\n");

            if (urlPath != "/")
            {
                sb.Append("  <li><a class=\"entry-item\" href=\"..\">..</a></li>\n");
            }

            foreach (var entry in Directory.EnumerateFileSystemEntries(path))
            {
                var basename = Path.GetFileName(entry);
                sb.AppendFormat("  <li><a class=\"entry-item\" href=\"{0}{2}\">{1}{2}</a></li>\n",
                    WebUtility.UrlEncode(basename),
                    WebUtility.HtmlEncode(basename),
                    Directory.Exists(entry) ? "/" : "");
            }

            sb.Append("</ul>\n");
        }

        /// <summary>
        /// Create com.chrome.devtools.json string.
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns>Json string of com.chrome.devtools.json.</returns>
        private static string CreateChromeDevToolJson(string rootPath)
        {
            return new StringBuilder("{\n")
                .Append("  \"workspace\": {\n")
                .AppendFormat("    \"root\": \"{0}\",\n", Path.GetFullPath(rootPath).Replace(@"\", @"/"))
                .AppendFormat("    \"uuid\": \"{0}\"\n", GetGuid().ToString())
                .Append("  }\n")
                .Append("}\n")
                .ToString();
        }

        /// <summary>
        /// Get new <see cref="Guid"/> or cache <see cref="Guid"/>.
        /// </summary>
        /// <returns><see cref="Guid"/> value.</returns>
        private static Guid GetGuid()
        {
            if (_guid != default(Guid))
            {
                return _guid;
            }

#if USE_ASMGUID_FOR_CHROME_DEVTOOL_JSON
            var guid = GetSelfAssemblyGuid();
            if (guid != default(Guid))
            {
                return _guid = guid;
            }
#endif  // USE_ASMGUID_FOR_CHROME_DEVTOOL_JSON

            return _guid = Guid.NewGuid();
        }

#if USE_ASMGUID_FOR_CHROME_DEVTOOL_JSON
        /// <summary>
        /// Get self <see cref="Guid"/>.
        /// </summary>
        /// <returns>Self <see cref="Guid"/> value.</returns>
        private static Guid GetSelfAssemblyGuid()
        {
            var attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
            return attrs.Length > 0 ? new Guid(((GuidAttribute)attrs[0]).Value) : default(Guid);
        }
#endif  // USE_ASMGUID_FOR_CHROME_DEVTOOL_JSON

#if USE_WIN32ICON_AS_FAVICON || USE_EMBEDDED_ICON_AS_FAVICON
        /// <summary>
        /// Get self icon data.
        /// </summary>
        /// <returns>Resource data.</returns>
        private static byte[] GetSelfIconData()
        {
#if USE_WIN32ICON_AS_FAVICON
#if NETFRAMEWORK || WINDOWS
            const bool isWindows = true;
#else
            var isWindows = _isWindows;
#endif  // NETFRAMEWORK || WINDOWS
#endif  // USE_WIN32ICON_AS_FAVICON

            if (_iconData != null)
            {
                return _iconData;
            }

#if USE_WIN32ICON_AS_FAVICON
            if (isWindows)
            {
                var iconDataList = Win32IconHelper.GetAllIconData(GetCurrentApplicationPath());
                if (iconDataList.Count > 0)
                {
                    return _iconData = iconDataList[0];
                }
            }
#endif  // USE_WIN32ICON_AS_FAVICON

#if USE_EMBEDDED_ICON_AS_FAVICON
            var iconData = GetSelfResourceData(FaviconResourcePath);
            if (iconData != null)
            {
                return _iconData = iconData;
            }
#endif  // USE_EMBEDDED_ICON_AS_FAVICON

            return _iconData = Array.Empty<byte>();
        }

#if NETFRAMEWORK || WINDOWS
        /// <summary>
        /// Get apple-touch-icon data. (PNG file data)
        /// </summary>
        /// <param name="faviconPath">Path to the <c>favicon.ico</c>.</param>
        /// <param name="size">Width and height of apple-touch-icon.</param>
        /// <returns>Apple-touch-icon data.</returns>
        private static byte[] CreateAppleTouchIconData(string faviconPath, int size)
        {
            if (size == -1)
            {
                size = 180;
            }

            if (size < 8 || size > 256)
            {
                return Array.Empty<byte>();
            }

            byte[] appleTouchIconData;
            if (_appleTouchIconDataCacheDict.TryGetValue(size, out appleTouchIconData))
            {
                return appleTouchIconData;
            }

            if (File.Exists(faviconPath))
            {
                using (var iconMs = new FileStream(faviconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 81920))
                {
                    appleTouchIconData = CreatePngFromIcon(iconMs, size, size);
                }
            }
            else
            {
                var iconData = GetSelfIconData();
                if (iconData.Length == 0)
                {
                    appleTouchIconData = Array.Empty<byte>();
                }
                else
                {
                    using (var iconMs = new MemoryStream(iconData))
                    {
                        appleTouchIconData = CreatePngFromIcon(iconMs, size, size);
                    }
                }
            }

            _appleTouchIconDataCacheDict.Add(size, appleTouchIconData);
            return appleTouchIconData;
        }

        /// <summary>
        /// Create PNG data from icon file data.
        /// </summary>
        /// <param name="iconStream"><see cref="Stream"/> of the source icon data.</param>
        /// <param name="width">Width of the result png data.</param>
        /// <param name="height">Height of the result png data.</param>
        /// <returns>PNG data.</returns>
        private static byte[] CreatePngFromIcon(Stream iconStream, int width, int height)
        {
            using (var icon = new Icon(iconStream, width, height))
            using (var bmp = icon.ToBitmap())
            using (var pngMs = new MemoryStream())
            {
                bmp.Save(pngMs, ImageFormat.Png);
                return pngMs.ToArray();
            }
        }
#endif  // NETFRAMEWORK || WINDOWS
#endif  // USE_WIN32ICON_AS_FAVICON || USE_EMBEDDED_ICON_AS_FAVICON

#if USE_EMBEDDED_ICON_AS_FAVICON
        /// <summary>
        /// Get specified resource data.
        /// </summary>
        /// <param name="resourcePath">Resource path.</param>
        /// <returns>Resource data.</returns>
        private static byte[] GetSelfResourceData(string resourcePath)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    return null;
                }

                var data = new byte[stream.Length];
                var _ = stream.Read(data, 0, data.Length);
                return data;
            }
        }
#endif  // USE_EMBEDDED_ICON_AS_FAVICON
    }

    /// <summary>
    /// Application options.
    /// </summary>
    public sealed class AppOptions
    {
        /// <summary>
        /// <see cref="List{T}"/> of the prefix.
        /// </summary>
        public List<string> PrefixList { get; private set; }
        /// <summary>
        /// Local root path.
        /// </summary>
        public string LocalRootPath { get; private set; }
        /// <summary>
        /// Root path of the prefix.
        /// </summary>
        public string PrefixRoot { get; private set; }
        /// <summary>
        /// True to treat the prefix root directory as the local root directory.
        /// </summary>
        public bool TreatPrefixRootAsLocalRoot { get; private set; }
        /// <summary>
        /// True to compress text data with GZIP format.
        /// </summary>
        public bool UseGzip { get; private set; }
        /// <summary>
        /// True to launch default web browser after starting listening.
        /// </summary>
        public bool ShouldLaunchWebBrowser { get; private set; }
        /// <summary>
        /// True to create HTML5 index page, otherwise false (create HTML4.01 index page).
        /// </summary>
        public bool IsGenerateHtml5IndexPage { get; private set; }
        /// <summary>
        /// Log format type.
        /// </summary>
        public LogFormatType LogFormatType { get; private set; }

        /// <summary>
        /// Create <see cref="AppOptions"/> instance.
        /// </summary>
        /// <param name="prefixList"><see cref="List{T}"/> of the prefix.</param>
        /// <param name="localRootPath">Local root path.</param>
        /// <param name="prefixRoot">Root path of the prefix.</param>
        /// <param name="treatPrefixRootAsLocalRoot">True to treat the prefix root directory as the local root directory.</param>
        /// <param name="useGzip">True to compress text data with GZIP format.</param>
        /// <param name="shouldLaunchWebBrowser">True to launch default web browser after starting listening.</param>
        /// <param name="isGenerateHtml5IndexPage">True to create HTML5 index page, otherwise false (create HTML4.01 index page).</param>
        /// <param name="logFormatType">Log format type.</param>
        public AppOptions(
            List<string> prefixList,
            string localRootPath,
            string prefixRoot,
            bool treatPrefixRootAsLocalRoot,
            bool useGzip,
            bool shouldLaunchWebBrowser,
            bool isGenerateHtml5IndexPage,
            LogFormatType logFormatType)
        {
            PrefixList = prefixList;
            LocalRootPath = localRootPath;
            PrefixRoot = prefixRoot;
            TreatPrefixRootAsLocalRoot = treatPrefixRootAsLocalRoot;
            UseGzip = useGzip;
            ShouldLaunchWebBrowser = shouldLaunchWebBrowser;
            LogFormatType = logFormatType;
            IsGenerateHtml5IndexPage = isGenerateHtml5IndexPage;
        }
    }

    /// <summary>
    /// Log format values.
    /// </summary>
    public enum LogFormatType
    {
        /// <summary>
        /// Represent that write logs in common log format.
        /// </summary>
        Common,
        /// <summary>
        /// Represent that write logs in combined log format.
        /// </summary>
        Combined
    }

#if !USE_SYSTEM_WEB_MIME_MAPPING
    /// <summary>
    /// MIME type mapper class.
    /// </summary>
    public static class MimeMapper
    {
        /// <summary>
        /// Fallback MIME type, which is used when extension is not exists in <see cref="_mimeTypeDict"/>.
        /// </summary>
        private const string FallbackMimeType = "application/octet-stream";
        /// <summary>
        /// Extension - MIME type dictionary.
        /// </summary>
#if NET8_0_OR_GREATER
        private static readonly FrozenDictionary<string, string> _mimeTypeDict = new Dictionary<string, string>()
#else
        private static readonly Dictionary<string, string> _mimeTypeDict = new Dictionary<string, string>()
#endif  // NET8_0_OR_GREATER
        {
            {".323", "text/h323"},
            {".aaf", "application/octet-stream"},
            {".aca", "application/octet-stream"},
            {".accdb", "application/msaccess"},
            {".accde", "application/msaccess"},
            {".accdt", "application/msaccess"},
            {".acx", "application/internet-property-stream"},
            {".afm", "application/octet-stream"},
            {".ai", "application/postscript"},
            {".aif", "audio/x-aiff"},
            {".aifc", "audio/aiff"},
            {".aiff", "audio/aiff"},
            {".application", "application/x-ms-application"},
            {".art", "image/x-jg"},
            {".asd", "application/octet-stream"},
            {".asf", "video/x-ms-asf"},
            {".asi", "application/octet-stream"},
            {".asm", "text/plain"},
            {".asr", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".atom", "application/atom+xml"},
            {".au", "audio/basic"},
            {".avi", "video/x-msvideo"},
            {".axs", "application/olescript"},
            {".bas", "text/plain"},
            {".bcpio", "application/x-bcpio"},
            {".bin", "application/octet-stream"},
            {".bmp", "image/bmp"},
            {".c", "text/plain"},
            {".cab", "application/octet-stream"},
            {".calx", "application/vnd.ms-office.calx"},
            {".cat", "application/vnd.ms-pki.seccat"},
            {".cdf", "application/x-cdf"},
            {".chm", "application/octet-stream"},
            {".class", "application/x-java-applet"},
            {".clp", "application/x-msclip"},
            {".cmx", "image/x-cmx"},
            {".cnf", "text/plain"},
            {".cod", "image/cis-cod"},
            {".cpio", "application/x-cpio"},
            {".cpp", "text/plain"},
            {".crd", "application/x-mscardfile"},
            {".crl", "application/pkix-crl"},
            {".crt", "application/x-x509-ca-cert"},
            {".csh", "application/x-csh"},
            {".cs", "text/plain"},
            {".css", "text/css"},
            {".csv", "application/octet-stream"},
            {".cur", "application/octet-stream"},
            {".dcr", "application/x-director"},
            {".deploy", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dib", "image/bmp"},
            {".dir", "application/x-director"},
            {".disco", "text/xml"},
            {".dll", "application/x-msdownload"},
            {".dll.config", "text/xml"},
            {".dlm", "text/dlm"},
            {".doc", "application/msword"},
            {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".dot", "application/msword"},
            {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
            {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
            {".dsp", "application/octet-stream"},
            {".dtd", "text/xml"},
            {".dvi", "application/x-dvi"},
            {".dwf", "drawing/x-dwf"},
            {".dwp", "application/octet-stream"},
            {".dxr", "application/x-director"},
            {".eml", "message/rfc822"},
            {".emz", "application/octet-stream"},
            {".eot", "application/octet-stream"},
            {".eps", "application/postscript"},
            {".etx", "text/x-setext"},
            {".evy", "application/envoy"},
            {".exe", "application/octet-stream"},
            {".exe.config", "text/xml"},
            {".fdf", "application/vnd.fdf"},
            {".fif", "application/fractals"},
            {".fla", "application/octet-stream"},
            {".flr", "x-world/x-vrml"},
            {".flv", "video/x-flv"},
            {".gif", "image/gif"},
            {".gtar", "application/x-gtar"},
            {".gz", "application/x-gzip"},
            {".h", "text/plain"},
            {".hdf", "application/x-hdf"},
            {".hdml", "text/x-hdml"},
            {".hhc", "application/x-oleobject"},
            {".hhk", "application/octet-stream"},
            {".hhp", "application/octet-stream"},
            {".hlp", "application/winhlp"},
            {".hqx", "application/mac-binhex40"},
            {".hta", "application/hta"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".htt", "text/webviewhtml"},
            {".hxt", "text/html"},
            {".ico", "image/x-icon"},
            {".ics", "application/octet-stream"},
            {".ief", "image/ief"},
            {".iii", "application/x-iphone"},
            {".inf", "application/octet-stream"},
            {".ins", "application/x-internet-signup"},
            {".isp", "application/x-internet-signup"},
            {".IVF", "video/x-ivf"},
            {".jar", "application/java-archive"},
            {".java", "application/octet-stream"},
            {".jck", "application/liquidmotion"},
            {".jcz", "application/liquidmotion"},
            {".jfif", "image/pjpeg"},
            {".jpb", "application/octet-stream"},
            {".jpe", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".jsx", "text/jscript"},
            {".latex", "application/x-latex"},
            {".lit", "application/x-ms-reader"},
            {".lpk", "application/octet-stream"},
            {".lsf", "video/x-la-asf"},
            {".lsx", "video/x-la-asf"},
            {".lzh", "application/octet-stream"},
            {".m13", "application/x-msmediaview"},
            {".m14", "application/x-msmediaview"},
            {".m1v", "video/mpeg"},
            {".m3u", "audio/x-mpegurl"},
            {".man", "application/x-troff-man"},
            {".manifest", "application/x-ms-manifest"},
            {".map", "text/plain"},
            {".mdb", "application/x-msaccess"},
            {".mdp", "application/octet-stream"},
            {".me", "application/x-troff-me"},
            {".mht", "message/rfc822"},
            {".mhtml", "message/rfc822"},
            {".mid", "audio/mid"},
            {".midi", "audio/mid"},
            {".mix", "application/octet-stream"},
            {".mmf", "application/x-smaf"},
            {".mno", "text/xml"},
            {".mny", "application/x-msmoney"},
            {".mov", "video/quicktime"},
            {".movie", "video/x-sgi-movie"},
            {".mp2", "video/mpeg"},
            {".mp3", "audio/mpeg"},
            {".mpa", "video/mpeg"},
            {".mpe", "video/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".mpp", "application/vnd.ms-project"},
            {".mpv2", "video/mpeg"},
            {".ms", "application/x-troff-ms"},
            {".msi", "application/octet-stream"},
            {".mso", "application/octet-stream"},
            {".mvb", "application/x-msmediaview"},
            {".mvc", "application/x-miva-compiled"},
            {".nc", "application/x-netcdf"},
            {".nsc", "video/x-ms-asf"},
            {".nws", "message/rfc822"},
            {".ocx", "application/octet-stream"},
            {".oda", "application/oda"},
            {".odc", "text/x-ms-odc"},
            {".ods", "application/oleobject"},
            {".one", "application/onenote"},
            {".onea", "application/onenote"},
            {".onetoc", "application/onenote"},
            {".onetoc2", "application/onenote"},
            {".onetmp", "application/onenote"},
            {".onepkg", "application/onenote"},
            {".osdx", "application/opensearchdescription+xml"},
            {".p10", "application/pkcs10"},
            {".p12", "application/x-pkcs12"},
            {".p7b", "application/x-pkcs7-certificates"},
            {".p7c", "application/pkcs7-mime"},
            {".p7m", "application/pkcs7-mime"},
            {".p7r", "application/x-pkcs7-certreqresp"},
            {".p7s", "application/pkcs7-signature"},
            {".pbm", "image/x-portable-bitmap"},
            {".pcx", "application/octet-stream"},
            {".pcz", "application/octet-stream"},
            {".pdf", "application/pdf"},
            {".pfb", "application/octet-stream"},
            {".pfm", "application/octet-stream"},
            {".pfx", "application/x-pkcs12"},
            {".pgm", "image/x-portable-graymap"},
            {".pko", "application/vnd.ms-pki.pko"},
            {".pma", "application/x-perfmon"},
            {".pmc", "application/x-perfmon"},
            {".pml", "application/x-perfmon"},
            {".pmr", "application/x-perfmon"},
            {".pmw", "application/x-perfmon"},
            {".png", "image/png"},
            {".pnm", "image/x-portable-anymap"},
            {".pnz", "image/png"},
            {".pot", "application/vnd.ms-powerpoint"},
            {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
            {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
            {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
            {".ppm", "image/x-portable-pixmap"},
            {".pps", "application/vnd.ms-powerpoint"},
            {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
            {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".prf", "application/pics-rules"},
            {".prm", "application/octet-stream"},
            {".prx", "application/octet-stream"},
            {".ps", "application/postscript"},
            {".psd", "application/octet-stream"},
            {".psm", "application/octet-stream"},
            {".psp", "application/octet-stream"},
            {".pub", "application/x-mspublisher"},
            {".qt", "video/quicktime"},
            {".qtl", "application/x-quicktimeplayer"},
            {".qxd", "application/octet-stream"},
            {".ra", "audio/x-pn-realaudio"},
            {".ram", "audio/x-pn-realaudio"},
            {".rar", "application/octet-stream"},
            {".ras", "image/x-cmu-raster"},
            {".rf", "image/vnd.rn-realflash"},
            {".rgb", "image/x-rgb"},
            {".rm", "application/vnd.rn-realmedia"},
            {".rmi", "audio/mid"},
            {".roff", "application/x-troff"},
            {".rpm", "audio/x-pn-realaudio-plugin"},
            {".rtf", "application/rtf"},
            {".rtx", "text/richtext"},
            {".scd", "application/x-msschedule"},
            {".sct", "text/scriptlet"},
            {".sea", "application/octet-stream"},
            {".setpay", "application/set-payment-initiation"},
            {".setreg", "application/set-registration-initiation"},
            {".sgml", "text/sgml"},
            {".sh", "application/x-sh"},
            {".shar", "application/x-shar"},
            {".sit", "application/x-stuffit"},
            {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
            {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
            {".smd", "audio/x-smd"},
            {".smi", "application/octet-stream"},
            {".smx", "audio/x-smd"},
            {".smz", "audio/x-smd"},
            {".snd", "audio/basic"},
            {".snp", "application/octet-stream"},
            {".spc", "application/x-pkcs7-certificates"},
            {".spl", "application/futuresplash"},
            {".src", "application/x-wais-source"},
            {".ssm", "application/streamingmedia"},
            {".sst", "application/vnd.ms-pki.certstore"},
            {".stl", "application/vnd.ms-pki.stl"},
            {".sv4cpio", "application/x-sv4cpio"},
            {".sv4crc", "application/x-sv4crc"},
            {".swf", "application/x-shockwave-flash"},
            {".t", "application/x-troff"},
            {".tar", "application/x-tar"},
            {".tcl", "application/x-tcl"},
            {".tex", "application/x-tex"},
            {".texi", "application/x-texinfo"},
            {".texinfo", "application/x-texinfo"},
            {".tgz", "application/x-compressed"},
            {".thmx", "application/vnd.ms-officetheme"},
            {".thn", "application/octet-stream"},
            {".tif", "image/tiff"},
            {".tiff", "image/tiff"},
            {".toc", "application/octet-stream"},
            {".tr", "application/x-troff"},
            {".trm", "application/x-msterminal"},
            {".tsv", "text/tab-separated-values"},
            {".ttf", "application/octet-stream"},
            {".txt", "text/plain"},
            {".u32", "application/octet-stream"},
            {".uls", "text/iuls"},
            {".ustar", "application/x-ustar"},
            {".vbs", "text/vbscript"},
            {".vcf", "text/x-vcard"},
            {".vcs", "text/plain"},
            {".vdx", "application/vnd.ms-visio.viewer"},
            {".vml", "text/xml"},
            {".vsd", "application/vnd.visio"},
            {".vss", "application/vnd.visio"},
            {".vst", "application/vnd.visio"},
            {".vsto", "application/x-ms-vsto"},
            {".vsw", "application/vnd.visio"},
            {".vsx", "application/vnd.visio"},
            {".vtx", "application/vnd.visio"},
            {".wav", "audio/wav"},
            {".wax", "audio/x-ms-wax"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wcm", "application/vnd.ms-works"},
            {".wdb", "application/vnd.ms-works"},
            {".wks", "application/vnd.ms-works"},
            {".wm", "video/x-ms-wm"},
            {".wma", "audio/x-ms-wma"},
            {".wmd", "application/x-ms-wmd"},
            {".wmf", "application/x-msmetafile"},
            {".wml", "text/vnd.wap.wml"},
            {".wmlc", "application/vnd.wap.wmlc"},
            {".wmls", "text/vnd.wap.wmlscript"},
            {".wmlsc", "application/vnd.wap.wmlscriptc"},
            {".wmp", "video/x-ms-wmp"},
            {".wmv", "video/x-ms-wmv"},
            {".wmx", "video/x-ms-wmx"},
            {".wmz", "application/x-ms-wmz"},
            {".wps", "application/vnd.ms-works"},
            {".wri", "application/x-mswrite"},
            {".wrl", "x-world/x-vrml"},
            {".wrz", "x-world/x-vrml"},
            {".wsdl", "text/xml"},
            {".wvx", "video/x-ms-wvx"},
            {".x", "application/directx"},
            {".xaf", "x-world/x-vrml"},
            {".xaml", "application/xaml+xml"},
            {".xap", "application/x-silverlight-app"},
            {".xbap", "application/x-ms-xbap"},
            {".xbm", "image/x-xbitmap"},
            {".xdr", "text/plain"},
            {".xla", "application/vnd.ms-excel"},
            {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
            {".xlc", "application/vnd.ms-excel"},
            {".xlm", "application/vnd.ms-excel"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
            {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".xlt", "application/vnd.ms-excel"},
            {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
            {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
            {".xlw", "application/vnd.ms-excel"},
            {".xml", "text/xml"},
            {".xof", "x-world/x-vrml"},
            {".xpm", "image/x-xpixmap"},
            {".xps", "application/vnd.ms-xpsdocument"},
            {".xsd", "text/xml"},
            {".xsf", "text/xml"},
            {".xsl", "text/xml"},
            {".xslt", "text/xml"},
            {".xsn", "application/octet-stream"},
            {".xtp", "application/octet-stream"},
            {".xwd", "image/x-xwindowdump"},
            {".z", "application/x-compress"},
            {".zip", "application/x-zip-compressed"}
        }
#if NET8_0_OR_GREATER
        .ToFrozenDictionary()
#endif  // NET8_0_OR_GREATER
        ;

        /// <summary>
        /// Get MIME type correspond to specified file name.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>MIME type string.</returns>
        public static string GetMimeType(string fileName)
        {
            return GetMimeType(fileName, FallbackMimeType);
        }

        /// <summary>
        /// Get MIME type correspond to specified file name.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="fallbackMimeType">Fallback MIME type.</param>
        /// <returns>MIME type string.</returns>
        public static string GetMimeType(string fileName, string fallbackMimeType)
        {
            string mimeType;
            if (_mimeTypeDict.TryGetValue(Path.GetExtension(fileName), out mimeType))
            {
                return mimeType;
            }
            else
            {
                return fallbackMimeType;
            }
        }
    }
#endif  // !USE_SYSTEM_WEB_MIME_MAPPING

#if USE_WIN32ICON_AS_FAVICON
    /// <summary>
    /// Utility class about icon extraction.
    /// </summary>
#if SUPPORT_LIBRARY_IMPORT
    public static partial class Win32IconHelper
#else
    public static class Win32IconHelper
#endif  // SUPPORT_LIBRARY_IMPORT
    {
        /// <summary>
        /// An application-defined callback function used
        /// with the <see cref="SafeNativeMethods.EnumResourceNames(IntPtr, IntPtr, EnumResourceNameProc, IntPtr)"/>
        /// and <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-enumresourcenamesexw">EnumResourceNamesEx</see> functions.
        /// It receives the type and name of a resource.
        /// The <see cref="EnumResourceNameProc"/> type defines a delegate of this callback method.
        /// EnumResNameProc is a placeholder for the application-defined function name.
        /// </summary>
        /// <param name="hModule">A handle to the module whose executable file contains the resources that are being enumerated.
        /// If this parameter is <see cref="IntPtr.Zero"/>, the function enumerates the resource names in the module used to create the current process.</param>
        /// <param name="lpType">The type of resource for which the name is being enumerated.
        /// Alternately, rather than a pointer, this parameter can be <c>MAKEINTRESOURCE(ID)</c>, where ID is an integer value representing a predefined resource type.
        /// For standard resource types, see <see href="https://learn.microsoft.com/en-us/windows/win32/menurc/resource-types">Resource Types</see>.
        /// For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nc-libloaderapi-enumresnameprocw#remarks">Remarks</see> section.</param>
        /// <param name="lpName">The name of a resource of the type being enumerated.
        /// Alternately, rather than a pointer, this parameter can be <c>MAKEINTRESOURCE(ID)</c>, where ID is the integer identifier of the resource.
        /// For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nc-libloaderapi-enumresnameprocw#remarks">Remarks</see> section.</param>
        /// <param name="lParam">An application-defined parameter
        /// passed to the <see cref="SafeNativeMethods.EnumResourceNames(IntPtr, IntPtr, EnumResourceNameProc, IntPtr)"/>
        /// or <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-enumresourcenamesexw">EnumResourceNamesEx</see> function.
        /// This parameter can be used in error checking.</param>
        /// <returns>Returns true to continue enumeration or false to stop enumeration.</returns>
        /// <remarks>
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nc-libloaderapi-enumresnameprocw"/>
        /// </remarks>
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        internal delegate bool EnumResourceNameProc(IntPtr hModule, IntPtr lpType, IntPtr lpName, IntPtr lParam);

        /// <summary>
        /// Flag values of the third argument of <see cref="SafeNativeMethods.LoadLibraryEx(string, IntPtr, LoadLibraryFlags)"/>.
        /// </summary>
        [Flags]
        internal enum LoadLibraryFlags : uint
        {
            /// <summary>
            /// If this value is used, the system maps the file into the calling process's virtual address space as if it were a data file.
            /// </summary>
            LoadLibraryAsDatafile = 0x00000002
        }
        /// <summary>
        /// Enum of resouce type.
        /// </summary>
        internal enum ResourceType
        {
            /// <summary>
            /// Hardware-dependent icon resource.
            /// </summary>
            Icon = 3,
            /// <summary>
            /// Hardware-independent icon resource.
            /// </summary>
            GroupIcon = 14
        }

        /// <summary>
        /// Get all icon data from specified file.
        /// </summary>
        /// <param name="filePath">A file path of executable file (.exe) or dynamic link file (.dll)</param>
        /// <returns>All icon data list.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null.</exception>
        /// <exception cref="Win32Exception">Thrown when any of Win32 API methods is failed.</exception>
        public static List<byte[]> GetAllIconData(string filePath)
        {
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(filePath);
#else
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
#endif  // NET7_0_OR_GREATER

            var hModule = IntPtr.Zero;
            try
            {
                hModule = SafeNativeMethods.LoadLibraryEx(filePath, IntPtr.Zero, LoadLibraryFlags.LoadLibraryAsDatafile);
                if (hModule == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                // Enumerate the icon resource and build .ico files in memory.
                var iconDataList = new List<byte[]>();

                SafeNativeMethods.EnumResourceNames(hModule, (IntPtr)ResourceType.GroupIcon, (hModule2, lpType, lpName, lParam) =>
                {
                    // Refer to the following URL for the data structures used here:
                    // http://msdn.microsoft.com/en-us/library/ms997538.aspx

                    // RT_GROUP_ICON resource consists of a GRPICONDIR and GRPICONDIRENTRY's.
                    var dir = GetDataFromResource(hModule2, (IntPtr)ResourceType.GroupIcon, lpName);

                    // Calculate the size of an entire .icon file.
                    var count = (int)BitConverter.ToUInt16(dir, 4);  // GRPICONDIR.idCount
                    var len = 6 + 16 * count;  // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count
                    for (int i = 0; i < count; i++)
                    {
                        len += BitConverter.ToInt32(dir, 6 + 14 * i + 8);  // GRPICONDIRENTRY.dwBytesInRes
                    }

                    using (var dst = new BinaryWriter(new MemoryStream(len)))
                    {
                        // Copy GRPICONDIR to ICONDIR.
                        dst.Write(dir, 0, 6);

                        var picOffset = 6 + 16 * count;  // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count

                        for (int i = 0; i < count; i++)
                        {
                            //
                            // Load the picture.
                            //
                            var id = BitConverter.ToUInt16(dir, 6 + 14 * i + 12);  // GRPICONDIRENTRY.nID
                            var pic = GetDataFromResource(hModule2, (IntPtr)ResourceType.Icon, (IntPtr)id);
                            //
                            // Copy GRPICONDIRENTRY to ICONDIRENTRY.
                            //
                            dst.Seek(6 + 16 * i, SeekOrigin.Begin);
                            dst.Write(dir, 6 + 14 * i, 8);  // First 8bytes are identical.
                            dst.Write(pic.Length);  // ICONDIRENTRY.dwBytesInRes
                            dst.Write(picOffset);  // ICONDIRENTRY.dwImageOffset
                            //
                            // Copy a picture.
                            //
                            dst.Seek(picOffset, SeekOrigin.Begin);
                            dst.Write(pic, 0, pic.Length);

                            picOffset += pic.Length;
                        }

                        iconDataList.Add(((MemoryStream)dst.BaseStream).ToArray());
                    }

                    return true;
                }, IntPtr.Zero);

                return iconDataList;
            }
            finally
            {
                if (hModule != IntPtr.Zero)
                {
                    SafeNativeMethods.FreeLibrary(hModule);
                }
            }
        }

        /// <summary>
        /// Get data from resource in module.
        /// </summary>
        /// <param name="hModule">Target module.</param>
        /// <param name="type">The resouce type.</param>
        /// <param name="id">The resource ID.</param>
        /// <returns><see cref="byte"/> array of the resouce data.</returns>
        /// <exception cref="Win32Exception">Thrown when any of Win32 API methods is failed.</exception>
        private static byte[] GetDataFromResource(IntPtr hModule, IntPtr type, IntPtr id)
        {
            var hResInfo = SafeNativeMethods.FindResource(hModule, id, type);
            if (hResInfo == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            var hResData = SafeNativeMethods.LoadResource(hModule, hResInfo);
            if (hResData == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            var pResData = SafeNativeMethods.LockResource(hResData);
            if (pResData == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            var size = SafeNativeMethods.SizeofResource(hModule, hResInfo);
            if (size == 0)
            {
                throw new Win32Exception();
            }

            var buf = new byte[size];
            Marshal.Copy(pResData, buf, 0, buf.Length);

            return buf;
        }

        /// <summary>
        /// Provides some native methods.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
#if SUPPORT_LIBRARY_IMPORT
        internal static partial class SafeNativeMethods
#else
        internal static class SafeNativeMethods
#endif  // SUPPORT_LIBRARY_IMPORT
        {
            /// <summary>
            /// Loads the specified module into the address space of the calling process.
            /// The specified module may cause other modules to be loaded.
            /// </summary>
            /// <param name="fileName">A string that specifies the file name of the module to load.
            /// This name is not related to the name stored in a library module itself,
            /// as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
            /// <param name="hFile">This parameter is reserved for future use. It must be <see cref="IntPtr.Zero"/>.</param>
            /// <param name="dwFlags">The action to be taken when loading the module.
            /// If no flags are specified, the behavior of this function is identical to that of the LoadLibrary function.
            /// This parameter can be one of the values shown <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-loadlibraryexw">here</see>.</param>
            /// <returns>
            /// <para>If the function succeeds, the return value is a handle to the loaded module.</para>
            /// <para>If the function fails, the return value is <see cref="IntPtr.Zero"/>.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-loadlibraryexw"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
            public static partial IntPtr LoadLibraryEx(string fileName, IntPtr hFile, LoadLibraryFlags dwFlags);
#else
            [DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr LoadLibraryEx(string fileName, IntPtr hFile, LoadLibraryFlags dwFlags);
#endif  // SUPPORT_LIBRARY_IMPORT

            /// <summary>
            /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.
            /// When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid.
            /// </summary>
            /// <param name="hModule">A handle to the loaded library module.
            /// The <see href="https://learn.microsoft.com/en-us/windows/desktop/api/libloaderapi/nf-libloaderapi-loadlibraryw">LoadLibrary</see>,
            /// <see cref="LoadLibraryEx"/>,
            /// <see href="https://learn.microsoft.com/en-us/windows/desktop/api/libloaderapi/nf-libloaderapi-getmodulehandlew">GetModuleHandle</see>,
            /// or <see href="https://learn.microsoft.com/en-us/windows/desktop/api/libloaderapi/nf-libloaderapi-getmodulehandleexw">GetModuleHandleEx</see> function returns this handle.</param>
            /// <returns>
            /// <para>If the function succeeds, the return value is true.</para>
            /// <para>If the function fails, the return value is false.
            /// To get extended error information, call the <see cref="Marshal.GetLastWin32Error"/> function.</para>
            /// </returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-freelibrary"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool FreeLibrary(IntPtr hModule);
#else
            [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", ExactSpelling = true, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeLibrary(IntPtr hModule);
#endif  // SUPPORT_LIBRARY_IMPORT

            /// <summary>
            /// Enumerates resources of a specified type within a binary module.
            /// For Windows Vista and later, this is typically a <see href="https://learn.microsoft.com/en-us/windows/win32/intl/mui-resource-management">language-neutral Portable Executable</see> (LN file),
            /// and the enumeration will also include resources from the corresponding language-specific resource files (.mui files)
            /// that contain localizable language resources.
            /// It is also possible for hModule to specify an .mui file, in which case only that file is searched for resources.
            /// </summary>
            /// <param name="hModule">
            /// <para>A handle to a module to be searched.
            /// Starting with Windows Vista, if this is an LN file, then appropriate .mui files (if any exist) are included in the search.</para>
            /// <para>If this parameter is <see cref="IntPtr.Zero"/>, that is equivalent to passing in a handle to the module used to create the current process.</para>
            /// </param>
            /// <param name="lpType">The type of the resource for which the name is being enumerated.
            /// Alternately, rather than a pointer, this parameter can be <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-makeintresourcew">MAKEINTRESOURCE</see>(ID),
            /// where ID is an integer value representing a predefined resource type.
            /// For a list of predefined resource types, see <see href="https://learn.microsoft.com/en-us/windows/win32/menurc/resource-types">Resource Types</see>.
            /// For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-enumresourcenamesw#remarks">Remarks</see> section.</param>
            /// <param name="enumFunc">A delegate of the callback function to be called for each enumerated resource name or ID.
            /// For more information, see <see cref="EnumResourceNameProc"/>.</param>
            /// <param name="lParam">An application-defined value passed to the callback function.
            /// This parameter can be used in error checking.</param>
            /// <returns>The return value is true if the function succeeds or false if the function does not find a resource of the type specified,
            /// or if the function fails for another reason.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-enumresourcenamesw"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = "EnumResourceNamesW", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool EnumResourceNames(IntPtr hModule, IntPtr lpType, EnumResourceNameProc enumFunc, IntPtr lParam);
#else
            [DllImport("kernel32.dll", EntryPoint = "EnumResourceNamesW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpType, EnumResourceNameProc enumFunc, IntPtr lParam);
#endif  // SUPPORT_LIBRARY_IMPORT

            /// <summary>
            /// <para>Determines the location of a resource with the specified type and name in the specified module.</para>
            /// <para>To specify a language, use the <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-findresourceexw">FindResourceEx</see> function.</para>
            /// </summary>
            /// <param name="hModule">A handle to the module whose portable executable file or an accompanying MUI file contains the resource.
            /// If this parameter is <see cref="IntPtr.Zero"/>, the function searches the module used to create the current process.</param>
            /// <param name="lpName">The resource type.
            /// Alternately, rather than a pointer, this parameter can be <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-makeintresourcew">MAKEINTRESOURCE</see>(ID),
            /// where ID is the integer identifier of the given resource type.
            /// For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-findresourceexw#remarks">Remarks</see> section.</param>
            /// <param name="lpType">
            /// <para>The resource type.
            /// Alternately, rather than a pointer, this parameter can be <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-makeintresourcew">MAKEINTRESOURCE</see>(ID),
            /// where ID is the integer identifier of the given</para>
            /// <para>resource type.
            /// For standard resource types, see <see href="https://learn.microsoft.com/en-us/windows/win32/menurc/resource-types">Resource Types</see>.
            /// For more information, see the <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-findresourceexw#remarks">Remarks</see> section.</para>
            /// </param>
            /// <returns>
            /// <para>If the function succeeds, the return value is a handle to the specified resource's information block.
            /// To obtain a handle to the resource, pass this handle to the <see cref="LoadResource(IntPtr, IntPtr)"/> function.</para>
            /// <para>If the function fails, the return value is <see cref="IntPtr.Zero"/>.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-findresourcew"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = "FindResourceW", SetLastError = true)]
            public static partial IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);
#else
            [DllImport("kernel32.dll", EntryPoint = "FindResourceW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);
#endif  // SUPPORT_LIBRARY_IMPORT

            /// <summary>
            /// Retrieves a handle that can be used to obtain a pointer to the first byte of the specified resource in memory.
            /// </summary>
            /// <param name="hModule">A handle to the module whose executable file contains the resource.
            /// If hModule is <see cref="IntPtr.Zero"/>, the system loads the resource from the module that was used to create the current process.</param>
            /// <param name="hResInfo">A handle to the resource to be loaded. This handle is returned
            /// by the <see cref="FindResource(IntPtr, IntPtr, IntPtr)"/>
            /// or <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-findresourceexw">FindResourceEx</see> function.</param>
            /// <returns>
            /// <para>If the function succeeds, the return value is a handle to the data associated with the resource.</para>
            /// <para>If the function fails, the return value is <see cref="IntPtr.Zero"/>.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-loadresource"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = "LoadResource", SetLastError = true)]
            public static partial IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
#else
            [DllImport("kernel32.dll", EntryPoint = "LoadResource", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
#endif  // SUPPORT_LIBRARY_IMPORT

            /// <summary>
            /// Retrieves a pointer to the specified resource in memory.
            /// </summary>
            /// <param name="hResData">A handle to the resource to be accessed.
            /// The <see cref="LoadResource(IntPtr, IntPtr)"/> function returns this handle.
            /// Note that this parameter is listed as an HGLOBAL variable only for backward compatibility.
            /// Do not pass any value as a parameter other than a successful return value from the <see cref="LoadResource(IntPtr, IntPtr)"/> function.</param>
            /// <returns></returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-lockresource"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = "LockResource", SetLastError = true)]
            public static partial IntPtr LockResource(IntPtr hResData);
#else
            [DllImport("kernel32.dll", EntryPoint = "LockResource", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr LockResource(IntPtr hResData);
#endif  // SUPPORT_LIBRARY_IMPORT

            /// <summary>
            /// Retrieves the size, in bytes, of the specified resource.
            /// </summary>
            /// <param name="hModule">A handle to the module whose executable file contains the resource.
            /// Default is the module used to create the current process.</param>
            /// <param name="hResInfo">A handle to the resource.
            /// This handle must be created by using the <see cref="FindResource(IntPtr, IntPtr, IntPtr)"/>
            /// or <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-findresourceexw">FindResourceEx</see> function.</param>
            /// <returns>
            /// <para>If the function succeeds, the return value is the number of bytes in the resource.</para>
            /// <para>If the function fails, the return value is zero.
            /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</para>
            /// </returns>
            /// <remarks>
            /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-sizeofresource"/>
            /// </remarks>
#if SUPPORT_LIBRARY_IMPORT
            [LibraryImport("kernel32.dll", EntryPoint = "SizeofResource", SetLastError = true)]
            public static partial uint SizeofResource(IntPtr hModule, IntPtr hResInfo);
#else
            [DllImport("kernel32.dll", EntryPoint = "SizeofResource", ExactSpelling = true, SetLastError = true)]
            public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);
#endif  // SUPPORT_LIBRARY_IMPORT
        }
    }
#endif  // USE_WIN32ICON_AS_FAVICON

#if (USE_WIN32ICON_AS_FAVICON || USE_EMBEDDED_ICON_AS_FAVICON) && (NETFRAMEWORK || WINDOWS)
    /// <summary>
    /// Provides some <see cref="Regex"/> instances.
    /// </summary>
#if SUPPORT_GENERATED_REGEX
    internal static partial class RegexProvider
#else
    internal static class RegexProvider
#endif  // SUPPORT_GENERATED_REGEX
    {
        /// <summary>
        /// Options for <see cref="Regex"/> instances.
        /// </summary>
        private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        /// <summary>
        /// <see cref="Regex"/> pattern <see cref="string"/> matching suffix of well-known apple-touch-icon file name.
        /// </summary>
        internal const string AppleTouchIconSuffixPattern = @"^(?:-(\d+)x(\d+))?(?:-precomposed)?\.png$";

        /// <summary>
        /// <see cref="Regex"/> instance matching suffix of well-known apple-touch-icon file name.
        /// </summary>
#if SUPPORT_GENERATED_REGEX_PROPERTY
        [GeneratedRegex(AppleTouchIconSuffixPattern, Options)]
        public static partial Regex AppleTouchIconSuffixRegex { get; }
#elif SUPPORT_GENERATED_REGEX
        public static Regex AppleTouchIconSuffixRegex => GetAppleTouchIconSuffixRegex();
        /// <summary>
        /// Get <see cref="Regex"/> instance matching suffix of well-known apple-touch-icon file name.
        /// </summary>
        /// <returns><see cref="Regex"/> instance matching suffix of well-known apple-touch-icon file name.</returns>
        [GeneratedRegex(AppleTouchIconSuffixPattern, Options)]
        private static partial Regex GetAppleTouchIconSuffixRegex();
#else
        public static Regex AppleTouchIconSuffixRegex { get { return  _appleTouchIconSuffixRegex ?? (_appleTouchIconSuffixRegex = new Regex(AppleTouchIconSuffixPattern, Options)); } }
        /// <summary>
        /// Backing field of <see cref="AppleTouchIconSuffixRegex"/>.
        /// </summary>
        private static Regex _appleTouchIconSuffixRegex;
#endif  // SUPPORT_GENERATED_REGEX_PROPERTY
    }
#endif  // (USE_WIN32ICON_AS_FAVICON || USE_EMBEDDED_ICON_AS_FAVICON) && (NETFRAMEWORK || WINDOWS)
}
