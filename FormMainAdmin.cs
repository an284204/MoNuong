using System;
using System.Windows.Forms;

namespace MoNuong
{
    public partial class FormMainAdmin : Form
    {
        private UserInfo _adminInfo;

        public FormMainAdmin(UserInfo userInfo)
        {
            InitializeComponent();
            this._adminInfo = userInfo;
            this.Text = "Trang quản trị - " + _adminInfo.HoTen;
        }

        // Cần có constructor mặc định cho Designer
        public FormMainAdmin()
        {
            InitializeComponent();
        }
    }
}