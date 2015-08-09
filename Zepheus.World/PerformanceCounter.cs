using System;
using System.Threading;
using System.Diagnostics;
using Zepheus.Util;

namespace Zepheus.World
{
    public class PerformCounter
    {
        protected PerformanceCounter cpuCounter;
        protected PerformanceCounter ramCounter;
        protected PerformanceCounter performanceCounterSent;
        protected PerformanceCounter performanceCounterReceived;

        public PerformCounter()
        {
            SetupConsole();
            SetupCounters();
            Thread PerfomanceCounter = new Thread(new ThreadStart(SetConsoleTitel));
            PerfomanceCounter.Start();
        }
        public string getAvailableRAM(){
         return  ramCounter.NextValue().ToString("N2") + " Mb";
        }
        public string getCurrentCpuUsage() {
            return cpuCounter.NextValue().ToString("N2");
        }
        public void SetupCounters()
        {
            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
            string instance = performanceCounterCategory.GetInstanceNames()[0]; // 1st NIC !
            performanceCounterSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
            performanceCounterReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);

        }
        public void SetupConsole()
        {
            Console.WindowWidth = 90;
        }
        public void SetConsoleTitel()
        {
            Thread.Sleep(2000);
            while (true)
            {
                try
                {
                    string memory = getAvailableRAM();
                    string cpu = getCurrentCpuUsage();
                    Console.Title = "World[" + Settings.Instance.ID + "] TickPerSecont : " + Worker.Instance.TicksPerSecond + " Free Memory : " + memory + " CPU: " + cpu + " Network : bytes send: " + (performanceCounterSent.NextValue() / 1024).ToString("N2") + " bytes received: " + (performanceCounterReceived.NextValue() / 1024).ToString("N2") + " ";
                 
                }
                finally
                {
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
