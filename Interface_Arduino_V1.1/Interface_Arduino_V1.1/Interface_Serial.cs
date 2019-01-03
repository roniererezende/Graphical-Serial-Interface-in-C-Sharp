using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports; // Necessarário declarar para ter acesso a porta serial

namespace Interface_Arduino_V1._1
{
    public partial class frmInterfaceSerial : Form
    {
        // Váriaveis úteis 
        string rxString     = " ";  // Recebe os dados da porta serial
        string serialHeader = " ";  // Cabeçalho inserido no protocolo de dados 
        string serialString = " ";  // Dados úteis inseridos no protocolo de dados
        string lcdLength = " ";     // Comprimento da string a ser exibida no display de LCD
        int lengthSerialString = 0; // Comprimento da string a ser transmitda pela porta serial
        int countOut1       = 0;    //
        int countOut2       = 0;    //
        int countOut3       = 0;    // Variáveis utilizadas na manipulação dos botões de ativação das saídas digitais 
        int countInput      = 0;    // Variável utilizada na manipulação do botão da entrada digital
        int[] lengthLcdString = new int[3];

        // Função de configuração inicial da porta serial
        void InitSerialPort()
        {
            serialPort1.BaudRate  = Convert.ToInt32(cbBaudRate.Items[cbBaudRate.SelectedIndex]);
            serialPort1.DataBits  = Convert.ToInt32(cbDataBits.Items[cbDataBits.SelectedIndex]);
            serialPort1.RtsEnable = Convert.ToBoolean(cbRtsEnable.Items[cbRtsEnable.SelectedIndex]);
            serialPort1.Handshake = Handshake.None;
            serialPort1.Parity    = Parity.None;
            serialPort1.StopBits  = StopBits.One;
        }

        // Função de configuração inicial do form
        void initConfig()
        {
            tmrCom1.Enabled = true;

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(TreatReceiverData);

            cmdConnect.BackColor = Color.LightBlue;
            cmdConnect.ForeColor = Color.Black;

            cmdLcd.BackColor = Color.Green;
            cmdLcd.ForeColor = Color.White;

            cmdSerial.BackColor = Color.Green;
            cmdSerial.ForeColor = Color.White;

            cmdOutput1.BackColor = Color.Red;
            cmdOutput2.BackColor = Color.Yellow;
            cmdOutput3.BackColor = Color.GreenYellow;

            cmdIndicator.BackColor = Color.Red;

            tmrCom1.Interval = 750;

            // Valor inicial do baudrate em 9600 bps
            foreach (string x in cbBaudRate.Items)
            {
                if (x.Equals("9600"))
                {
                    cbBaudRate.SelectedIndex = cbBaudRate.Items.IndexOf(x);
                }
            }

            // Valor inicial do tamanho do pacote de dados em 8 bits
            foreach (string x in cbDataBits.Items)
            {
                if (x.Equals("8"))
                {
                    cbDataBits.SelectedIndex = cbDataBits.Items.IndexOf(x);
                }
            }

            // Valor de inicial do RTS (Request To Send) em "true", ou seja, ativado 
            foreach (string x in cbRtsEnable.Items)
            {
                if (x.Equals("True"))
                {
                    cbRtsEnable.SelectedIndex = cbRtsEnable.Items.IndexOf(x);
                }
            }
        }

        // Método principal
        public frmInterfaceSerial()
        {
            InitializeComponent();
            initConfig();
        }

