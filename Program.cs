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
        private System.IO.FileSystemWatcher watcher;

        private string TimerFileDirectoryPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), @".emacs.d");
        private string TimerFileName = ".task";

        private Regex clockString = new Regex(@"\s*\[(\d*):(\d*)\]\s*\((.*)\)");

        public OrgClockTrayProgram()
        {
            watcher = new System.IO.FileSystemWatcher();
            watcher.Path = TimerFileDirectoryPath;
            watcher.Filter = TimerFileName;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(onFileChanged);
            watcher.EnableRaisingEvents = true;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (OrgClockTrayProgram orgClock = new OrgClockTrayProgram())
            {
                orgClock.init();
                Application.Run();
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
            setIdleIcon();
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

        private void onFileChanged(object sender, FileSystemEventArgs e)
        {
            String toolTip = string.Empty;
            try
            {
                if (e.FullPath == Path.Combine(TimerFileDirectoryPath, TimerFileName))
                {
                    var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
                                String hourStr = m.Groups[1].Value;
                                String minStr = m.Groups[2].Value;
                                toolTip = m.Groups[3].Value; // clocked item

                                int min = int.Parse(minStr);
                                double minRounded = min / 60.0;
                                string timeStr;
                                if (hourStr.Length < 3)
                                {
                                    if (min > 0)
                                        timeStr = string.Format("{0}.{1}", hourStr, minRounded.ToString()[2]); 
                                    else
                                        timeStr = string.Format("{0}.0", hourStr);
                                }
                                else
                                    timeStr = hourStr;
                                setTextIcon(timeStr);
                            }
                        }
                    }
                }
            }
            catch(Exception oops)
            {
                toolTip = "Error: " + oops.Message;
            }
            setToolTip(toolTip);
        }

        public void setTextIcon(string str)
        {
            Font font = new Font("Arial", 9, FontStyle.Regular, GraphicsUnit.Pixel);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = System.Drawing.Graphics.FromImage(bitmapText);
            g.Clear(Color.FromArgb(30, 30, 30));
            int xOffset = -2;
            if (str.Length == 3 && str.Contains("."))
                xOffset = 0;
            Point p = new Point(xOffset, 2);
            TextRenderer.DrawText(g, str, font, p, Color.White);
            hIcon = (bitmapText.GetHicon());
            ni.Icon = System.Drawing.Icon.FromHandle(hIcon);
        }
    }
}
