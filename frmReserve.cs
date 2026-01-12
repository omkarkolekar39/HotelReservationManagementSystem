
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Linq;
using System;
using System.Collections;
using System.Xml.Linq;
using System.Windows.Forms;


using System.Data.OleDb;

namespace HBRS
{
	public partial class frmReserve
	{
		public frmReserve()
		{
			InitializeComponent();
			

		}
		
#region Default Instance
		
		private static frmReserve defaultInstance;
		

		public static frmReserve Default
		{
			get
			{
				if (defaultInstance == null)
				{
					defaultInstance = new frmReserve();
					defaultInstance.FormClosed += new FormClosedEventHandler(defaultInstance_FormClosed);
				}
				
				return defaultInstance;
			}
		}
		
		static void defaultInstance_FormClosed(object sender, FormClosedEventArgs e)
		{
			defaultInstance = null;
		}
		
#endregion
		string trans_id;
		int id;
		int guest_id;
		int room_num;
		
		public void dtCheckOutDate_ValueChanged(System.Object sender, System.EventArgs e)
        {
            TimeSpan T = dtCheckOutDate.Value - dtCheckInDate.Value;
            if (T.Days < 1)
            {
                dtCheckOutDate.Value = dtCheckInDate.Value.AddDays(1); // Use Value, not Text
                txtDaysNumber.Text = "1";
            }
            else
            {
                txtDaysNumber.Text = T.Days.ToString();
            }

            double roomRate = Conversion.Val(txtRoomRate.Text);
            double days = Conversion.Val(txtDaysNumber.Text);
            lblTotal.Text = (roomRate * days).ToString();
            txtSubTotal.Text = lblTotal.Text;
            lblDateNow.Text = DateTime.Now.ToString("MM/dd/yyyy");
        }

        public void bttnSearchGuest_Click(System.Object sender, System.EventArgs e)
		{
			frmSelectGuest.Default.ShowDialog();
		}
		
		public void bttnSearchRoom_Click(System.Object sender, System.EventArgs e)
		{
			frmSelectRoom.Default.ShowDialog();
		}
		
		public void bttnCancel_Click(System.Object sender, System.EventArgs e)
		{
			this.Close();
		}
		
		public void frmReserve_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
		{
			string a = System.Convert.ToString(Interaction.MsgBox("Cancel Transaction?", (int) Constants.vbQuestion + Constants.vbYesNo, "Cancel"));
			if (a == Constants.vbNo.ToString())
			{
				e.Cancel = true;
			}
			else
			{
				clear_text();
			}
		}
		private void clear_text()
		{
			txtGuestName.Clear();
			txtRoomNumber.Clear();
			txtRoomType.Clear();
			txtRoomRate.Clear();
			txtChildren.Text = "0";
			txtAdults.Text = "0";
			cboDiscount.Refresh();
			txtAdvance.Clear();
			txtSubTotal.Clear();
			txtTotal.Clear();
			lblDiscountID.Text = "";
			lblDiscountRate.Text = "";
			lblGuestID.Text = "";
			lblAdvancePayment.Text = "";
			lblNoOfOccupancy.Text = "0";

            dtCheckInDate.Value = DateTime.Now.Date;
            dtCheckOutDate.Value = DateTime.Now.Date.AddDays(1);
        }
		
		public void frmReserve_Load(object sender, System.EventArgs e)
        {
            clear_text();

            dtCheckInDate.Value = DateTime.Now.Date;
            dtCheckOutDate.Value = dtCheckInDate.Value.AddDays(1);

            dtCheckInDate.Format = DateTimePickerFormat.Custom;
            dtCheckInDate.CustomFormat = "MM/dd/yyyy";
            dtCheckOutDate.Format = DateTimePickerFormat.Custom;
            dtCheckOutDate.CustomFormat = "MM/dd/yyyy";

            lblDateNow.Text = DateTime.Now.ToString("MM/dd/yyyy");

            transID();
            pop_discount();
            display_reserve();
        }