        // Método que permite receber os dados da serial e manipular os controles
        public void SetTextBox(String text)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate () { SetTextBox(text); });
                return;
            }
            //txtDataReceiver.Clear();
            txtDataReceiver.Text = text;
            cmdSerial.Text = "SEND SERIAL";
            cmdSerial.ForeColor = Color.White;
        }

        // Atualiza a lista de portas seriais disponíveis
        private void updateListComs()
        {
            int i = 0;
            bool differentAmount = false;

            if(cbPort1.Items.Count == SerialPort.GetPortNames().Length)
            {
                foreach(string s in SerialPort.GetPortNames())
                {
                    if(cbPort1.Items[i++].Equals(s) == false)
                    {
                        differentAmount = true;
                    }
                }
            }
            else
            {
                differentAmount = true;
            }

            if(differentAmount == false)
            {
                return;
            }

            cbPort1.Items.Clear();

            foreach(string s in SerialPort.GetPortNames())
            {
                cbPort1.Items.Add(s);
            }

            cbPort1.SelectedIndex = 0;
        }

        // Método que habilita a comunicação com a porta serial 
        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cbBaudRate.SelectedIndex == -1 || cbDataBits.SelectedIndex == -1 || cbRtsEnable.SelectedIndex == -1 )
            {
                DialogResult message = MessageBox.Show("Empty Properties!\n Please, Insert a valid values",
                                                        "ATTENTION!",
                                                        MessageBoxButtons.OK, 
                                                        MessageBoxIcon.Exclamation); 
                return;
            }
            else
            {
                InitSerialPort();
                if (serialPort1.IsOpen == false)
                {
                    try
                    {
                        serialPort1.PortName = cbPort1.Items[cbPort1.SelectedIndex].ToString();
                        serialPort1.Open();
                    }
                    catch
                    {
                        serialPort1.Close();
                        return;
                    }

                    if (serialPort1.IsOpen)
                    {
                        cmdConnect.Text      = "DISCONNECT";   
                        cmdConnect.BackColor = Color.Blue;
                        cmdConnect.ForeColor = Color.White;
                        cbPort1.Enabled      = false;
                        cbBaudRate.Enabled   = false;
                        cbDataBits.Enabled   = false;
                        cbRtsEnable.Enabled  = false;

                        txtDataReceiver.Clear();
                    }
                }
                else
                {
                    try
                    {
                        serialPort1.Close();
                        cbPort1.Enabled        = true;
                        cbBaudRate.Enabled     = true;
                        cbDataBits.Enabled     = true;
                        cbRtsEnable.Enabled    = true;  
                        cmdConnect.BackColor   = Color.LightBlue;
                        cmdConnect.ForeColor   = Color.Black;
                        cmdLcd.BackColor       = Color.Green;
                        cmdLcd.ForeColor       = Color.White;
                        cmdSerial.BackColor    = Color.Green;
                        cmdSerial.ForeColor    = Color.White;
                        cmdIndicator.BackColor = Color.DarkRed;
                        cmdConnect.Text        = "CONNECT";
                        cmdLcd.Text            = "SEND LCD";
                        cmdSerial.Text         = "SEND SERIAL";
                    }
                    catch
                    {
                        serialPort1.Close();
                        return;
                    }

                }
            }
        }

        // Desabilita a comunicação serial quando o "Form" é fechado
        private void frmInterface_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(serialPort1.IsOpen == true)
            {
                serialPort1.Close();
            }
        }

        // Método que transmite os dados para serem escritos no LCD
        private void cmdLcd_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen == true)
            {
                cmdLcd.BackColor = Color.Yellow;
                cmdLcd.ForeColor = Color.Black;
                cmdLcd.Text = "SENDING...";

                lengthLcdString[0] = txtLCDData.TextLength / 100;
                lengthLcdString[1] = (txtLCDData.TextLength % 100) / 10;
                lengthLcdString[2] = (txtLCDData.TextLength % 100) % 10;
                lcdLength = lengthLcdString[0].ToString() + lengthLcdString[1].ToString() + lengthLcdString[2].ToString(); ;
                serialPort1.Write("LCD" + lcdLength + txtLCDData.Text);
            }
        }

        // Método que envia dados através da porta serial
        private void cmdSerial_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen == true)
            {
                cmdSerial.BackColor = Color.Yellow;
                cmdSerial.ForeColor = Color.Black;
                cmdSerial.Text = "SENDING...";
                lengthSerialString = txtSerialData.TextLength;
                serialPort1.Write("SER" + txtSerialData.Text);
            }
        }

        // Método que recebe os dados da porta serial
        private void TreatReceiverData(object sender, EventArgs e)
        {
            try
            {
                rxString = serialPort1.ReadExisting();
                serialHeader = rxString.Substring(0, 6);

                if (serialHeader != "INPUT ")
                {
                    if (serialHeader == "OUTPUT")
                    {
                        serialString = rxString.Substring(0, 13);
                        SetTextBox(serialString);
                    }
                    else
                    {
                        serialString = rxString.Substring(0, lengthSerialString);
                        SetTextBox(serialString);
                    }
                }
                cmdLcd.BackColor = Color.Green;
                cmdSerial.BackColor = Color.Green;
            }
            catch
            {
                return;
            }
            
        }

        // Gera eventos para atualizar a verificação das portas seriais disponíveis
        private void tmrCom1_Tick(object sender, EventArgs e)
        {
            updateListComs();
            try
            {
                cmdLcd.Text = "SEND LCD";
                cmdLcd.BackColor = Color.Green;
                cmdLcd.ForeColor = Color.White;

                if (serialPort1.IsOpen == true)
                {
                    //txtDataReceiver.Clear();
                    serialPort1.Write("INP");

                    if (rxString == "INPUT HIGH")
                    {
                        cmdIndicator.BackColor = Color.Green;
                    }
                    else
                    {
                        cmdIndicator.BackColor = Color.DarkRed;
                    }
                }
            }
            catch
            {
                serialPort1.Close();
                return;
            }
            
        }

        // Evento que enviar dados para se manipular a Saída Digital 1 
        private void cmdOutput1_Click(object sender, EventArgs e)
        {
            try
            {
                if (countOut1 == 0)
                {
                    txtDataReceiver.Clear();
                    serialPort1.Write("OUT01");
                    cmdOutput1.BackColor = Color.DarkRed;
                    cmdOutput1.ForeColor = Color.White;
                    countOut1 = 1;
                }
                else
                {
                    txtDataReceiver.Clear();
                    serialPort1.Write("OUT00");
                    cmdOutput1.BackColor = Color.Red;
                    cmdOutput1.ForeColor = Color.Black;
                    countOut1 = 0;
                }
            }
            catch
            {
                serialPort1.Close();
                return;
            }
            
        }

        //Evento que enviar dados para se manipular a Saída Digital 2
        private void cmdOutput2_Click(object sender, EventArgs e)
        {
            try
            {
                if (countOut2 == 0)
                {
                    txtDataReceiver.Clear();
                    serialPort1.Write("OUT11");
                    cmdOutput2.BackColor = Color.Orange;
                    countOut2 = 1;
                }
                else
                {
                    txtDataReceiver.Clear();
                    serialPort1.Write("OUT10");
                    cmdOutput2.BackColor = Color.Yellow;
                    countOut2 = 0;
                }
            }
            catch
            {
                serialPort1.Close();                
                return;
            }
            
        }

        // Evento que enviar dados para se manipular a Saída Digital 3
        private void cmdOutput3_Click(object sender, EventArgs e)
        {
            try
            {
                if (countOut3 == 0)
                {
                    txtDataReceiver.Clear();
                    serialPort1.Write("OUT21");
                    cmdOutput3.BackColor = Color.GreenYellow;
                    cmdOutput3.ForeColor = Color.Black;
                    countOut3 = 1;
                }
                else
                {
                    txtDataReceiver.Clear();
                    serialPort1.Write("OUT20");
                    cmdOutput3.BackColor = Color.DarkGreen;
                    cmdOutput3.ForeColor = Color.White;
                    countOut3 = 0;
                }
            }
            catch
            {
                serialPort1.Close();                
                return;
            }
        }
    }
}
