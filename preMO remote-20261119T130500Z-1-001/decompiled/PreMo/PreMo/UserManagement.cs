using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace PreMo;

public class UserManagement : Form
{
	private string xmlUserInfoPath = "C:\\Users\\Anthony P. Kuzub\\Desktop\\PreMo1.15\\PreMo\\Resources\\UserInfo.xml";

	private IContainer components = null;

	private Button btnUpdate;

	private Label lbError;

	private DataGridView dataGridView1;

	private Button btnCancel;

	public UserManagement()
	{
		InitializeComponent();
		DataSet dataSet = new DataSet();
		dataSet.ReadXml(xmlUserInfoPath);
		dataGridView1.DataSource = dataSet.Tables[0];
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected Obj, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected Obj, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		DataSet dataSet = new DataSet();
		DataTable dataTable = new DataTable();
		DialogResult val = MessageBox.Show("Update records?", "Confirm", (MessageBoxButtons)1, (MessageBoxIcon)32);
		if ((int)val == 1)
		{
			foreach (DataGridViewColumn item in (BaseCollection)dataGridView1.Columns)
			{
				DataGridViewColumn val2 = item;
				dataTable.Columns.Add(val2.DataPropertyName, val2.ValueType);
			}
			foreach (DataGridViewRow item2 in (IEnumerable)dataGridView1.Rows)
			{
				DataGridViewRow val3 = item2;
				DataRow dataRow = dataTable.NewRow();
				for (int i = 0; i < dataGridView1.ColumnCount; i++)
				{
					dataRow[i] = ((val3.Cells[i].Value == null) ? DBNull.Value : val3.Cells[i].Value);
				}
				dataTable.Rows.Add(dataRow);
			}
			dataSet.Tables.Add(dataTable);
			dataSet.WriteXml(xmlUserInfoPath);
			RemoveChildlessNode("Table1");
			MessageBox.Show("Updated records.", "Success");
		}
		else
		{
			MessageBox.Show("Changes were not applied.", "Aborted");
		}
	}

	private void btnRevert_Click(object sender, EventArgs e)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		DataSet dataSet = new DataSet();
		DataTable dataTable = new DataTable();
		DialogResult val = MessageBox.Show("Revert to last update?\nThis will undo any unsaved changes.", "Confirm", (MessageBoxButtons)1, (MessageBoxIcon)32);
		if ((int)val == 1)
		{
			dataSet.ReadXml(xmlUserInfoPath);
			dataGridView1.DataSource = dataSet.Tables[0];
			MessageBox.Show("Rolled back to last update.", "Rollback");
		}
	}

	private void RemoveChildlessNode(string nodeName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(xmlUserInfoPath);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//" + nodeName);
		foreach (XmlNode item in xmlNodeList)
		{
			if (!item.HasChildNodes)
			{
				item.ParentNode.RemoveChild(item);
			}
		}
		xmlDocument.Save(xmlUserInfoPath);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected Obj, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
		btnUpdate = new Button();
		lbError = new Label();
		dataGridView1 = new DataGridView();
		btnCancel = new Button();
		((ISupportInitialize)dataGridView1).BeginInit();
		((Control)this).SuspendLayout();
		((Control)btnUpdate).Location = new Point(446, 12);
		((Control)btnUpdate).Name = "btnUpdate";
		((Control)btnUpdate).Size = new Size(80, 23);
		((Control)btnUpdate).TabIndex = 3;
		((Control)btnUpdate).Text = "Update";
		((ButtonBase)btnUpdate).UseVisualStyleBackColor = true;
		((Control)btnUpdate).Click += btnUpdate_Click;
		((Control)lbError).AutoSize = true;
		((Control)lbError).ForeColor = Color.Red;
		((Control)lbError).Location = new Point(445, 147);
		((Control)lbError).Name = "lbError";
		((Control)lbError).Size = new Size(0, 13);
		((Control)lbError).TabIndex = 10;
		dataGridView1.ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode)2;
		((Control)dataGridView1).Location = new Point(12, 12);
		((Control)dataGridView1).Name = "dataGridView1";
		((Control)dataGridView1).Size = new Size(427, 265);
		((Control)dataGridView1).TabIndex = 11;
		((Control)btnCancel).Location = new Point(448, 254);
		((Control)btnCancel).Name = "btnCancel";
		((Control)btnCancel).Size = new Size(80, 23);
		((Control)btnCancel).TabIndex = 12;
		((Control)btnCancel).Text = "Revert";
		((ButtonBase)btnCancel).UseVisualStyleBackColor = true;
		((Control)btnCancel).Click += btnRevert_Click;
		((ContainerControl)this).AutoScaleDimensions = new SizeF(6f, 13f);
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)1;
		((Form)this).ClientSize = new Size(538, 289);
		((Control)this).Controls.Add((Control)(object)btnCancel);
		((Control)this).Controls.Add((Control)(object)dataGridView1);
		((Control)this).Controls.Add((Control)(object)lbError);
		((Control)this).Controls.Add((Control)(object)btnUpdate);
		((Control)this).Name = "UserManagement";
		((Control)this).Text = "UserManagement";
		((ISupportInitialize)dataGridView1).EndInit();
		((Control)this).ResumeLayout(false);
		((Control)this).PerformLayout();
	}
}
