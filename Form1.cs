using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Esercizio_chat_18_09_2023
{
    public partial class Form1 : Form
    {
        string jsonString;

        MqttClient mqttClient;
        public Form1()
        {
            InitializeComponent();
        }

        public class Dati
        {
            
            public string messaggio { get; set; }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var factory = new MqttFactory();
                var mqttclient = factory.CreateMqttClient();

                var option = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com", 1883)
                .Build();

                await mqttclient.ConnectAsync(option, CancellationToken.None);

                await mqttclient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("Chat2").Build());

                mqttclient.UseDisconnectedHandler(async e =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    try
                    {
                        await mqttclient.ConnectAsync(option, CancellationToken.None);
                    }
                    catch
                    {

                    }

                }
                );

                mqttclient.UseApplicationMessageReceivedHandler(async e =>
                {
                    string topic = e.ApplicationMessage.Topic;
                    string payload = e.ApplicationMessage.ConvertPayloadToString();

                    Dati messaggio1 = JsonSerializer.Deserialize<Dati>(payload);

                    textBoxInvia
                        .Invoke(
                            (MethodInvoker)(() =>
                                textBoxOutput.Text += DateTime.Now + " " + messaggio1.messaggio + "\r\n"
                            )
                        );

                }
                );
            }
            );
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Dati chat = new Dati()
            {
                messaggio = "Michael: " + textBoxInvia.Text, 
            };

            jsonString = JsonSerializer.Serialize(chat);

            var factory = new MqttFactory();
            var mqttclient = factory.CreateMqttClient();
            
            var option = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com", 1883)
                .Build();

            await mqttclient.ConnectAsync(option,CancellationToken.None);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("Chat2")
                .WithPayload(jsonString)
                .Build();
            await mqttclient.PublishAsync(message,CancellationToken.None);
            await mqttclient.DisconnectAsync(CancellationToken.None);   
        }

        private void textBoxOutput_TextChanged(object sender, EventArgs e)
        {
            textBoxOutput.ScrollBars = ScrollBars.Both;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string text = File.ReadAllText(openFileDialog1.FileName);
                textBoxOutput.Text = text;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog(); 

            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (Stream s = File.Open(saveFileDialog1.FileName, FileMode.CreateNew))
                using (StreamWriter sw = new StreamWriter(s))
                {
                    sw.WriteLine(textBoxOutput.Text);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}