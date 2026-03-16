<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmKenoHelp
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
        BtnClose = New Button()
        ScHelp = New SplitContainer()
        TvwTopics = New TreeView()
        RtbContent = New RichTextBox()
        CType(ScHelp, ComponentModel.ISupportInitialize).BeginInit()
        ScHelp.Panel1.SuspendLayout()
        ScHelp.Panel2.SuspendLayout()
        ScHelp.SuspendLayout()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.BackColor = Color.ForestGreen
        Label1.BorderStyle = BorderStyle.Fixed3D
        Label1.Dock = DockStyle.Top
        Label1.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        Label1.ForeColor = Color.WhiteSmoke
        Label1.Location = New Point(0, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(1254, 45)
        Label1.TabIndex = 0
        Label1.Text = "Keno Game Help && Instructions"
        Label1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' BtnClose
        ' 
        BtnClose.AutoSize = True
        BtnClose.BackColor = Color.MistyRose
        BtnClose.Dock = DockStyle.Bottom
        BtnClose.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        BtnClose.Location = New Point(0, 736)
        BtnClose.Name = "BtnClose"
        BtnClose.Size = New Size(1254, 45)
        BtnClose.TabIndex = 1
        BtnClose.Text = "Close"
        BtnClose.UseVisualStyleBackColor = False
        ' 
        ' ScHelp
        ' 
        ScHelp.Dock = DockStyle.Fill
        ScHelp.Location = New Point(0, 45)
        ScHelp.Name = "ScHelp"
        ' 
        ' ScHelp.Panel1
        ' 
        ScHelp.Panel1.Controls.Add(TvwTopics)
        ' 
        ' ScHelp.Panel2
        ' 
        ScHelp.Panel2.Controls.Add(RtbContent)
        ScHelp.Size = New Size(1254, 691)
        ScHelp.SplitterDistance = 418
        ScHelp.TabIndex = 2
        ' 
        ' TvwTopics
        ' 
        TvwTopics.Dock = DockStyle.Fill
        TvwTopics.Font = New Font("Segoe UI", 10.0F)
        TvwTopics.Location = New Point(0, 0)
        TvwTopics.Name = "TvwTopics"
        TvwTopics.Size = New Size(418, 691)
        TvwTopics.TabIndex = 0
        ' 
        ' RtbContent
        ' 
        RtbContent.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(240))
        RtbContent.Dock = DockStyle.Fill
        RtbContent.Font = New Font("Segoe UI", 10.0F)
        RtbContent.Location = New Point(0, 0)
        RtbContent.Margin = New Padding(3, 3, 15, 3)
        RtbContent.Name = "RtbContent"
        RtbContent.ReadOnly = True
        RtbContent.ScrollBars = RichTextBoxScrollBars.Vertical
        RtbContent.ShowSelectionMargin = True
        RtbContent.Size = New Size(832, 691)
        RtbContent.TabIndex = 0
        RtbContent.TabStop = False
        RtbContent.Text = ""
        ' 
        ' FrmKenoHelp
        ' 
        AutoScaleDimensions = New SizeF(10.0F, 25.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1254, 781)
        Controls.Add(ScHelp)
        Controls.Add(BtnClose)
        Controls.Add(Label1)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmKenoHelp"
        Text = "Keno Help"
        ScHelp.Panel1.ResumeLayout(False)
        ScHelp.Panel2.ResumeLayout(False)
        CType(ScHelp, ComponentModel.ISupportInitialize).EndInit()
        ScHelp.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents BtnClose As Button
    Friend WithEvents ScHelp As SplitContainer
    Friend WithEvents TvwTopics As TreeView
    Friend WithEvents RtbContent As RichTextBox
End Class
