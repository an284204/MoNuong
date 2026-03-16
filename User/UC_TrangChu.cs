using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace MoNuong
{
    public partial class UC_TrangChu : UserControl
    {
        private UserInfo currentUser;
        public UC_TrangChu()
        {
            InitializeComponent();
            
        }
        public UC_TrangChu(UserInfo user) : this()
        {
            currentUser = user;
        }
        public void SetUser(UserInfo user)
        {
            currentUser = user;
        }

    }
}