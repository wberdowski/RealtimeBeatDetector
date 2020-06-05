using CSCore.SoundIn;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RealtimeBeatDetector.Source.Forms
{
    public partial class FormMain : Form
    {
        private readonly BeatDetector beatDetector;
        private readonly WasapiLoopbackCapture wasapi;

        public FormMain()
        {
            InitializeComponent();
            Application.ApplicationExit += Application_ApplicationExit;

            // Initialize BeatDetector
            beatDetector = new BeatDetector();
            beatDetector.BeatDetected += BeatDetector_BeatDetected;
            beatDetector.BpmDetected += BeatDetector_BpmDetected;

            // Initialize WASAPI
            try
            {
                wasapi = new WasapiLoopbackCapture();
                wasapi.DataAvailable += Wasapi_DataAvailable;
                wasapi.Initialize();
                wasapi.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nThe application will now exit.", "WASAPI error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }

            Timer t = new Timer();
            t.Tick += T_Tick;
            t.Interval = 16;
            t.Start();
        }

        private void BeatDetector_BpmDetected(object sender, BpmEventArgs e)
        {
            if (!IsDisposed && !Disposing)
            {
                bpmCounter.Invoke(new Action(() =>
                {
                    bpmCounter.Text = e.AvgBpm.ToString("#");
                }));
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(255, val, val, val);
            Invalidate();

            val = Math.Max(0, val - 40);
        }

        private int val = 0;

        private void BeatDetector_BeatDetected(object sender, EventArgs e)
        {
            val = 255;
        }

        private void Wasapi_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            beatDetector.ProcessBuffer(e.Data, e.ByteCount);
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            beatDetector.BeatDetected -= BeatDetector_BeatDetected;
            beatDetector.BpmDetected -= BeatDetector_BpmDetected;

            wasapi.DataAvailable -= Wasapi_DataAvailable;
            wasapi.Stop();
            wasapi.Dispose();
        }
    }
}
