using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.Win32;

namespace IconPing
{
    public class IconPing : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private int updateCount = 0;

        private System.Timers.Timer pingTimer;

        private MenuItem timer_1000;
        private MenuItem timer_5000;
        private MenuItem timer_30000;
        private MenuItem startUp;

        Icon
            goodIcon = Properties.Resources.iconok,
            slowIcon = Properties.Resources.iconslow,
            badIcon = Properties.Resources.iconko;

        public IconPing()
        {
            trayMenu = new ContextMenu();

            trayMenu.MenuItems.Add("Update time:", OnExit).Enabled = false;

            timer_1000 = trayMenu.MenuItems.Add("1 second", PingTimer_1000);
            timer_5000 = trayMenu.MenuItems.Add("5 seconds", PingTimer_5000);
            timer_30000 = trayMenu.MenuItems.Add("30 seconds", PingTimer_30000);

            timer_5000.Checked = true;

            trayMenu.MenuItems.Add("-");
            startUp = trayMenu.MenuItems.Add("Open at startup", SetStartup);
            startUp.Checked = CheckStartup();

            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "IconPing";
            trayIcon.Icon = Properties.Resources.world_icon;

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            PingTimer(this, null);

            pingTimer = new System.Timers.Timer();
            pingTimer.Elapsed += new ElapsedEventHandler(PingTimer);
            pingTimer.Interval = 5000;
            pingTimer.Enabled = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void PingTimer_1000(object sender, EventArgs e)
        {
            timer_1000.Checked = true;
            timer_5000.Checked = false;
            timer_30000.Checked = false;

            pingTimer.Interval = 1000;
        }

        private void PingTimer_5000(object sender, EventArgs e)
        {
            timer_1000.Checked = false;
            timer_5000.Checked = true;
            timer_30000.Checked = false;

            pingTimer.Interval = 5000;
        }

        private void PingTimer_30000(object sender, EventArgs e)
        {
            timer_1000.Checked = false;
            timer_5000.Checked = false;
            timer_30000.Checked = true;

            pingTimer.Interval = 30000;
        }

        private void SetStartup(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk != null)
            {
                string val = (string) rk.GetValue("IconPing");
                if (val == null)
                {
                    startUp.Checked = true;
                    rk.SetValue("IconPing", Application.ExecutablePath.ToString());
                }
                else
                {
                    startUp.Checked = false;
                    rk.DeleteValue("IconPing", false);
                }
            }
            else
            {
                MessageBox.Show("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                                "Unhandled error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private bool CheckStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rk != null)
            {
                string val = (string)rk.GetValue("IconPing");
                if (val != null)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        private static Ping pingSender = new Ping();
        private static IPAddress address = IPAddress.Parse("8.8.8.8"); //Australian ping for testing: 139.130.4.5

        private void PingTimer(object source, ElapsedEventArgs e)
        {
            updateCount++;

            PingReply reply = pingSender.Send(address);

            if (reply.Status == IPStatus.Success)
            {
                trayIcon.Text = reply.RoundtripTime + " ms (" + updateCount + ")";

                if (reply.RoundtripTime >= 300)
                {
                    trayIcon.Icon = slowIcon;
                }
                else
                {
                    trayIcon.Icon = goodIcon;
                }
            }
            else
            {
                trayIcon.Text = "Network error (" + updateCount + ")";

                trayIcon.Icon = badIcon;
            }
        }
    }
}
