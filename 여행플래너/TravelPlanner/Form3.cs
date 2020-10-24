using System;
using System.Windows.Forms;
using ClassLibrary;
using System.Drawing;


namespace TravelPlanner
{
    public partial class Form3 : Form
    {
        MyVehicle GetWidget;
        Button GetButton;

        public Form3(MyVehicle ParentsWidget, Button button)
        {
            InitializeComponent();
            this.GetWidget = ParentsWidget;
            this.GetButton = button;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            txtName.Text = GetWidget.name;
            txtDepart.Text = GetWidget.departTime;
            txtArrive.Text = GetWidget.arriveTime;
            txtTake.Text = GetWidget.takeTime;
            txtFee.Text = GetWidget.fee;
            txtOther.Text = GetWidget.other;
        }

        public delegate void SendContextDele(MyVehicle widget, Button button);
        public event SendContextDele SendContext;

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            GetWidget.name = txtName.Text;
            GetWidget.departTime= txtDepart.Text;
            GetWidget.arriveTime = txtArrive.Text;
            GetWidget.takeTime = txtTake.Text;
            GetWidget.fee = txtFee.Text;
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
