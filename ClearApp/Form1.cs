using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ClearApp
{
    public partial class Form1 : Form
    {
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private PerformanceCounter diskCounter;
        public Form1()
        {
            InitializeComponent();
            InitialiseCPUCounter();
            InitializeRAMCounter();
            InitializeDiskCounter();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            textBox3.Text = Convert.ToInt32(cpuCounter.NextValue()).ToString() + "%";
            textBox4.Text = Convert.ToInt32(ramCounter.NextValue()).ToString() + "Mb";
            textBox5.Text = Convert.ToInt32(diskCounter.NextValue()).ToString() + "%";
        }
        //Get Total CPU
        #region
        private void InitialiseCPUCounter()
        {
            cpuCounter = new PerformanceCounter(
            "Processor",
            "% Processor Time",
            "_Total",
            true
            );
        }
        #endregion
        //Get Total Ram
        #region
        private void InitializeRAMCounter()
        {
            ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
        }
        #endregion
        //Get Infor Disk
        #region
        private void InitializeDiskCounter()
        {
            diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
        }
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            //Load List App
            #region
            //Total Ram Physical
            ComputerInfo CI = new ComputerInfo();
            ulong mem = ulong.Parse(CI.TotalPhysicalMemory.ToString());
            textBox1.Text = (mem / (1024 * 1024) + " MB").ToString();
            numericUpDown2.Maximum = Convert.ToInt32(mem / (1024 * 1024));


            #endregion
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (System.Windows.MessageBox.Show("Bạn có muốn đóng ứng dụng & chạy ngầm", "Thông Báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {


                this.Hide();
                notifyIcon1.Visible = true;
            }
            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            this.Show();
            WindowState = FormWindowState.Normal;
        }
        DispatcherTimer timerstart = new DispatcherTimer();
        DispatcherTimer timerload = new DispatcherTimer();
        List<getlist> dsprocess;
        private void button1_Click(object sender, EventArgs e)
        {
            if(listView1.Items.Count>1)
            {
                timerstart.Interval = TimeSpan.FromSeconds(1);
                timerstart.Tick += Timerstart_Tick; ;
                timerstart.Start();
            }    
            else
            {
                System.Windows.MessageBox.Show("App Clear lớn hơn 1", "Thông Báo");
            }    
        }

        private void Timerstart_Tick(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 1)
            {
                Process[] processList = Process.GetProcesses();
                foreach (Process process in processList)
                {
                    if (process.ProcessName.Equals(textBox2.Text) && BytesToReadableValues(process.PrivateMemorySize64) >= numericUpDown2.Value)
                    {
                        Process.GetProcessById(process.Id).Kill();
                    }
                }
            }

        }
        //Convert
        public string BytesToReadableValue(long number)
        {
            List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < suffixes.Count; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return (number / (int)Math.Pow(1024, i)) + suffixes[i];
                }
            }

            return number.ToString();
        }
        public decimal BytesToReadableValues(long number)
        {
            //List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < 6; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return (number / (int)Math.Pow(1024, i)) ;
                }
            }

            return number;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            timerstart.Stop();
        }
        public class getlist
        {
            public int stt { get; set; }
            public string name { get; set; }
            public int id { get; set; }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                System.Windows.MessageBox.Show("DisplayName App không được trống","Thông Báo");
            }
            else
            {
                timerload.Interval = TimeSpan.FromSeconds(1);
                timerload.Tick += Timerload_Tick;
                timerload.Start();
            }    
            
        }

        private void Timerload_Tick(object sender, EventArgs e)
        {
            Process[] processList = Process.GetProcesses();
            listView1.Items.Clear();
            foreach (Process process in processList)
            {
                if (process.ProcessName.Equals(textBox2.Text))
                {
                    var counter = GetPerfCounterForProcessId(process.Id);
                    // start capturing
                    counter.NextValue();
                    Thread.Sleep(200);
                    var cpu = counter.NextValue() / (float)Environment.ProcessorCount;
                    string[] row = {
                    process.Id.ToString(),
                    process.ProcessName,
                    BytesToReadableValue(process.PrivateMemorySize64),
                    cpu.ToString()
                 };
                    ListViewItem item = new ListViewItem(row);
                    listView1.Items.Add(item);
                }
            }
        }
        //Get Cpu in Application
        #region
        public static PerformanceCounter GetPerfCounterForProcessId(int processId, string processCounterName = "% Processor Time")
        {
            string instance = GetInstanceNameForProcessId(processId);
            if (string.IsNullOrEmpty(instance))
                return null;

            return new PerformanceCounter("Process", processCounterName, instance);
        }

        public static string GetInstanceNameForProcessId(int processId)
        {
            var process = Process.GetProcessById(processId);
            string processName = Path.GetFileNameWithoutExtension(process.ProcessName);

            PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");
            string[] instances = cat.GetInstanceNames()
                .Where(inst => inst.StartsWith(processName))
                .ToArray();

            foreach (string instance in instances)
            {
                using (PerformanceCounter cnt = new PerformanceCounter("Process",
                    "ID Process", instance, true))
                {
                    int val = (int)cnt.RawValue;
                    if (val == processId)
                    {
                        return instance;
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
