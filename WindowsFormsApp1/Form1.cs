using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public string Mqtt_IP { get; set; }
        public string Mqtt_PORT { get; set; }
        public string Mqtt_Topic { get; set; }
        public string Mqtt_Message { get; set; }
        public string Mqtt_File { get; set; }
        public bool Mqtt_Retain { get; set; }
        public bool Mqtt_Connected { get; set; }
        public string Mqtt_Command { get; private set; }
        public string Mqtt_CommandItemName { get; private set; }
        public string Mqtt_CommandItemValue { get; private set; }
        public string Mqtt_CommandItem2Name { get; private set; }
        public string Mqtt_CommandItem2Value { get; private set; }
        private MqttClient _mqttClient;
        string clientId;


        public Form1()
        {
            InitializeComponent();
            clientId = Guid.NewGuid().ToString();
            Mqtt_Retain = false;
            Mqtt_IP = "localhost";
            Mqtt_PORT = "1883";
            this.button3.Visible = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_IP = textBox.Text;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_PORT = textBox.Text;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_Topic = textBox.Text;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            RichTextBox textBox = sender as RichTextBox;
            if (textBox != null)
            {
                Mqtt_Message = textBox.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_mqttClient?.IsConnected == true)
            {
                Mqtt_Connected =  true;
            }
            else
            {        
                try
                {
                    // Create Client instance
                    _mqttClient = new MqttClient(Mqtt_IP);
                    _mqttClient.Connect(clientId);
                    Mqtt_Connected = true;
                    this.button1.Visible = false;
                    this.button3.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Alert");
                }
            }
        }
        private void _mqttClient_ConnectionClosed(object sender, EventArgs e)
        {
            this.button3.Visible = false;
            this.button1.Visible = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                _mqttClient.Publish(Mqtt_Topic, Encoding.ASCII.GetBytes(Mqtt_Message), 0, Mqtt_Retain);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Alert");
            }
      
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox textBox = sender as CheckBox;
            if (textBox != null)
            {
                if(textBox.CheckState == CheckState.Checked)
                {
                    Mqtt_Retain = true;
                }
                else
                {
                    Mqtt_Retain = false;
                }
             
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_mqttClient?.IsConnected == true)
            {
                try
                {
  
                    _mqttClient.Disconnect();
                    Mqtt_Connected = false;
                    this.button3.Visible = false;
                    this.button1.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Alert");
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_File = textBox.Text;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            try
            {
                IList<TopicMessage> topicMessages = new List<TopicMessage>();
                StreamReader reader = File.OpenText(Mqtt_File);
            string line;
            while ((line = reader.ReadLine()) != null)
                {
                    string newString = null;
                    int indexLine = line.IndexOf("/ ");
                    if(indexLine > 10)
                    {                   
                        newString = line.Remove(0, indexLine + 2);
                    }
                    else
                    {
                        newString = line.Remove(0, line.IndexOf(" ") + 1);              
                    }
         


                string[] items = line.Split(' ');
                    topicMessages.Add(new TopicMessage
                    {
                        Topic = items[0],
                        MQTTMessage = newString
                    });
                }

                foreach(TopicMessage top in topicMessages)
                {
                    _mqttClient.Publish(top.Topic, Encoding.ASCII.GetBytes(top.MQTTMessage), 0, Mqtt_Retain);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Alert");
            }
        }

        public class TopicMessage
        {
            public string Topic { get; set; }
            public string MQTTMessage { get; set; }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_Command = textBox.Text;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                List<CommandItemDTO> commandItems = new List<CommandItemDTO>();
                commandItems.Add(new CommandItemDTO
                {
                    Name = Mqtt_CommandItemName,
                    Value = Mqtt_CommandItemValue
                });
                if(Mqtt_CommandItem2Name !=null)
                {
                    commandItems.Add(new CommandItemDTO
                    {
                        Name = Mqtt_CommandItem2Name,
                        Value = Mqtt_CommandItem2Value
                    });
                }
                var stringGuid = Guid.NewGuid().ToString().ToUpper();
                CommandDTO commandDTO = new CommandDTO
                {
                    Command = Mqtt_Command,
                    CommandId = stringGuid,
                    CommandItems = commandItems,
                    ResultTopicMQTT = Mqtt_Topic + stringGuid
                };

                _mqttClient.Publish(Mqtt_Topic, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(commandDTO)), 0, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Alert");
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_CommandItemName = textBox.Text;
            }
        }
        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_CommandItemValue = textBox.Text;
            }
        }

        public class CommandItemDTO
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }


        public class CommandDTO
        {
            public string Command { get; set; }
            public string ResultTopicMQTT { get; set; }
            public DateTime TimeSent { get; set; }
            public string CommandId { get; set; }
            public IList<CommandItemDTO> CommandItems { get; set; }
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_CommandItem2Name = textBox.Text;
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                Mqtt_CommandItem2Value = textBox.Text;
            }
        }
    }
}