        public void transID()
		{
			
			Module1.con.Open();
			DataTable dt = new DataTable("tblTransaction");
			Module1.rs = new OleDbDataAdapter("SELECT * FROM tblTransaction ORDER BY TransID DESC", Module1.con);
			Module1.rs.Fill(dt);
			
			if (dt.Rows.Count == 0)
			{
				txtTransID.Text = "TransID - 0001";
			}
			else
			{
				int value = (int) (Conversion.Val(dt.Rows[0]["TransID"]));
				value++;
				txtTransID.Text = "TransID - " + value.ToString("0000");
			}
			Module1.rs.Dispose();
			Module1.con.Close();
			
		}
		
		private void pop_discount()
        {
            try
            {
                if (Module1.con.State != ConnectionState.Open)
                    Module1.con.Open();

                DataTable dt = new DataTable();
                using (OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM tblDiscount", Module1.con))
                {
                    da.Fill(dt);
                }

                cboDiscount.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    cboDiscount.Items.Add(row["DiscountType"].ToString());
                }

                // Select first discount by default if available
                if (cboDiscount.Items.Count > 0)
                    cboDiscount.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading discounts: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (Module1.con.State == ConnectionState.Open)
                    Module1.con.Close();
            }
        }

        private void display_reserve()
		{
			
			DataTable Dt = new DataTable("tblGuest");
			OleDbDataAdapter rs = default(OleDbDataAdapter);
			
			rs = new OleDbDataAdapter("Select * from tblTransaction, tblGuest, tblDiscount, tblRoom WHERE tblTransaction.GuestID = tblGuest.ID AND tblTransaction.DiscountID = tblDiscount.ID AND tblTransaction.RoomNum = tblRoom.RoomNumber AND tblTransaction.Remarks = \'Reserve\'", Module1.con);
			
			rs.Fill(Dt);
			int indx = default(int);
			lvlreserve.Items.Clear();
			for (indx = 0; indx <= Dt.Rows.Count - 1; indx++)
			{
				ListViewItem lv = new ListViewItem();
				TimeSpan getdate = new TimeSpan();
				int days = default(int);
				int subtotal = default(int);
				int total = default(int);
				int rate = default(int);
				double discount = default(double);
				
				int value = (int) (Conversion.Val(Dt.Rows[indx]["TransID"]));
				
				lv.Text = "TransID - " + value.ToString("0000");
				lv.SubItems.Add(Dt.Rows[indx]["GuestFName"] + " " + Dt.Rows[indx]["GuestLName"]);
				lv.SubItems.Add(Dt.Rows[indx]["RoomNum"].ToString());
				
				rate = System.Convert.ToInt32(Dt.Rows[indx]["RoomRate"]);
				
				lv.SubItems.Add(Dt.Rows[indx]["CheckInDate"].ToString());
				lv.SubItems.Add(Dt.Rows[indx]["CheckOutDate"].ToString());
				
				dtIn.Value = System.Convert.ToDateTime(Dt.Rows[indx]["CheckOutDate"]);
				dtOut.Value = System.Convert.ToDateTime(Dt.Rows[indx]["CheckInDate"]);
				
				getdate = dtIn.Value - dtOut.Value;
				days = getdate.Days;
				
				lv.SubItems.Add(days.ToString());
				lv.SubItems.Add(Dt.Rows[indx]["NoOfChild"].ToString());
				lv.SubItems.Add(Dt.Rows[indx]["NoOfAdult"].ToString());
				lv.SubItems.Add(Dt.Rows[indx]["AdvancePayment"].ToString());
				lv.SubItems.Add(Dt.Rows[indx]["DiscountType"].ToString());
				
				discount = Conversion.Val(Dt.Rows[indx]["DiscountRate"]);
				
				subtotal = (int) ((days * rate) - ((days * rate) * discount));
				
				total = (int) (Conversion.Val(subtotal.ToString()) - Conversion.Val(Dt.Rows[indx]["AdvancePayment"]));
				
				lv.SubItems.Add(System.Convert.ToString(Conversion.Val(total.ToString())));
				lvlreserve.Items.Add(lv);
			}
			rs.Dispose();
			
		}
		
