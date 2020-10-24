using System;
using System.Windows.Forms;
using ClassLibrary;
using System.Drawing;

namespace TravelPlanner
{
    public partial class Form2 : Form
    {
        MyAccommodation GetWidget;
        Button GetButton;

        public Form2(MyAccommodation ParentsWidget, Button button)
        {
            InitializeComponent();
            this.GetWidget = ParentsWidget;
            this.GetButton = button;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            txtName.Text = GetWidget.name;
            txtCheckIn.Text = GetWidget.checkInTime;
            txtCheckOut.Text = GetWidget.checkOutTime;
            txtFee.Text = GetWidget.fee;
            txtPhone.Text = GetWidget.phoneNumber;
            txtAddress.Text = GetWidget.address;
            txtOther.Text = GetWidget.other;
        }

        public delegate void SendContextDele(MyAccommodation widget, Button button);
        public event SendContextDele SendContext;

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            GetWidget.name = txtName.Text;
            GetWidget.checkInTime = txtCheckIn.Text;
            GetWidget.checkOutTime = txtCheckOut.Text;
            GetWidget.fee = txtFee.Text;
            GetWidget.phoneNumber = txtPhone.Text;
            GetWidget.address = txtAddress.Text;
            GetWidget.other = txtOther.Text;
            GetButton.Text = txtName.Text;
            SendContext(GetWidget, GetButton);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panHeader_MouseMove(object sender, MouseEventArgs e)
        {
            var s = sender as Panel;
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            s.Parent.Left = this.Left + (e.X - ((Point)s.Tag).X);
            s.Parent.Top = this.Top + (e.Y - ((Point)s.Tag).Y);
        }
        private void panHeader_MouseDown(object sender, MouseEventArgs e)
        {
            var s = sender as Panel;
            s.Tag = new Point(e.X, e.Y);
        }
    }
}