using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

namespace OrgClockTray
{
    class OrgClockTrayProgram : IDisposable
    {
        private NotifyIcon ni = new NotifyIcon();
        private IntPtr hIcon;
        private FileSystemWatcher watcher;

        private static string timerFilePath;

        private Regex clockString = new Regex(@"\s*\[(\d*):(\d*)\]\s*\((.*)\)");

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
                    timerFilePath = args[0];
                else
                    timerFilePath = Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("HOME"), @".emacs.d"), ".task");

                // Create the file to be monitored first in case org-mode hasn't created it yet
                if (!File.Exists(timerFilePath))
                    File.Create(timerFilePath).Dispose();

                Console.WriteLine("Using file: {0}", timerFilePath);

                using (OrgClockTrayProgram orgClock = new OrgClockTrayProgram())
                {
                    orgClock.init();
                    Application.Run();
                }
            }
            catch(Exception oops)
            {
                Console.WriteLine(oops);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        public void init()
        {
            ni.Visible = true;
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = "Quit";
            item.Click += new System.EventHandler((sender, e) => Application.Exit());
            menu.Items.Add(item);
            ni.ContextMenuStrip = menu;

            watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(timerFilePath);
            watcher.Filter = Path.GetFileName(timerFilePath);
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) =>
            {
                if (e.FullPath.Equals(timerFilePath))
                    updateTime();
            });
            watcher.EnableRaisingEvents = true;

            updateTime();
        }

        public void Dispose()
        {
            DestroyIcon(hIcon);
            ni.Dispose();
        }

        private void setIdleIcon()
        {
            ni.Icon = System.Drawing.Icon.FromHandle(Resource.CoffeeBreak.GetHicon());
        }

        private void setToolTip(String text)
        {
            // notify icon's tool tip text must not exceed 63 characters
            ni.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
        }

        private void updateTime()
        {
            String toolTip = string.Empty;
            try
            {
                var fs = new FileStream(timerFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var r = new StreamReader(fs))
                {
                    string line = r.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        setIdleIcon();
                    else
                    {
                        Match m = clockString.Match(line);
                        if (m.Success)
                        {
                            toolTip = m.Groups[3].Value; // clocked item                                
                            String hourStr = m.Groups[1].Value;
                            String timeStr;
                            int xOffset;
                            if (hourStr.Length == 1)
                            {
                                String minStr = m.Groups[2].Value;
                                int min = int.Parse(minStr);
                                double min2 = min / 60.0;
                                timeStr = string.Format("{0}.{1}", hourStr, (min > 0 ? min2.ToString()[2].ToString() : "0"));
                                xOffset = 0;
                            }
                            else
                            {
                                timeStr = hourStr;
                                xOffset = hourStr.Length == 2 ? 2 : -1;
                            }
                            setTextIcon(timeStr, xOffset);
                        }
                    }
                }
            }
            catch (Exception oops)
            {
                toolTip = "Error: " + oops.Message;
            }
            setToolTip(toolTip);
        }

        public void setTextIcon(string str, int xOffset)
        {
            Font font = new Font("Arial", 9, FontStyle.Regular, GraphicsUnit.Pixel);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = System.Drawing.Graphics.FromImage(bitmapText);
            g.Clear(Color.FromArgb(30, 30, 30));
            Point p = new Point(xOffset, 2);
            TextRenderer.DrawText(g, str, font, p, Color.White);
            hIcon = (bitmapText.GetHicon());
            ni.Icon = System.Drawing.Icon.FromHandle(hIcon);
        }
    }
}
