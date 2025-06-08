using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using ModbusTCP;
using ATDriver_Server;

namespace ConsoleApp
{
    class Program
    {
        static ATDriver driver = new ATDriver();

        static SendPack sendPack;

        static void Main(string[] args)
        {
            driver.ChannelName = "newchannel";
            driver.ChannelAddress = "10";
            driver.DeviceName = "NewDevice";
            //driver.DeviceID = "127.0.0.1|502|1|1000|1|20|4-1-1-200";
            driver.DeviceID = "192.168.2.150|502|1|30000|1|20|";
            //driver.DeviceID = "127.0.0.1|502|1|1000|1|5|";

            driver.Connect();

            var value = 1;
            while (true)
            {               
                driver.ChannelAddress = "10";
                driver.DeviceID = "192.168.2.150|502|1|30000|1|20|";

                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"--------- {DateTime.Now:dd/MM/yyyy HH:mm:ss:fff} ----------   ");

                for(var i = 0; i < 10; i++)
                {
                    var address = 400000 + Convert.ToInt32(i) + 1;
                    Read("DWord", $"{address}");
                }


                //Read("Float", "400001");
                //Read("Float", "400003");
                //Read("Float", "400005");
                //Read("Float", "400007");
                //Read("Float", "400009");

                //driver.Write(new SendPack()
                //{
                //    ChannelAddress = "1000",
                //    DeviceID = "127.0.0.1|502|1|1000|1|20|",
                //    TagAddress = "400001",
                //    TagType = "Word",
                //    Value = value.ToString()
                //});
                //driver.ChannelAddress = "1000";
                //driver.DeviceID = "127.0.0.1|502|1|1000|1|5|1-1-1-2000/4-1-1-2000";
                //driver.TagAddress = "400011";
                //driver.TagType = "Float";
                //sendPack = driver.Read();

                value++;
                Thread.Sleep(1000);
            }
        }

        private static void Read(string type, string address)
        {            
            driver.TagAddress = address;
            driver.TagType = type;
            sendPack = driver.Read();
            if (sendPack != null)
            {
                Console.WriteLine($"{driver.TagAddress} : {sendPack.Value}          ");
            }
            else
            {
                Console.WriteLine($"{driver.TagAddress} : BAD      ");
            }
        }
    }
}