		public void bttnReserve_Click(System.Object sender, System.EventArgs e)
        {
            try
            {
                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(lblGuestID.Text) ||
                    string.IsNullOrWhiteSpace(txtRoomNumber.Text) ||
                    (Convert.ToInt32(txtChildren.Text) + Convert.ToInt32(txtAdults.Text)) == 0)
                {
                    Interaction.MsgBox("Please fill all required fields.", MsgBoxStyle.Information, "Note");
                    return;
                }

                MsgBoxResult confirm = Interaction.MsgBox(
                    "Confirm Reservation Transaction?",
                    MsgBoxStyle.Question | MsgBoxStyle.YesNo,
                    "Reservation");

                if (confirm != MsgBoxResult.Yes)
                    return;

                // ===== OPEN CONNECTION =====
                if (Module1.con.State != ConnectionState.Open)
                    Module1.con.Open();

                // ===== INSERT TRANSACTION =====
                using (OleDbCommand checkin = new OleDbCommand(
                    "INSERT INTO tblTransaction " +
                    "(GuestID, RoomNum, CheckInDate, CheckOutDate, ReservationDate, " +
                    "NoOfChild, NoOfAdult, AdvancePayment, DiscountID, Remarks, Status) " +
                    "VALUES (?,?,?,?,?,?,?,?,?,?,?)", Module1.con))
                {
                    // IMPORTANT: ORDER MATTERS IN OleDb
                    checkin.Parameters.Add("@GuestID", OleDbType.Integer).Value =
                        Convert.ToInt32(lblGuestID.Text);

                    checkin.Parameters.Add("@RoomNum", OleDbType.Integer).Value =
                        Convert.ToInt32(txtRoomNumber.Text);

                    checkin.Parameters.Add("@CheckInDate", OleDbType.Date).Value =
                        dtCheckInDate.Value;

                    checkin.Parameters.Add("@CheckOutDate", OleDbType.Date).Value =
                        dtCheckOutDate.Value;

                    checkin.Parameters.Add("@ReservationDate", OleDbType.Date).Value =
                        DateTime.Now;

                    checkin.Parameters.Add("@NoOfChild", OleDbType.Integer).Value =
                        Convert.ToInt32(txtChildren.Text);

                    checkin.Parameters.Add("@NoOfAdult", OleDbType.Integer).Value =
                        Convert.ToInt32(txtAdults.Text);

                    checkin.Parameters.Add("@AdvancePayment", OleDbType.Currency).Value =
                        Convert.ToDecimal(txtAdvance.Text);

                    checkin.Parameters.Add("@DiscountID", OleDbType.Integer).Value =
                        Convert.ToInt32(lblDiscountID.Text);

                    checkin.Parameters.Add("@Remarks", OleDbType.VarChar).Value =
                        "Reserve";

                    checkin.Parameters.Add("@Status", OleDbType.VarChar).Value =
                        "Active";

                    checkin.ExecuteNonQuery();
                }

                // ===== UPDATE GUEST STATUS =====
                using (OleDbCommand update_guest = new OleDbCommand(
                    "UPDATE tblGuest SET Remarks = ? WHERE ID = ?", Module1.con))
                {
                    update_guest.Parameters.Add("@Remarks", OleDbType.VarChar).Value = "Reserve";
                    update_guest.Parameters.Add("@ID", OleDbType.Integer).Value =
                        Convert.ToInt32(lblGuestID.Text);

                    update_guest.ExecuteNonQuery();
                }

                // ===== UPDATE ROOM STATUS =====
                using (OleDbCommand update_room = new OleDbCommand(
                    "UPDATE tblRoom SET Status = ? WHERE RoomNumber = ?", Module1.con))
                {
                    update_room.Parameters.Add("@Status", OleDbType.VarChar).Value = "Reserve";
                    update_room.Parameters.Add("@RoomNumber", OleDbType.Integer).Value =
                        Convert.ToInt32(txtRoomNumber.Text);

                    update_room.ExecuteNonQuery();
                }

                Interaction.MsgBox(
                    "Guest successfully reserved!",
                    MsgBoxStyle.Information,
                    "Reservation");

                // ===== RESET FORM =====
                clear_text();
                transID();
                display_reserve();
            }
            catch (Exception ex)
            {
                Interaction.MsgBox(
                    "Error during reservation: " + ex.Message,
                    MsgBoxStyle.Critical,
                    "Error");
            }
            finally
            {
                if (Module1.con.State == ConnectionState.Open)
                    Module1.con.Close();
            }
        }

