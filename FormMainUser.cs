using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;

namespace MoNuong
{
    public partial class FormMainUser : Form
    {
        private Guna2Button[] menuButtons;
        private UC_TrangChu homeUC; 
       
        private UC_TaiKhoan taiKhoanUC;
        

        private readonly UserInfo currentUser;

        private readonly Color menuActiveColor = Color.FromArgb(229, 9, 20);
        private readonly Color menuInactiveColor = Color.FromArgb(30, 30, 45);

        public FormMainUser(UserInfo userInfo)
        {
            InitializeComponent();
            currentUser = userInfo; 
            ConfigureMenuButtons();
        }

        private void AddUserControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            panelMain.Controls.Clear();
            panelMain.Controls.Add(uc);
            uc.BringToFront();
        }

        private void FormMainUser_Load(object sender, EventArgs e)
        {
            homeUC = new UC_TrangChu(currentUser);
             
            taiKhoanUC = new UC_TaiKhoan(currentUser);
           


            AddUserControl(homeUC);
            SetActiveButton(btnTrangChu);

           
        }

        private void btnTrangChu_Click(object sender, EventArgs e)
        {
            AddUserControl(homeUC);
            
            SetActiveButton(btnTrangChu);
        }

       
        private void btnQuanLyTK_Click(object sender, EventArgs e)
        {
            AddUserControl(taiKhoanUC);
            SetActiveButton(btnQuanLyTK);
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn thoát không?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
                this.Close();
        }

       

       
        private void panelMenu_Paint(object sender, PaintEventArgs e)
        {
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {
        }

        private void picLogo_Click(object sender, EventArgs e)
        {
            AddUserControl(homeUC);
            SetActiveButton(btnTrangChu);
        }


        
        private void ConfigureMenuButtons()
        {
            menuButtons = new[]
            {
                btnTrangChu,
               
                btnQuanLyTK
               
            };

            foreach (var btn in menuButtons)
            {
                if (btn == null)
                {
                    continue;
                }

                btn.ButtonMode = ButtonMode.RadioButton;
                btn.FillColor = menuInactiveColor;
                btn.HoverState.FillColor = Color.FromArgb(200, 20, 35);
                btn.CheckedState.FillColor = menuActiveColor;
                btn.CheckedState.ForeColor = Color.White;
                btn.CheckedState.CustomBorderColor = menuActiveColor;
                btn.TextAlign = HorizontalAlignment.Left;
                btn.Padding = new Padding(4, 0, 0, 0);
            }

            if (btnTrangChu != null)
            {
                btnTrangChu.Checked = true;
            }
        }

        private void SetActiveButton(Guna2Button targetButton)
        {
            if (targetButton == null)
            {
                return;
            }

            targetButton.Checked = true;
        }
    }
}
