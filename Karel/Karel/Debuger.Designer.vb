<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Debuger
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
		Me.Label1 = New System.Windows.Forms.Label()
		Me.listPrikazy = New System.Windows.Forms.ListBox()
		Me.txtKonzole = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.btnStep = New System.Windows.Forms.Button()
		Me.btnEnd = New System.Windows.Forms.Button()
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 43)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(69, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Zdrojový kód"
		'
		'listPrikazy
		'
		Me.listPrikazy.FormattingEnabled = True
		Me.listPrikazy.Location = New System.Drawing.Point(12, 59)
		Me.listPrikazy.Name = "listPrikazy"
		Me.listPrikazy.Size = New System.Drawing.Size(192, 108)
		Me.listPrikazy.TabIndex = 1
		'
		'txtKonzole
		'
		Me.txtKonzole.Enabled = False
		Me.txtKonzole.Location = New System.Drawing.Point(12, 186)
		Me.txtKonzole.Multiline = True
		Me.txtKonzole.Name = "txtKonzole"
		Me.txtKonzole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtKonzole.Size = New System.Drawing.Size(192, 138)
		Me.txtKonzole.TabIndex = 2
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(12, 170)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(45, 13)
		Me.Label2.TabIndex = 3
		Me.Label2.Text = "Konzole"
		'
		'btnStep
		'
		Me.btnStep.Location = New System.Drawing.Point(12, 12)
		Me.btnStep.Name = "btnStep"
		Me.btnStep.Size = New System.Drawing.Size(75, 23)
		Me.btnStep.TabIndex = 4
		Me.btnStep.Text = "Krok"
		Me.btnStep.UseVisualStyleBackColor = True
		'
		'btnEnd
		'
		Me.btnEnd.Location = New System.Drawing.Point(93, 12)
		Me.btnEnd.Name = "btnEnd"
		Me.btnEnd.Size = New System.Drawing.Size(75, 23)
		Me.btnEnd.TabIndex = 5
		Me.btnEnd.Text = "Konec"
		Me.btnEnd.UseVisualStyleBackColor = True
		'
		'Debuger
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(216, 336)
		Me.Controls.Add(Me.btnEnd)
		Me.Controls.Add(Me.btnStep)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.txtKonzole)
		Me.Controls.Add(Me.listPrikazy)
		Me.Controls.Add(Me.Label1)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "Debuger"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Debuger"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents Label1 As System.Windows.Forms.Label
	Friend WithEvents listPrikazy As System.Windows.Forms.ListBox
	Friend WithEvents txtKonzole As System.Windows.Forms.TextBox
	Friend WithEvents Label2 As System.Windows.Forms.Label
	Friend WithEvents btnStep As System.Windows.Forms.Button
	Friend WithEvents btnEnd As System.Windows.Forms.Button

End Class