        public void dtCheckInDate_ValueChanged(System.Object sender, System.EventArgs e)
		{
			DateTime t = dtCheckInDate.Value;
			if (t.Date < DateTime.Now.Date)
			{
				dtCheckInDate.Value = DateTime.Now.Date;
			}
			else
			{
				dtCheckOutDate.Value = dtCheckInDate.Value.Date.AddDays(1);
			}
		}
		
		public void bttnAddAdult_Click(System.Object sender, System.EventArgs e)
		{
			int tao;
			tao = (int) (Conversion.Val(txtAdults.Text) + Conversion.Val(txtChildren.Text));
			if (tao == Conversion.Val(lblNoOfOccupancy.Text))
			{
				
			}
			else
			{
				txtAdults.Text = System.Convert.ToString(Conversion.Val(txtAdults.Text) + 1);
			}
		}
		
		public void bttnAddChildren_Click(System.Object sender, System.EventArgs e)
		{
			int tao;
			tao = (int) (Conversion.Val(txtAdults.Text) + Conversion.Val(txtChildren.Text));
			if (tao == Conversion.Val(lblNoOfOccupancy.Text))
			{
				
			}
			else
			{
				txtChildren.Text = System.Convert.ToString(Conversion.Val(txtChildren.Text) + 1);
			}
		}
		
		public void bttnSubAdult_Click(System.Object sender, System.EventArgs e)
		{
			if (Conversion.Val(txtAdults.Text) == 0)
			{
				txtAdults.Text = System.Convert.ToString(Conversion.Val(txtAdults.Text));
			}
			else
			{
				txtAdults.Text = System.Convert.ToString(Conversion.Val(txtAdults.Text) - 1);
			}
		}
		
		public void bttnSubChildren_Click(System.Object sender, System.EventArgs e)
		{
			if (Conversion.Val(txtChildren.Text) == 0)
			{
				txtChildren.Text = System.Convert.ToString(Conversion.Val(txtChildren.Text));
			}
			else
			{
				txtChildren.Text = System.Convert.ToString(Conversion.Val(txtChildren.Text) - 1);
			}
		}
		
		public void cboDiscount_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                Module1.con.Open();
                DataTable dt = new DataTable();
                using (OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM tblDiscount WHERE DiscountType = ?", Module1.con))
                {
                    da.SelectCommand.Parameters.AddWithValue("@DiscountType", cboDiscount.Text);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    lblDiscountID.Text = dt.Rows[0]["ID"].ToString();
                    lblDiscountRate.Text = dt.Rows[0]["DiscountRate"].ToString();

                    double total = Conversion.Val(lblTotal.Text);
                    double discountRate = Conversion.Val(lblDiscountRate.Text);
                    txtSubTotal.Text = (total - (total * discountRate)).ToString();

                    lblAdvancePayment.Text = "Advance payment must be at least Php " + (Conversion.Val(txtSubTotal.Text) * 0.5);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error applying discount: " + ex.Message);
            }
            finally
            {
                if (Module1.con.State == ConnectionState.Open)
                    Module1.con.Close();
            }
        }

