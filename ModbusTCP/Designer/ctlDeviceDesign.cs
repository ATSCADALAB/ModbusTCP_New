using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ModbusTCP
{
    public partial class ctlDeviceDesign : UserControl
    {       
        private readonly ATDriver driver;

        private readonly Dictionary<string, Control> controlDictionary;

        #region PROPERTIES

        public string DeviceName
        {
            get => this.txtDeviceName.Text.Trim();
            set => this.txtDeviceName.Text = value.Trim();
        }

        public string DeviceAddress
        {
            get => this.txtDeviceAddress.Text.Trim();
            set => this.txtDeviceAddress.Text = value.Trim();
        }

        public string Port
        {
            get => this.txtPort.Text.Trim();
            set => this.txtPort.Text = value.Trim();
        }

        public string UnitID
        {
            get => this.txtUnitID.Text.Trim();
            set => this.txtUnitID.Text = value.Trim();
        }

        public string TimeOut
        {
            get => this.txtTimeOut.Text.Trim();
            set => this.txtTimeOut.Text = value.Trim();
        }

        public OrderBy OrderBy
        {
            get => this.ckbOrderBy.Checked ? OrderBy.High : OrderBy.Low;
            set => this.ckbOrderBy.Checked = value == OrderBy.High;
        }

        public string ReadTimes
        {
            get => this.txtReadTimes.Text.Trim();
            set => this.txtReadTimes.Text = value.Trim();
        }

        #endregion
       
        public ctlDeviceDesign(ATDriver driver)
        {
            InitializeComponent();
            this.driver = driver;
            if (this.driver is null)
            {
                this.btnOk.Enabled = false;
                return;
            }

            this.controlDictionary = new Dictionary<string, Control>();
            Load += (sender, e) => Init();
            btnOk.Click += (sender, e) => UpdateDevice();
            KeyPress += (sender, e) => CheckKey(e.KeyChar);
            btnOk.KeyPress += (sender, e) => CheckKey(e.KeyChar);
        }

        private void Init()
        {
            foreach (var item in tabControl.Controls)
            {
                if (item is TabPage tabPage)
                {
                    foreach (Control control in tabPage.Controls)
                    {
                        if (control is CheckBox || control is TextBox)
                            controlDictionary.Add(control.Name, control);
                    }
                }
            }

            txtDeviceName.Text = this.driver.DeviceNameDesignMode;
            var deviceIDDesignMode = this.driver.DeviceIDDesignMode;
            if (deviceIDDesignMode != "NewId")
            {
                var deviceIDSplit = deviceIDDesignMode.Split('|');
                if (deviceIDSplit.Length == 7)
                {
                    DeviceAddress = deviceIDSplit[0];
                    Port = deviceIDSplit[1];
                    UnitID = deviceIDSplit[2];
                    TimeOut = deviceIDSplit[3];
                    OrderBy = deviceIDSplit[4].Equals("1") ? OrderBy.High : OrderBy.Low;
                    ReadTimes = deviceIDSplit[5];
                    var blockSetting = deviceIDSplit[6];

                    if (!string.IsNullOrWhiteSpace(blockSetting))
                    {
                        var areaSettings = blockSetting.Split('/');
                        foreach (var areaSetting in areaSettings)
                        {
                            var areaSettingSplit = areaSetting.Split('-');
                            if (areaSettingSplit.Length != 4) continue;
                            if (!uint.TryParse(areaSettingSplit[2], out uint from) ||
                                !uint.TryParse(areaSettingSplit[3], out uint to))
                                continue;
                            if (from > to || from < 1 || to > 99999) continue;

                            var areaName = string.Empty;
                            var position = areaSettingSplit[1];
                            switch (areaSettingSplit[0])
                            {
                                case "0":
                                    areaName = "Coil";
                                    break;
                                case "1":
                                    areaName = "InputContact";
                                    break;
                                case "3":
                                    areaName = "InputRegister";
                                    break;
                                case "4":
                                    areaName = "HoldingRegister";
                                    break;
                            }

                            if (controlDictionary[$"ckb{areaName}{position}"] is CheckBox checkBox)
                            {
                                checkBox.Checked = true;
                                controlDictionary[$"txtFrom{areaName}{position}"].Text = $"{from}";
                                controlDictionary[$"txtTo{areaName}{position}"].Text = $"{to}";
                            }
                        }
                    }
                }
            }
        }

        private void CheckKey(char keyChar)
        {
            if (keyChar == (char)13)
            {
                UpdateDevice();
            }
            else if (keyChar == (char)27)
            {
                Parent.Dispose();
            }
        }

        private void UpdateDevice()
        {
            if (!CheckDevicePropertis()) return;
            if (!CheckBlockSetings()) return;

            this.driver.DeviceNameDesignMode = DeviceName;
            var blockSettingsBuilder = new StringBuilder();

            blockSettingsBuilder.Append(
                $"{DeviceAddress}|{Port}|{UnitID}|" +
                $"{TimeOut}|{(int)OrderBy}|{ReadTimes}|");

            var separator = string.Empty;
            foreach (var control in controlDictionary.Values)
            {
                if (control is CheckBox checkBox)
                {
                    if (checkBox.Checked)
                    {
                        if (checkBox.Tag is string tagValue)
                        {
                            var area = tagValue.Split('-')[0];
                            var position = tagValue.Split('-')[1];
                            var areaName = string.Empty;
                            switch (area)
                            {
                                case "0":
                                    areaName = "Coil";
                                    break;
                                case "1":
                                    areaName = "InputContact";
                                    break;
                                case "3":
                                    areaName = "InputRegister";
                                    break;
                                case "4":
                                    areaName = "HoldingRegister";
                                    break;
                            }

                            var from = controlDictionary[$"txtFrom{areaName}{position}"].Text?.Trim();
                            var to = controlDictionary[$"txtTo{areaName}{position}"].Text?.Trim();
                            blockSettingsBuilder.Append($"{separator}{area}-{position}-{from}-{to}");

                            separator = "/";
                        }
                    }
                }
            }

            this.driver.DeviceIDDesignMode = blockSettingsBuilder.ToString();
            Parent.Dispose();
        }

        private bool CheckDevicePropertis()
        {
            if (string.IsNullOrWhiteSpace(DeviceName))
            {
                MessageBox.Show($"The device name can't be empty.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!ushort.TryParse(Port, out _))
            {
                MessageBox.Show($"The port value must be a number.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!uint.TryParse(UnitID, out _))
            {
                MessageBox.Show($"The unitID value must be a number.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!uint.TryParse(TimeOut, out _))
            {
                MessageBox.Show($"The time out value must be a number.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!uint.TryParse(ReadTimes, out _))
            {
                MessageBox.Show($"The read block setting tag read times value must be a number.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool CheckBlockSetings()
        {
            var checkResult = true;
            foreach (var control in controlDictionary.Values)
            {
                if (control is CheckBox checkBox)
                {
                    if (checkBox.Checked)
                    {
                        if (checkBox.Tag is string tagValue)
                        {
                            var area = tagValue.Split('-')[0];
                            var position = tagValue.Split('-')[1];
                            var areaName = string.Empty;
                            var fromStr = string.Empty;
                            var toStr = string.Empty;
                            var min = int.MinValue;
                            var max = int.MaxValue;

                            switch (area)
                            {
                                case "0":
                                    areaName = "Coil";
                                    min = 0;
                                    max = 99999;
                                    break;
                                case "1":
                                    areaName = "InputContact";
                                    min = 0;
                                    max = 99999;
                                    break;
                                case "3":
                                    areaName = "InputRegister";
                                    min = 0;
                                    max = 99999;
                                    break;
                                case "4":
                                    areaName = "HoldingRegister";
                                    min = 0;
                                    max = 99999;
                                    break;
                            }

                            fromStr = controlDictionary[$"txtFrom{areaName}{position}"].Text?.Trim();
                            toStr = controlDictionary[$"txtTo{areaName}{position}"].Text?.Trim();

                            checkResult = checkResult && CheckRange($"{areaName} - {position}", fromStr, toStr, min, max);
                        }
                    }
                }
            }

            return checkResult;
        }

        private bool CheckRange(string name, string fromStr, string toStr, int min, int max)
        {
            if (!int.TryParse(fromStr, out int from))
            {
                MessageBox.Show($"Error in Read Block Setting - {name} Device. The `From` value must be a number.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(toStr, out int to))
            {
                MessageBox.Show($"Error in Read Block Setting - {name} Device. The `To` value must be a number.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (from > to)
            {
                MessageBox.Show($"Error in Read Block Setting - {name} Device. The `From` value must be less than or equal to `To` value.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (from < min || from > max || to < min || to > max)
            {
                MessageBox.Show($"Error in Read Block Setting - {name} Device. Address must be between {min} and {max}.", "ATSCADA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
