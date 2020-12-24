using SharpPcap;using SharpPcap.LibPcap;using PacketDotNet;
using System;using System.Collections.Generic;using System.ComponentModel;using System.Data;using System.Drawing;using System.Linq;using System.Text;using System.Windows.Forms;
namespace WindowsFormsApplication1{
    public partial class Form1 : Form{
        public Form1(){InitializeComponent();}
        private static int packetIndex = 0;
        private void button1_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK){
                label1.Text = openFileDialog1.FileName;
                dataGridView1.Rows.Clear();
                dataGridView2.Rows.Clear();
                packetIndex = 0;
                ICaptureDevice device;
                try{device = new CaptureFileReaderDevice(label1.Text);device.Open();}
                catch (Exception e2){MessageBox.Show("Caught exception when opening file" + e2.ToString());return;}
                device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
                device.Capture();
                device.Close();
                 }
            }
        private void device_OnPacketArrival(object sender, CaptureEventArgs e){
            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet) {
                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var ethernetPacket = (PacketDotNet.EthernetPacket)packet;
                var ipPacket = IpPacket.GetEncapsulated(packet);
                try{
                    for (int i = 0; i < dataGridView2.RowCount - 1; i++){
                       if (dataGridView2.Rows[i].Cells[0].Value.ToString() == ipPacket.SourceAddress.ToString()&&dataGridView2.Rows[i].Cells[1].Value.ToString() == ipPacket.DestinationAddress.ToString()){
                            dataGridView2.Rows[i].Cells[2].Value = Int32.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString()) + 1;
                            goto ok;
                        }
                    }
                    dataGridView2.Rows.Add(new string[]{ipPacket.SourceAddress.ToString(), ipPacket.DestinationAddress.ToString(), "1" });
                   }
                catch { }
                ok:
                try{
                    switch (ipPacket.Protocol){
                        case IPProtocolType.TCP:
                            var tcppacket = TcpPacket.GetEncapsulated(packet);
                            dataGridView1.Rows.Add(new string[]{
                                        packetIndex.ToString(),
                                        e.Packet.Timeval.Date.ToString(),
                                        ethernetPacket.SourceHwAddress.ToString(),
                                        ethernetPacket.DestinationHwAddress.ToString(),
                                        ipPacket.SourceAddress.ToString(),
                                        ipPacket.DestinationAddress.ToString(),
                                        ipPacket.Protocol.ToString(),
                                        tcppacket.SourcePort.ToString(),
                                        tcppacket.DestinationPort.ToString()
                                     });
                            break;
                        case IPProtocolType.UDP:
                            var udppacket = UdpPacket.GetEncapsulated(packet);
                            dataGridView1.Rows.Add(new string[]{
                                      packetIndex.ToString(),
                                      e.Packet.Timeval.Date.ToString(),
                                      ethernetPacket.SourceHwAddress.ToString(),
                                      ethernetPacket.DestinationHwAddress.ToString(),
                                      ipPacket.SourceAddress.ToString(),
                                      ipPacket.DestinationAddress.ToString(),
                                      ipPacket.Protocol.ToString(),
                                      udppacket.SourcePort.ToString(),
                                      udppacket.DestinationPort.ToString()
                                     });
                            break;
                        default:
                            dataGridView1.Rows.Add(new string[]{
                                      packetIndex.ToString(),
                                      e.Packet.Timeval.Date.ToString(),
                                      ethernetPacket.SourceHwAddress.ToString(),
                                      ethernetPacket.DestinationHwAddress.ToString(),
                                      ipPacket.SourceAddress.ToString(),
                                      ipPacket.DestinationAddress.ToString(),
                                      ipPacket.Protocol.ToString()
                                     });
                            break;
                    }
                }
                catch { dataGridView1.Rows.Add(new string[]{
                                      packetIndex.ToString(),
                                      e.Packet.Timeval.Date.ToString(),
                                      ethernetPacket.SourceHwAddress.ToString(),
                                      ethernetPacket.DestinationHwAddress.ToString(),
                                      "","",ethernetPacket.Type.ToString()
                                     });}
                 packetIndex++;
            }
        }
    }
}