        public void txtAdvance_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if ((e.KeyChar < '0'|| e.KeyChar > '9') && e.KeyChar != ControlChars.Back && e.KeyChar != '.')
			{
				//cancel keys
				e.Handled = true;
			}
		}
		
		public void txtAdvance_TextChanged(System.Object sender, System.EventArgs e)
		{
			txtTotal.Text = System.Convert.ToString(Conversion.Val(lblTotal.Text) - Conversion.Val(txtAdvance.Text));
		}
		
		public void txtRoomRate_TextChanged(System.Object sender, System.EventArgs e)
		{
			lblTotal.Text = System.Convert.ToString(Conversion.Val(txtRoomRate.Text) * Conversion.Val(txtDaysNumber.Text));
			txtSubTotal.Text = System.Convert.ToString(Conversion.Val(txtRoomRate.Text) * Conversion.Val(txtDaysNumber.Text));
		}
		
		public void lvlreserve_Click(object sender, System.EventArgs e)
        {
            if (lvlreserve.SelectedItems.Count == 0) return; // Check selection

            trans_id = lvlreserve.SelectedItems[0].Text;

            Module1.con.Open();
            DataTable dt = new DataTable("tblTransaction");
            using (OleDbDataAdapter rs = new OleDbDataAdapter("SELECT * FROM tblTransaction", Module1.con))
            {
                rs.Fill(dt);
            }

            for (int indx = 0; indx < dt.Rows.Count; indx++)
            {
                if (trans_id == "TransID - " + Conversion.Val(dt.Rows[indx]["TransID"]).ToString("0000"))
                {
                    guest_id = Convert.ToInt32(dt.Rows[indx]["GuestID"]);
                    room_num = Convert.ToInt32(dt.Rows[indx]["RoomNum"]);
                    id = Convert.ToInt32(dt.Rows[indx]["TransID"]);
                    break;
                }
            }

            Module1.con.Close();
        }

        public void bttnCheckin_Click(System.Object sender, System.EventArgs e)
		{
			string check_in = System.Convert.ToString(Interaction.MsgBox("Checkin Guest?", (int) Constants.vbQuestion + Constants.vbYesNo, "Checkin"));
			if (check_in == Constants.vbYes.ToString())
			{
				Module1.con.Open();
				OleDbCommand update_trans = new OleDbCommand("UPDATE tblTransaction SET Remarks = \'Checkin\' WHERE TransID = " + id.ToString() + "", Module1.con);
				update_trans.ExecuteNonQuery();
				
				OleDbCommand update_guest = new OleDbCommand("UPDATE tblGuest SET Remarks = \'Checkin\' WHERE ID = " + guest_id.ToString() + "", Module1.con);
				update_guest.ExecuteNonQuery();
				
				OleDbCommand update_room = new OleDbCommand("UPDATE tblRoom SET Status = \'Occupied\' WHERE RoomNumber = " + room_num.ToString() + "", Module1.con);
				update_room.ExecuteNonQuery();
				Module1.con.Close();
				display_reserve();
				Interaction.MsgBox("Guest Checkedin!", Constants.vbInformation, "Checkin");
			}
		}
		
		public void bttnCancelReserve_Click(System.Object sender, System.EventArgs e)
		{
			string check_in = System.Convert.ToString(Interaction.MsgBox("Cancel Reservation?", (int) Constants.vbQuestion + Constants.vbYesNo, "Cancel"));
			if (check_in == Constants.vbYes.ToString())
			{
				Module1.con.Open();
				
				OleDbCommand update_trans = new OleDbCommand("UPDATE tblTransaction SET Remarks = \'Cancelled\' WHERE TransID = " + id.ToString() + "", Module1.con);
				update_trans.ExecuteNonQuery();
				
				OleDbCommand update_guest = new OleDbCommand("UPDATE tblGuest SET Remarks = \'Available\' WHERE ID = " + guest_id.ToString() + "", Module1.con);
				update_guest.ExecuteNonQuery();
				
				OleDbCommand update_room = new OleDbCommand("UPDATE tblRoom SET Status = \'Available\' WHERE RoomNumber = " + room_num.ToString() + "", Module1.con);
				update_room.ExecuteNonQuery();
				
				Module1.con.Close();
				display_reserve();
				Interaction.MsgBox("Reservation Cancelled!", Constants.vbInformation, "Cancel");
			}
		}
		
		public void lvlreserve_SelectedIndexChanged(System.Object sender, System.EventArgs e)
		{
			
		}
	}
}
