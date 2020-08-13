<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Napoveda
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
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.ListBox1 = New System.Windows.Forms.ListBox()
		Me.lblPopis = New System.Windows.Forms.Label()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
		Me.OK_Button.Location = New System.Drawing.Point(116, 280)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "OK"
		'
		'ListBox1
		'
		Me.ListBox1.FormattingEnabled = True
		Me.ListBox1.Location = New System.Drawing.Point(12, 12)
		Me.ListBox1.Name = "ListBox1"
		Me.ListBox1.Size = New System.Drawing.Size(171, 160)
		Me.ListBox1.TabIndex = 1
		'
		'lblPopis
		'
		Me.lblPopis.Location = New System.Drawing.Point(12, 175)
		Me.lblPopis.Name = "lblPopis"
		Me.lblPopis.Size = New System.Drawing.Size(171, 79)
		Me.lblPopis.TabIndex = 2
		Me.lblPopis.Text = "Vyberte polužku nápovědy"
		'
		'Napoveda
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(195, 315)
		Me.Controls.Add(Me.lblPopis)
		Me.Controls.Add(Me.ListBox1)
		Me.Controls.Add(Me.OK_Button)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "Napoveda"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Nápověda"
		Me.ResumeLayout(False)

	End Sub
	Friend WithEvents OK_Button As System.Windows.Forms.Button
	Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
	Friend WithEvents lblPopis As System.Windows.Forms.Label

End Class
