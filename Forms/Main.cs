using System;
using System.Net;
using System.Windows.Forms;

namespace RConWin
{
    public partial class Main : Form
    {
        protected RCON rcon = new RCON();
        protected bool connected = false;

        public Main()
        {
            InitializeComponent();
        }

        protected void btn1()
        {
            if (!string.IsNullOrEmpty(cmd.Text.Trim()))
            {
                string command = cmd.Text.Trim();
                cmd.Text = "";

                if (command.Split(' ')[0] == "--connect")
                {
                    int port = 0;

                    try
                    {
                        port = int.Parse(command.Split(' ')[1].Split(':')[1]);
                        connected = rcon.Connect(new IPEndPoint(IPAddress.Parse(command.Split(' ')[1].Split(':')[0]), port), command.Split(' ')[2]);
                    }
                    catch
                    {
                        MessageBox.Show("Error: incorrect ip, port or rcon-password.", "Admin Console", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (connected)
                        MessageBox.Show("Connect: OK!", "Admin Console", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Error: incorrect ip, port or rcon-password.", "Admin Console", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (command == "--disconnect")
                {
                    output.Text = "";
                    try { rcon.Disconnect(); } catch { }
                }
                else
                {
					try
					{
						string result = rcon.RSendCommand(command.Trim());

						if (!string.IsNullOrEmpty(result))
						{
							result = (output.Text.Trim() + "\n\n" + result.Trim()).Trim().Replace("\n", Environment.NewLine);

							output.AppendText(result);
						}
					}
					catch { }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btn1();
        }

        private void cleanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            output.Text = "";
        }

        private void cmd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn1();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
