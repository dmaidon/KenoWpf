<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmGameLog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Label1 = New Label()
        TableLayoutPanel1 = New TableLayoutPanel()
        BtnClearFile = New Button()
        BtnClose = New Button()
        RtbContent = New RichTextBox()
        TableLayoutPanel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.BackColor = Color.ForestGreen
        Label1.BorderStyle = BorderStyle.Fixed3D
        Label1.Dock = DockStyle.Top
        Label1.Font = New Font("Segoe UI", 14F, FontStyle.Bold)
        Label1.ForeColor = Color.White
        Label1.Location = New Point(0, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(1153, 45)
        Label1.TabIndex = 0
        Label1.Text = "Keno Game Log"
        Label1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.BackColor = Color.FromArgb(CByte(0), CByte(192), CByte(0))
        TableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble
        TableLayoutPanel1.ColumnCount = 2
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.Controls.Add(BtnClearFile, 0, 0)
        TableLayoutPanel1.Controls.Add(BtnClose, 1, 0)
        TableLayoutPanel1.Dock = DockStyle.Bottom
        TableLayoutPanel1.Location = New Point(0, 851)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 1
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.Size = New Size(1153, 45)
        TableLayoutPanel1.TabIndex = 1
        ' 
        ' BtnClearFile
        ' 
        BtnClearFile.Dock = DockStyle.Fill
        BtnClearFile.Location = New Point(6, 6)
        BtnClearFile.Name = "BtnClearFile"
        BtnClearFile.Size = New Size(566, 33)
        BtnClearFile.TabIndex = 0
        BtnClearFile.Text = "Clear File"
        BtnClearFile.UseVisualStyleBackColor = True
        ' 
        ' BtnClose
        ' 
        BtnClose.Dock = DockStyle.Fill
        BtnClose.Location = New Point(581, 6)
        BtnClose.Name = "BtnClose"
        BtnClose.Size = New Size(566, 33)
        BtnClose.TabIndex = 1
        BtnClose.Text = "Close"
        BtnClose.UseVisualStyleBackColor = True
        ' 
        ' RtbContent
        ' 
        RtbContent.BackColor = SystemColors.ButtonFace
        RtbContent.Dock = DockStyle.Fill
        RtbContent.Location = New Point(0, 45)
        RtbContent.Name = "RtbContent"
        RtbContent.ReadOnly = True
        RtbContent.ShowSelectionMargin = True
        RtbContent.Size = New Size(1153, 806)
        RtbContent.TabIndex = 2
        RtbContent.Text = ""
        ' 
        ' FrmGameLog
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1153, 896)
        Controls.Add(RtbContent)
        Controls.Add(TableLayoutPanel1)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmGameLog"
        ShowInTaskbar = False
        SizeGripStyle = SizeGripStyle.Hide
        StartPosition = FormStartPosition.CenterParent
        Text = "Keno Game Log Display"
        TableLayoutPanel1.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents BtnClearFile As Button
    Friend WithEvents BtnClose As Button
    Friend WithEvents RtbContent As RichTextBox
End Class
