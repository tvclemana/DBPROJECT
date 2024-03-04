﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBPROJECT
{
    public partial class frmUserProfile : Form
    {
        long iduser; 
        String loginname;
        public frmUserProfile(long liduser, String lname)
        {
            InitializeComponent();
            this.iduser = liduser;
            this.loginname = lname;
        }

        private frmChangePassword ChangePasswordfrm;
        private void ChangePasswordfrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ChangePasswordfrm.Dispose();
        }

        private void btnChangePwd_Click(object sender, EventArgs e)
        {
            ChangePasswordfrm = new frmChangePassword(this.iduser, this.loginname);
            ChangePasswordfrm.FormClosed += ChangePasswordfrm_FormClosed;
            //ChangePasswordfrm.MdiParent = this;
            ChangePasswordfrm.ShowDialog(); //show a modal ie showdialog - required to respond to the dialog
        }

        private void frmUserProfile_LoadUserData()
        {

            if(Globals.glOpenSqlConn())
            {
                SqlCommand cmd = new SqlCommand("spGetUserProfile", Globals.sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@liduser", this.iduser);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                int rowcount = dt.Rows.Count;

                if (rowcount == 0)
                {
                    csMessageBox.Show("Invalid User", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    this.txtLoginName.Text = dt.Rows[0][0].ToString();
                    this.txtEmail.Text = dt.Rows[0][1].ToString();
                    this.txtSMTPHOST.Text = dt.Rows[0][2].ToString();
                    this.txtSMTPport.Text = dt.Rows[0][3].ToString();
                    this.pkrBirthdate.Value = Globals.glConvertBlankDate(dt.Rows[0][4].ToString());
                    this.cbxGender.SelectedItem = Globals.glConvertBlankGender(dt.Rows[0][5].ToString());                      
                }

            }
        }
        private void frmUserProfile_GetPhotofromField() {
            if (Globals.glOpenSqlConn())
            {
                SqlCommand cmd = new SqlCommand("select isnull(photo,'') from users where id=@liduser", Globals.sqlconn);
                cmd.Parameters.AddWithValue("@liduser", this.iduser);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                int rowcount = dt.Rows.Count;

                if (rowcount == 0)
                {
                    csMessageBox.Show("Invalid User", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if(dt.Rows[0][0] != null) 
                    {
                        byte[] UserImg = (byte[])dt.Rows[0][0];

                        MemoryStream imgstream = new MemoryStream(UserImg);

                        if (imgstream.Length > 0)
                            this.pictBoxUser.Image = Image.FromStream(imgstream);
                    }
                    da.Dispose();
                }

            }
        }
        private void frmUserProfile_Load(object sender, EventArgs e) //event fired when opening form
        {
            this.frmUserProfile_LoadUserData();
            this.frmUserProfile_GetPhotofromField();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (csMessageBox.Show("User photo is cleared...", "Information",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.ClearPhototoField();
            }
        }

        private void btnLoadPhoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog openPhoto = new OpenFileDialog();
            openPhoto.Filter = "Choose Image(*.jpg; *.png; *.gif)|*.jpg; *.png; *.gif";
            if (openPhoto.ShowDialog() == DialogResult.OK)
            {
                pictBoxUser.Image = Image.FromFile(openPhoto.FileName);
                this.SavePhototoField();
            }
        }
        private void SavePhototoField()
        {
            MemoryStream ms = new MemoryStream();
            this.pictBoxUser.Image.Save(ms, pictBoxUser.Image.RawFormat);
            byte[] img = ms.ToArray();

            if (Globals.glOpenSqlConn())
            {
                String qrystr = "update users set photo=@img where id =" +
                    Globals.gIdUser.ToString();

                SqlCommand cmd = new SqlCommand(qrystr, Globals.sqlconn);

                cmd.Parameters.Add("@img", SqlDbType.Image); //MySqlDbType.Blob
                cmd.Parameters["@img"].Value = img;

                if (cmd.ExecuteNonQuery() == 1)
                    csMessageBox.Show("New photo is saved...", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Globals.glCloseSqlConn();
        }
        private void ClearPhototoField()
        {
            if (Globals.glOpenSqlConn())
            {
                String qrystr = "update users set photo=null where id =" +
                    Globals.gIdUser.ToString();

                SqlCommand cmd = new SqlCommand(qrystr, Globals.sqlconn);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    this.pictBoxUser.Image = null;

                    csMessageBox.Show("User photo is cleared...", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            Globals.glCloseSqlConn();
        }
        private void frmUserProfile_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
        private void EnableSaveButton(object sender, EventArgs e)
        {
            this.btnSave.Enabled = true;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.txtLoginName.Text.Trim() == "")
            {
                csMessageBox.Show("Please provide a valid login name.", "Empty Login Name",
                 MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.frmUserProfile_UpdateUser();
                this.btnSave.Enabled = false;
                Globals.gLoginName = this.txtLoginName.Text;
            }

        }
        private void frmUserProfile_UpdateUser()
        {
            if (Globals.glOpenSqlConn())
            {
                SqlCommand cmd = new SqlCommand("spUpdateUser", Globals.sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@lidUser", Globals.gIdUser);
                cmd.Parameters.AddWithValue("@lloginName", this.txtLoginName.Text);
                cmd.Parameters.AddWithValue("@lemail", this.txtEmail.Text);
                cmd.Parameters.AddWithValue("@lsmtphost", this.txtSMTPHOST.Text);
                cmd.Parameters.AddWithValue("@lsmtpport", this.txtSMTPport.Text);
                cmd.Parameters.AddWithValue("@lbirthdate",
                     Globals.glConvertBlankDate(this.pkrBirthdate.Value.ToString()));
                cmd.Parameters.AddWithValue("@lgender",
                    Globals.glConvertBlankGender(this.cbxGender.SelectedItem.ToString()));
                cmd.ExecuteNonQuery();
            }
            Globals.glCloseSqlConn();
        }
        private void frmUserProfile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.btnSave.Enabled)
            {
                DialogResult dr;

                dr = csMessageBox.Show("Changes not saved! Save changes", "Please confirm.",
                 MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (dr)
                {
                    case DialogResult.Yes:
                        if (this.txtLoginName.Text.Trim() == "")
                        {
                            csMessageBox.Show("Please provide a valid login name.", "Empty Login Name",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                            e.Cancel = true;
                            this.txtLoginName.Focus();
                        }
                        else
                        {
                            this.frmUserProfile_UpdateUser();
                            Globals.gLoginName = this.txtLoginName.Text;
                        }
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }

        }

        private void frmUserProfile_RefreshUser()
        {
            String uname = "", uemail = "", ugender = "MALE", usmtphost = "", usmtpport = "";
            DateTime ubirthdate = Convert.ToDateTime("01/01/1900");

            this.GetUserPhotofromField();
            this.frmUserProfile_GetUser(Globals.gIdUser, ref uname, ref uemail,
                ref usmtphost, ref usmtpport,
                ref ubirthdate, ref ugender);

            this.txtLoginName.Text = uname;
            this.txtEmail.Text = uemail;
            this.txtSMTPHOST.Text = usmtphost;
            this.txtSMTPport.Text = usmtpport;

            this.pkrBirthdate.Value = ubirthdate;
            this.cbxGender.SelectedItem = ugender;

            this.btnSave.Enabled = false;

            this.txtEmail.Focus();



        }

        private void frmUserProfile_GetUser(long iduser, ref String loginName,
           ref String email, ref String smtphost, ref String smtpport,
           ref DateTime birthdate, ref String gender)
        {

            if (Globals.glOpenSqlConn())
            {
                SqlCommand cmd = new SqlCommand("spGetUserProfile",
                    Globals.sqlconn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@liduser", iduser);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    loginName = reader["loginName"].ToString();
                    email = reader["email"].ToString();
                    smtphost = reader["smtphost"].ToString();
                    smtpport = reader["smtpport"].ToString();

                    birthdate = Globals.glConvertBlankDate(reader["birthdate"].ToString());

                    gender = Globals.glConvertBlankGender(reader["gender"].ToString());

                }
                else csMessageBox.Show("No such User ID:" + iduser.ToString() + " is found!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Globals.glCloseSqlConn();

        }

        void GetUserPhotofromField()
        {
            if (Globals.glOpenSqlConn())
            {
                String qrystr = "Select isnull(photo,'') as photo from users where id=" + Globals.gIdUser.ToString();
                SqlCommand cmd = new SqlCommand(qrystr, Globals.sqlconn);

                SqlDataAdapter UserAdapter = new SqlDataAdapter(cmd);

                DataTable UserTable = new DataTable();
                UserAdapter.Fill(UserTable);

                if (UserTable.Rows[0][0] != null)
                {
                    byte[] UserImg = (byte[])UserTable.Rows[0][0];

                    MemoryStream imgstream = new MemoryStream(UserImg);

                    if (imgstream.Length > 0)
                        this.pictBoxUser.Image = Image.FromStream(imgstream);
                }
                UserAdapter.Dispose();
            }
            Globals.glCloseSqlConn();

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.frmUserProfile_RefreshUser();
        }
    }
}
