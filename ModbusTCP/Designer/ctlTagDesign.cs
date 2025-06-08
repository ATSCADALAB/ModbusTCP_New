using System;
using System.Windows.Forms;

namespace ModbusTCP
{
    public partial class ctlTagDesign : UserControl
    {
        private readonly ATDriver driver;

        #region PROPERTIES

        public string TagName
        {
            get => this.txtTagName.Text.Trim();
            set => this.txtTagName.Text = value.Trim();
        }

        public string TagAddress
        {
            get => this.txtTagAddress.Text.Trim();
            set => this.txtTagAddress.Text = value.Trim();
        }

        public DataType TagType
        {
            get
            {
                var item = this.cbxDataType.SelectedItem;
                if (item is null) return DataType.Default;
                return item.ToString().GetDataType();
            }
            set => this.cbxDataType.SelectedItem = value.ToDisplayName();
        }

        public string Description
        {
            get => this.txtDescription.Text.Trim();
            set => this.txtDescription.Text = value;
        }

        public bool IsValid
        {
            get => this.btnOk.Enabled;
            set => this.btnOk.Enabled = value;
        }

        #endregion
        
        public ctlTagDesign(ATDriver driver)
        {
            InitializeComponent();
            this.driver = driver;

            Load += (sender, e) => Init();
            KeyPress += (sender, e) => CheckKey(e.KeyChar);

            btnOk.Enabled = false;
            btnOk.KeyPress += (sender, e) => CheckKey(e.KeyChar);
            btnOk.Click += (sender, e) => UpdateTag();

            btnCheck.Click += (sender, e) => CheckTag();
            cbxDataType.DataSource = Enum.GetNames(typeof(DataType));
            cbxDataType.SelectedIndex = 0;
        }

        private void Init()
        {
            TagName = driver.TagNameDesignMode;
            TagAddress = driver.TagAddressDesignMode;
            TagType = driver.TagTypeDesignMode.GetDataType();

            if (string.IsNullOrEmpty(driver.TagAddressDesignMode))
            {
                btnOk.Enabled = true;
                return;
            }

            var tagType = TagType;
            IsValid = TagAddress.GetAddress(tagType, out string description) != null;
            TagType = tagType;
            Description = description;
        }

        private void CheckKey(char keyChar)
        {
            if (keyChar == (char)13)
            {
                UpdateTag();
            }
            else if (keyChar == (char)27)
            {
                Parent.Dispose();
            }
        }

        private void UpdateTag()
        {
            if (!CheckPropertis()) return;
            var tagType = TagType;
            var address = TagAddress.GetAddress(tagType, out string description);
            if(address != null)            
            {
                driver.TagNameDesignMode = TagName;
                driver.TagAddressDesignMode = TagAddress;
                driver.TagTypeDesignMode = address.DataType.ToDisplayName();
                driver.TagClientAccessDesignMode = address.AccessRight.ToDisplayName();

                Parent.Dispose();
            }
            else
            {
                Description = description;
                IsValid = false;
            }
        }

        private void CheckTag()
        {
            if (!CheckPropertis()) return;
            var tagType = TagType;
            var address = TagAddress.GetAddress(tagType, out string description);
            if (address != null)
            {
                TagType = address.DataType;
                Description = description;
                IsValid = true;
            }
            else
            {
                Description = description;
                IsValid = false;
            }
        } 

        private bool CheckPropertis()
        {
            if (string.IsNullOrWhiteSpace(TagName))
            {
                IsValid = false;
                Description = $"The tag name can't be empty.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(TagAddress))
            {
                IsValid = false;
                Description = $"The tag address can't be empty.";
                return false;
            }

            return true;
        }
    }
}
