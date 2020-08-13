<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.TextBox1 = New System.Windows.Forms.TextBox()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.nudInterval = New System.Windows.Forms.NumericUpDown()
		Me.nudX = New System.Windows.Forms.NumericUpDown()
		Me.nudY = New System.Windows.Forms.NumericUpDown()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.cchDebug = New System.Windows.Forms.CheckBox()
		Me.btnNapoveda = New System.Windows.Forms.Button()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.nudInterval, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.nudX, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.nudY, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.White
		Me.PictureBox1.Location = New System.Drawing.Point(12, 12)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(200, 200)
		Me.PictureBox1.TabIndex = 0
		Me.PictureBox1.TabStop = False
		'
		'TextBox1
		'
		Me.TextBox1.Location = New System.Drawing.Point(12, 246)
		Me.TextBox1.Multiline = True
		Me.TextBox1.Name = "TextBox1"
		Me.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.TextBox1.Size = New System.Drawing.Size(200, 91)
		Me.TextBox1.TabIndex = 1
		'
		'Button1
		'
		Me.Button1.Location = New System.Drawing.Point(137, 343)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(54, 23)
		Me.Button1.TabIndex = 2
		Me.Button1.Text = "Proveď"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'nudInterval
		'
		Me.nudInterval.Location = New System.Drawing.Point(63, 343)
		Me.nudInterval.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
		Me.nudInterval.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
		Me.nudInterval.Name = "nudInterval"
		Me.nudInterval.Size = New System.Drawing.Size(69, 20)
		Me.nudInterval.TabIndex = 3
		Me.nudInterval.Value = New Decimal(New Integer() {2, 0, 0, 0})
		'
		'nudX
		'
		Me.nudX.Location = New System.Drawing.Point(35, 218)
		Me.nudX.Maximum = New Decimal(New Integer() {9, 0, 0, 0})
		Me.nudX.Name = "nudX"
		Me.nudX.Size = New System.Drawing.Size(55, 20)
		Me.nudX.TabIndex = 4
		Me.nudX.Value = New Decimal(New Integer() {5, 0, 0, 0})
		'
		'nudY
		'
		Me.nudY.Location = New System.Drawing.Point(119, 218)
		Me.nudY.Maximum = New Decimal(New Integer() {9, 0, 0, 0})
		Me.nudY.Name = "nudY"
		Me.nudY.Size = New System.Drawing.Size(55, 20)
		Me.nudY.TabIndex = 5
		Me.nudY.Value = New Decimal(New Integer() {5, 0, 0, 0})
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 220)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(17, 13)
		Me.Label1.TabIndex = 6
		Me.Label1.Text = "X:"
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(96, 220)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(17, 13)
		Me.Label2.TabIndex = 7
		Me.Label2.Text = "Y:"
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(12, 345)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(45, 13)
		Me.Label3.TabIndex = 8
		Me.Label3.Text = "Interval:"
		'
		'cchDebug
		'
		Me.cchDebug.AutoSize = True
		Me.cchDebug.Location = New System.Drawing.Point(197, 345)
		Me.cchDebug.Name = "cchDebug"
		Me.cchDebug.Size = New System.Drawing.Size(15, 14)
		Me.cchDebug.TabIndex = 9
		Me.cchDebug.UseVisualStyleBackColor = True
		'
		'btnNapoveda
		'
		Me.btnNapoveda.Location = New System.Drawing.Point(180, 217)
		Me.btnNapoveda.Name = "btnNapoveda"
		Me.btnNapoveda.Size = New System.Drawing.Size(32, 23)
		Me.btnNapoveda.TabIndex = 10
		Me.btnNapoveda.Text = "?"
		Me.btnNapoveda.UseVisualStyleBackColor = True
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(224, 378)
		Me.Controls.Add(Me.btnNapoveda)
		Me.Controls.Add(Me.cchDebug)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.nudY)
		Me.Controls.Add(Me.nudX)
		Me.Controls.Add(Me.nudInterval)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.TextBox1)
		Me.Controls.Add(Me.PictureBox1)
		Me.Name = "Form1"
		Me.Text = "Karel robot"
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.nudInterval, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.nudX, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.nudY, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
	Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
	Friend WithEvents Button1 As System.Windows.Forms.Button
	Friend WithEvents nudInterval As System.Windows.Forms.NumericUpDown
	Friend WithEvents nudX As System.Windows.Forms.NumericUpDown
	Friend WithEvents nudY As System.Windows.Forms.NumericUpDown
	Friend WithEvents Label1 As System.Windows.Forms.Label
	Friend WithEvents Label2 As System.Windows.Forms.Label
	Friend WithEvents Label3 As System.Windows.Forms.Label
	Friend WithEvents cchDebug As System.Windows.Forms.CheckBox
	Friend WithEvents btnNapoveda As System.Windows.Forms.Button

End Class
